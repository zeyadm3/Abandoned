using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The molten channel down the middle of the plant: a refractory-lined trench 2.6 m deep, a glowing surface
    /// with no collider (whatever goes in sinks onto the bed, out of sight), the host's <see cref="LavaVolume"/>,
    /// and a dented guard rail along the edges with gaps where it has broken away.
    /// Real iron works additions: a fiery runner trough tapped from the blast furnace with an arched footbridge,
    /// a heavy ladle transfer car on rail tracks, radiation heat shields, tool racks and rising molten sparks.
    /// </summary>
    public static class IronFactoryChannel
    {
        private const float RailHeight = 1.05f;
        private const float RailBar = .08f;
        private const float PostSpacing = 2f;

        private static readonly (float, float)[] WestRuns = { (20f, 32f), (35f, 47f), (52f, 64f), (69f, 80f) };
        private static readonly (float, float)[] EastRuns = { (20f, 27f), (31f, 44f), (48f, 61f), (63f, 80f) };
        private static readonly (float, float)[] SouthRuns = { (52f, 66f), (70f, 72f) };
        private static readonly (float, float)[] NorthRuns = { (52f, 58f), (61f, 72f) };

        public static void Build(Transform root, IronFactoryArt art)
        {
            Rect c = L.ChannelMeters;
            Transform channel = Group("Channel", root);
            channel.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

            float depth = -L.ChannelFloorY;
            const float lining = .4f;
            GameObject bed = Box("Bed", channel, new Vector3(c.center.x, L.ChannelBedY - .2f, c.center.y), new Vector3(c.width, .4f, c.height), art.Bed);
            bed.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            Box("Lining_W", channel, new Vector3(c.xMin + lining * .5f, -depth * .5f, c.center.y), new Vector3(lining, depth, c.height), art.Lining);
            Box("Lining_E", channel, new Vector3(c.xMax - lining * .5f, -depth * .5f, c.center.y), new Vector3(lining, depth, c.height), art.Lining);
            Box("Lining_S", channel, new Vector3(c.center.x, -depth * .5f, c.yMin + lining * .5f), new Vector3(c.width, depth, lining), art.Lining);
            Box("Lining_N", channel, new Vector3(c.center.x, -depth * .5f, c.yMax - lining * .5f), new Vector3(c.width, depth, lining), art.Lining);

            // Flat steel coping on the lip
            Box("Coping_W", channel, new Vector3(c.xMin - .15f, .015f, c.center.y), new Vector3(.3f, .03f, c.height + .6f), art.Safety, false);
            Box("Coping_E", channel, new Vector3(c.xMax + .15f, .015f, c.center.y), new Vector3(.3f, .03f, c.height + .6f), art.Safety, false);
            Box("Coping_S", channel, new Vector3(c.center.x, .015f, c.yMin - .15f), new Vector3(c.width, .03f, .3f), art.Safety, false);
            Box("Coping_N", channel, new Vector3(c.center.x, .015f, c.yMax + .15f), new Vector3(c.width, .03f, .3f), art.Safety, false);
            MarkStatic(channel);

            GameObject surface = Box("LavaSurface", channel, new Vector3(c.center.x, L.LavaSurfaceY, c.center.y),
                new Vector3(c.width - 2f * lining, .06f, c.height - 2f * lining), art.Lava, false);
            Renderer surfaceRenderer = surface.GetComponent<Renderer>();
            surfaceRenderer.shadowCastingMode = ShadowCastingMode.Off;

            Transform lava = Group("Lava", channel);
            lava.gameObject.AddComponent<LavaVolume>().EditorSetup(L.LavaKill);
            lava.gameObject.AddComponent<PowerIndependentLight>();
            lava.gameObject.AddComponent<LavaSurface>().EditorSetup(surfaceRenderer, Glow(lava, c));
            Sparks(lava, c);

            Rails(Group("ChannelRails", root), c, art);
            Runner(Group("TappingRunner", root), c, art);
            LadleTransferCar(Group("LadleCar", root), c, art);
            BlastShields(Group("HeatShields", root), c, art);
            TappingToolRacks(Group("TappingTools", root), art);
        }

        // Fiery refractory runner trough running from the blast furnace taphole into the molten channel.
        private static void Runner(Transform parent, Rect c, IronFactoryArt art)
        {
            float x0 = 8.5f, x1 = c.xMin + .2f, z = 24f;
            float run = x1 - x0, midX = (x0 + x1) * .5f;
            const float wallThick = .22f, troughH = .36f, innerW = .65f;

            // Refractory side walls and bed
            Box("Bed", parent, new Vector3(midX, .05f, z), new Vector3(run, .1f, innerW + wallThick * 2f), art.Lining);
            Box("Wall_S", parent, new Vector3(midX, troughH * .5f, z - (innerW * .5f + wallThick * .5f)), new Vector3(run, troughH, wallThick), art.Lining);
            Box("Wall_N", parent, new Vector3(midX, troughH * .5f, z + (innerW * .5f + wallThick * .5f)), new Vector3(run, troughH, wallThick), art.Lining);

            // Glowing molten iron stream inside
            Box("Stream", parent, new Vector3(midX, .18f, z), new Vector3(run - .4f, .06f, innerW), art.Lava, false);

            // Steel armor straps every 4 m
            for (float x = x0 + 2f; x < x1 - 1f; x += 4f)
                Box("ArmorBand", parent, new Vector3(x, troughH * .5f, z), new Vector3(.1f, troughH + .02f, innerW + wallThick * 2f + .04f), art.Steel, false);

            // Skimmer dam and slag branch at x = 21 m
            Box("SkimmerBlock", parent, new Vector3(21f, .38f, z), new Vector3(.3f, .22f, innerW + .1f), art.Lining);
            Box("SlagBranch", parent, new Vector3(21f, .12f, z + 2.2f), new Vector3(.7f, .24f, 3.2f), art.Lining);
            Box("SlagCrust", parent, new Vector3(21f, .16f, z + 2.2f), new Vector3(.45f, .05f, 2.8f), art.Bed, false);

            // Pouring spout over the channel edge
            Box("PouringSpout", parent, new Vector3(x1 + .35f, .16f, z), new Vector3(.8f, .16f, innerW + .1f), art.Steel, false);

            // Arched footbridge spanning over the runner at x = 32 m so the main north-south route is clear
            Transform bridge = Group("Footbridge", parent);
            bridge.position = new Vector3(32f, 0f, z);
            Box("Deck", bridge, Vector3.up * .48f, new Vector3(2f, .08f, 2.6f), art.Steel);
            foreach (float side in new[] { -1f, 1f })
            {
                Box("Ramp", bridge, new Vector3(0f, .24f, side * 1.55f), new Vector3(2f, .06f, .8f), art.Steel)
                    .transform.localRotation = Quaternion.Euler(side * 28f, 0f, 0f);
                Box("Rail", bridge, new Vector3(side * 1.05f, 1f, 0f), new Vector3(.08f, 1f, 2.8f), art.Safety);
            }
            bridge.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            MarkStatic(parent);
        }

        // Broad-gauge ladle transfer car parked on the east bank
        private static void LadleTransferCar(Transform parent, Rect c, IronFactoryArt art)
        {
            float railX = c.xMax + 2.2f;
            const float railZ0 = 34f, railZ1 = 58f, gauge = 2.4f;
            float midZ = (railZ0 + railZ1) * .5f, len = railZ1 - railZ0;

            Box("Ballast", parent, new Vector3(railX, -.01f, midZ), new Vector3(gauge + 1.2f, .04f, len), art.Bed, false);
            foreach (float side in new[] { -1f, 1f })
            {
                Box("Rail", parent, new Vector3(railX + side * gauge * .5f, .08f, midZ), new Vector3(.1f, .14f, len), art.Steel, false);
                Box("Buffer", parent, new Vector3(railX + side * gauge * .5f, .5f, railZ0), new Vector3(.25f, .9f, .5f), art.Safety);
                Box("Buffer", parent, new Vector3(railX + side * gauge * .5f, .5f, railZ1), new Vector3(.25f, .9f, .5f), art.Safety);
            }

            // Transfer car at z = 42
            Transform car = Group("Car", parent);
            car.position = new Vector3(railX, 0f, 42f);
            Box("Chassis", car, Vector3.up * .55f, new Vector3(3.2f, .5f, 4.4f), art.Steel);
            foreach (float x in new[] { -1.2f, 1.2f })
            foreach (float z in new[] { -1.5f, 1.5f })
                Primitive(PrimitiveType.Cylinder, "Wheel", car, new Vector3(x, .35f, z), new Vector3(.6f, .12f, .6f), art.Bed, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // 50-ton pouring ladle tilted 12 degrees toward the lava channel
            Transform pot = Group("Ladle", car);
            pot.localPosition = Vector3.up * 1.8f;
            pot.localRotation = Quaternion.Euler(0f, 0f, 12f);
            Primitive(PrimitiveType.Cylinder, "Body", pot, Vector3.zero, new Vector3(2.8f, 1.4f, 2.8f), art.Rust, false);
            Primitive(PrimitiveType.Cylinder, "Rim", pot, Vector3.up * 1.4f, new Vector3(3f, .1f, 3f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "Interior", pot, Vector3.up * 1.35f, new Vector3(2.5f, .06f, 2.5f), art.Lining, false);
            Box("Spout", pot, new Vector3(-1.45f, 1.35f, 0f), new Vector3(.5f, .2f, .7f), art.Steel, false);
            foreach (float z in new[] { -1.6f, 1.6f })
                Primitive(PrimitiveType.Cylinder, "Trunnion", pot, new Vector3(0f, .5f, z), new Vector3(.35f, .3f, .35f), art.Steel, false)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            car.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            MarkStatic(parent);
        }

        // Curved radiant heat shields along the banks
        private static void BlastShields(Transform parent, Rect c, IronFactoryArt art)
        {
            Vector3[] positions =
            {
                new(c.xMin - 1.2f, 0f, 33.5f),
                new(c.xMin - 1.2f, 0f, 49.5f),
                new(c.xMax + 1.2f, 0f, 29f),
                new(c.xMax + 1.2f, 0f, 46f),
            };
            for (int i = 0; i < positions.Length; i++)
            {
                Transform shield = Group($"HeatShield_{i}", parent);
                shield.position = positions[i];
                bool west = positions[i].x < c.center.x;
                shield.rotation = Quaternion.Euler(0f, west ? 90f : -90f, 0f);

                // Two heavy steel posts
                foreach (float x in new[] { -.9f, .9f })
                    Box("Post", shield, new Vector3(x, 1f, 0f), new Vector3(.1f, 2f, .1f), art.Steel, false);
                // Lower and upper corrugated plates with a narrow viewing slit between them
                Box("LowerPlate", shield, new Vector3(0f, .7f, 0f), new Vector3(2.1f, 1.35f, .04f), art.Rust);
                Box("UpperPlate", shield, new Vector3(0f, 1.7f, 0f), new Vector3(2.1f, .45f, .04f), art.Rust);
                Box("SlitFrame", shield, new Vector3(0f, 1.42f, .02f), new Vector3(1.2f, .08f, .05f), art.Safety, false);
                shield.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            }
            MarkStatic(parent);
        }

        // Tapping bars, slag rakes and sampling spoons
        private static void TappingToolRacks(Transform parent, IronFactoryArt art)
        {
            Vector3[] rackSpots = { new(18f, 0f, 22.2f), new(50f, 0f, 22.2f) };
            for (int i = 0; i < rackSpots.Length; i++)
            {
                Transform rack = Group($"ToolRack_{i}", parent);
                rack.position = rackSpots[i];
                Box("FrameA", rack, new Vector3(-.7f, .75f, 0f), new Vector3(.06f, 1.5f, .6f), art.Steel, false);
                Box("FrameB", rack, new Vector3(.7f, .75f, 0f), new Vector3(.06f, 1.5f, .6f), art.Steel, false);
                Box("Bar", rack, new Vector3(0f, 1.2f, 0f), new Vector3(1.5f, .05f, .05f), art.Steel, false);

                // Leaning tapping rods (3.5 m steel bars)
                for (int r = 0; r < 4; r++)
                {
                    GameObject rod = Box("TappingRod", rack, new Vector3(-.5f + r * .32f, 1.4f, .1f), new Vector3(.03f, 3.2f, .03f), art.Steel, false);
                    rod.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
                }
            }
            MarkStatic(parent);
        }

        // Sparks popping and drifting up from the molten iron channel
        private static void Sparks(Transform lava, Rect c)
        {
            var go = new GameObject("MoltenSparks");
            go.transform.SetParent(lava, false);
            go.transform.localPosition = new Vector3(c.center.x, L.LavaSurfaceY + .2f, c.center.y);

            var ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startLifetime = 3.2f;
            main.startSpeed = 2.4f;
            main.startSize = new ParticleSystem.MinMaxCurve(.04f, .12f);
            main.startColor = new Color(1f, .78f, .28f, .95f);
            main.maxParticles = 140;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -.08f; // rise with heat

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 32f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(c.width * .88f, .1f, c.height * .88f);

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = .3f;
            noise.frequency = .4f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = SparksMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static Material SparksMaterial()
        {
            const string path = "Assets/_Project/Art/Factory/Materials/IronFactory_Sparks.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")) { name = "IronFactory_Sparks" };
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetColor("_BaseColor", new Color(1f, .75f, .25f, .95f));
            m.SetColor("_EmissionColor", new Color(3.5f, 1.8f, .4f));
            m.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(m);
            return m;
        }

        private static Light[] Glow(Transform lava, Rect c)
        {
            var lights = new List<Light>();
            for (float z = c.yMin + 5f; z < c.yMax; z += 10f)
            {
                Transform holder = Group($"Glow_{lights.Count}", lava);
                holder.position = new Vector3(c.center.x, .6f, z);
                Light light = holder.gameObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 17f;
                light.intensity = 9f;
                light.color = new Color(1f, .42f, .1f);
                light.shadows = LightShadows.None;
                lights.Add(light);
            }
            return lights.ToArray();
        }

        private static void Rails(Transform rails, Rect c, IronFactoryArt art)
        {
            const float outside = .35f;
            foreach ((float a, float b) in WestRuns) Run(rails, "W", new Vector3(c.xMin - outside, 0f, a), new Vector3(c.xMin - outside, 0f, b), art);
            foreach ((float a, float b) in EastRuns) Run(rails, "E", new Vector3(c.xMax + outside, 0f, a), new Vector3(c.xMax + outside, 0f, b), art);
            foreach ((float a, float b) in SouthRuns) Run(rails, "S", new Vector3(a, 0f, c.yMin - outside), new Vector3(b, 0f, c.yMin - outside), art);
            foreach ((float a, float b) in NorthRuns) Run(rails, "N", new Vector3(a, 0f, c.yMax + outside), new Vector3(b, 0f, c.yMax + outside), art);
            MarkStatic(rails);
        }

        private static void Run(Transform rails, string side, Vector3 from, Vector3 to, IronFactoryArt art)
        {
            Transform run = Group($"Rail_{side}_{rails.childCount:00}", rails);
            Vector3 along = to - from;
            float length = along.magnitude;
            Quaternion facing = Quaternion.LookRotation(along.normalized, Vector3.up);
            Vector3 mid = (from + to) * .5f;
            foreach (float y in new[] { RailHeight, RailHeight * .5f })
            {
                GameObject bar = Box(y > .9f ? "TopRail" : "MidRail", run, mid + Vector3.up * y, new Vector3(RailBar, RailBar, length), art.Safety);
                bar.transform.rotation = facing;
            }
            int posts = Mathf.Max(2, Mathf.RoundToInt(length / PostSpacing) + 1);
            for (int i = 0; i < posts; i++)
            {
                Vector3 at = Vector3.Lerp(from, to, i / (float)(posts - 1));
                Box($"Post_{i:00}", run, at + Vector3.up * RailHeight * .5f, new Vector3(RailBar, RailHeight, RailBar), art.Steel);
            }
        }
    }
}

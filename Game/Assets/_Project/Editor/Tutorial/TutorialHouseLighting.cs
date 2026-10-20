using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A Michigan afternoon in mid-December, about 3 PM: the sun already low in the south-west behind lake-effect
    /// overcast, light snow falling over the yard. Inside, the power is off: ceiling fixtures in every room (dark
    /// until the basement breaker relights their sector), bare bulbs in the basement, and the breaker panel on the
    /// furnace wall. Fixtures never cast shadows (the shadow director picks the nearest lit ones).
    /// </summary>
    public static class TutorialHouseLighting
    {
        public static readonly LevelAtmosphere.Mood December3PM = new(
            sky: new Color(.50f, .56f, .64f), equator: new Color(.55f, .59f, .64f), ground: new Color(.63f, .66f, .71f),
            fog: new Color(.71f, .75f, .81f), fogDensity: .011f, bloom: .16f, vignette: .2f, grain: .08f,
            saturation: -18f, contrast: 10f, sunColor: new Color(1f, .86f, .72f), sunIntensity: .75f, sunEuler: new Vector3(13f, 38f, 0f));

        public static readonly Vector3 BreakerAt = new(.14f, L.BasementFloor, TutorialHouseBasement.FurnaceZ - 2.3f);

        public static void Build(Transform root, TutorialHouseArt art)
        {
            Transform lights = Group("Lighting", root);
            Transform fixtures = Group("CeilingFixtures", lights);
            int seed = 1400;
            Vector3 C(Rect r, float dx = 0f, float dz = 0f) => new(r.center.x + dx, 0f, r.center.y + dz);
            // One dome per room, two in the long ones; the dining room's is a pendant over the table.
            (string room, Vector3 at, bool pendant)[] rooms =
            {
                ("BedroomA", C(L.BedroomA), false), ("BedroomB", C(L.BedroomB), false), ("HallBath", C(L.HallBath), false),
                ("Hallway", C(L.Hallway), false), ("Kitchen_South", C(L.Kitchen, 0f, -1.8f), false), ("Kitchen_North", C(L.Kitchen, 0f, 1.8f), false),
                ("Dining", TutorialHouseFittings.DiningTable, true), ("Living_West", C(L.Living, -1.7f), false), ("Living_East", C(L.Living, 1.7f), false),
                ("Laundry", C(L.Laundry), false), ("Ensuite", C(L.Ensuite), false), ("Master_West", C(L.Master, -2.2f), false),
                ("Master_East", C(L.Master, 2.2f), false),
            };
            foreach ((string room, Vector3 at, bool pendant) in rooms)
                Dome(fixtures, room, at, pendant, seed++, art);
            Porch(lights, art, seed++);
            Vector3 bulb = Basement(Group("BasementBulbs", lights), art, ref seed);
            Breaker(root, bulb, art);
            Reflections(Group("ReflectionProbes", lights));
            Snowfall(Group("Weather", root), art);
        }

        // A frosted glass dome on a round canopy, one warm lamp pointed at the floor.
        private static void Dome(Transform parent, string room, Vector3 at, bool pendant, int seed, TutorialHouseArt art)
        {
            Transform f = Group($"Fixture_{room}", parent);
            float y = pendant ? L.Ceiling - .85f : L.Ceiling - .06f;
            f.position = new Vector3(at.x, y, at.z);
            Primitive(PrimitiveType.Cylinder, "Canopy", f, new Vector3(0f, .03f, 0f), new Vector3(.26f, .02f, .26f), pendant ? art.Brass : art.Trim, false);
            GameObject glow = Primitive(PrimitiveType.Sphere, "FrostedDome", f, new Vector3(0f, -.04f, 0f), new Vector3(.34f, .14f, .34f), art.BulbGlow, false);
            if (pendant) Primitive(PrimitiveType.Cylinder, "Shade", f, new Vector3(0f, .02f, 0f), new Vector3(.5f, .12f, .5f), art.Brass, false);
            Light lamp = Spot(f, "Lamp", f.position + Vector3.down * .12f, new Color(1f, .8f, .58f), pendant ? 1.6f : 1.45f, 7.5f, 125f);
            f.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glow.GetComponent<Renderer>(), seed % 5 == 0, false, seed, art.BulbGlow.GetColor("_EmissionColor"));
        }

        private static void Porch(Transform parent, TutorialHouseArt art, int seed)
        {
            Transform f = Group("Fixture_Porch", parent);
            f.position = new Vector3(L.HouseEast + L.ExteriorWall * .5f + .1f, 2.5f, L.FrontDoorZ + L.FrontDoorWidth * .5f + .35f);
            Box("Backplate", f, Vector3.zero, new Vector3(.04f, .26f, .14f), art.DarkMetal, false);
            GameObject glass = Box("Lantern", f, new Vector3(.1f, -.06f, 0f), new Vector3(.16f, .26f, .16f), art.BulbGlow, false);
            Light lamp = Spot(f, "Lamp", f.position + new Vector3(.12f, -.2f, 0f), new Color(1f, .78f, .5f), 1.2f, 5f, 110f);
            f.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glass.GetComponent<Renderer>(), false, false, seed, art.BulbGlow.GetColor("_EmissionColor"));
        }

        // Bare bulbs on the joists, pull-chains hanging. Returns the one by the breaker (its sector is the breaker's).
        private static Vector3 Basement(Transform parent, TutorialHouseArt art, ref int seed)
        {
            // The first is by the breaker (its sector is the breaker's); none hangs over the rotten patch's landing.
            Vector3[] bulbs =
            {
                new(1.4f, -.5f, BreakerAt.z), new(2.2f, -.5f, 15.6f), new(6.8f, -.5f, 8.8f), new(9.3f, -.5f, 7.6f), new(6.7f, -.5f, 2.4f),
            };
            foreach (Vector3 at in bulbs)
            {
                Transform f = Group("BareBulb", parent);
                f.position = at;
                Primitive(PrimitiveType.Cylinder, "Socket", f, new Vector3(0f, .1f, 0f), new Vector3(.07f, .06f, .07f), art.Rubber, false);
                GameObject glass = Primitive(PrimitiveType.Sphere, "Bulb", f, Vector3.zero, new Vector3(.09f, .12f, .09f), art.BulbGlow, false);
                Box("PullChain", f, new Vector3(.04f, -.22f, 0f), new Vector3(.006f, .4f, .006f), art.Chrome, false);
                Light lamp = Spot(f, "Lamp", at + Vector3.down * .06f, new Color(1f, .74f, .45f), 1.5f, 6f, 140f);
                f.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glass.GetComponent<Renderer>(), seed++ % 3 == 0, false, seed, art.BulbGlow.GetColor("_EmissionColor"));
            }
            return bulbs[0];
        }

        // The panel on the furnace wall. Its sector is the one its own bulb belongs to (the horror pacing doesn't run
        // in training, so the power controller is handed the same sector map to use).
        private static void Breaker(Transform root, Vector3 bulb, TutorialHouseArt art)
        {
            HorrorConfig sectors = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            int sector = sectors != null ? sectors.SectorOf(bulb) : 0;
            Transform panel = Group("BreakerPanel", root);
            panel.SetPositionAndRotation(BreakerAt, Quaternion.Euler(0f, 90f, 0f));
            Box("Cabinet", panel, new Vector3(0f, 1.5f, .07f), new Vector3(.5f, .72f, .14f), art.Steel);
            Box("Door", panel, new Vector3(-.27f, 1.5f, .3f), new Vector3(.03f, .7f, .48f), art.Steel, false).transform.localRotation = Quaternion.Euler(0f, 70f, 0f);
            for (int i = 0; i < 8; i++)
                Box("BranchBreaker", panel, new Vector3(i % 2 == 0 ? -.08f : .08f, 1.32f + (i / 2) * .1f, .15f), new Vector3(.1f, .05f, .03f), art.DarkMetal, false);
            Transform handle = Group("MainHandle", panel);
            handle.localPosition = new Vector3(0f, 1.74f, .16f);
            Box("Lever", handle, new Vector3(0f, .07f, 0f), new Vector3(.06f, .16f, .05f), art.OrnamentRed, false);
            GameObject lamp = Box("PilotLamp", panel, new Vector3(.17f, 1.8f, .145f), new Vector3(.04f, .04f, .02f), art.OrnamentRed, false);
            Box("Conduit", panel, new Vector3(0f, 2.05f, .05f), new Vector3(.05f, .5f, .05f), art.DarkMetal, false);
            var hit = panel.gameObject.AddComponent<BoxCollider>();
            hit.center = new Vector3(0f, 1.5f, .1f);
            hit.size = new Vector3(.55f, .8f, .22f);
            panel.gameObject.AddComponent<BreakerPanel>().EditorSetup(sector, handle, lamp.GetComponent<Renderer>());
        }

        /// <summary>After the run plumbing exists: the power controller gets the sector map the breaker was placed by.</summary>
        public static void WirePower()
        {
            PowerController power = Object.FindFirstObjectByType<PowerController>();
            HorrorConfig sectors = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            if (power == null || sectors == null)
            {
                Debug.LogWarning("[TrainingHouse] No power controller or horror config: the breaker won't relight its lights.");
                return;
            }
            SerializedWiring.Set(power, "sectorsWithoutHorror", sectors);
        }

        private static Light Spot(Transform parent, string name, Vector3 world, Color color, float intensity, float range, float angle)
        {
            Transform holder = Group(name, parent);
            holder.position = world;
            holder.rotation = Quaternion.Euler(90f, 0f, 0f);
            Light light = holder.gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = angle;
            light.innerSpotAngle = angle * .5f;
            light.shadows = LightShadows.None;
            return light;
        }

        private static void Reflections(Transform parent)
        {
            (string name, Bounds b)[] spaces =
            {
                ("Band", L.Box(L.Band, 0f, L.Ceiling)), ("WestWing", L.Box(L.WestWing, 0f, L.Ceiling)),
                ("Suite", L.Box(L.Suite, 0f, L.Ceiling)), ("Basement", L.Box(L.BasementMain, L.BasementFloor, -.25f)),
            };
            foreach ((string name, Bounds b) in spaces)
            {
                Transform at = Group(name + "Reflection", parent);
                at.position = b.center;
                var probe = at.gameObject.AddComponent<ReflectionProbe>();
                probe.mode = ReflectionProbeMode.Realtime;
                probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
                probe.resolution = 64;
                probe.size = b.size;
                probe.boxProjection = true;
                probe.blendDistance = .8f;
                probe.intensity = .6f;
                probe.cullingMask = LayerMask.GetMask("Default");
                probe.clearFlags = ReflectionProbeClearFlags.SolidColor;
                probe.backgroundColor = new Color(.1f, .11f, .13f);
            }
        }

        // Snow over the yards only (never over the roof: there is no collider up there to stop it at the ceiling).
        private static void Snowfall(Transform parent, TutorialHouseArt art)
        {
            Snow(parent, "FrontYardSnow", new Vector3(45f, 11f, 10f), new Vector3(30f, .1f, 40f), 44f, art);
            Snow(parent, "NorthYardSnow", new Vector3(13f, 11f, 25.5f), new Vector3(40f, .1f, 14f), 26f, art);
            Snow(parent, "SouthYardSnow", new Vector3(13f, 11f, -8f), new Vector3(40f, .1f, 14f), 26f, art);
            Snow(parent, "BackYardSnow", new Vector3(-8f, 11f, 8.75f), new Vector3(14f, .1f, 30f), 18f, art);
            Snow(parent, "PatioSnow", new Vector3(12.2f, 8f, 3.2f), new Vector3(3.4f, .1f, 6f), 6f, art);
        }

        private static void Snow(Transform parent, string name, Vector3 at, Vector3 area, float rate, TutorialHouseArt art)
        {
            Transform emitter = Group(name, parent);
            emitter.position = at;
            var particles = emitter.gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.loop = true; main.playOnAwake = true; main.duration = 8f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 8f);
            main.startSpeed = 0f; main.startSize = new ParticleSystem.MinMaxCurve(.03f, .07f);
            main.startColor = new Color(.92f, .95f, 1f, .85f);
            main.maxParticles = 600; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;
            var emission = particles.emission;
            emission.rateOverTime = rate;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box; shape.scale = area;
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-.5f, -.1f);
            velocity.y = new ParticleSystem.MinMaxCurve(-1.9f, -1.3f);
            velocity.z = new ParticleSystem.MinMaxCurve(-.35f, .05f);
            var noise = particles.noise;
            noise.enabled = true; noise.strength = .25f; noise.frequency = .3f; noise.scrollSpeed = .2f;
            var collision = particles.collision;
            collision.enabled = true; collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            collision.collidesWith = LayerMask.GetMask("Default");
            collision.quality = ParticleSystemCollisionQuality.Low;
            collision.lifetimeLoss = 1f; collision.bounce = 0f;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = art.WeatherParticle;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.maxParticleSize = .08f;
        }
    }
}

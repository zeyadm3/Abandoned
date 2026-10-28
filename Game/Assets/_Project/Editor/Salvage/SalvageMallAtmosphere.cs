using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Actual mall-only work lamps, dusty air, exit guides and partial skylight demolition.</summary>
    public static class SalvageMallAtmosphere
    {
        public const string ConfigPath = "Assets/_Project/Data/Structure/SalvagePresentationConfig.asset";
        public const string GroupName = "_LIGHTING_VOLUMETRICS";
        private const string ArtFolder = "Assets/_Project/Art/Generated/Salvage/Atmosphere";

        public static void ConfigurePreset()
        {
            SalvagePresentationConfig salvage = LoadConfig();
            var data = new SerializedObject(MeridianPresentationConfigBuilder.Load());
            data.FindProperty("deadPercent").intValue = salvage.DeadFixturePercent;
            data.FindProperty("faultyPercent").intValue = salvage.FaultyFixturePercent;
            data.FindProperty("fixtureIntensity").floatValue = salvage.RetailFixtureIntensity;
            data.FindProperty("atriumFixtureIntensity").floatValue = salvage.AtriumFixtureIntensity;
            data.FindProperty("interiorFogDensity").floatValue = salvage.InteriorFogDensity;
            // Normal drifting air keeps the existing population cap; active demolition has its own cap.
            data.FindProperty("dustPerZoneLimit").intValue = 56;
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data.targetObject);
        }

        public static void Build(Transform mall)
        {
            SalvagePresentationConfig config = LoadConfig();
            Transform old = mall.Find(GroupName);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform root = Group(GroupName, mall);
            root.gameObject.AddComponent<WalkThroughObject>()
                .EditorSetup("Worksite light beams, exit guide housings and local dust are nonphysical visual effects.");
            WorkLights(mall, config);
            ExitGuides(root, config);
            DustPockets(root, config);
            SkylightDamage(Group("PartialSkylightDemolition", root));
            Generators(mall, config);
        }

        public static SalvagePresentationConfig LoadConfig()
        {
            PolishAssets.EnsureFolder("Assets/_Project/Data/Structure");
            var config = SerializedWiring.LoadOrCreateAsset<SalvagePresentationConfig>(ConfigPath);
            EditorUtility.SetDirty(config);
            return config;
        }

        private static bool Named(Transform target, string id, string alias)
        {
            if (!target.name.Contains(id) && !target.name.Contains(alias)) return false;
            // Dressing contains a nested model prefab with the same asset name: power the outer stand once.
            for (Transform ancestor = target.parent; ancestor != null; ancestor = ancestor.parent)
                if (ancestor.name.Contains(id) || ancestor.name.Contains(alias)) return false;
            return true;
        }

        private static void WorkLights(Transform mall, SalvagePresentationConfig config)
        {
            Material shaft = AssetDatabase.LoadAssetAtPath<Material>(LightingAssets.Folder + "/LightShaft.mat");
            if (shaft == null) shaft = LightingAssets.Shaft();
            Mesh beam = BeamMesh(config.BeamAlpha, config.WorkLightColor);
            int count = 0;
            foreach (Transform stand in mall.GetComponentsInChildren<Transform>(true))
            {
                if (stand == null) continue;
                if (!Named(stand, "work_floodlight", "WorkFloodlight")) continue;
                Transform old = stand.Find("_ClearanceWorkLamps");
                if (old != null) Object.DestroyImmediate(old.gameObject);
                if (count >= config.MaximumWorkLights) continue;
                Transform lamps = Group("_ClearanceWorkLamps", stand);
                Vector3 at = mall.InverseTransformPoint(stand.position);
                Vector3 target = at.x < 12f ? new Vector3(7.4f, .42f, 13.6f)
                    : at.x > 44f ? new Vector3(49f, .42f, 29f)
                    : new Vector3(at.x < 28f ? 24.3f : 32.1f, .42f, 25.7f);
                for (int side = -1; side <= 1 && count < config.MaximumWorkLights; side += 2)
                {
                    Transform head = Group("TaskLamp_" + side, lamps);
                    head.localPosition = new Vector3(side * .30f, 2.22f, .10f);
                    Vector3 aim = mall.TransformPoint(target + Vector3.right * side * .70f) - head.position;
                    head.rotation = Quaternion.LookRotation(aim.normalized, Vector3.up);
                    Light light = head.gameObject.AddComponent<Light>();
                    light.type = LightType.Spot;
                    light.color = config.WorkLightColor;
                    light.intensity = config.WorkLightIntensity;
                    light.range = config.WorkLightRange;
                    light.spotAngle = config.WorkLightAngle;
                    light.innerSpotAngle = config.WorkLightAngle * .61f;
                    light.shadows = LightShadows.None;
                    light.shadowBias = .025f;
                    light.shadowNormalBias = .18f;
                    light.shadowNearPlane = .20f;
                    head.gameObject.AddComponent<SalvageWorkLight>().EditorSetup(light,
                        at.x > 44 ? "Titan independent clearance feed" : at.x < 12 ? "Apex independent clearance feed" : "Concourse generator feed", stand);
                    Transform haze = Group("DustLitBeam", head);
                    float length = Mathf.Min(4.8f, aim.magnitude);
                    float radius = Mathf.Tan(config.WorkLightAngle * Mathf.Deg2Rad * .5f) * length * 2f;
                    haze.localScale = new Vector3(radius, radius, length);
                    haze.gameObject.AddComponent<MeshFilter>().sharedMesh = beam;
                    var renderer = haze.gameObject.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = shaft;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    count++;
                }
            }
        }

        private static void ExitGuides(Transform root, SalvagePresentationConfig config)
        {
            Guide("FrontExtraction", new Vector3(28, 2.7f, 1.05f), new Vector3(28, .15f, 4.7f));
            foreach (MallLayout.Flight flight in MallLayout.AllFlights)
            {
                Vector3 forward = new(flight.Dir.x, 0, flight.Dir.y);
                Guide(flight.Name + "_Foot", flight.Foot + Vector3.up * 2.5f,
                    flight.Foot + forward * 2f + Vector3.up * .2f);
                Vector3 head = flight.Foot + forward * flight.Length + Vector3.up * flight.Rise;
                Guide(flight.Name + "_Head", head + Vector3.up * 2.5f,
                    head - forward * 2f + Vector3.up * .2f);
            }

            void Guide(string name, Vector3 at, Vector3 target)
            {
                Transform holder = Group("BatteryExitGuide_" + name, root);
                holder.localPosition = at;
                holder.localRotation = Quaternion.LookRotation(target - at, Vector3.up);
                Box("EmergencyHousing", holder, new Vector3(0, 0, -.025f), new Vector3(.26f, .14f, .10f), MeridianRetailMaterials.Dark, false);
                Box("AmberDiffuser", holder, new Vector3(0, 0, .031f), new Vector3(.21f, .09f, .007f), MeridianRetailMaterials.Brass, false);
                Light lamp = holder.gameObject.AddComponent<Light>();
                lamp.type = LightType.Spot;
                lamp.color = config.EmergencyColor;
                lamp.intensity = config.EmergencyIntensity;
                lamp.range = config.EmergencyRange;
                lamp.spotAngle = 84f;
                lamp.innerSpotAngle = 53f;
                lamp.shadows = LightShadows.None;
                LevelStoreys.LightEverything(lamp);
            }
        }

        private static void DustPockets(Transform root, SalvagePresentationConfig config)
        {
            Material material = MeridianAtmosphereDust.Material();
            int remaining = config.MaximumLocalDustParticles;
            Vector3[] pockets = { new(9f, 1.4f, 12f), new(45f, 1.35f, 8f), new(45f, 5.4f, 8f),
                new(49f, 1.5f, 29f), new(28f, 1.3f, 22.5f) };
            for (int i = 0; i < pockets.Length && remaining > 0; i++)
            {
                int count = Mathf.Min(remaining, config.DustParticlesPerPocket);
                remaining -= count;
                if (count == 0) continue;
                Transform air = Group("DemolitionDust_" + i, root);
                air.localPosition = pockets[i];
                var particles = air.gameObject.AddComponent<ParticleSystem>();
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles.useAutoRandomSeed = false;
                particles.randomSeed = (uint)(1550 + i * 17);
                var main = particles.main;
                main.loop = true;
                main.prewarm = true;
                main.startLifetime = new ParticleSystem.MinMaxCurve(9f, 16f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(0f, .026f);
                main.startSize = new ParticleSystem.MinMaxCurve(config.DustMinimumSize, config.DustMaximumSize);
                main.startColor = new Color(.83f, .78f, .68f, .40f);
                main.maxParticles = count;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                var emission = particles.emission;
                emission.rateOverTime = count / 12.5f;
                var shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(4.2f, 2.3f, 3.3f);
                var noise = particles.noise;
                noise.enabled = true;
                noise.strength = config.DustDrift;
                noise.frequency = .17f;
                noise.scrollSpeed = .03f;
                var velocity = particles.velocityOverLifetime;
                velocity.enabled = true;
                velocity.y = -.013f;
                var renderer = particles.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static void SkylightDamage(Transform root)
        {
            float roof = MallLayout.Floors * MallLayout.StoryHeight;
            foreach (Vector3 at in new[] { new Vector3(22.1f, roof + .53f, 18.0f), new Vector3(33.0f, roof + .72f, 28.8f) })
            {
                Transform patch = Group("BoardedSingleSkylightBay", root);
                patch.localPosition = at;
                patch.localRotation = Quaternion.Euler(0, 0, at.x < 28 ? 9 : -9);
                for (int i = 0; i < 5; i++)
                    Box("SalvagePlywoodBoard", patch, new Vector3((i - 2) * .35f, 0, 0),
                        new Vector3(.345f, .04f, 2.65f), MeridianRetailMaterials.Wood, false);
                Box("TemporaryRoofBrace", patch, new Vector3(0, -.04f, -.8f), new Vector3(1.85f, .05f, .12f), MeridianRetailMaterials.Rust, false);
                Box("TemporaryRoofBrace", patch, new Vector3(0, -.04f, .8f), new Vector3(1.85f, .05f, .12f), MeridianRetailMaterials.Rust, false);
            }
            Transform cracks = Group("CrackedGlazingOverlay", root);
            cracks.localPosition = new Vector3(24.2f, roof + .79f, 24.1f);
            cracks.localRotation = Quaternion.Euler(0, 0, 9);
            for (int i = 0; i < 7; i++)
            {
                GameObject line = Box("RadiatingPaneCrack", cracks, new Vector3((i % 2 == 0 ? .13f : -.15f), -.025f, (i - 3) * .16f),
                    new Vector3(.010f, .006f, .56f + (i % 3) * .22f), MeridianRetailMaterials.Pale, false);
                line.transform.localRotation = Quaternion.Euler(0, -61 + i * 21, 0);
                line.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
            // Existing SealedSkylightPane colliders remain the continuous roof envelope.
        }

        private static void Generators(Transform mall, SalvagePresentationConfig config)
        {
            foreach (Transform generator in mall.GetComponentsInChildren<Transform>(true))
            {
                if (!Named(generator, "generator_hub", "GeneratorHub")) continue;
                var audio = Abandoned.EditorTools.SerializedWiring.GetOrAdd<SalvageGeneratorAudio>(generator.gameObject);
                audio.EditorSetup(config);
            }
        }

        private static Mesh BeamMesh(float alpha, Color color)
        {
            PolishAssets.EnsureFolder(ArtFolder);
            string path = ArtFolder + "/WorkLampDustBeam.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) { mesh = new Mesh { name = "WorkLampDustBeam" }; AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear();
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            const int across = 6, down = 6;
            for (int plane = 0; plane < 2; plane++)
            {
                int first = vertices.Count;
                for (int row = 0; row <= down; row++)
                    for (int column = 0; column <= across; column++)
                    {
                        float t = row / (float)down, u = column / (float)across;
                        float x = (u * 2f - 1f) * Mathf.Lerp(.012f, .50f, t);
                        vertices.Add(plane == 0 ? new Vector3(x, 0, t) : new Vector3(0, x, t));
                        colors.Add(new Color(color.r, color.g, color.b,
                            Mathf.Sin(t * Mathf.PI) * Mathf.Pow(1f - Mathf.Abs(u * 2f - 1f), 2f) * alpha));
                        uv.Add(new Vector2(u, t));
                    }
                for (int row = 0; row < down; row++)
                    for (int column = 0; column < across; column++)
                    {
                        int a = first + row * (across + 1) + column, b = a + across + 1;
                        triangles.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
                    }
            }
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }
    }
}

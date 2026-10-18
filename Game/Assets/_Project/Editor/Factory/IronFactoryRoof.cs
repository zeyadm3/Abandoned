using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// L3: the roof, fallen in in four places (<see cref="IronFactoryLayout.RoofHoles"/>). The roof is laid as strips
    /// around the holes, with torn sheets hanging from each edge. Through every hole falls cold moonlight: a narrow
    /// beam that ignores the building's power, a faint shaft of lit air with dust drifting in it, and the fallen
    /// sheets lying on the floor below. Landmarks you can find your way by, and pools of light in a dark hall.
    /// </summary>
    public static class IronFactoryRoof
    {
        private static readonly Color Moon = new(.55f, .66f, .9f);
        private const float MoonHeight = 10f;

        /// <summary>The roof itself, under the shell (it stays "Shell/Roof" for the NavMesh's ignore list).</summary>
        public static void BuildRoof(Transform shell, IronFactoryArt art)
        {
            Transform roof = Group("Roof", shell);
            float y = L.WallHeight + L.Slab * .5f;
            for (int z = 0; z < L.Depth; z++)
            {
                int start = -1;
                for (int x = 0; x <= L.Width; x++)
                {
                    bool solid = x < L.Width && !InHole(new Vector2((x + .5f) * L.Tile, (z + .5f) * L.Tile));
                    if (solid && start < 0) start = x;
                    if (solid || start < 0) continue;
                    float x0 = start * L.Tile, x1 = x * L.Tile;
                    Box($"Strip_{z}_{start}", roof, new Vector3((x0 + x1) * .5f, y, (z + .5f) * L.Tile), new Vector3(x1 - x0, L.Slab, L.Tile), art.Steel);
                    start = -1;
                }
            }
            var rng = new System.Random(77);
            foreach (Rect hole in L.RoofHoles)
                for (int i = 0; i < 5; i++)
                {
                    // A torn sheet folded down from the rim.
                    int side = rng.Next(4);
                    float along = (float)rng.NextDouble();
                    Vector3 rim = side switch
                    {
                        0 => new Vector3(Mathf.Lerp(hole.xMin, hole.xMax, along), L.WallHeight, hole.yMin),
                        1 => new Vector3(Mathf.Lerp(hole.xMin, hole.xMax, along), L.WallHeight, hole.yMax),
                        2 => new Vector3(hole.xMin, L.WallHeight, Mathf.Lerp(hole.yMin, hole.yMax, along)),
                        _ => new Vector3(hole.xMax, L.WallHeight, Mathf.Lerp(hole.yMin, hole.yMax, along)),
                    };
                    float drop = 1.2f + (float)rng.NextDouble() * 1.6f;
                    GameObject sheet = Box("TornSheet", roof, rim + Vector3.down * drop * .4f, new Vector3(1.4f, drop, .03f), art.Rust, false);
                    sheet.transform.rotation = Quaternion.Euler((float)rng.NextDouble() * 30f - 15f, side < 2 ? 0f : 90f, (float)rng.NextDouble() * 40f - 20f);
                }
        }

        /// <summary>Moonlight, lit air and dust through each hole, and the roof sheets that fell to the floor.</summary>
        public static void BuildMoonlight(Transform root, IronFactoryArt art)
        {
            Transform group = Group("Moonlight", root);
            group.gameObject.AddComponent<PowerIndependentLight>();
            Material shaft = ShaftMaterial();
            Material dust = DustMaterial();
            var rng = new System.Random(78);
            int index = 0;
            foreach (Rect hole in L.RoofHoles)
            {
                Transform at = Group($"Hole_{index++}", group);
                at.position = new Vector3(hole.center.x, 0f, hole.center.y);

                // A narrow beam from high above, its cone about as wide as the hole by the time it reaches the floor.
                Transform beam = Group("Moon", at);
                float height = L.WallHeight + MoonHeight;
                beam.localPosition = Vector3.up * height;
                beam.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Light moon = beam.gameObject.AddComponent<Light>();
                moon.type = LightType.Spot;
                moon.range = height + 6f;
                moon.spotAngle = 2f * Mathf.Atan(hole.width * .55f / height) * Mathf.Rad2Deg;
                moon.innerSpotAngle = moon.spotAngle * .6f;
                moon.intensity = 26f;
                moon.color = Moon;
                moon.shadows = LightShadows.None;

                // Two crossed planes of lit air from the hole to the floor.
                foreach (float yaw in new[] { 0f, 90f })
                {
                    GameObject plane = Primitive(PrimitiveType.Quad, "Shaft", at, Vector3.up * L.WallHeight * .5f,
                        new Vector3(hole.width * .8f, L.WallHeight, 1f), shaft, false);
                    plane.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                    Quiet(plane);
                }
                Dust(at, hole, dust);

                // What fell: torn sheets flat on the floor. Walk-over height, so no colliders.
                for (int i = 0; i < 3; i++)
                {
                    Vector3 p = new((float)rng.NextDouble() * hole.width * .7f - hole.width * .35f, .04f, (float)rng.NextDouble() * hole.height * .7f - hole.height * .35f);
                    GameObject sheet = Box("FallenSheet", at, p, new Vector3(1.6f, .03f, 2.2f), art.Rust, false);
                    sheet.transform.localRotation = Quaternion.Euler((float)rng.NextDouble() * 8f, (float)rng.NextDouble() * 360f, (float)rng.NextDouble() * 8f);
                }
            }
        }

        private static void Dust(Transform at, Rect hole, Material material)
        {
            var go = new GameObject("Dust");
            go.transform.SetParent(at, false);
            go.transform.localPosition = Vector3.up * L.WallHeight * .5f;
            var ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startLifetime = 10f;
            main.startSpeed = .04f;
            main.startSize = new ParticleSystem.MinMaxCurve(.02f, .05f);
            main.startColor = new Color(.78f, .84f, .95f, .35f);
            main.maxParticles = 90;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = .002f;
            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 9f;
            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(hole.width * .7f, L.WallHeight, hole.height * .7f);
            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = .06f;
            noise.frequency = .3f;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        // The shaft glows by itself (it is moonlight in the air, not a lit surface), see-through and unshadowed.
        private static Material ShaftMaterial()
        {
            Material m = IronFactoryMaterialKit.Textured("IronFactory_LightShaft", "LightShaft", Color.white, IronFactoryMaterialKit.Surface.Transparent);
            m.SetTexture("_EmissionMap", m.GetTexture("_BaseMap"));
            m.SetColor("_EmissionColor", Moon * .9f);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(m);
            return m;
        }

        private static Material DustMaterial()
        {
            const string path = "Assets/_Project/Art/Factory/Materials/IronFactory_Dust.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")) { name = "IronFactory_Dust" };
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetColor("_BaseColor", new Color(.8f, .86f, .95f, .4f));
            EditorUtility.SetDirty(m);
            return m;
        }

        private static bool InHole(Vector2 p)
        {
            foreach (Rect hole in L.RoofHoles) if (hole.Contains(p)) return true;
            return false;
        }

        private static void Quiet(GameObject go)
        {
            var r = go.GetComponent<Renderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }
    }
}

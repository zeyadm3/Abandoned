using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Localized lit motes and faint shafts; the HQ floor stays readable between work pools.</summary>
    public static class HqSalvageAir
    {
        public static void Build(Transform root)
        {
            Transform group = Group("SalvageWarehouseAir", root);
            group.gameObject.AddComponent<Abandoned.Core.WalkThroughObject>()
                .EditorSetup("Dust and light shafts are air effects with no physical volume.");
            Material dust = HqSalvageAtmosphereAssets.Dust();
            Material shaft = HqSalvageAtmosphereAssets.Shaft();
            Mesh mesh = ShaftMesh();
            Pool("DeskAir", new Vector3(18f, 2.50f, 5f), 1.55f, 1.6f, 28, 811, true);
            Pool("DepotAir", new Vector3(5.3f, 2.67f, 14.7f), 1.9f, 1.5f, 34, 823, true);
            Pool("GearAir", new Vector3(18f, 2.53f, 13.5f), 1.55f, 1.7f, 28, 837, true);
            Pool("BayAir", new Vector3(6f, 3.33f, 9f), 2.9f, 2.4f, 46, 213, false);
            Pool("OfficeAir", new Vector3(15.1f, 3.33f, 2.6f), 2.3f, 2f, 28, 441, false);

            void Pool(string name, Vector3 top, float height, float spread, int count, uint seed, bool hasShaft)
            {
                Transform air = Group(name, group);
                air.localPosition = top + Vector3.down * height * .5f;
                var particles = air.gameObject.AddComponent<ParticleSystem>();
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles.useAutoRandomSeed = false;
                particles.randomSeed = seed;
                ParticleSystem.MainModule main = particles.main;
                main.loop = true;
                main.prewarm = true;
                main.startLifetime = new ParticleSystem.MinMaxCurve(16f, 24f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(.004f, .017f);
                main.startSize = new ParticleSystem.MinMaxCurve(.009f, .023f);
                main.startColor = new Color(.94f, .90f, .79f, .55f);
                main.maxParticles = count;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                ParticleSystem.EmissionModule emission = particles.emission;
                emission.rateOverTime = count / 20f;
                ParticleSystem.ShapeModule shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(spread, height * .80f, spread);
                ParticleSystem.NoiseModule noise = particles.noise;
                noise.enabled = true;
                noise.strength = .024f;
                noise.frequency = .13f;
                noise.scrollSpeed = .045f;
                var renderer = particles.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = dust;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;

                // Shafts belong only to steady task lamps so a failed tube cannot leave a lit beam behind.
                if (!hasShaft) return;
                Transform beam = Group("LightShaft", group);
                beam.localPosition = top;
                beam.localScale = new Vector3(spread, height, spread);
                beam.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                var beamRenderer = beam.gameObject.AddComponent<MeshRenderer>();
                beamRenderer.sharedMaterial = shaft;
                beamRenderer.shadowCastingMode = ShadowCastingMode.Off;
                beamRenderer.receiveShadows = false;
            }
        }

        private static Mesh ShaftMesh()
        {
            string path = HqSalvageAtmosphereAssets.Folder + "/ShopShaft.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;
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
                    float t = row / (float)down, side = column / (float)across;
                    float width = Mathf.Lerp(.045f, .50f, t);
                    float x = (side * 2f - 1f) * width;
                    vertices.Add(plane == 0 ? new Vector3(x, -t, 0f) : new Vector3(0f, -t, x));
                    float alpha = Mathf.Sin(t * Mathf.PI) * Mathf.Pow(1f - Mathf.Abs(side * 2f - 1f), 2f) * .13f;
                    colors.Add(new Color(1f, 1f, 1f, alpha));
                    uv.Add(new Vector2(side, t));
                }
                for (int row = 0; row < down; row++)
                for (int column = 0; column < across; column++)
                {
                    int a = first + row * (across + 1) + column, b = a + across + 1;
                    triangles.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
                }
            }
            var mesh = new Mesh { name = "ShopShaft" };
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}

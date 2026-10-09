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
            Material daylight = HqSalvageAtmosphereAssets.Shaft(true);
            Mesh mesh = ShaftMesh(false), roofMesh = ShaftMesh(true);
            Pool("DispatchAir", HqLayout.Dispatch + Vector3.up * 2.60f, 1.65f, new Vector2(1.9f, 1.9f), 30, 811, true);
            Pool("DepotAir", HqLayout.Depot + Vector3.up * 2.85f, 1.9f, new Vector2(2.5f, 2.5f), 36, 823, true);
            Pool("MaintenanceAir", HqLayout.Maintenance + Vector3.up * 2.70f, 1.7f, new Vector2(2.3f, 1.8f), 32, 837, true);
            Pool("ArrivalAir", new Vector3(5f, 5.30f, 5f), 4.4f, new Vector2(5f, 5f), 52, 213, false);
            Pool("StockAisleWest", new Vector3(4.5f, 5.30f, 22f), 4.4f, new Vector2(3f, 5.5f), 48, 441, false);
            Pool("StockAisleCentre", new Vector3(11.8f, 5.30f, 23.7f), 4.4f, new Vector2(3f, 3.5f), 40, 452, false);
            Pool("CrewRoomAir", new Vector3(30f, 2.65f, 14.1f), 1.9f, new Vector2(2.2f, 2.2f), 26, 849, false);
            int slot = 0;
            foreach (float z in new[] { 6f, 14f, 22f })
            {
                Pool("ClerestoryAir_" + z, new Vector3(HqLayout.HallWidth * .5f, HqLayout.Ridge - .30f, z),
                    HqLayout.Ridge - 1.05f, new Vector2(6f, 1.4f), 76, (uint)(901 + slot++), true, true);
            }

            void Pool(string name, Vector3 top, float height, Vector2 spread, int count, uint seed, bool hasShaft, bool roof = false)
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
                shape.scale = new Vector3(spread.x, height * .80f, spread.y);
                ParticleSystem.NoiseModule noise = particles.noise;
                noise.enabled = true;
                noise.strength = .024f;
                noise.frequency = .13f;
                noise.scrollSpeed = .045f;
                var renderer = particles.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = dust;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;

                // Shafts follow steady task lamps or real roof glazing; failing tubes only light the motes.
                if (!hasShaft) return;
                Transform beam = Group("LightShaft", group);
                beam.localPosition = top;
                beam.localScale = new Vector3(spread.x, height, spread.y);
                beam.gameObject.AddComponent<MeshFilter>().sharedMesh = roof ? roofMesh : mesh;
                var beamRenderer = beam.gameObject.AddComponent<MeshRenderer>();
                beamRenderer.sharedMaterial = roof ? daylight : shaft;
                beamRenderer.shadowCastingMode = ShadowCastingMode.Off;
                beamRenderer.receiveShadows = false;
            }
        }

        private static Mesh ShaftMesh(bool roof)
        {
            string name = roof ? "ClerestoryShaft" : "ShopShaft";
            string path = HqSalvageAtmosphereAssets.Folder + "/" + name + ".asset";
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
                    float width = roof ? Mathf.Lerp(.50f, .86f, t) : Mathf.Lerp(.045f, .50f, t);
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
            var mesh = new Mesh { name = name };
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

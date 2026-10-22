using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.ShowcaseSitePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The TestMap forecourt, so the asphalt between the gate and the warehouse reads as a place and not a plain:
    /// rain darkening its low spots, cars left under covers, a box trailer without its cab,
    /// a skip, pallet stacks, dead lamp posts, and a guard hut at the gate. The players' spawn row (x 4.5..9, z -6) and
    /// the lanes to both doors stay clear.
    /// </summary>
    public static class ShowcaseForecourt
    {
        public static void Build(Transform root)
        {
            Transform court = Group("Forecourt", root);
            Puddles(Group("Puddles", court));
            Vehicles(Group("Vehicles", court));
            Stores(Group("Stores", court));
            GuardHut(Group("GuardHut", court));
            MarkStatic(court);
        }

        // Irregular flat shapes 4 mm over the asphalt: dark, glassy water that mirrors the sky and the buildings' probes.
        private static void Puddles(Transform puddles)
        {
            // Damp, not mirrors: without real reflections of the yard a glossy puddle only shows the bright horizon and
            // reads as snow, so the rain is a dark stain on the asphalt.
            Material water = ShowcaseMaterials.Plain("TestMap_Puddle_Water", new Color(.01f, .01f, .009f, .55f), .5f);
            water.SetFloat("_EnvironmentReflections", 0f);
            water.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            water.SetFloat("_SpecularHighlights", 0f);
            water.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            water.SetFloat("_Surface", 1f);
            water.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            water.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            water.SetFloat("_ZWrite", 0f);
            water.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            water.SetOverrideTag("RenderType", "Transparent");
            water.renderQueue = 2990;
            var random = new System.Random(5150);
            var spots = new List<Vector2>
            {
                new(-3f, -22f), new(14f, -27f), new(24f, -12f), new(2f, -38f), new(20f, -41f), new(-5f, -45f), new(26f, -31f),
                new(9f, -47f), new(16f, -19f), new(-6f, -10f), new(12f, -35f), new(40f, -36f), new(49f, -8f), new(36f, 14f),
            };
            for (int i = 0; i < spots.Count; i++)
            {
                Mesh mesh = Blob(random, Mathf.Lerp(1.2f, 3.4f, (float)random.NextDouble()), Mathf.Lerp(.5f, 1.6f, (float)random.NextDouble()));
                AssetDatabase.DeleteAsset($"Assets/_Project/Art/TestMap/Terrain/Puddle_{i}.asset");
                AssetDatabase.CreateAsset(mesh, $"Assets/_Project/Art/TestMap/Terrain/Puddle_{i}.asset");
                var go = new GameObject($"Puddle_{i}");
                go.transform.SetParent(puddles, false);
                go.transform.SetPositionAndRotation(new Vector3(spots[i].x, ShowcaseTerrain.HeightAt(spots[i].x, spots[i].y) + .004f, spots[i].y),
                    Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = water;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        // A lumpy flat disc: radius r along x, squashed by `squash` along z, its edge wandering.
        private static Mesh Blob(System.Random random, float r, float squash)
        {
            const int sides = 28;
            var vertices = new List<Vector3> { Vector3.zero };
            var triangles = new List<int>();
            float seed = (float)random.NextDouble() * 100f;
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides;
                float wobble = .7f + .5f * Mathf.PerlinNoise(seed + Mathf.Cos(a) * 1.3f, seed + Mathf.Sin(a) * 1.3f);
                vertices.Add(new Vector3(Mathf.Cos(a) * r * wobble, 0f, Mathf.Sin(a) * r * wobble / Mathf.Max(.5f, squash)));
                triangles.AddRange(new[] { 0, (i + 1) % sides + 1, i + 1 });
            }
            var mesh = new Mesh { name = "Puddle" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void Vehicles(Transform vehicles)
        {
            foreach ((Vector3 at, float yaw) in new[] { (new Vector3(-4.5f, 0f, -14f), 8f), (new Vector3(-4f, 0f, -27f), -12f), (new Vector3(22.5f, 0f, -36f), 95f) })
                ShowcaseModels.Place("covered_car", vehicles, at + Vector3.up * Ground, yaw, new Vector3(2.4f, 2f, 5.4f));

            // A box trailer left without its cab: a container body on a chassis, two axles of scanned tyres, its legs down.
            Transform trailer = Group("Trailer", vehicles);
            trailer.SetPositionAndRotation(new Vector3(26f, Ground, -20f), Quaternion.Euler(0f, -6f, 0f));
            Material body = ShowcaseMaterials.Surface("TestMap_Corrugated_Cladding_Trailer", new Color(.55f, .55f, .53f));
            Box("Box", trailer, new Vector3(0f, 2.55f, 0f), new Vector3(2.5f, 2.7f, 12f), body);
            Box("Chassis", trailer, new Vector3(0f, 1.05f, 0f), new Vector3(1.1f, .3f, 12f), ShowcaseMaterials.Steel);
            Material rubber = ShowcaseMaterials.Surface("TestMap_Rubber_Tyre", new Color(.06f, .06f, .06f));
            foreach (float z in new[] { -4.2f, -5.5f })
            foreach (float x in new[] { -1.05f, 1.05f })
            {
                GameObject wheel = Primitive(PrimitiveType.Cylinder, "Wheel", trailer, new Vector3(x, .5f, z), new Vector3(1f, .15f, 1f), rubber);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            foreach (float x in new[] { -.8f, .8f })
                Box("Leg", trailer, new Vector3(x, .6f, 3.8f), new Vector3(.12f, 1.2f, .12f), ShowcaseMaterials.Steel);
        }

        private static void Stores(Transform stores)
        {
            // A skip, rusted, half full of rubble.
            Transform skip = Group("Skip", stores);
            skip.SetPositionAndRotation(new Vector3(23.5f, Ground, 4f), Quaternion.Euler(0f, 90f, 0f));
            Material rust = ShowcaseMaterials.Surface("TestMap_Rust_Oxide_Skip", new Color(.4f, .22f, .12f));
            Box("Floor", skip, new Vector3(0f, .12f, 0f), new Vector3(1.7f, .2f, 3.2f), rust);
            foreach (float x in new[] { -.92f, .92f }) Box("Side", skip, new Vector3(x, .75f, 0f), new Vector3(.08f, 1.3f, 3.6f), rust);
            foreach (float z in new[] { -1.75f, 1.75f }) Box("End", skip, new Vector3(0f, .75f, z), new Vector3(1.9f, 1.3f, .08f), rust);
            for (int i = 0; i < 7; i++)
            {
                GameObject chunk = Box("Rubble", skip, new Vector3(-.5f + (i % 3) * .5f, .6f + (i % 2) * .2f, -1.2f + i * .4f), new Vector3(.5f, .3f, .4f),
                    ShowcaseMaterials.Rescan(ShowcaseMaterials.Surface("TestMap_Ruin_Brick", new Color(.45f, .27f, .2f)), "factory_brick", 1.5f), false);
                chunk.transform.localRotation = Quaternion.Euler(i * 23f, i * 47f, i * 11f);
            }

            // Pallet stacks by the warehouse's east wall and in the lot.
            var random = new System.Random(808);
            foreach (Vector3 at in new[] { new Vector3(23f, 0f, 9.5f), new Vector3(23.2f, 0f, 12f), new Vector3(31f, 0f, -44f), new Vector3(-10f, 0f, -48f) })
            {
                int count = 3 + random.Next(6);
                for (int k = 0; k < count; k++)
                {
                    GameObject pallet = Box("Pallet", stores, at + new Vector3(0f, Ground + .07f + k * .145f, 0f), new Vector3(1.2f, .14f, 1f), ShowcaseMaterials.Pallet);
                    pallet.transform.rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 8f - 4f, 0f);
                }
            }

            // Lamp posts along the forecourt's west edge and by the gate, dark since the power was cut.
            foreach (Vector3 at in new[] { new Vector3(-8.5f, 0f, -20f), new Vector3(-8.5f, 0f, -36f), new Vector3(2f, 0f, -51f), new Vector3(30.5f, 0f, -10f) })
                ShowcaseNature.Part("street_lamp_02", null, stores, at + Vector3.up * Ground, at.x > 0f ? 270f : 90f, 1f, solid: true);
        }

        // A breeze-block guard hut beside the gate: door gone, window broken out, flat roof with a drip edge.
        private static void GuardHut(Transform hut)
        {
            var c = new Vector3(-1.5f, Ground, -50.5f);
            Material block = ShowcaseMaterials.BlockWall;
            Box("Base", hut, c + new Vector3(0f, .05f, 0f), new Vector3(3f, .1f, 3f), ShowcaseMaterials.CastConcrete);
            Box("Wall_W", hut, c + new Vector3(-1.4f, 1.35f, 0f), new Vector3(.2f, 2.5f, 3f), block);
            Box("Wall_N", hut, c + new Vector3(0f, 1.35f, 1.4f), new Vector3(2.6f, 2.5f, .2f), block);
            Box("Wall_S_Low", hut, c + new Vector3(0f, .55f, -1.4f), new Vector3(2.6f, .9f, .2f), block);
            Box("Wall_S_Head", hut, c + new Vector3(0f, 2.35f, -1.4f), new Vector3(2.6f, .5f, .2f), block);
            Box("Wall_E_Low", hut, c + new Vector3(1.4f, 1.35f, .8f), new Vector3(.2f, 2.5f, 1.4f), block);
            Box("Wall_E_Head", hut, c + new Vector3(1.4f, 2.4f, -.7f), new Vector3(.2f, .4f, 1.6f), block);
            Box("Roof", hut, c + new Vector3(0f, 2.7f, 0f), new Vector3(3.4f, .2f, 3.4f), ShowcaseMaterials.Roof);
        }
    }
}

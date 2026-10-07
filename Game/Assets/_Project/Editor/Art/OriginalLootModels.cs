using System.Collections.Generic;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Original low-poly art for collectibles without an imported model, preserving their authored collider sizes.</summary>
    public static class OriginalLootModels
    {
        private const string Folder = "Assets/_Project/Art/Polish/Loot";
        public static bool AssignedAny { get; private set; }
        private static readonly string[] Ids = {
            "antique_vase", "cash_bundle", "collector_figure", "designer_sunglasses", "desktop_pc",
            "diamond_ring", "film_projector", "game_console", "glass_sculpture", "gold_watch", "grand_piano",
            "grandfather_clock", "large_painting", "marble_statue", "military_generator", "pearl_necklace",
            "safe", "server_rack", "silver_lighter", "small_painting", "smartphone", "vending_machine"
        };

        public static void AssignMissing()
        {
            AssignedAny = false;
            PolishAssets.EnsureFolder(Folder);
            foreach (string id in Ids)
            {
                LootDefinition definition = LootCatalogBuilder.Load(id);
                if (definition == null || definition.Model != null) continue;
                var root = new GameObject(id);
                try
                {
                    Build(root.transform, definition);
                    Combine(root, id);
                    GameObject model = PrefabUtility.SaveAsPrefabAsset(root, $"{Folder}/{id}.prefab");
                    // Art changes must not alter door clearance, floor load or crew handles.
                    definition.EditorSetModel(model, 0f, definition.Size);
                    EditorUtility.SetDirty(definition);
                    AssignedAny = true;
                }
                finally { Object.DestroyImmediate(root); }
            }
        }

        private static void Build(Transform root, LootDefinition d)
        {
            Vector3 size = d.Size;
            Vector3 Scale(Vector3 fraction) => Vector3.Scale(size, fraction);
            Material dark = PolishAssets.Material("Loot_Charcoal", new Color(0.065f, 0.075f, 0.08f));
            Material metal = PolishAssets.Material("Loot_Steel", new Color(0.42f, 0.47f, 0.49f), 0.45f);
            Material gold = PolishAssets.Material("Loot_Brass", new Color(0.64f, 0.47f, 0.17f), 0.4f);
            Material ivory = PolishAssets.Material("Loot_Ivory", new Color(0.83f, 0.81f, 0.7f));
            Material wood = PolishAssets.Material("Loot_Walnut", new Color(0.24f, 0.12f, 0.07f));
            Material accent = PolishAssets.Material("Loot_" + d.Id, d.Color);
            GameObject Box(string name, Vector3 at, Vector3 dimensions, Material m) =>
                GreyboxFactory.Box(name, root, Scale(at), Scale(dimensions), m, false);
            GameObject Round(string name, Vector3 at, Vector3 dimensions, Material m) =>
                PolishAssets.Shape(name, root, Scale(at), Scale(dimensions), m, true);
            void Stripes(int n, float y, float z, Material m)
            {
                for (int i = 0; i < n; i++) Box("Vent", new Vector3(-0.35f + i * 0.7f / (n - 1), y, z), new Vector3(0.028f, 0.32f, 0.012f), m);
            }
            switch (d.Id)
            {
                case "grand_piano":
                    Box("Cabinet", new(0f, 0.21f, -0.1f), new(0.94f, 0.23f, 0.72f), dark);
                    Box("Wing", new(-0.18f, 0.22f, 0.28f), new(0.58f, 0.2f, 0.3f), dark);
                    Box("ClosedLid", new(0f, 0.35f, -0.08f), new(0.98f, 0.045f, 0.78f), dark);
                    Box("KeyboardBed", new(0f, 0.18f, -0.43f), new(0.95f, 0.12f, 0.12f), wood);
                    for (int i = 0; i < 18; i++)
                    {
                        float x = -0.43f + i * 0.05f;
                        Box("IvoryKey", new(x, 0.26f, -0.445f), new(0.047f, 0.025f, 0.095f), ivory);
                        if (i % 7 != 2 && i % 7 != 6) Box("BlackKey", new(x + 0.02f, 0.285f, -0.415f), new(0.026f, 0.022f, 0.05f), dark);
                    }
                    foreach (Vector3 at in new[] { new Vector3(-0.38f, -0.15f, -0.32f), new Vector3(0.38f, -0.15f, -0.32f), new Vector3(-0.12f, -0.15f, 0.32f) })
                    {
                        Box("Leg", at, new(0.065f, 0.66f, 0.055f), dark);
                        Round("Caster", at + Vector3.down * 0.31f, new(0.09f, 0.06f, 0.06f), gold);
                    }
                    break;
                case "marble_statue":
                    Box("Plinth", new(0f, -0.43f, 0f), new(0.9f, 0.14f, 0.9f), accent);
                    PolishAssets.Shape("DrapedFigure", root, Scale(new(0f, -0.04f, 0f)), Scale(new(0.48f, 0.68f, 0.4f)), accent);
                    Round("Shoulders", new(0f, 0.2f, 0f), new(0.59f, 0.16f, 0.4f), accent);
                    Round("Head", new(0f, 0.38f, 0f), new(0.3f, 0.22f, 0.32f), accent);
                    Box("Fold", new(-0.08f, -0.02f, 0.18f), new(0.055f, 0.5f, 0.05f), ivory);
                    foreach (float side in new[] { -1f, 1f })
                        Round("Arm", new(side * 0.31f, 0.03f, 0.01f), new(0.13f, 0.38f, 0.16f), accent);
                    Box("Plaque", new(0f, -0.42f, -0.455f), new(0.31f, 0.045f, 0.015f), gold);
                    break;
                case "military_generator":
                    Box("EngineHousing", new(0f, 0.02f, 0f), new(0.85f, 0.69f, 0.83f), accent);
                    Box("Skid", new(0f, -0.4f, 0f), new(1f, 0.12f, 1f), dark);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Box("Frame", new(side * 0.45f, 0.02f, 0f), new(0.055f, 0.85f, 0.92f), metal);
                        Box("LiftBar", new(side * 0.44f, 0.43f, 0f), new(0.065f, 0.07f, 0.94f), metal);
                    }
                    Stripes(10, 0.04f, -0.423f, dark);
                    Box("Exhaust", new(0.24f, 0.36f, 0.24f), new(0.08f, 0.26f, 0.1f), dark);
                    Box("ControlPanel", new(0f, 0.12f, 0.425f), new(0.43f, 0.24f, 0.025f), dark);
                    Round("Gauge", new(-0.11f, 0.15f, 0.445f), new(0.1f, 0.1f, 0.025f), ivory);
                    break;
                case "server_rack":
                case "safe":
                case "vending_machine":
                case "desktop_pc":
                case "game_console":
                    Box("Case", Vector3.zero, new(0.98f, 0.98f, 0.98f), d.Id == "safe" ? metal : accent);
                    Box("InsetFront", new(0f, 0.01f, -0.496f), new(0.82f, 0.84f, 0.012f), dark);
                    if (d.Id == "safe")
                    {
                        Box("Door", new(0f, 0f, -0.505f), new(0.74f, 0.74f, 0.04f), metal);
                        Round("Dial", new(-0.13f, 0.04f, -0.54f), new(0.16f, 0.16f, 0.055f), dark);
                        Box("Handle", new(0.2f, -0.04f, -0.55f), new(0.08f, 0.27f, 0.055f), gold);
                    }
                    else if (d.Id == "server_rack")
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            float y = -0.37f + i * 0.082f;
                            Box("Server", new(0f, y, -0.51f), new(0.72f, 0.067f, 0.025f), metal);
                            Box("StatusLight", new(0.27f, y, -0.529f), new(0.03f, 0.018f, 0.009f), ivory);
                        }
                    }
                    else if (d.Id == "vending_machine")
                    {
                        Box("PaymentPanel", new(0.32f, 0.08f, -0.52f), new(0.18f, 0.58f, 0.028f), metal);
                        Box("CoinSlot", new(0.32f, 0.16f, -0.54f), new(0.065f, 0.014f, 0.01f), dark);
                        Box("Delivery", new(-0.05f, -0.33f, -0.52f), new(0.59f, 0.11f, 0.024f), dark);
                        for (int row = 0; row < 3; row++)
                        for (int col = 0; col < 4; col++)
                            Round("Bottle", new(-0.32f + col * 0.13f, 0.29f - row * 0.16f, -0.52f), new(0.07f, 0.11f, 0.026f), row == 1 ? gold : ivory);
                    }
                    else
                    {
                        for (int i = 0; i < 5; i++) Box("Vent", new(-0.18f + i * 0.07f, -0.15f, -0.51f), new(0.02f, 0.24f, 0.014f), metal);
                        Box("DiscSlot", new(0f, 0.29f, -0.513f), new(0.54f, 0.04f, 0.014f), dark);
                        Round("Power", new(0.29f, 0.19f, -0.515f), new(0.055f, 0.055f, 0.015f), ivory);
                    }
                    break;
                case "grandfather_clock":
                    Box("Cabinet", new(0f, -0.03f, 0f), new(0.84f, 0.86f, 0.83f), wood);
                    Box("Base", new(0f, -0.46f, 0f), new(1f, 0.08f, 1f), wood);
                    Box("Crown", new(0f, 0.45f, 0f), new(1f, 0.1f, 0.95f), wood);
                    Box("Recess", new(0f, -0.16f, -0.426f), new(0.55f, 0.45f, 0.025f), dark);
                    Round("ClockFace", new(0f, 0.25f, -0.44f), new(0.65f, 0.2f, 0.035f), ivory);
                    Box("MinuteHand", new(0f, 0.275f, -0.465f), new(0.025f, 0.067f, 0.009f), dark);
                    Box("HourHand", new(0.06f, 0.25f, -0.465f), new(0.15f, 0.013f, 0.009f), dark);
                    Box("Pendulum", new(0f, -0.15f, -0.45f), new(0.024f, 0.34f, 0.016f), gold);
                    Round("PendulumBob", new(0f, -0.28f, -0.46f), new(0.3f, 0.09f, 0.04f), gold);
                    break;
                case "film_projector":
                    Box("Projector", new(0f, -0.13f, 0f), new(0.8f, 0.57f, 0.63f), dark);
                    Box("Foot", new(0f, -0.45f, 0f), new(1f, 0.1f, 0.8f), metal);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Round("Reel", new(0f, 0.25f, side * 0.23f), new(0.96f, 0.43f, 0.1f), metal);
                        Round("ReelHub", new(0f, 0.25f, side * 0.235f), new(0.27f, 0.12f, 0.11f), dark);
                    }
                    Round("Lens", new(0f, -0.05f, -0.42f), new(0.48f, 0.32f, 0.16f), metal);
                    Round("Glass", new(0f, -0.05f, -0.49f), new(0.34f, 0.24f, 0.02f), accent);
                    break;
                case "large_painting":
                case "small_painting":
                    Box("Frame", Vector3.zero, Vector3.one, gold);
                    Box("Canvas", new(0f, 0f, -0.51f), new(0.87f, 0.84f, 0.035f), ivory);
                    Box("DistantHills", new(0f, -0.03f, -0.54f), new(0.82f, 0.19f, 0.02f), accent);
                    Box("Landscape", new(0f, -0.26f, -0.56f), new(0.82f, 0.27f, 0.02f), wood);
                    Round("Sun", new(0.2f, 0.2f, -0.56f), new(0.16f, 0.2f, 0.03f), gold);
                    break;
                case "antique_vase":
                    var vase = new GameObject("Vase"); vase.transform.SetParent(root, false);
                    vase.transform.localScale = size;
                    vase.AddComponent<MeshFilter>().sharedMesh = VaseMesh(); vase.AddComponent<MeshRenderer>().sharedMaterial = accent;
                    Round("Foot", new(0f, -0.45f, 0f), new(0.65f, 0.08f, 0.65f), gold);
                    break;
                case "glass_sculpture":
                    Box("Plinth", new(0f, -0.46f, 0f), new(0.9f, 0.08f, 0.9f), dark);
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject facet = PolishAssets.Shape("GlassFacet", root, Scale(new(0f, -0.28f + i * 0.21f, 0f)), Scale(new(0.72f - i * 0.09f, 0.23f, 0.65f - i * 0.07f)), accent);
                        facet.transform.localRotation = Quaternion.Euler(0f, i * 35f, i % 2 == 0 ? 12f : -12f);
                    }
                    break;
                case "diamond_ring":
                case "pearl_necklace":
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * Mathf.PI / 6f;
                        Vector3 at = new(Mathf.Cos(a) * 0.38f, d.Id == "diamond_ring" ? -0.16f : 0f, Mathf.Sin(a) * 0.38f);
                        Round("Bead", at, new(0.2f, d.Id == "diamond_ring" ? 0.18f : 0.72f, 0.2f), d.Id == "diamond_ring" ? gold : ivory);
                    }
                    if (d.Id == "diamond_ring") Round("Diamond", new(0f, 0.21f, -0.35f), new(0.4f, 0.5f, 0.4f), ivory);
                    break;
                case "gold_watch":
                    Box("Bracelet", Vector3.zero, new(0.45f, 0.65f, 1f), gold);
                    Round("WatchCase", new(0f, 0.12f, 0f), new(0.9f, 0.75f, 0.72f), gold);
                    Round("Dial", new(0f, 0.44f, 0f), new(0.72f, 0.07f, 0.55f), ivory);
                    Box("Hand", new(0.1f, 0.485f, 0f), new(0.22f, 0.015f, 0.04f), dark);
                    break;
                case "designer_sunglasses":
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Round("Frame", new(side * 0.25f, 0f, -0.25f), new(0.47f, 0.94f, 0.2f), gold);
                        Round("Lens", new(side * 0.25f, 0f, -0.33f), new(0.39f, 0.77f, 0.09f), dark);
                        Box("Temple", new(side * 0.46f, 0.11f, 0.1f), new(0.035f, 0.13f, 0.76f), dark);
                    }
                    Box("Bridge", new(0f, 0f, -0.25f), new(0.2f, 0.12f, 0.06f), gold);
                    break;
                case "cash_bundle":
                    Box("Notes", Vector3.zero, new(1f, 0.96f, 1f), ivory);
                    Box("Band", new(0f, 0.02f, 0f), new(0.28f, 1f, 1.02f), accent);
                    Box("PrintedSeal", new(-0.29f, 0.49f, 0f), new(0.12f, 0.015f, 0.43f), gold);
                    break;
                case "smartphone":
                    Box("Phone", Vector3.zero, new(1f, 0.96f, 1f), dark);
                    Box("Screen", new(0f, 0.49f, 0.02f), new(0.88f, 0.025f, 0.83f), accent);
                    Box("Earpiece", new(0f, 0.5f, 0.44f), new(0.22f, 0.015f, 0.025f), metal);
                    break;
                case "silver_lighter":
                    Box("Body", new(0f, -0.12f, 0f), new(0.98f, 0.72f, 0.94f), metal);
                    Box("Cap", new(0f, 0.35f, 0f), new(1f, 0.27f, 1f), metal);
                    Box("Seam", new(0f, 0.2f, -0.48f), new(0.94f, 0.023f, 0.015f), dark);
                    break;
                case "collector_figure":
                    Round("Body", new(0f, -0.15f, 0f), new(0.75f, 0.6f, 0.75f), accent);
                    Round("Head", new(0f, 0.29f, 0f), new(0.85f, 0.32f, 0.85f), accent);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Round("Ear", new(side * 0.31f, 0.44f, 0f), new(0.27f, 0.12f, 0.27f), accent);
                        Round("Eye", new(side * 0.18f, 0.31f, -0.39f), new(0.13f, 0.06f, 0.08f), dark);
                        Round("Foot", new(side * 0.22f, -0.43f, -0.09f), new(0.35f, 0.13f, 0.54f), dark);
                    }
                    break;
            }
        }

        private static Mesh VaseMesh()
        {
            float[] radii = { 0.24f, 0.35f, 0.49f, 0.46f, 0.23f, 0.17f, 0.21f };
            var vertices = new List<Vector3>(); var indices = new List<int>();
            Vector3 At(int ring, int side)
            {
                float a = side * Mathf.PI / 6f;
                return new Vector3(Mathf.Cos(a) * radii[ring], ring / 6f - 0.5f, Mathf.Sin(a) * radii[ring]);
            }
            void Triangle(Vector3 a, Vector3 b, Vector3 c)
            {
                int i = vertices.Count; vertices.Add(a); vertices.Add(b); vertices.Add(c);
                indices.Add(i); indices.Add(i + 1); indices.Add(i + 2);
            }
            for (int ring = 0; ring < 6; ring++)
            for (int side = 0; side < 12; side++)
            {
                Triangle(At(ring, side), At(ring + 1, side), At(ring + 1, side + 1));
                Triangle(At(ring, side), At(ring + 1, side + 1), At(ring, side + 1));
            }
            for (int side = 0; side < 12; side++) Triangle(new(0f, -0.5f, 0f), At(0, side), At(0, side + 1));
            var mesh = new Mesh { name = "Vase" };
            mesh.SetVertices(vertices); mesh.SetTriangles(indices, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        // Art stays cheap even when dozens of valuables are in view: one mesh per material per model.
        private static void Combine(GameObject root, string id)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            MeshFilter[] parts = root.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter part in parts)
            {
                Material m = part.GetComponent<Renderer>().sharedMaterial;
                if (!groups.TryGetValue(m, out List<CombineInstance> list)) groups[m] = list = new();
                list.Add(new CombineInstance { mesh = part.sharedMesh, transform = root.transform.worldToLocalMatrix * part.transform.localToWorldMatrix });
            }
            foreach (MeshFilter part in parts) Object.DestroyImmediate(part.gameObject);
            foreach (var pair in groups)
            {
                string path = $"{Folder}/{id}_{pair.Key.name}.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null) { mesh = new Mesh { name = id + "_" + pair.Key.name }; AssetDatabase.CreateAsset(mesh, path); }
                else mesh.Clear();
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
                var go = new GameObject(pair.Key.name); go.transform.SetParent(root.transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterial = pair.Key;
            }
        }
    }
}

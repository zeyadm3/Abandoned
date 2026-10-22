using System.Collections.Generic;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Generates modular low-poly models for each of the 25 Skyscraper Restaurant salvage definitions.
    /// Preserves exact physics bounds and colliders while rendering themed culinary and luxury pieces.
    /// </summary>
    public static class SkyscraperLootModels
    {
        public const string Folder = "Assets/_Project/Art/Skyscraper/Loot";

        public static int AssignMissing(string dataFolder)
        {
            PolishAssets.EnsureFolder(Folder);
            int count = 0;

            foreach (SkyscraperLootItems.ItemSpec item in SkyscraperLootItems.Items)
            {
                string defPath = $"{dataFolder}/{item.Id}.asset";
                var def = AssetDatabase.LoadAssetAtPath<LootDefinition>(defPath);
                if (def == null || def.Model != null) continue;

                var root = new GameObject(item.Id);
                try
                {
                    BuildModel(root.transform, def);
                    CombineByMaterial(root, item.Id);
                    string prefabPath = $"{Folder}/{item.Id}.prefab";
                    GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    def.EditorSetModel(prefab, 0f, def.Size);
                    EditorUtility.SetDirty(def);
                    count++;
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }
            }
            return count;
        }

        private static void BuildModel(Transform root, LootDefinition d)
        {
            Vector3 s = d.Size;
            Material silver = PolishAssets.Material("SkyscraperLoot_Silver", new Color(0.82f, 0.84f, 0.88f), 0.25f);
            Material gold = PolishAssets.Material("SkyscraperLoot_Gold", new Color(0.88f, 0.72f, 0.24f), 0.20f);
            Material copper = PolishAssets.Material("SkyscraperLoot_Copper", new Color(0.82f, 0.48f, 0.28f), 0.35f);
            Material brass = PolishAssets.Material("SkyscraperLoot_Brass", new Color(0.75f, 0.62f, 0.28f), 0.30f);
            Material steel = PolishAssets.Material("SkyscraperLoot_Steel", new Color(0.50f, 0.52f, 0.54f), 0.40f);
            Material darkWood = PolishAssets.Material("SkyscraperLoot_DarkWood", new Color(0.24f, 0.15f, 0.10f), 0.65f);
            Material mahogany = PolishAssets.Material("SkyscraperLoot_Mahogany", new Color(0.38f, 0.18f, 0.12f), 0.45f);
            Material glass = PolishAssets.Material("SkyscraperLoot_Glass", new Color(0.75f, 0.88f, 0.92f), 0.10f);
            Material darkGlass = PolishAssets.Material("SkyscraperLoot_DarkGlass", new Color(0.12f, 0.18f, 0.14f), 0.15f);
            Material porcelain = PolishAssets.Material("SkyscraperLoot_Porcelain", new Color(0.94f, 0.94f, 0.92f), 0.20f);
            Material leather = PolishAssets.Material("SkyscraperLoot_Leather", new Color(0.26f, 0.16f, 0.12f), 0.80f);
            Material velvet = PolishAssets.Material("SkyscraperLoot_Velvet", new Color(0.42f, 0.08f, 0.12f), 0.90f);

            void P(string name, Vector3 at, Vector3 dims, Material m, bool round = false) =>
                PolishAssets.Shape(name, root, Vector3.Scale(s, at), Vector3.Scale(s, dims), m, round);

            switch (d.Id)
            {
                case "silver_cutlery_canteen":
                    P("Box", new(0f, -0.1f, 0f), new(1f, 0.7f, 1f), mahogany);
                    P("Lining", new(0f, 0.26f, 0f), new(0.92f, 0.1f, 0.92f), velvet);
                    P("Handles", new(0.51f, 0f, 0f), new(0.04f, 0.2f, 0.3f), brass);
                    P("HandlesL", new(-0.51f, 0f, 0f), new(0.04f, 0.2f, 0.3f), brass);
                    P("Clasp", new(0f, 0.15f, 0.51f), new(0.15f, 0.15f, 0.03f), brass);
                    break;

                case "caviar_tin_reserve":
                    P("Tin", Vector3.zero, new(1f, 0.85f, 1f), gold, true);
                    P("Rim", new(0f, 0.45f, 0f), new(1.04f, 0.15f, 1.04f), gold, true);
                    P("Label", new(0f, 0.05f, 0f), new(1.01f, 0.5f, 1.01f), velvet, true);
                    break;

                case "crystal_wine_decanter":
                    P("Base", new(0f, -0.2f, 0f), new(1f, 0.6f, 1f), glass, true);
                    P("Neck", new(0f, 0.25f, 0f), new(0.35f, 0.45f, 0.35f), glass, true);
                    P("Stopper", new(0f, 0.48f, 0f), new(0.4f, 0.15f, 0.4f), glass, true);
                    break;

                case "silver_serving_cloche":
                    P("Platter", new(0f, -0.42f, 0f), new(1f, 0.12f, 1f), silver, true);
                    P("Dome", new(0f, 0f, 0f), new(0.85f, 0.75f, 0.85f), silver, true);
                    P("Handle", new(0f, 0.45f, 0f), new(0.18f, 0.15f, 0.18f), silver, true);
                    break;

                case "gilded_china_stack":
                    for (int i = 0; i < 5; i++)
                    {
                        P($"Plate_{i}", new(0f, -0.4f + i * 0.18f, 0f), new(1f, 0.12f, 1f), porcelain, true);
                        P($"GoldRim_{i}", new(0f, -0.4f + i * 0.18f, 0f), new(1.02f, 0.04f, 1.02f), gold, true);
                    }
                    break;

                case "silver_candelabra":
                    P("Foot", new(0f, -0.45f, 0f), new(0.6f, 0.1f, 0.6f), silver, true);
                    P("Stem", new(0f, -0.05f, 0f), new(0.12f, 0.7f, 0.12f), silver, true);
                    P("ArmsX", new(0f, 0.2f, 0f), new(0.9f, 0.06f, 0.12f), silver);
                    P("ArmsZ", new(0f, 0.2f, 0f), new(0.12f, 0.06f, 0.9f), silver);
                    P("CenterCup", new(0f, 0.42f, 0f), new(0.18f, 0.18f, 0.18f), silver, true);
                    break;

                case "copper_saute_pan":
                    P("Pan", new(0f, 0f, -0.2f), new(0.8f, 0.85f, 0.6f), copper, true);
                    P("Handle", new(0f, 0.2f, 0.35f), new(0.12f, 0.1f, 0.55f), brass);
                    break;

                case "damascus_knife_roll":
                    P("Roll", Vector3.zero, new(0.95f, 0.9f, 0.95f), leather, true);
                    P("Strap1", new(0f, 0f, -0.25f), new(1.02f, 1.02f, 0.12f), darkWood, true);
                    P("Strap2", new(0f, 0f, 0.25f), new(1.02f, 1.02f, 0.12f), darkWood, true);
                    P("Buckle", new(0.48f, 0f, -0.25f), new(0.08f, 0.12f, 0.14f), brass);
                    break;

                case "black_truffle_crate":
                    P("CrateBottom", new(0f, -0.4f, 0f), new(0.95f, 0.1f, 0.95f), darkWood);
                    P("SideL", new(-0.45f, 0f, 0f), new(0.08f, 0.8f, 0.95f), darkWood);
                    P("SideR", new(0.45f, 0f, 0f), new(0.08f, 0.8f, 0.95f), darkWood);
                    P("TruffleMound", new(0f, 0.1f, 0f), new(0.8f, 0.5f, 0.8f), velvet, true);
                    break;

                case "aged_iberico_ham":
                    P("Stand", new(0f, -0.38f, 0f), new(0.9f, 0.15f, 0.4f), darkWood);
                    P("Clamp", new(-0.35f, 0f, 0f), new(0.15f, 0.5f, 0.3f), steel);
                    P("HamLeg", new(0.05f, 0.12f, 0f), new(0.85f, 0.65f, 0.6f), leather, true);
                    P("Bone", new(-0.35f, 0.22f, 0f), new(0.2f, 0.15f, 0.15f), porcelain, true);
                    break;

                case "commercial_espresso_machine":
                    P("Chassis", new(0f, -0.1f, 0f), new(0.95f, 0.75f, 0.9f), steel);
                    P("SidePanels", new(0f, 0f, 0f), new(1.02f, 0.7f, 0.85f), mahogany);
                    P("GroupHeads", new(0f, -0.1f, 0.48f), new(0.7f, 0.25f, 0.12f), brass);
                    P("DripTray", new(0f, -0.45f, 0.3f), new(0.9f, 0.08f, 0.35f), steel);
                    P("TopRail", new(0f, 0.45f, 0f), new(0.88f, 0.12f, 0.8f), brass);
                    break;

                case "bronze_bull_statue":
                    P("MarbleBase", new(0f, -0.4f, 0f), new(1f, 0.2f, 0.85f), darkWood);
                    P("BullTorso", new(0f, 0.05f, 0f), new(0.8f, 0.55f, 0.55f), brass, true);
                    P("BullHead", new(-0.32f, 0.25f, 0f), new(0.4f, 0.4f, 0.4f), brass, true);
                    P("Horns", new(-0.4f, 0.42f, 0f), new(0.15f, 0.18f, 0.65f), brass);
                    P("LegsF", new(-0.25f, -0.2f, 0f), new(0.25f, 0.35f, 0.5f), brass);
                    P("LegsR", new(0.25f, -0.2f, 0f), new(0.25f, 0.35f, 0.5f), brass);
                    break;

                case "executive_wall_safe":
                    P("Body", Vector3.zero, new(0.95f, 0.95f, 0.95f), steel);
                    P("DoorFrame", new(0f, 0f, 0.48f), new(0.9f, 0.9f, 0.06f), darkWood);
                    P("Dial", new(0f, 0.15f, 0.53f), new(0.25f, 0.25f, 0.08f), brass, true);
                    P("SpokeWheel", new(0f, -0.15f, 0.53f), new(0.35f, 0.35f, 0.08f), silver, true);
                    break;

                case "grand_wine_cask":
                    P("BarrelBody", Vector3.zero, new(0.95f, 1f, 0.95f), darkWood, true);
                    P("HoopTop", new(0f, 0.35f, 0f), new(0.98f, 0.06f, 0.98f), steel, true);
                    P("HoopMidT", new(0f, 0.15f, 0f), new(1.02f, 0.06f, 1.02f), steel, true);
                    P("HoopMidB", new(0f, -0.15f, 0f), new(1.02f, 0.06f, 1.02f), steel, true);
                    P("HoopBot", new(0f, -0.35f, 0f), new(0.98f, 0.06f, 0.98f), steel, true);
                    P("Spigot", new(0f, -0.2f, 0.52f), new(0.12f, 0.15f, 0.18f), brass);
                    break;

                case "vintage_cognac_1945":
                case "grand_cru_bordeaux":
                case "champagne_dom_magnum":
                case "vintage_single_malt_whisky":
                    P("Bottle", new(0f, -0.15f, 0f), new(0.85f, 0.65f, 0.85f), darkGlass, true);
                    P("Shoulder", new(0f, 0.22f, 0f), new(0.7f, 0.2f, 0.7f), darkGlass, true);
                    P("Neck", new(0f, 0.38f, 0f), new(0.28f, 0.3f, 0.28f), darkGlass, true);
                    P("FoilCap", new(0f, 0.46f, 0f), new(0.3f, 0.15f, 0.3f), gold, true);
                    P("Label", new(0f, -0.1f, 0.44f), new(0.6f, 0.4f, 0.02f), velvet);
                    break;

                case "maitre_d_leather_ledger":
                case "executive_leather_briefcase":
                    P("BriefcaseBody", Vector3.zero, new(0.95f, 0.85f, 0.35f), leather);
                    P("BrassLocks", new(0f, 0.45f, 0.18f), new(0.3f, 0.1f, 0.05f), brass);
                    P("Handle", new(0f, 0.52f, 0f), new(0.25f, 0.15f, 0.06f), darkWood);
                    break;

                case "brass_observation_telescope":
                    P("PillarStand", new(0f, -0.3f, 0f), new(0.3f, 0.65f, 0.3f), brass, true);
                    P("TelescopeBarrel", new(0f, 0.25f, 0f), new(0.35f, 0.35f, 0.95f), brass, true);
                    P("Eyepiece", new(0f, 0.25f, -0.48f), new(0.18f, 0.18f, 0.15f), silver, true);
                    P("LensRim", new(0f, 0.25f, 0.48f), new(0.4f, 0.4f, 0.1f), gold, true);
                    break;

                case "brass_meat_slicer":
                    P("SlicerBase", new(0f, -0.35f, 0f), new(0.95f, 0.2f, 0.85f), steel);
                    P("CircularBlade", new(0.15f, 0.1f, 0f), new(0.08f, 0.7f, 0.7f), silver, true);
                    P("BladeGuard", new(0.18f, 0.15f, 0f), new(0.05f, 0.75f, 0.75f), brass, true);
                    P("ProductCarriage", new(-0.2f, 0f, 0f), new(0.5f, 0.4f, 0.5f), steel);
                    break;

                case "brass_draught_tap_tower":
                    P("TowerColumn", new(0f, 0f, 0f), new(0.4f, 0.95f, 0.4f), brass, true);
                    P("TapHandle1", new(-0.18f, 0.35f, 0.22f), new(0.08f, 0.35f, 0.08f), darkWood, true);
                    P("TapHandle2", new( 0.0f,  0.35f, 0.24f), new(0.08f, 0.35f, 0.08f), darkWood, true);
                    P("TapHandle3", new( 0.18f, 0.35f, 0.22f), new(0.08f, 0.35f, 0.08f), darkWood, true);
                    break;

                case "gantry_anemometer_gauge":
                    P("GaugeBox", new(0f, -0.15f, 0f), new(0.85f, 0.6f, 0.75f), steel);
                    P("MeterDial", new(0f, -0.15f, 0.38f), new(0.55f, 0.55f, 0.05f), silver, true);
                    P("Spindle", new(0f, 0.35f, 0f), new(0.1f, 0.4f, 0.1f), silver, true);
                    P("CupsArm", new(0f, 0.45f, 0f), new(0.75f, 0.06f, 0.06f), brass);
                    break;

                default:
                    P("Casing", Vector3.zero, Vector3.one * 0.9f, steel);
                    P("Trim", new(0f, 0.45f, 0f), new(0.95f, 0.1f, 0.95f), gold);
                    break;
            }
        }

        private static void CombineByMaterial(GameObject root, string id)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            MeshFilter[] parts = root.GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter part in parts)
            {
                Material m = part.GetComponent<Renderer>().sharedMaterial;
                if (!groups.TryGetValue(m, out List<CombineInstance> list))
                    groups[m] = list = new List<CombineInstance>();

                list.Add(new CombineInstance
                {
                    mesh = part.sharedMesh,
                    transform = root.transform.worldToLocalMatrix * part.transform.localToWorldMatrix
                });
            }

            foreach (MeshFilter part in parts)
                Object.DestroyImmediate(part.gameObject);

            foreach (KeyValuePair<Material, List<CombineInstance>> pair in groups)
            {
                string path = $"{Folder}/{id}_{pair.Key.name}.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null)
                {
                    mesh = new Mesh { name = $"{id}_{pair.Key.name}" };
                    AssetDatabase.CreateAsset(mesh, path);
                }
                else mesh.Clear();

                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                EditorUtility.SetDirty(mesh);

                var partGo = new GameObject(pair.Key.name);
                partGo.transform.SetParent(root.transform, false);
                partGo.AddComponent<MeshFilter>().sharedMesh = mesh;
                partGo.AddComponent<MeshRenderer>().sharedMaterial = pair.Key;
            }
        }
    }
}

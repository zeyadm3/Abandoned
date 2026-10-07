using System.Collections.Generic;
using System.Linq;
using Abandoned.Player;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Cosmetics (M7.5, GDD 18): coverall colours and hats as CosmeticDefinition assets in a catalog
    /// (order = network index: only append), and the hat prefabs. Hats are low-poly primitives in the
    /// greybox style, plus Kenney CC0 props worn as hats (a traffic cone, a lamp, a plant, a box).
    /// Existing definitions keep their inspector tuning; hats are rebuilt.
    /// </summary>
    public static class CosmeticsBuilder
    {
        public const string DataFolder = "Assets/_Project/Data/Player/Cosmetics";
        public const string CatalogPath = DataFolder + "/CosmeticCatalog.asset";
        public const string PrefabFolder = "Assets/_Project/Prefabs/Cosmetics";

        // id, name, colour, runs, escapes, haul
        private static readonly (string id, string name, Color color, int runs, int escapes, long haul)[] Coveralls =
        {
            ("orange", "Hazard Orange", new Color(0.95f, 0.45f, 0.1f), 0, 0, 0),
            ("yellow", "Safety Yellow", new Color(0.95f, 0.8f, 0.15f), 0, 0, 0),
            ("navy", "Navy", new Color(0.15f, 0.22f, 0.42f), 0, 0, 0),
            ("green", "Forest Green", new Color(0.2f, 0.42f, 0.22f), 1, 0, 0),
            ("grey", "Concrete Grey", new Color(0.5f, 0.5f, 0.52f), 2, 0, 0),
            ("red", "Fire Red", new Color(0.75f, 0.12f, 0.1f), 0, 3, 0),
            ("pink", "Flamingo Pink", new Color(0.95f, 0.45f, 0.65f), 5, 0, 0),
            ("black", "Night Shift", new Color(0.08f, 0.08f, 0.09f), 0, 10, 0),
            ("gold", "Gold Plated", new Color(0.95f, 0.75f, 0.25f), 0, 0, 150000),
            // M10.6 (append only)
            ("white", "Cleanroom White", new Color(0.92f, 0.92f, 0.9f), 3, 0, 0),
            ("purple", "Royal Purple", new Color(0.4f, 0.18f, 0.55f), 0, 6, 0),
            ("teal", "Teal", new Color(0.1f, 0.55f, 0.55f), 12, 0, 0),
        };

        // id, name, runs, escapes, haul (models built below)
        private static readonly (string id, string name, int runs, int escapes, long haul)[] Hats =
        {
            ("none", "No hat", 0, 0, 0),
            ("hardhat", "Hard Hat", 0, 0, 0),
            ("beanie", "Beanie", 0, 0, 0),
            ("cap", "Cap", 1, 0, 0),
            ("tophat", "Top Hat", 0, 3, 0),
            ("cone", "Traffic Cone", 5, 0, 0),
            ("box", "Cardboard Box", 8, 0, 0),
            ("lamp", "Lamp", 0, 5, 0),
            ("plant", "Potted Plant", 0, 0, 100000),
            // M10.6 (append only)
            ("bucket", "Bucket Hat", 2, 0, 0),
            ("miner", "Miner's Helmet", 0, 2, 0),
            ("cowboy", "Cowboy Hat", 10, 0, 0),
            ("chef", "Chef's Hat", 15, 0, 0),
            ("crown", "Paper Crown", 0, 0, 250000),
            ("trashcan", "Trash Can", 0, 15, 0),
            ("bear", "Teddy Bear", 0, 0, 500000),
        };

        // M10.6 accessories: id, name, runs, escapes, haul, worn on the body (not the head)
        private static readonly (string id, string name, int runs, int escapes, long haul, bool body)[] Accessories =
        {
            ("none", "Nothing", 0, 0, 0, false),
            ("goggles", "Safety Goggles", 0, 0, 0, false),
            ("dustmask", "Dust Mask", 1, 0, 0, false),
            ("shades", "Shades", 0, 2, 0, false),
            ("rucksack", "Rucksack", 4, 0, 0, true),
            ("gasmask", "Gas Mask", 6, 0, 0, false),
            ("moustache", "Moustache", 0, 8, 0, false),
        };

        [MenuItem("Tools/Abandoned/Cosmetics/Build Cosmetics")]
        public static void CreateMissing()
        {
            EnsureFolder("Assets/_Project/Data", "Player");
            EnsureFolder("Assets/_Project/Data/Player", "Cosmetics");
            EnsureFolder("Assets/_Project/Prefabs", "Cosmetics");

            var coveralls = Coveralls.Select(c => Definition($"Coverall_{c.id}", c.id, c.name, CosmeticKind.Coverall, c.color, null, c.runs, c.escapes, c.haul)).ToList();
            var hats = Hats.Select(h => Definition($"Hat_{h.id}", h.id, h.name, CosmeticKind.Hat, Color.white, h.id == "none" ? null : HatPrefab(h.id), h.runs, h.escapes, h.haul)).ToList();
            var extras = Accessories.Select(a => Definition($"Accessory_{a.id}", a.id, a.name, CosmeticKind.Accessory, Color.white,
                a.id == "none" ? null : AccessoryPrefab(a.id), a.runs, a.escapes, a.haul, a.body)).ToList();

            var catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<CosmeticCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            // Keep the existing order (network indices) and append anything new.
            catalog.EditorSet(Merge(catalog.Coveralls, coveralls), Merge(catalog.Hats, hats), Merge(catalog.Accessories, extras), 1);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private static List<CosmeticDefinition> Merge(IReadOnlyList<CosmeticDefinition> existing, List<CosmeticDefinition> wanted)
        {
            var list = existing.Where(d => d != null).ToList();
            foreach (CosmeticDefinition d in wanted) if (!list.Contains(d)) list.Add(d);
            return list;
        }

        private static CosmeticDefinition Definition(string file, string id, string name, CosmeticKind kind, Color color, GameObject hat, int runs, int escapes, long haul,
            bool bodyMounted = false)
        {
            string path = $"{DataFolder}/{file}.asset";
            var d = AssetDatabase.LoadAssetAtPath<CosmeticDefinition>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<CosmeticDefinition>();
                d.EditorSetup(id, name, kind, color, hat, runs, escapes, haul, bodyMounted);
                AssetDatabase.CreateAsset(d, path);
            }
            else if (kind != CosmeticKind.Coverall && d.HatPrefab != hat)
            {
                d.EditorSetup(d.Id, d.DisplayName, d.Kind, d.Color, hat, d.RequiredRuns, d.RequiredEscapes, d.RequiredHaul, d.BodyMounted);
                EditorUtility.SetDirty(d);
            }
            return d;
        }

        /// <summary>A hat prefab, origin at its bottom centre; ~0.3 m across to sit on a 0.6 m wide capsule.</summary>
        private static GameObject HatPrefab(string id)
        {
            var root = new GameObject($"Hat_{id}");
            switch (id)
            {
                case "hardhat":
                {
                    Material yellow = GetMaterial("Hat_HardHat", new Color(0.98f, 0.78f, 0.1f));
                    Primitive(PrimitiveType.Sphere, "Dome", root.transform, new Vector3(0f, 0.02f, 0f), new Vector3(0.36f, 0.26f, 0.38f), yellow, false);
                    Primitive(PrimitiveType.Cylinder, "Brim", root.transform, new Vector3(0f, 0.01f, 0.03f), new Vector3(0.44f, 0.012f, 0.48f), yellow, false);
                    break;
                }
                case "beanie":
                {
                    Material wool = GetMaterial("Hat_Beanie", new Color(0.55f, 0.12f, 0.14f));
                    Primitive(PrimitiveType.Sphere, "Knit", root.transform, new Vector3(0f, 0.03f, 0f), new Vector3(0.34f, 0.26f, 0.34f), wool, false);
                    Primitive(PrimitiveType.Sphere, "Pompom", root.transform, new Vector3(0f, 0.17f, 0f), Vector3.one * 0.08f, wool, false);
                    break;
                }
                case "cap":
                {
                    Material blue = GetMaterial("Hat_Cap", new Color(0.12f, 0.3f, 0.6f));
                    Primitive(PrimitiveType.Sphere, "Crown", root.transform, new Vector3(0f, 0.02f, 0f), new Vector3(0.33f, 0.2f, 0.33f), blue, false);
                    Box("Peak", root.transform, new Vector3(0f, 0.015f, 0.2f), new Vector3(0.24f, 0.015f, 0.16f), blue, false);
                    break;
                }
                case "bucket":
                {
                    Material khaki = GetMaterial("Hat_Bucket", new Color(0.62f, 0.56f, 0.38f));
                    Primitive(PrimitiveType.Cylinder, "Brim", root.transform, new Vector3(0f, 0.02f, 0f), new Vector3(0.44f, 0.02f, 0.44f), khaki, false);
                    Primitive(PrimitiveType.Cylinder, "Crown", root.transform, new Vector3(0f, 0.08f, 0f), new Vector3(0.3f, 0.07f, 0.3f), khaki, false);
                    break;
                }
                case "miner":
                {
                    Material white = GetMaterial("Hat_Miner", new Color(0.92f, 0.92f, 0.88f));
                    Material lamp = GetMaterial("Hat_MinerLamp", new Color(1f, 0.9f, 0.4f));
                    Primitive(PrimitiveType.Sphere, "Dome", root.transform, new Vector3(0f, 0.02f, 0f), new Vector3(0.36f, 0.26f, 0.38f), white, false);
                    Primitive(PrimitiveType.Cylinder, "Brim", root.transform, new Vector3(0f, 0.01f, 0f), new Vector3(0.42f, 0.012f, 0.44f), white, false);
                    Primitive(PrimitiveType.Cylinder, "Lamp", root.transform, new Vector3(0f, 0.07f, 0.18f), new Vector3(0.09f, 0.03f, 0.09f), lamp, false)
                        .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    break;
                }
                case "cowboy":
                {
                    Material leather = GetMaterial("Hat_Cowboy", new Color(0.45f, 0.28f, 0.14f));
                    Primitive(PrimitiveType.Cylinder, "Brim", root.transform, new Vector3(0f, 0.015f, 0f), new Vector3(0.58f, 0.012f, 0.52f), leather, false);
                    Primitive(PrimitiveType.Cylinder, "Crown", root.transform, new Vector3(0f, 0.1f, 0f), new Vector3(0.26f, 0.1f, 0.3f), leather, false);
                    break;
                }
                case "chef":
                {
                    Material cotton = GetMaterial("Hat_Chef", new Color(0.97f, 0.97f, 0.95f));
                    Primitive(PrimitiveType.Cylinder, "Band", root.transform, new Vector3(0f, 0.1f, 0f), new Vector3(0.3f, 0.1f, 0.3f), cotton, false);
                    Primitive(PrimitiveType.Sphere, "Puff", root.transform, new Vector3(0f, 0.27f, 0f), new Vector3(0.38f, 0.18f, 0.38f), cotton, false);
                    break;
                }
                case "crown":
                {
                    Material gold = GetMaterial("Hat_Crown", new Color(0.98f, 0.8f, 0.2f));
                    Primitive(PrimitiveType.Cylinder, "Ring", root.transform, new Vector3(0f, 0.05f, 0f), new Vector3(0.3f, 0.05f, 0.3f), gold, false);
                    for (int i = 0; i < 5; i++)
                    {
                        float a = i * 72f * Mathf.Deg2Rad;
                        Box("Point", root.transform, new Vector3(Mathf.Sin(a) * 0.13f, 0.12f, Mathf.Cos(a) * 0.13f), new Vector3(0.05f, 0.05f, 0.05f), gold, false)
                            .transform.localRotation = Quaternion.Euler(45f, i * 72f, 45f);
                    }
                    break;
                }
                case "tophat":
                {
                    Material black = GetMaterial("Hat_TopHat", new Color(0.06f, 0.06f, 0.07f));
                    Material band = GetMaterial("Hat_TopHatBand", new Color(0.55f, 0.08f, 0.1f));
                    Primitive(PrimitiveType.Cylinder, "Brim", root.transform, new Vector3(0f, 0.01f, 0f), new Vector3(0.42f, 0.01f, 0.42f), black, false);
                    Primitive(PrimitiveType.Cylinder, "Crown", root.transform, new Vector3(0f, 0.17f, 0f), new Vector3(0.27f, 0.16f, 0.27f), black, false);
                    Primitive(PrimitiveType.Cylinder, "Band", root.transform, new Vector3(0f, 0.05f, 0f), new Vector3(0.275f, 0.03f, 0.275f), band, false);
                    break;
                }
                default:
                {
                    // Props worn as hats: Kenney models fitted into a head-sized box, standing on the head.
                    (string path, Vector3 box) = id switch
                    {
                        "cone" => (ThirdPartyModelImport.Root + "Kenney/CarKit/cone.fbx", new Vector3(0.36f, 0.5f, 0.36f)),
                        "box" => (LootModelBuilder.Furniture + "cardboardBoxClosed.fbx", new Vector3(0.74f, 0.56f, 0.74f)),
                        "lamp" => (LootModelBuilder.Furniture + "lampRoundTable.fbx", new Vector3(0.34f, 0.5f, 0.34f)),
                        "trashcan" => (LootModelBuilder.Furniture + "trashcan.fbx", new Vector3(0.62f, 0.6f, 0.62f)),
                        "bear" => (LootModelBuilder.Furniture + "bear.fbx", new Vector3(0.3f, 0.36f, 0.3f)),
                        _ => (LootModelBuilder.Furniture + "pottedPlant.fbx", new Vector3(0.34f, 0.55f, 0.34f)),
                    };
                    var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (model == null) Debug.LogError($"[Cosmetics] Missing hat model {path}.");
                    // The box goes down over the whole head; the rest stand on top of it.
                    else ModelFit.Place(model, root.transform, new Vector3(0f, box.y / 2f - (id == "box" || id == "trashcan" ? 0.3f : 0f), 0f), box, 0f, standOnFloor: true);
                    break;
                }
            }
            foreach (Collider c in root.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            string prefabPath = $"{PrefabFolder}/Hat_{id}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <summary>
        /// An accessory prefab (M10.6), origin at the crown like a hat: face pieces sit in front of the head
        /// (eyes ~0.15 m below the crown on a 0.3 m radius head), the rucksack on the back of the body.
        /// </summary>
        private static GameObject AccessoryPrefab(string id)
        {
            var root = new GameObject($"Accessory_{id}");
            switch (id)
            {
                case "goggles":
                {
                    Material strap = GetMaterial("Acc_GogglesStrap", new Color(0.15f, 0.15f, 0.16f));
                    Material lens = GetMaterial("Acc_GogglesLens", new Color(0.35f, 0.65f, 0.85f));
                    Box("Strap", root.transform, new Vector3(0f, -0.15f, 0.25f), new Vector3(0.3f, 0.035f, 0.03f), strap, false);
                    foreach (float x in new[] { -0.07f, 0.07f })
                        Primitive(PrimitiveType.Cylinder, "Lens", root.transform, new Vector3(x, -0.15f, 0.27f), new Vector3(0.09f, 0.015f, 0.09f), lens, false)
                            .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    break;
                }
                case "dustmask":
                {
                    Material paper = GetMaterial("Acc_DustMask", new Color(0.95f, 0.95f, 0.93f));
                    Primitive(PrimitiveType.Sphere, "Mask", root.transform, new Vector3(0f, -0.3f, 0.24f), new Vector3(0.2f, 0.13f, 0.1f), paper, false);
                    break;
                }
                case "shades":
                {
                    Material black = GetMaterial("Acc_Shades", new Color(0.04f, 0.04f, 0.05f));
                    foreach (float x in new[] { -0.065f, 0.065f })
                        Box("Lens", root.transform, new Vector3(x, -0.15f, 0.275f), new Vector3(0.1f, 0.05f, 0.02f), black, false);
                    Box("Bridge", root.transform, new Vector3(0f, -0.14f, 0.28f), new Vector3(0.04f, 0.012f, 0.012f), black, false);
                    break;
                }
                case "rucksack":
                {
                    Material canvas = GetMaterial("Acc_Rucksack", new Color(0.3f, 0.38f, 0.22f));
                    Box("Pack", root.transform, new Vector3(0f, -0.72f, -0.36f), new Vector3(0.38f, 0.46f, 0.2f), canvas, false);
                    Primitive(PrimitiveType.Cylinder, "Bedroll", root.transform, new Vector3(0f, -0.45f, -0.38f), new Vector3(0.12f, 0.2f, 0.12f), canvas, false)
                        .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    break;
                }
                case "gasmask":
                {
                    Material rubber = GetMaterial("Acc_GasMask", new Color(0.12f, 0.13f, 0.12f));
                    Material lens = GetMaterial("Acc_GasMaskLens", new Color(0.5f, 0.6f, 0.55f));
                    Primitive(PrimitiveType.Sphere, "Face", root.transform, new Vector3(0f, -0.24f, 0.2f), new Vector3(0.26f, 0.24f, 0.16f), rubber, false);
                    Primitive(PrimitiveType.Cylinder, "Filter", root.transform, new Vector3(0f, -0.33f, 0.3f), new Vector3(0.09f, 0.04f, 0.09f), rubber, false)
                        .transform.localRotation = Quaternion.Euler(70f, 0f, 0f);
                    foreach (float x in new[] { -0.06f, 0.06f })
                        Primitive(PrimitiveType.Cylinder, "Eye", root.transform, new Vector3(x, -0.17f, 0.27f), new Vector3(0.07f, 0.01f, 0.07f), lens, false)
                            .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    break;
                }
                default: // moustache
                {
                    Material hair = GetMaterial("Acc_Moustache", new Color(0.25f, 0.15f, 0.08f));
                    foreach (float s in new[] { -1f, 1f })
                        Box("Half", root.transform, new Vector3(s * 0.045f, -0.29f, 0.285f), new Vector3(0.09f, 0.025f, 0.02f), hair, false)
                            .transform.localRotation = Quaternion.Euler(0f, 0f, s * -12f);
                    break;
                }
            }
            foreach (Collider c in root.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/Accessory_{id}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}")) AssetDatabase.CreateFolder(parent, name);
        }
    }
}

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
        };

        [MenuItem("Tools/Abandoned/Cosmetics/Build Cosmetics")]
        public static void CreateMissing()
        {
            EnsureFolder("Assets/_Project/Data", "Player");
            EnsureFolder("Assets/_Project/Data/Player", "Cosmetics");
            EnsureFolder("Assets/_Project/Prefabs", "Cosmetics");

            var coveralls = Coveralls.Select(c => Definition($"Coverall_{c.id}", c.id, c.name, CosmeticKind.Coverall, c.color, null, c.runs, c.escapes, c.haul)).ToList();
            var hats = Hats.Select(h => Definition($"Hat_{h.id}", h.id, h.name, CosmeticKind.Hat, Color.white, h.id == "none" ? null : HatPrefab(h.id), h.runs, h.escapes, h.haul)).ToList();

            var catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<CosmeticCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            // Keep the existing order (network indices) and append anything new.
            catalog.EditorSet(Merge(catalog.Coveralls, coveralls), Merge(catalog.Hats, hats), 1);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private static List<CosmeticDefinition> Merge(IReadOnlyList<CosmeticDefinition> existing, List<CosmeticDefinition> wanted)
        {
            var list = existing.Where(d => d != null).ToList();
            foreach (CosmeticDefinition d in wanted) if (!list.Contains(d)) list.Add(d);
            return list;
        }

        private static CosmeticDefinition Definition(string file, string id, string name, CosmeticKind kind, Color color, GameObject hat, int runs, int escapes, long haul)
        {
            string path = $"{DataFolder}/{file}.asset";
            var d = AssetDatabase.LoadAssetAtPath<CosmeticDefinition>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<CosmeticDefinition>();
                d.EditorSetup(id, name, kind, color, hat, runs, escapes, haul);
                AssetDatabase.CreateAsset(d, path);
            }
            else if (kind == CosmeticKind.Hat && d.HatPrefab != hat)
            {
                d.EditorSetup(d.Id, d.DisplayName, d.Kind, d.Color, hat, d.RequiredRuns, d.RequiredEscapes, d.RequiredHaul);
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
                        _ => (LootModelBuilder.Furniture + "pottedPlant.fbx", new Vector3(0.34f, 0.55f, 0.34f)),
                    };
                    var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (model == null) Debug.LogError($"[Cosmetics] Missing hat model {path}.");
                    // The box goes down over the whole head; the rest stand on top of it.
                    else ModelFit.Place(model, root.transform, new Vector3(0f, box.y / 2f - (id == "box" ? 0.3f : 0f), 0f), box, 0f, standOnFloor: true);
                    break;
                }
            }
            foreach (Collider c in root.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            string prefabPath = $"{PrefabFolder}/Hat_{id}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}")) AssetDatabase.CreateFolder(parent, name);
        }
    }
}

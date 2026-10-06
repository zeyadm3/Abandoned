using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Creates the GDD 7.3 example loot definitions and the shared damage config if they don't
    /// exist yet. Never overwrites an existing asset, so tuning done in the inspector is kept.
    /// </summary>
    public static class LootCatalogBuilder
    {
        public const string Folder = "Assets/_Project/Data/Loot";
        public const string DamageConfigPath = Folder + "/LootDamageConfig.asset";

        [MenuItem("Tools/Abandoned/Create Example Loot Definitions")]
        public static void CreateMissing()
        {
            SerializedWiring.LoadOrCreateAsset<LootDamageConfig>(DamageConfigPath);

            // id, name, value range, kg, class, size (m), fragility, material, noise, rarity, shape, colour
            Add("gold_watch", "Gold Watch", 7500, 7500, 0.15f, CarryClass.Pocket, new(0.05f, 0.02f, 0.05f), Fragility.Low, SurfaceMaterial.Metal, 0.05f, 0.3f, PlaceholderShape.Cylinder, new(1f, 0.78f, 0.2f));
            Add("cash_bundle", "Cash Bundle", 500, 2000, 0.1f, CarryClass.Pocket, new(0.16f, 0.04f, 0.08f), Fragility.None, SurfaceMaterial.Paper, 0.02f, 3f, PlaceholderShape.Cube, new(0.35f, 0.7f, 0.35f));
            Add("laptop", "Laptop", 1200, 1200, 2.5f, CarryClass.OneHand, new(0.35f, 0.03f, 0.25f), Fragility.Medium, SurfaceMaterial.Plastic, 0.2f, 1.5f, PlaceholderShape.Cube, new(0.25f, 0.25f, 0.28f));
            Add("small_painting", "Small Painting", 3000, 15000, 4f, CarryClass.OneHand, new(0.6f, 0.45f, 0.05f), Fragility.Medium, SurfaceMaterial.Wood, 0.2f, 0.8f, PlaceholderShape.Cube, new(0.6f, 0.3f, 0.55f));
            Add("flatscreen_tv", "Flat-screen TV", 2500, 2500, 15f, CarryClass.TwoHand, new(1f, 0.6f, 0.08f), Fragility.High, SurfaceMaterial.Plastic, 0.35f, 1.2f, PlaceholderShape.Cube, new(0.08f, 0.08f, 0.1f));
            Add("antique_vase", "Antique Vase", 25000, 25000, 8f, CarryClass.TwoHand, new(0.35f, 0.6f, 0.35f), Fragility.Extreme, SurfaceMaterial.Glass, 0.4f, 0.4f, PlaceholderShape.Cylinder, new(0.3f, 0.45f, 0.9f));
            Add("glass_sculpture", "Glass Sculpture", 40000, 40000, 10f, CarryClass.TwoHand, new(0.4f, 0.7f, 0.4f), Fragility.Extreme, SurfaceMaterial.Glass, 0.8f, 0.2f, PlaceholderShape.Capsule, new(0.6f, 0.95f, 1f));
            Add("server_rack", "Server Rack", 18000, 18000, 300f, CarryClass.Heavy, new(0.6f, 2f, 1f), Fragility.Low, SurfaceMaterial.Metal, 0.7f, 0.6f, PlaceholderShape.Cube, new(0.18f, 0.2f, 0.25f));
            Add("safe", "Safe", 5000, 5000, 400f, CarryClass.Heavy, new(0.7f, 0.9f, 0.7f), Fragility.None, SurfaceMaterial.Metal, 0.8f, 0.5f, PlaceholderShape.Cube, new(0.35f, 0.36f, 0.3f));
            Add("vending_machine", "Vending Machine", 6000, 6000, 350f, CarryClass.Heavy, new(1f, 1.9f, 0.8f), Fragility.Low, SurfaceMaterial.Metal, 0.9f, 0.5f, PlaceholderShape.Cube, new(0.8f, 0.15f, 0.15f));
            Add("grand_piano", "Grand Piano", 35000, 35000, 500f, CarryClass.Huge, new(1.5f, 1f, 2.4f), Fragility.Medium, SurfaceMaterial.Wood, 0.9f, 0.15f, PlaceholderShape.Cube, new(0.05f, 0.05f, 0.05f));
            Add("marble_statue", "Marble Statue", 100000, 100000, 2000f, CarryClass.Huge, new(0.9f, 2.2f, 0.9f), Fragility.Medium, SurfaceMaterial.Stone, 0.9f, 0.05f, PlaceholderShape.Capsule, new(0.92f, 0.92f, 0.88f));
            Add("military_generator", "Military Generator", 60000, 60000, 1200f, CarryClass.Huge, new(1.2f, 1.2f, 2f), Fragility.Low, SurfaceMaterial.Metal, 1f, 0.1f, PlaceholderShape.Cube, new(0.3f, 0.38f, 0.22f), requiredCarriers: 4); // GDD: needs 4 people

            AssetDatabase.SaveAssets();
        }

        public static string PathFor(string id) => $"{Folder}/Loot_{id}.asset";

        public static LootDefinition Load(string id) => AssetDatabase.LoadAssetAtPath<LootDefinition>(PathFor(id));

        private static void Add(string id, string displayName, int min, int max, float weight, CarryClass carryClass,
            Vector3 size, Fragility fragility, SurfaceMaterial material, float noise, float rarity,
            PlaceholderShape shape, Color color, int requiredCarriers = 0)
        {
            string path = PathFor(id);
            if (AssetDatabase.LoadAssetAtPath<LootDefinition>(path) != null) return;
            var definition = ScriptableObject.CreateInstance<LootDefinition>();
            definition.EditorSetup(id, displayName, min, max, weight, carryClass, size, fragility, material, noise, rarity, shape, color,
                requiredCarriers: requiredCarriers);
            AssetDatabase.CreateAsset(definition, path);
        }
    }
}

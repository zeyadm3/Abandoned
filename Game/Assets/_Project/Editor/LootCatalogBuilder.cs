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
        public const string CatalogPath = Folder + "/LootCatalog.asset";
        public const string SpawnConfigPath = Folder + "/LootSpawnConfig.asset";

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

            // M5: ten more for the mall (20+ total, GDD 26).
            Add("diamond_ring", "Diamond Ring", 4000, 9000, 0.02f, CarryClass.Pocket, new(0.03f, 0.03f, 0.03f), Fragility.Low, SurfaceMaterial.Metal, 0.03f, 0.5f, PlaceholderShape.Sphere, new(0.85f, 0.95f, 1f));
            Add("pearl_necklace", "Pearl Necklace", 2000, 6000, 0.1f, CarryClass.Pocket, new(0.12f, 0.03f, 0.12f), Fragility.Medium, SurfaceMaterial.Stone, 0.05f, 0.8f, PlaceholderShape.Cylinder, new(0.95f, 0.93f, 0.88f));
            Add("smartphone", "Smartphone", 300, 900, 0.2f, CarryClass.Pocket, new(0.08f, 0.01f, 0.16f), Fragility.Medium, SurfaceMaterial.Glass, 0.05f, 2f, PlaceholderShape.Cube, new(0.1f, 0.1f, 0.12f));
            Add("game_console", "Game Console", 300, 600, 3f, CarryClass.OneHand, new(0.3f, 0.08f, 0.28f), Fragility.Medium, SurfaceMaterial.Plastic, 0.2f, 1.5f, PlaceholderShape.Cube, new(0.92f, 0.92f, 0.94f));
            Add("collector_figure", "Collector's Figure", 500, 3000, 1f, CarryClass.OneHand, new(0.15f, 0.3f, 0.15f), Fragility.High, SurfaceMaterial.Plastic, 0.1f, 0.8f, PlaceholderShape.Capsule, new(0.9f, 0.4f, 0.2f));
            Add("desktop_pc", "Desktop PC", 800, 1500, 12f, CarryClass.TwoHand, new(0.22f, 0.45f, 0.45f), Fragility.Medium, SurfaceMaterial.Metal, 0.35f, 1.2f, PlaceholderShape.Cube, new(0.15f, 0.15f, 0.17f));
            Add("large_painting", "Large Painting", 8000, 40000, 12f, CarryClass.TwoHand, new(1.4f, 1f, 0.08f), Fragility.High, SurfaceMaterial.Wood, 0.3f, 0.4f, PlaceholderShape.Cube, new(0.75f, 0.55f, 0.3f));
            Add("espresso_machine", "Espresso Machine", 1500, 3000, 20f, CarryClass.TwoHand, new(0.4f, 0.45f, 0.45f), Fragility.Medium, SurfaceMaterial.Metal, 0.5f, 0.9f, PlaceholderShape.Cube, new(0.7f, 0.7f, 0.72f));
            Add("grandfather_clock", "Grandfather Clock", 4000, 8000, 70f, CarryClass.Heavy, new(0.6f, 2f, 0.4f), Fragility.High, SurfaceMaterial.Wood, 0.8f, 0.4f, PlaceholderShape.Cube, new(0.4f, 0.24f, 0.12f));
            Add("film_projector", "Film Projector", 5000, 12000, 90f, CarryClass.Heavy, new(0.7f, 0.8f, 1.1f), Fragility.Medium, SurfaceMaterial.Metal, 0.7f, 0.4f, PlaceholderShape.Cube, new(0.25f, 0.25f, 0.3f));

            // M7.1: household and electronics loot with Kenney Furniture Kit models (LootModelBuilder sizes the box to the model).
            Add("vintage_tv", "Vintage TV", 600, 1500, 18f, CarryClass.TwoHand, new(0.6f, 0.5f, 0.45f), Fragility.Medium, SurfaceMaterial.Glass, 0.4f, 0.9f, PlaceholderShape.Cube, new(0.45f, 0.35f, 0.25f));
            Add("hifi_speaker", "Hi-fi Speaker", 400, 1200, 14f, CarryClass.TwoHand, new(0.35f, 0.8f, 0.35f), Fragility.Low, SurfaceMaterial.Wood, 0.3f, 1f, PlaceholderShape.Cube, new(0.2f, 0.2f, 0.2f));
            Add("microwave", "Microwave", 100, 300, 12f, CarryClass.TwoHand, new(0.5f, 0.3f, 0.35f), Fragility.Low, SurfaceMaterial.Metal, 0.4f, 1.2f, PlaceholderShape.Cube, new(0.85f, 0.85f, 0.85f));
            Add("toaster", "Toaster", 40, 120, 2f, CarryClass.OneHand, new(0.3f, 0.2f, 0.18f), Fragility.Low, SurfaceMaterial.Metal, 0.3f, 1.5f, PlaceholderShape.Cube, new(0.8f, 0.8f, 0.8f));
            Add("blender", "Blender", 60, 200, 2.5f, CarryClass.OneHand, new(0.18f, 0.4f, 0.18f), Fragility.High, SurfaceMaterial.Glass, 0.3f, 1.2f, PlaceholderShape.Cylinder, new(0.7f, 0.8f, 0.9f));
            Add("designer_lamp", "Designer Lamp", 200, 900, 3f, CarryClass.OneHand, new(0.3f, 0.55f, 0.3f), Fragility.High, SurfaceMaterial.Glass, 0.3f, 1f, PlaceholderShape.Cylinder, new(0.95f, 0.9f, 0.7f));
            Add("computer_monitor", "Computer Monitor", 150, 500, 5f, CarryClass.OneHand, new(0.55f, 0.45f, 0.2f), Fragility.Medium, SurfaceMaterial.Plastic, 0.3f, 1.3f, PlaceholderShape.Cube, new(0.15f, 0.15f, 0.15f));
            Add("retro_radio", "Retro Radio", 300, 900, 3f, CarryClass.OneHand, new(0.35f, 0.25f, 0.15f), Fragility.Medium, SurfaceMaterial.Wood, 0.3f, 1f, PlaceholderShape.Cube, new(0.55f, 0.35f, 0.2f));
            Add("washing_machine", "Washing Machine", 300, 700, 70f, CarryClass.Heavy, new(0.65f, 0.9f, 0.65f), Fragility.Low, SurfaceMaterial.Metal, 0.6f, 0.7f, PlaceholderShape.Cube, new(0.9f, 0.9f, 0.9f));
            Add("fridge", "Fridge", 500, 1200, 90f, CarryClass.Heavy, new(0.7f, 1.8f, 0.7f), Fragility.Low, SurfaceMaterial.Metal, 0.6f, 0.6f, PlaceholderShape.Cube, new(0.9f, 0.9f, 0.95f));
            Add("designer_chair", "Designer Chair", 1500, 5000, 12f, CarryClass.TwoHand, new(0.75f, 0.8f, 0.75f), Fragility.Medium, SurfaceMaterial.Fabric, 0.2f, 0.7f, PlaceholderShape.Cube, new(0.6f, 0.3f, 0.2f));
            Add("designer_sofa", "Designer Sofa", 3000, 9000, 80f, CarryClass.Heavy, new(1.9f, 0.8f, 0.9f), Fragility.Medium, SurfaceMaterial.Fabric, 0.3f, 0.5f, PlaceholderShape.Cube, new(0.3f, 0.4f, 0.6f));
            Add("bear_head", "Mounted Bear Head", 300, 1200, 6f, CarryClass.TwoHand, new(0.5f, 0.6f, 0.4f), Fragility.Low, SurfaceMaterial.Fabric, 0.1f, 0.8f, PlaceholderShape.Capsule, new(0.6f, 0.4f, 0.25f));

            // M9.3: toward GDD 7.3's ~60 items. Pocket money, small office/household things, big furniture.
            Add("gold_watch", "Gold Watch", 6000, 9000, 0.1f, CarryClass.Pocket, new(0.05f, 0.02f, 0.05f), Fragility.Low, SurfaceMaterial.Metal, 0.03f, 0.25f, PlaceholderShape.Cylinder, new(0.95f, 0.78f, 0.3f));
            Add("cash_bundle", "Cash Bundle", 500, 2000, 0.1f, CarryClass.Pocket, new(0.16f, 0.03f, 0.08f), Fragility.None, SurfaceMaterial.Paper, 0.02f, 1.2f, PlaceholderShape.Cube, new(0.45f, 0.7f, 0.4f));
            Add("silver_lighter", "Silver Lighter", 150, 600, 0.1f, CarryClass.Pocket, new(0.04f, 0.06f, 0.015f), Fragility.Low, SurfaceMaterial.Metal, 0.03f, 1f, PlaceholderShape.Cube, new(0.8f, 0.82f, 0.86f));
            Add("designer_sunglasses", "Designer Sunglasses", 200, 900, 0.1f, CarryClass.Pocket, new(0.15f, 0.05f, 0.05f), Fragility.Medium, SurfaceMaterial.Plastic, 0.02f, 1f, PlaceholderShape.Cube, new(0.1f, 0.1f, 0.12f));
            Add("computer_mouse", "Computer Mouse", 20, 90, 0.15f, CarryClass.Pocket, new(0.07f, 0.04f, 0.12f), Fragility.Low, SurfaceMaterial.Plastic, 0.03f, 1.6f, PlaceholderShape.Cube, new(0.2f, 0.2f, 0.22f));
            Add("small_painting", "Small Painting", 3000, 15000, 3f, CarryClass.OneHand, new(0.5f, 0.4f, 0.04f), Fragility.Medium, SurfaceMaterial.Wood, 0.15f, 0.5f, PlaceholderShape.Cube, new(0.55f, 0.35f, 0.5f));
            Add("table_lamp", "Table Lamp", 120, 450, 2f, CarryClass.OneHand, new(0.25f, 0.5f, 0.25f), Fragility.High, SurfaceMaterial.Glass, 0.25f, 1.1f, PlaceholderShape.Cylinder, new(0.9f, 0.85f, 0.7f));
            Add("computer_keyboard", "Mechanical Keyboard", 60, 250, 1f, CarryClass.OneHand, new(0.45f, 0.04f, 0.15f), Fragility.Low, SurfaceMaterial.Plastic, 0.2f, 1.3f, PlaceholderShape.Cube, new(0.15f, 0.15f, 0.17f));
            Add("book_stack", "First Editions", 200, 2500, 4f, CarryClass.OneHand, new(0.25f, 0.2f, 0.2f), Fragility.Low, SurfaceMaterial.Paper, 0.15f, 0.9f, PlaceholderShape.Cube, new(0.5f, 0.3f, 0.25f));
            Add("bonsai", "Bonsai", 300, 1500, 3f, CarryClass.OneHand, new(0.25f, 0.35f, 0.25f), Fragility.Medium, SurfaceMaterial.Dirt, 0.15f, 0.8f, PlaceholderShape.Cylinder, new(0.3f, 0.55f, 0.3f));
            Add("silk_cushion", "Silk Cushion", 80, 300, 0.8f, CarryClass.OneHand, new(0.45f, 0.15f, 0.45f), Fragility.None, SurfaceMaterial.Fabric, 0.05f, 1.2f, PlaceholderShape.Cube, new(0.3f, 0.45f, 0.8f));
            Add("bookshelf_speaker", "Bookshelf Speaker", 150, 600, 4f, CarryClass.OneHand, new(0.2f, 0.35f, 0.22f), Fragility.Low, SurfaceMaterial.Wood, 0.25f, 1f, PlaceholderShape.Cube, new(0.2f, 0.18f, 0.16f));
            Add("antenna_tv", "Antenna TV", 300, 900, 16f, CarryClass.TwoHand, new(0.55f, 0.6f, 0.45f), Fragility.Medium, SurfaceMaterial.Glass, 0.4f, 0.9f, PlaceholderShape.Cube, new(0.4f, 0.32f, 0.25f));
            Add("glass_coffee_table", "Glass Coffee Table", 400, 1400, 20f, CarryClass.TwoHand, new(1.1f, 0.45f, 0.6f), Fragility.High, SurfaceMaterial.Glass, 0.6f, 0.6f, PlaceholderShape.Cube, new(0.7f, 0.85f, 0.9f));
            Add("relax_chair", "Recliner", 400, 1300, 18f, CarryClass.TwoHand, new(0.8f, 0.9f, 0.8f), Fragility.Low, SurfaceMaterial.Fabric, 0.2f, 0.8f, PlaceholderShape.Cube, new(0.5f, 0.3f, 0.2f));
            Add("bedside_cabinet", "Bedside Cabinet", 100, 350, 12f, CarryClass.TwoHand, new(0.5f, 0.55f, 0.45f), Fragility.Low, SurfaceMaterial.Wood, 0.3f, 1f, PlaceholderShape.Cube, new(0.6f, 0.45f, 0.3f));
            Add("ceiling_fan", "Ceiling Fan", 80, 300, 8f, CarryClass.TwoHand, new(1.2f, 0.4f, 1.2f), Fragility.Medium, SurfaceMaterial.Metal, 0.35f, 0.9f, PlaceholderShape.Cylinder, new(0.75f, 0.75f, 0.78f));
            Add("floor_lamp", "Designer Floor Lamp", 300, 1100, 7f, CarryClass.TwoHand, new(0.45f, 1.6f, 0.45f), Fragility.High, SurfaceMaterial.Glass, 0.3f, 0.8f, PlaceholderShape.Cylinder, new(0.95f, 0.9f, 0.75f));
            Add("bar_stool", "Bar Stool", 60, 220, 6f, CarryClass.TwoHand, new(0.45f, 0.8f, 0.45f), Fragility.Low, SurfaceMaterial.Metal, 0.35f, 1.1f, PlaceholderShape.Cylinder, new(0.3f, 0.3f, 0.32f));
            Add("kitchen_stove", "Kitchen Stove", 300, 900, 80f, CarryClass.Heavy, new(0.75f, 0.9f, 0.65f), Fragility.Low, SurfaceMaterial.Metal, 0.7f, 0.6f, PlaceholderShape.Cube, new(0.85f, 0.85f, 0.88f));
            Add("tumble_dryer", "Tumble Dryer", 250, 650, 65f, CarryClass.Heavy, new(0.65f, 0.9f, 0.65f), Fragility.Low, SurfaceMaterial.Metal, 0.6f, 0.6f, PlaceholderShape.Cube, new(0.9f, 0.9f, 0.92f));
            Add("clawfoot_bathtub", "Clawfoot Bathtub", 900, 3000, 130f, CarryClass.Heavy, new(1.7f, 0.7f, 0.8f), Fragility.Medium, SurfaceMaterial.Stone, 0.9f, 0.35f, PlaceholderShape.Cube, new(0.95f, 0.95f, 0.95f));
            Add("corner_sofa", "Corner Sofa", 1200, 3500, 95f, CarryClass.Heavy, new(2f, 0.85f, 2f), Fragility.Low, SurfaceMaterial.Fabric, 0.3f, 0.45f, PlaceholderShape.Cube, new(0.4f, 0.4f, 0.45f));

            // Gear that behaves like loot (M6.4b): a plank from the Planks equipment. Worth nothing, never spawned.
            Add("plank", "Plank", 0, 0, 12f, CarryClass.TwoHand, new(0.4f, 0.08f, 4.4f), Fragility.None, SurfaceMaterial.Wood, 0.3f, 0f, PlaceholderShape.Cube, new(0.62f, 0.47f, 0.28f));
            LootDefinition plank = Load("plank");
            if (plank != null && !plank.Utility)
            {
                plank.EditorSetUtility(true);
                EditorUtility.SetDirty(plank);
            }

            // Where each can turn up (store kinds = MallLayout zone tags). Only filled in when empty.
            Spawn("gold_watch", "jewelry", "office");
            Spawn("cash_bundle", "concourse", "office", "jewelry", "food", "stock", "cinema", "clothing", "toys", "walkway");
            Spawn("laptop", "electronics", "office", "furniture");
            Spawn("small_painting", "gallery", "furniture", "office");
            Spawn("flatscreen_tv", "electronics", "cinema", "furniture");
            Spawn("antique_vase", "gallery", "furniture");
            Spawn("glass_sculpture", "gallery", "jewelry");
            Spawn("server_rack", "office", "electronics", "stock");
            Spawn("safe", "office", "jewelry", "stock");
            Spawn("vending_machine", "food", "concourse", "cinema", "walkway");
            Spawn("diamond_ring", "jewelry", "clothing");
            Spawn("pearl_necklace", "jewelry", "clothing");
            Spawn("smartphone", "electronics", "office", "concourse", "food", "cinema", "toys");
            Spawn("game_console", "electronics", "toys");
            Spawn("collector_figure", "toys", "cinema");
            Spawn("desktop_pc", "electronics", "office");
            Spawn("large_painting", "gallery", "furniture", "cinema");
            Spawn("espresso_machine", "food", "furniture");
            Spawn("grandfather_clock", "furniture", "gallery");
            Spawn("film_projector", "cinema", "stock");
            Spawn("vintage_tv", "electronics", "furniture", "stock");
            Spawn("hifi_speaker", "electronics", "cinema");
            Spawn("microwave", "food", "stock");
            Spawn("toaster", "food", "furniture");
            Spawn("blender", "food", "furniture");
            Spawn("designer_lamp", "furniture", "gallery", "office");
            Spawn("computer_monitor", "electronics", "office");
            Spawn("retro_radio", "electronics", "furniture", "stock");
            Spawn("washing_machine", "stock", "furniture");
            Spawn("fridge", "food", "stock");
            Spawn("designer_chair", "furniture", "gallery");
            Spawn("designer_sofa", "furniture");
            Spawn("bear_head", "furniture", "gallery", "toys");
            Spawn("gold_watch", "jewelry", "clothing");
            Spawn("cash_bundle", "office", "food", "stock", "clothing", "walkway");
            Spawn("silver_lighter", "jewelry", "office", "cinema");
            Spawn("designer_sunglasses", "clothing", "jewelry");
            Spawn("computer_mouse", "electronics", "office");
            Spawn("small_painting", "gallery", "furniture", "office");
            Spawn("table_lamp", "furniture", "office");
            Spawn("computer_keyboard", "electronics", "office");
            Spawn("book_stack", "office", "gallery", "toys");
            Spawn("bonsai", "furniture", "gallery", "food");
            Spawn("silk_cushion", "furniture", "clothing", "cinema");
            Spawn("bookshelf_speaker", "electronics", "cinema");
            Spawn("antenna_tv", "electronics", "stock");
            Spawn("glass_coffee_table", "furniture");
            Spawn("relax_chair", "furniture", "cinema");
            Spawn("bedside_cabinet", "furniture", "stock");
            Spawn("ceiling_fan", "stock", "furniture");
            Spawn("floor_lamp", "furniture", "gallery");
            Spawn("bar_stool", "food", "furniture");
            Spawn("kitchen_stove", "food", "stock");
            Spawn("tumble_dryer", "stock");
            Spawn("clawfoot_bathtub", "furniture", "stock");
            Spawn("corner_sofa", "furniture");
            // Jackpots: only on jackpot points (gallery, furniture floor, loading bay).
            Spawn("marble_statue", true, "gallery");
            Spawn("grand_piano", true, "furniture", "gallery");
            Spawn("military_generator", true, "stock");

            SerializedWiring.LoadOrCreateAsset<LootSpawnConfig>(SpawnConfigPath);
            AssetDatabase.SaveAssets();
        }

        public static string PathFor(string id) => $"{Folder}/Loot_{id}.asset";

        private static void Spawn(string id, params string[] tags) => Spawn(id, false, tags);

        private static void Spawn(string id, bool jackpot, params string[] tags)
        {
            LootDefinition definition = Load(id);
            if (definition == null || (definition.SpawnTags != null && definition.SpawnTags.Length > 0)) return;
            definition.EditorSetSpawning(tags, jackpot);
            EditorUtility.SetDirty(definition);
        }

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

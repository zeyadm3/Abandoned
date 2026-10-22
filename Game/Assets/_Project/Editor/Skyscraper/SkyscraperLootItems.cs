using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Content definitions for all 25 Skyscraper Restaurant salvage items.
    /// Spawns on tags: skyscraper_dining, skyscraper_kitchen, skyscraper_bar, skyscraper_cellar, skyscraper_executive.
    /// </summary>
    public static class SkyscraperLootItems
    {
        public const string TagDining = "skyscraper_dining";
        public const string TagKitchen = "skyscraper_kitchen";
        public const string TagBar = "skyscraper_bar";
        public const string TagCellar = "skyscraper_cellar";
        public const string TagExecutive = "skyscraper_executive";

        public readonly struct ItemSpec
        {
            public readonly string Id, Name;
            public readonly int Min, Max;
            public readonly float Kg, Noise, Rarity;
            public readonly CarryClass Class;
            public readonly Vector3 Size;
            public readonly Fragility Fragility;
            public readonly SurfaceMaterial Material;
            public readonly PlaceholderShape Shape;
            public readonly Color Color;
            public readonly bool Jackpot;
            public readonly string[] Tags;

            public ItemSpec(string id, string name, int min, int max, float kg, CarryClass carry, Vector3 size,
                Fragility fragility, SurfaceMaterial material, float noise, float rarity, PlaceholderShape shape,
                Color color, bool jackpot, params string[] tags)
            {
                Id = id; Name = name; Min = min; Max = max; Kg = kg; Class = carry; Size = size;
                Fragility = fragility; Material = material; Noise = noise; Rarity = rarity; Shape = shape;
                Color = color; Jackpot = jackpot; Tags = tags;
            }
        }

        private const PlaceholderShape Box = PlaceholderShape.Cube;
        private const PlaceholderShape Drum = PlaceholderShape.Cylinder;
        private const PlaceholderShape Ball = PlaceholderShape.Sphere;

        public static readonly ItemSpec[] Items =
        {
            // ---- Dining Room Salvage -------------------------------------------------------------
            new("silver_cutlery_canteen", "Silver Cutlery Canteen", 800, 2200, 3.5f, CarryClass.OneHand, new(0.45f, 0.12f, 0.32f), Fragility.High, SurfaceMaterial.Metal, 0.15f, 1.2f, Box, new(0.78f, 0.78f, 0.82f), false, TagDining),
            new("caviar_tin_reserve", "Reserve Imperial Caviar", 1500, 4000, 1.2f, CarryClass.Pocket, new(0.18f, 0.08f, 0.18f), Fragility.Medium, SurfaceMaterial.Metal, 0.05f, 0.8f, Drum, new(0.85f, 0.75f, 0.35f), false, TagDining),
            new("crystal_wine_decanter", "Crystal Wine Decanter", 600, 1800, 1.8f, CarryClass.OneHand, new(0.22f, 0.36f, 0.22f), Fragility.Extreme, SurfaceMaterial.Glass, 0.10f, 1.0f, Drum, new(0.88f, 0.92f, 0.95f), false, TagDining),
            new("silver_serving_cloche", "Sterling Serving Cloche", 450, 1300, 1.5f, CarryClass.OneHand, new(0.32f, 0.20f, 0.32f), Fragility.Low, SurfaceMaterial.Metal, 0.20f, 1.4f, Ball, new(0.82f, 0.82f, 0.85f), false, TagDining),
            new("gilded_china_stack", "Gilded Fine China Stack", 900, 2600, 6.0f, CarryClass.TwoHand, new(0.30f, 0.25f, 0.30f), Fragility.Extreme, SurfaceMaterial.Stone, 0.25f, 0.7f, Drum, new(0.95f, 0.94f, 0.88f), false, TagDining),
            new("silver_candelabra", "Silver 5-Arm Candelabra", 550, 1600, 2.8f, CarryClass.OneHand, new(0.35f, 0.50f, 0.35f), Fragility.Medium, SurfaceMaterial.Metal, 0.15f, 1.1f, Box, new(0.80f, 0.80f, 0.82f), false, TagDining),
            new("maitre_d_leather_ledger", "Maître D' Leather Ledger", 400, 1200, 1.4f, CarryClass.OneHand, new(0.35f, 0.08f, 0.28f), Fragility.Low, SurfaceMaterial.Fabric, 0.08f, 1.2f, Box, new(0.30f, 0.18f, 0.14f), false, TagDining),
            new("brass_observation_telescope", "Tower Viewing Telescope", 3500, 8800, 52.0f, CarryClass.Heavy, new(0.60f, 1.40f, 0.60f), Fragility.Medium, SurfaceMaterial.Metal, 0.55f, 0.35f, Drum, new(0.82f, 0.70f, 0.30f), false, TagDining),

            // ---- Commercial Kitchen Salvage ------------------------------------------------------
            new("copper_saute_pan", "Solid Copper Saute Pan", 700, 1900, 3.2f, CarryClass.OneHand, new(0.30f, 0.12f, 0.52f), Fragility.None, SurfaceMaterial.Metal, 0.25f, 1.3f, Drum, new(0.85f, 0.52f, 0.32f), false, TagKitchen),
            new("damascus_knife_roll", "Master Chef Damascus Roll", 1200, 3200, 2.5f, CarryClass.OneHand, new(0.48f, 0.15f, 0.15f), Fragility.Low, SurfaceMaterial.Fabric, 0.10f, 0.9f, Box, new(0.25f, 0.20f, 0.18f), false, TagKitchen),
            new("black_truffle_crate", "Crate of Black Truffles", 2000, 5500, 4.0f, CarryClass.OneHand, new(0.35f, 0.22f, 0.28f), Fragility.High, SurfaceMaterial.Wood, 0.10f, 0.5f, Box, new(0.42f, 0.32f, 0.22f), false, TagKitchen),
            new("aged_iberico_ham", "Aged Jamon Iberico Leg", 1100, 2900, 8.5f, CarryClass.TwoHand, new(0.85f, 0.25f, 0.22f), Fragility.None, SurfaceMaterial.Fabric, 0.20f, 0.9f, Box, new(0.55f, 0.28f, 0.22f), false, TagKitchen),
            new("sous_vide_circulator", "Commercial Immersion Unit", 850, 2400, 4.2f, CarryClass.OneHand, new(0.18f, 0.42f, 0.18f), Fragility.Medium, SurfaceMaterial.Metal, 0.15f, 1.0f, Drum, new(0.30f, 0.32f, 0.35f), false, TagKitchen),
            new("heavy_copper_stockpot", "Heavy Copper Stockpot", 1400, 3800, 12.0f, CarryClass.TwoHand, new(0.45f, 0.45f, 0.45f), Fragility.None, SurfaceMaterial.Metal, 0.40f, 0.8f, Drum, new(0.82f, 0.48f, 0.28f), false, TagKitchen),
            new("brass_meat_slicer", "Commercial Gravity Meat Slicer", 3200, 7500, 48.0f, CarryClass.Heavy, new(0.70f, 0.55f, 0.60f), Fragility.Medium, SurfaceMaterial.Metal, 0.60f, 0.45f, Box, new(0.70f, 0.62f, 0.32f), false, TagKitchen),

            // ---- Cocktail Bar Salvage ------------------------------------------------------------
            new("gold_cocktail_shaker", "Gold-Plated Cocktail Shaker", 650, 1800, 0.7f, CarryClass.Pocket, new(0.12f, 0.26f, 0.12f), Fragility.Low, SurfaceMaterial.Metal, 0.08f, 1.2f, Drum, new(0.92f, 0.80f, 0.32f), false, TagBar),
            new("crystal_tumbler_set", "Lead Crystal Tumblers (4)", 400, 1100, 2.0f, CarryClass.OneHand, new(0.32f, 0.15f, 0.32f), Fragility.High, SurfaceMaterial.Glass, 0.12f, 1.3f, Box, new(0.88f, 0.92f, 0.96f), false, TagBar),
            new("vintage_soda_siphon", "Wire-Mesh Soda Siphon", 300, 850, 2.2f, CarryClass.OneHand, new(0.15f, 0.38f, 0.15f), Fragility.Medium, SurfaceMaterial.Metal, 0.15f, 1.5f, Drum, new(0.65f, 0.68f, 0.72f), false, TagBar),
            new("commercial_espresso_machine", "Dual-Boiler Espresso Unit", 4500, 9500, 65.0f, CarryClass.Heavy, new(0.85f, 0.65f, 0.60f), Fragility.Medium, SurfaceMaterial.Metal, 0.70f, 0.4f, Box, new(0.75f, 0.75f, 0.78f), false, TagBar),
            new("brass_draught_tap_tower", "Triple Brass Tap Tower", 1800, 4200, 14.5f, CarryClass.TwoHand, new(0.40f, 0.65f, 0.35f), Fragility.Low, SurfaceMaterial.Metal, 0.30f, 0.6f, Box, new(0.80f, 0.68f, 0.28f), false, TagBar),

            // ---- Wine Cellar & Reserve Spirits ----------------------------------------------------
            new("vintage_cognac_1945", "1945 Imperial Cognac", 1800, 4800, 1.6f, CarryClass.Pocket, new(0.12f, 0.32f, 0.12f), Fragility.High, SurfaceMaterial.Glass, 0.08f, 0.6f, Drum, new(0.62f, 0.32f, 0.18f), false, TagCellar),
            new("grand_cru_bordeaux", "1982 Grand Cru Bordeaux", 2500, 7000, 1.5f, CarryClass.Pocket, new(0.11f, 0.33f, 0.11f), Fragility.High, SurfaceMaterial.Glass, 0.08f, 0.4f, Drum, new(0.42f, 0.12f, 0.18f), false, TagCellar),
            new("sommelier_tasting_kit", "Master Sommelier Kit", 950, 2700, 2.8f, CarryClass.OneHand, new(0.38f, 0.14f, 0.28f), Fragility.Medium, SurfaceMaterial.Wood, 0.12f, 0.9f, Box, new(0.35f, 0.22f, 0.15f), false, TagCellar),
            new("champagne_dom_magnum", "Reserve Champagne Magnum", 1600, 4200, 3.0f, CarryClass.OneHand, new(0.15f, 0.42f, 0.15f), Fragility.High, SurfaceMaterial.Glass, 0.10f, 0.6f, Drum, new(0.35f, 0.55f, 0.35f), false, TagCellar),
            new("vintage_single_malt_whisky", "50-Year Islay Single Malt", 2200, 5600, 1.4f, CarryClass.Pocket, new(0.12f, 0.32f, 0.12f), Fragility.High, SurfaceMaterial.Glass, 0.08f, 0.5f, Drum, new(0.55f, 0.28f, 0.12f), false, TagCellar),
            new("grand_wine_cask", "Grand Vintage Cask (Jackpot)", 25000, 60000, 340.0f, CarryClass.Huge, new(1.20f, 1.40f, 1.20f), Fragility.Low, SurfaceMaterial.Wood, 0.95f, 0.10f, Drum, new(0.45f, 0.28f, 0.18f), true, TagCellar),

            // ---- Penthouse Executive Suite & Jackpots ---------------------------------------------
            new("platinum_fountain_pen", "Solid Platinum Desk Pen", 1200, 3500, 0.15f, CarryClass.Pocket, new(0.04f, 0.04f, 0.18f), Fragility.Medium, SurfaceMaterial.Metal, 0.02f, 0.7f, Drum, new(0.85f, 0.88f, 0.92f), false, TagExecutive),
            new("vintage_phonograph_console", "Executive Turntable Console", 2200, 6000, 16.0f, CarryClass.TwoHand, new(0.65f, 0.40f, 0.45f), Fragility.Medium, SurfaceMaterial.Wood, 0.35f, 0.5f, Box, new(0.40f, 0.25f, 0.18f), false, TagExecutive),
            new("executive_leather_briefcase", "Executive Leather Briefcase", 1500, 4500, 3.0f, CarryClass.OneHand, new(0.46f, 0.34f, 0.16f), Fragility.Medium, SurfaceMaterial.Fabric, 0.12f, 0.8f, Box, new(0.24f, 0.15f, 0.10f), false, TagExecutive),
            new("gantry_anemometer_gauge", "Roof Wind Anemometer Kit", 750, 2100, 2.6f, CarryClass.OneHand, new(0.32f, 0.38f, 0.24f), Fragility.Medium, SurfaceMaterial.Metal, 0.15f, 1.0f, Box, new(0.45f, 0.50f, 0.55f), false, TagExecutive),
            new("bronze_bull_statue", "Solid Bronze Bull (Jackpot)", 14000, 32000, 85.0f, CarryClass.Heavy, new(0.95f, 0.65f, 0.50f), Fragility.None, SurfaceMaterial.Metal, 0.85f, 0.15f, Box, new(0.65f, 0.48f, 0.25f), true, TagExecutive),
            new("executive_wall_safe", "Executive Wall Safe (Jackpot)", 8000, 22000, 220.0f, CarryClass.Heavy, new(0.70f, 0.90f, 0.65f), Fragility.None, SurfaceMaterial.Metal, 0.90f, 0.25f, Box, new(0.28f, 0.30f, 0.32f), true, TagExecutive)
        };
    }
}

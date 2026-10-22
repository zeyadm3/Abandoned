using System.Collections.Generic;
using Abandoned.Interaction;
using UnityEngine;
using L = Abandoned.EditorTools.SkyscraperLayout;
using Tags = Abandoned.EditorTools.SkyscraperLootItems;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Generates LootPointSpec locations matching the 32 Skyscraper salvage items and tags.
    /// Distributes loot across the street lobby, flooded floor (12m), mechanical deck (20m),
    /// looted office (32m), the grand dining room, cocktail bar, commercial kitchen, penthouse, and roof terrace (50m).
    /// </summary>
    public static class SkyscraperLootPlan
    {
        public static List<LootPointSpec> Points()
        {
            var points = new List<LootPointSpec>();
            float y = L.RestaurantY;

            // 1. Grand Dining Room Tables (skyscraper_dining)
            Vector3[] tables = {
                new(-10f, y + 0.85f, 6f), new(-6f, y + 0.85f, 6f), new(6f, y + 0.85f, 6f), new(10f, y + 0.85f, 6f),
                new(-12f, y + 0.85f, 12f), new(-6f, y + 0.85f, 14f), new(6f, y + 0.85f, 14f), new(12f, y + 0.85f, 12f),
                new(-8f, y + 0.85f, 18f), new(8f, y + 0.85f, 18f)
            };

            for (int t = 0; t < tables.Length; t++)
            {
                points.Add(new LootPointSpec($"Loot_DiningTable_{t}_A", Tags.TagDining, tables[t] + new Vector3(-0.35f, 0f, 0f), CarryClass.Pocket, CarryClass.OneHand, false));
                points.Add(new LootPointSpec($"Loot_DiningTable_{t}_B", Tags.TagDining, tables[t] + new Vector3( 0.35f, 0f, 0f), CarryClass.OneHand, CarryClass.TwoHand, false));
            }

            // Central Grand Plinth (Jackpot 2: Bronze Bull)
            points.Add(new LootPointSpec("Loot_Plinth_BronzeBull_Jackpot", Tags.TagExecutive, new Vector3(0f, y + 1.25f, 10f), CarryClass.Heavy, CarryClass.Heavy, true));

            // Maître D' Podium
            points.Add(new LootPointSpec("Loot_MaitreD_Podium", Tags.TagDining, new Vector3(0f, y + 1.28f, -5.5f), CarryClass.OneHand, CarryClass.OneHand, false));

            // 2. Cocktail Lounge & Bar (skyscraper_bar)
            points.Add(new LootPointSpec("Loot_BarCounter_West", Tags.TagBar, new Vector3(6f, y + 1.25f, -2f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_BarCounter_Center", Tags.TagBar, new Vector3(8.5f, y + 1.25f, -2f), CarryClass.TwoHand, CarryClass.Heavy, false));
            points.Add(new LootPointSpec("Loot_BarCounter_East", Tags.TagBar, new Vector3(11f, y + 1.25f, -2f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_BackBar_Shelf1", Tags.TagBar, new Vector3(7.5f, y + 1.7f, 0.3f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_BackBar_Shelf2", Tags.TagBar, new Vector3(9.5f, y + 1.7f, 0.3f), CarryClass.OneHand, CarryClass.TwoHand, false));

            // 3. Commercial Kitchen & Cold Store (skyscraper_kitchen)
            points.Add(new LootPointSpec("Loot_Kitchen_Island_L", Tags.TagKitchen, new Vector3(-11f, y + 1.15f, -2f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Kitchen_Island_R", Tags.TagKitchen, new Vector3(-7f, y + 1.15f, -2f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Kitchen_Prep_1", Tags.TagKitchen, new Vector3(-14.5f, y + 1.05f, -3f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Kitchen_Prep_2", Tags.TagKitchen, new Vector3(-14.5f, y + 1.05f, -1f), CarryClass.TwoHand, CarryClass.Heavy, false));
            points.Add(new LootPointSpec("Loot_Kitchen_Dishwash", Tags.TagKitchen, new Vector3(-10f, y + 1.05f, 0.8f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_ColdStore_Shelf", Tags.TagKitchen, new Vector3(-14.5f, y + 1.35f, -6f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_ColdStore_HamStand", Tags.TagKitchen, new Vector3(-12.5f, y + 0.2f, -6f), CarryClass.TwoHand, CarryClass.TwoHand, false));

            // 4. Wine Display & Cellar Reserves (skyscraper_cellar)
            points.Add(new LootPointSpec("Loot_WineWall_Tier1", Tags.TagCellar, new Vector3(15.2f, y + 1.5f, 11f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_WineWall_Tier2", Tags.TagCellar, new Vector3(15.2f, y + 1.5f, 15f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Cellar_Sommelier", Tags.TagCellar, new Vector3(11f, y + 1.15f, 4.5f), CarryClass.OneHand, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Cellar_GrandCask_Jackpot", Tags.TagCellar, new Vector3(14.5f, y + 0.7f, 17f), CarryClass.Huge, CarryClass.Huge, true));

            // 5. Penthouse Executive VIP Suite (skyscraper_executive)
            points.Add(new LootPointSpec("Loot_ExecutiveTable_North", Tags.TagExecutive, new Vector3(11f, y + 0.88f, 15f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_ExecutiveTable_South", Tags.TagExecutive, new Vector3(11f, y + 0.88f, 11f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Executive_WallSafe_Jackpot", Tags.TagExecutive, new Vector3(11f, y + 2.2f, 4.2f), CarryClass.Heavy, CarryClass.Heavy, true));
            points.Add(new LootPointSpec("Loot_Executive_Credenza", Tags.TagExecutive, new Vector3(13f, y + 1.15f, 4.5f), CarryClass.Pocket, CarryClass.OneHand, false));

            // 6. Ground Lobby Concierge & Mechanical Deck (20m)
            points.Add(new LootPointSpec("Loot_Concierge_Counter", Tags.TagExecutive, new Vector3(6f, L.GroundY + 1.3f, -3f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Mechanical_Workbench", Tags.TagKitchen, new Vector3(-8f, L.MechanicalY + 0.9f, 6f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Mechanical_ToolRack", Tags.TagKitchen, new Vector3(8f, L.MechanicalY + 1.1f, 8f), CarryClass.TwoHand, CarryClass.Heavy, false));

            // 7. Intermediate Stairwell Stops: Flooded Floor (12m) & Looted Office (32m)
            points.Add(new LootPointSpec("Loot_FloodedFloor_SupplyCabinet", Tags.TagExecutive, new Vector3(-6f, 12.0f + 0.8f, 2f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_FloodedFloor_WetDesk", Tags.TagBar, new Vector3(-4f, 12.0f + 0.85f, 5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Office_ExecutiveDesk", Tags.TagExecutive, new Vector3(-6f, 32.0f + 0.85f, 4f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Office_ServerRack", Tags.TagExecutive, new Vector3(-3f, 32.0f + 1.1f, 6f), CarryClass.TwoHand, CarryClass.Heavy, false));

            // 8. Roof Terrace & Window Gantry (50m)
            points.Add(new LootPointSpec("Loot_Roof_ViewingTelescope", Tags.TagDining, new Vector3(-8f, L.RoofY + 1.2f, 16f), CarryClass.TwoHand, CarryClass.Heavy, false));
            points.Add(new LootPointSpec("Loot_Roof_GantryAnemometer", Tags.TagExecutive, new Vector3(0f, L.RoofY + 1.2f, 19f), CarryClass.OneHand, CarryClass.OneHand, false));

            return points;
        }
    }
}

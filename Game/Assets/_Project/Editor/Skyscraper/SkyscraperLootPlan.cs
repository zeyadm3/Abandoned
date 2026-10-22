using System.Collections.Generic;
using UnityEngine;
using L = Abandoned.EditorTools.SkyscraperLayout;
using Tags = Abandoned.EditorTools.SkyscraperLootItems;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Generates LootPointSpec locations matching the 25 Skyscraper salvage items and tags.
    /// Distributes loot across dining tables, cocktail bar, commercial kitchen, and penthouse executive suite.
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
                points.Add(new LootPointSpec($"Loot_DiningTable_{t}_A", Tags.TagDining, tables[t] + new Vector3(-0.35f, 0f, 0f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
                points.Add(new LootPointSpec($"Loot_DiningTable_{t}_B", Tags.TagDining, tables[t] + new Vector3( 0.35f, 0f, 0f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            }

            // Central Grand Plinth (Jackpot Bronze Bull location)
            points.Add(new LootPointSpec("Loot_Plinth_Jackpot", Tags.TagExecutive, new Vector3(0f, y + 1.25f, 10f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)1, true));

            // 2. Cocktail Lounge & Bar (skyscraper_bar)
            points.Add(new LootPointSpec("Loot_BarCounter_West", Tags.TagBar, new Vector3(6f, y + 1.25f, -2f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_BarCounter_Center", Tags.TagBar, new Vector3(8.5f, y + 1.25f, -2f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_BarCounter_East", Tags.TagBar, new Vector3(11f, y + 1.25f, -2f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_BackBar_Shelf1", Tags.TagBar, new Vector3(7.5f, y + 1.7f, 0.3f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_BackBar_Shelf2", Tags.TagBar, new Vector3(9.5f, y + 1.7f, 0.3f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));

            // 3. Commercial Kitchen & Cold Store (skyscraper_kitchen)
            points.Add(new LootPointSpec("Loot_Kitchen_Island_L", Tags.TagKitchen, new Vector3(-11f, y + 1.15f, -2f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_Kitchen_Island_R", Tags.TagKitchen, new Vector3(-7f, y + 1.15f, -2f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_Kitchen_Prep_1", Tags.TagKitchen, new Vector3(-14.5f, y + 1.05f, -3f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_Kitchen_Prep_2", Tags.TagKitchen, new Vector3(-14.5f, y + 1.05f, -1f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_Kitchen_Dishwash", Tags.TagKitchen, new Vector3(-10f, y + 1.05f, 0.8f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_ColdStore_Shelf", Tags.TagKitchen, new Vector3(-14.5f, y + 1.35f, -6f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_ColdStore_HamStand", Tags.TagKitchen, new Vector3(-12.5f, y + 0.2f, -6f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)1, false));

            // 4. Wine Display & Cellar Reserves (skyscraper_cellar)
            points.Add(new LootPointSpec("Loot_WineWall_Tier1", Tags.TagCellar, new Vector3(15.2f, y + 1.5f, 11f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_WineWall_Tier2", Tags.TagCellar, new Vector3(15.2f, y + 1.5f, 15f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_Cellar_Sommelier", Tags.TagCellar, new Vector3(11f, y + 1.15f, 4.5f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)1, false));
            points.Add(new LootPointSpec("Loot_Cellar_JackpotCask", Tags.TagCellar, new Vector3(14.5f, y + 0.7f, 17f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)1, true));

            // 5. Penthouse Executive VIP Suite (skyscraper_executive)
            points.Add(new LootPointSpec("Loot_ExecutiveTable_North", Tags.TagExecutive, new Vector3(11f, y + 0.88f, 15f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_ExecutiveTable_South", Tags.TagExecutive, new Vector3(11f, y + 0.88f, 11f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_Executive_WallSafe_Jackpot", Tags.TagExecutive, new Vector3(11f, y + 2.2f, 4.2f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)1, true));
            points.Add(new LootPointSpec("Loot_Executive_Credenza", Tags.TagExecutive, new Vector3(13f, y + 1.15f, 4.5f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));

            // 6. Ground Lobby Concierge & Mechanical Deck
            points.Add(new LootPointSpec("Loot_Concierge_Counter", Tags.TagExecutive, new Vector3(6f, L.GroundY + 1.3f, -3f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)2, false));
            points.Add(new LootPointSpec("Loot_Mechanical_Workbench", Tags.TagKitchen, new Vector3(-8f, L.MechanicalY + 0.9f, 6f), (Abandoned.Interaction.CarryClass)1, (Abandoned.Interaction.CarryClass)1, false));

            return points;
        }
    }
}

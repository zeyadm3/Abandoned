using System.Collections.Generic;
using Abandoned.Interaction;
using UnityEngine;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Distributes 45+ authored salvage loot points (including high-value Jackpots) and 18
    /// strategic monster threat spawn positions throughout Pine Hollow Middle School
    /// across all academic, athletic, performance, and utility wings for responsive threat director pacing.
    /// </summary>
    public static class BurleySchoolLootAndThreats
    {
        public static List<LootPointSpec> GetLootPoints()
        {
            var points = new List<LootPointSpec>();

            // ---- 1. Administration & Principal's Suite (Office & High Value) ----
            points.Add(new LootPointSpec("Loot_Principal_Safe", "office", new Vector3(61.5f, 0.05f, 43f), CarryClass.TwoHand, CarryClass.Heavy, true));
            points.Add(new LootPointSpec("Loot_Principal_Desk", "office", new Vector3(61.5f, 0.85f, 46f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Admin_CashBox", "office", new Vector3(57f, 0.9f, 50.6f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Admin_Cabinet", "office", new Vector3(62.5f, 0.53f, 50.2f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Clinic_Medicine", "stock", new Vector3(34.5f, 1.2f, 48f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 2. Central Corridors & Athletics Trophy Hall ----
            points.Add(new LootPointSpec("Loot_TrophyCase_Jackpot", "gallery", new Vector3(36.7f, 0.82f, 39.42f), CarryClass.OneHand, CarryClass.TwoHand, true));
            points.Add(new LootPointSpec("Loot_TrophyCase_Gold1", "gallery", new Vector3(35.35f, 0.82f, 39.42f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_TrophyCase_Gold2", "gallery", new Vector3(38.05f, 0.82f, 39.42f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Locker_G1", "office", new Vector3(30f, 0.5f, 32.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Locker_G2", "office", new Vector3(56f, 0.5f, 32.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Locker_U1", "office", new Vector3(30f, 4.5f, 32.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Locker_U2", "office", new Vector3(56f, 4.5f, 32.5f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 3. Classrooms (Ground Floor) ----
            points.Add(new LootPointSpec("Loot_Class101_TeacherDesk", "office", new Vector3(32f, 0.8f, 29.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Class101_Backpack", "office", new Vector3(28f, 0.2f, 24.5f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Class102_TeacherDesk", "office", new Vector3(58f, 0.8f, 29.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Class102_Laptop", "office", new Vector3(56f, 0.75f, 25f), CarryClass.OneHand, CarryClass.OneHand, false));

            // ---- 4. Science Labs & Upper Classrooms (Upper Floor, y = 4.0m) ----
            points.Add(new LootPointSpec("Loot_ScienceLab_Microscope", "office", new Vector3(34f, 4.95f, 26f), CarryClass.OneHand, CarryClass.TwoHand, true));
            points.Add(new LootPointSpec("Loot_ScienceLab_Bench1", "office", new Vector3(31f, 4.95f, 28.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_ScienceLab_Bench2", "stock", new Vector3(37f, 4.95f, 28.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_ScienceLab_Hood", "stock", new Vector3(39.2f, 4.95f, 23f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Class203_Desk", "office", new Vector3(58f, 4.8f, 29.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Class203_Books", "gallery", new Vector3(54f, 4.2f, 25f), CarryClass.OneHand, CarryClass.OneHand, false));

            // ---- 5. Library & Media Center (Upper Floor, y = 4.0m) ----
            points.Add(new LootPointSpec("Loot_Library_RareBooks", "gallery", new Vector3(32f, 5.2f, 66.2f), CarryClass.OneHand, CarryClass.TwoHand, true));
            points.Add(new LootPointSpec("Loot_Library_PC_1", "office", new Vector3(30f, 4.85f, 57.5f), CarryClass.TwoHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Library_CircDesk", "office", new Vector3(32f, 4.85f, 57.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Library_StackA", "gallery", new Vector3(29.5f, 5.2f, 70.7f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Library_StackB", "gallery", new Vector3(34f, 5.2f, 75.2f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Library_StudyLamp", "gallery", new Vector3(58f, 4.8f, 68f), CarryClass.OneHand, CarryClass.OneHand, false));

            // ---- 6. Cafeteria & Commercial Kitchen (Ground Floor) ----
            points.Add(new LootPointSpec("Loot_Kitchen_Appliance", "food", new Vector3(66.5f, 0.95f, 66f), CarryClass.TwoHand, CarryClass.Heavy, true));
            points.Add(new LootPointSpec("Loot_Kitchen_Counter", "food", new Vector3(60f, 0.95f, 62f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Kitchen_Pantry", "food", new Vector3(64f, 0.85f, 75f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Cafeteria_Table1", "food", new Vector3(31f, 0.78f, 58f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Cafeteria_Table2", "food", new Vector3(41f, 0.78f, 65f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 7. Gymnasium Complex & Athletics ----
            points.Add(new LootPointSpec("Loot_Gym_ScoreboardJackpot", "furniture", new Vector3(96.5f, 0.05f, 44f), CarryClass.TwoHand, CarryClass.Heavy, true));
            points.Add(new LootPointSpec("Loot_Gym_Bleachers", "furniture", new Vector3(74.2f, 0.05f, 25f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Gym_BallCart", "furniture", new Vector3(92f, 0.6f, 24f), CarryClass.TwoHand, CarryClass.Heavy, false));
            points.Add(new LootPointSpec("Loot_Gym_LockerEast", "office", new Vector3(82f, 0.5f, 66f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 8. Auditorium & Stage (West Wing) ----
            points.Add(new LootPointSpec("Loot_Stage_GrandPiano", "furniture", new Vector3(19.5f, 1.45f, 51.5f), CarryClass.Heavy, CarryClass.Huge, true));
            points.Add(new LootPointSpec("Loot_Stage_Podium", "cinema", new Vector3(11.5f, 1.85f, 49f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Stage_Projector", "cinema", new Vector3(12.5f, 1.85f, 49f), CarryClass.TwoHand, CarryClass.Heavy, false));
            points.Add(new LootPointSpec("Loot_Auditorium_Seats", "cinema", new Vector3(8f, 0.5f, 32f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 9. Basement Boiler Room (Utility, y = -3.5m) ----
            points.Add(new LootPointSpec("Loot_Boiler_CoreJackpot", "stock", new Vector3(54.5f, -3.45f, 81.5f), CarryClass.Heavy, CarryClass.Huge, true));
            points.Add(new LootPointSpec("Loot_Boiler_Workbench", "stock", new Vector3(63.5f, -2.6f, 85.5f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Boiler_ToolChest", "stock", new Vector3(64.5f, -2.55f, 85.5f), CarryClass.TwoHand, CarryClass.Heavy, false));
            points.Add(new LootPointSpec("Loot_Boiler_CopperPipes", "stock", new Vector3(63.5f, -3.1f, 89.5f), CarryClass.TwoHand, CarryClass.Heavy, false));

            // Upstairs side rooms reward leaving the protected central return route.
            points.Add(new LootPointSpec("Loot_Art_Printmaking", "gallery", new Vector3(30f, 4.95f, 46f), CarryClass.OneHand, CarryClass.TwoHand, true));
            points.Add(new LootPointSpec("Loot_Art_DrawingDesk", "office", new Vector3(36f, 4.95f, 44.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Art_Supplies", "stock", new Vector3(30f, 5f, 50.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Staff_Records", "office", new Vector3(57f, 4.9f, 46f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Staff_Media", "office", new Vector3(58f, 4.9f, 46f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Staff_Sideboard", "gallery", new Vector3(62f, 4.95f, 49.7f), CarryClass.Pocket, CarryClass.OneHand, false));

            return points;
        }

        public static List<Vector3> GetThreatSpawns()
        {
            return new List<Vector3>
            {
                // Main Hallway patrol intersections (Ground Floor)
                new(34f, 0.05f, 35f),
                new(58f, 0.05f, 35f),
                // Classroom back corners (hidden around door sightlines)
                new(28f, 0.05f, 24f),
                new(64f, 0.05f, 24f),
                // Administration Records Closet & Clinic
                new(64f, 0.05f, 48f),
                new(29f, 0.05f, 46f),
                // Gymnasium Center Court & Bleacher blind spots
                new(82f, 0.05f, 44f),
                new(75.5f, 0.05f, 34f),
                new(92f, 0.05f, 62f),
                // Auditorium Backstage & Audience Aisle
                new(12f, 1.15f, 53f),
                new(6f, 0.05f, 24f),
                // Cafeteria Dishroom & Commercial Kitchen Pantry
                new(28f, 0.05f, 74f),
                new(64f, 0.05f, 74f),
                // Upper Hallway & Central Landing
                new(46f, 4.05f, 35f),
                new(46f, 4.05f, 46f),
                // Science Lab Fume Hood corner
                new(28f, 4.05f, 28f),
                // Library Book Stacks
                new(36.5f, 4.05f, 68f),
                new(56f, 4.05f, 74f),
                // Basement Boiler Room shadow recesses (y = -3.5m)
                new(54f, -3.45f, 84f),
                new(60f, -3.45f, 88f),
                new(34f, 4.05f, 48f),
                new(61f, 4.05f, 44f)
            };
        }
    }
}

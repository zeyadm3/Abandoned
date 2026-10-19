using System.Collections.Generic;
using Abandoned.Interaction;
using UnityEngine;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Distributes 50+ thematic loot points and monster threat spawn positions
    /// throughout Jackson P. Burley Middle School across all academic, athletic,
    /// performance, and utility wings.
    /// </summary>
    public static class BurleySchoolLootAndThreats
    {
        public static List<LootPointSpec> GetLootPoints()
        {
            var points = new List<LootPointSpec>();

            // ---- 1. Administration & Principal's Suite (Office & High Value) ----
            points.Add(new LootPointSpec("Loot_Principal_Safe", "office", new Vector3(53f, 0.45f, 44f), CarryClass.TwoHand, CarryClass.Heavy, true));
            points.Add(new LootPointSpec("Loot_Principal_Desk", "office", new Vector3(51.5f, 0.85f, 42f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Admin_CashBox", "office", new Vector3(59.5f, 1.15f, 42.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Admin_Cabinet", "office", new Vector3(54f, 0.85f, 44.5f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Clinic_Medicine", "stock", new Vector3(34.5f, 1.2f, 44f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 2. Central Corridors & Athletics Trophy Hall ----
            points.Add(new LootPointSpec("Loot_TrophyCase_Jackpot", "gallery", new Vector3(46f, 0.85f, 32.2f), CarryClass.OneHand, CarryClass.TwoHand, true));
            points.Add(new LootPointSpec("Loot_TrophyCase_Gold1", "gallery", new Vector3(44.5f, 0.75f, 32.2f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_TrophyCase_Gold2", "gallery", new Vector3(47.5f, 0.75f, 32.2f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Locker_G1", "office", new Vector3(30f, 0.5f, 32.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Locker_G2", "office", new Vector3(56f, 0.5f, 32.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Locker_U1", "office", new Vector3(30f, 4.5f, 32.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Locker_U2", "office", new Vector3(56f, 4.5f, 32.5f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 3. Classrooms (Ground Floor) ----
            points.Add(new LootPointSpec("Loot_Class101_TeacherDesk", "office", new Vector3(34f, 0.8f, 29.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Class101_Backpack", "office", new Vector3(29f, 0.2f, 24.5f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Class102_TeacherDesk", "office", new Vector3(58f, 0.8f, 29.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Class102_Laptop", "office", new Vector3(54f, 0.75f, 26f), CarryClass.OneHand, CarryClass.OneHand, false));

            // ---- 4. Science Labs & Upper Classrooms (Upper Floor, y = 4.0m) ----
            points.Add(new LootPointSpec("Loot_ScienceLab_Microscope", "office", new Vector3(35f, 4.95f, 25f), CarryClass.OneHand, CarryClass.TwoHand, true));
            points.Add(new LootPointSpec("Loot_ScienceLab_Bench1", "office", new Vector3(31f, 4.95f, 28.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_ScienceLab_Bench2", "stock", new Vector3(39f, 4.95f, 28.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_ScienceLab_Hood", "stock", new Vector3(28.5f, 4.95f, 30.5f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Class203_Desk", "office", new Vector3(58f, 4.8f, 29.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Class203_Books", "gallery", new Vector3(52f, 4.75f, 24f), CarryClass.OneHand, CarryClass.OneHand, false));

            // ---- 5. Library & Media Center (Upper Floor, y = 4.0m) ----
            points.Add(new LootPointSpec("Loot_Library_RareBooks", "gallery", new Vector3(36f, 5.2f, 62f), CarryClass.OneHand, CarryClass.TwoHand, true));
            points.Add(new LootPointSpec("Loot_Library_PC_1", "office", new Vector3(45f, 4.85f, 76.5f), CarryClass.TwoHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Library_PC_2", "office", new Vector3(49f, 4.85f, 76.5f), CarryClass.TwoHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Library_CircDesk", "office", new Vector3(29f, 5.0f, 73.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Library_StackA", "gallery", new Vector3(40f, 5.2f, 67f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Library_StackB", "gallery", new Vector3(32f, 5.2f, 72f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 6. Cafeteria & Commercial Kitchen (Ground Floor) ----
            points.Add(new LootPointSpec("Loot_Kitchen_Appliance", "food", new Vector3(59f, 0.95f, 73f), CarryClass.TwoHand, CarryClass.Heavy, true));
            points.Add(new LootPointSpec("Loot_Kitchen_Counter", "food", new Vector3(57f, 0.95f, 69.5f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Kitchen_Pantry", "food", new Vector3(64f, 0.85f, 75f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Cafeteria_Table1", "food", new Vector3(35f, 0.78f, 58f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Cafeteria_Table2", "food", new Vector3(43f, 0.78f, 64f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 7. Gymnasium Complex & Athletics ----
            points.Add(new LootPointSpec("Loot_Gym_ScoreboardJackpot", "furniture", new Vector3(94f, 0.5f, 44f), CarryClass.TwoHand, CarryClass.Heavy, true));
            points.Add(new LootPointSpec("Loot_Gym_Bleachers", "furniture", new Vector3(72f, 0.8f, 38f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Gym_BallCart", "furniture", new Vector3(94f, 0.6f, 24f), CarryClass.TwoHand, CarryClass.Heavy, false));
            points.Add(new LootPointSpec("Loot_Gym_LockerEast", "office", new Vector3(82f, 0.5f, 66f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 8. Auditorium & Stage (West Wing) ----
            points.Add(new LootPointSpec("Loot_Stage_GrandPiano", "furniture", new Vector3(19f, 0.95f, 51.5f), CarryClass.Heavy, CarryClass.Huge, true));
            points.Add(new LootPointSpec("Loot_Stage_Projector", "cinema", new Vector3(12f, 0.95f, 49f), CarryClass.TwoHand, CarryClass.Heavy, false));
            points.Add(new LootPointSpec("Loot_Stage_Podium", "cinema", new Vector3(12f, 0.95f, 47.5f), CarryClass.Pocket, CarryClass.OneHand, false));
            points.Add(new LootPointSpec("Loot_Auditorium_TechDesk", "cinema", new Vector3(12f, 0.95f, 23f), CarryClass.TwoHand, CarryClass.Heavy, false));
            points.Add(new LootPointSpec("Loot_Auditorium_Seats", "cinema", new Vector3(8f, 0.5f, 32f), CarryClass.Pocket, CarryClass.OneHand, false));

            // ---- 9. Basement Boiler Room (Utility, y = -3.5m) ----
            points.Add(new LootPointSpec("Loot_Boiler_CoreJackpot", "stock", new Vector3(56f, -3.4f, 85f), CarryClass.Heavy, CarryClass.Huge, true));
            points.Add(new LootPointSpec("Loot_Boiler_Workbench", "stock", new Vector3(64f, -2.6f, 79f), CarryClass.OneHand, CarryClass.TwoHand, false));
            points.Add(new LootPointSpec("Loot_Boiler_ToolChest", "stock", new Vector3(64.5f, -2.5f, 80f), CarryClass.TwoHand, CarryClass.Heavy, false));
            points.Add(new LootPointSpec("Loot_Boiler_CopperPipes", "stock", new Vector3(51f, -3.4f, 88f), CarryClass.TwoHand, CarryClass.Heavy, false));

            return points;
        }

        public static List<Vector3> GetThreatSpawns()
        {
            return new List<Vector3>
            {
                // Main Hallway patrol intersections (Ground Floor)
                new(34f, 0.05f, 35f),
                new(58f, 0.05f, 35f),
                // Gymnasium Center Court & Bleacher blind spot
                new(82f, 0.05f, 44f),
                new(74f, 0.05f, 58f),
                // Auditorium Backstage & Audience Aisle
                new(12f, 0.95f, 51f),
                new(12f, 0.05f, 32f),
                // Upper Hallway & Science Wing
                new(46f, 4.05f, 35f),
                new(34f, 4.05f, 27f),
                // Library Book Stacks
                new(42f, 4.05f, 65f),
                // Cafeteria Dishroom & Kitchen
                new(58f, 0.05f, 74f),
                // Basement Boiler Room shadow
                new(56f, -3.45f, 82f)
            };
        }
    }
}

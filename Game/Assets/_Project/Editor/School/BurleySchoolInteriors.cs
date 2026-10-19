using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Authors the complete interior facilities of Jackson P. Burley Middle School:
    /// Gymnasium (basketball court, hoops, bleachers, scoreboard), Auditorium (stage, curtain, seating, podium),
    /// Cafeteria and Commercial Kitchen, Library & Media Center, Classrooms (History, Math, Science Labs),
    /// Administration Suite, Nurse Clinic, and the Basement Boiler Room.
    /// </summary>
    public static class BurleySchoolInteriors
    {
        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform interiors = Group("SchoolInteriors", root);

            // 1. Gymnasium Complex (East Wing)
            BuildGymnasium(Group("Gymnasium", interiors), art);

            // 2. Auditorium & Performing Arts (West Wing)
            BuildAuditorium(Group("Auditorium", interiors), art);

            // 3. Cafeteria & Commercial Kitchen (North Wing Ground)
            BuildCafeteriaAndKitchen(Group("CafeteriaAndKitchen", interiors), art);

            // 4. Library & Media Center (North Wing Upper)
            BuildLibraryAndMediaCenter(Group("LibraryAndMediaCenter", interiors), art);

            // 5. Academic Classrooms (Center Wing Ground & Upper)
            BuildClassrooms(Group("AcademicClassrooms", interiors), art);

            // 6. Science Wing & Laboratory (Center Wing Upper)
            BuildScienceWing(Group("ScienceLaboratory", interiors), art);

            // 7. Administration Suite & Nurse Clinic (Center Wing Ground)
            BuildAdminAndClinic(Group("AdministrationAndClinic", interiors), art);

            // 8. Basement Boiler Room (Utility Wing)
            BuildBoilerRoom(Group("BasementBoilerRoom", interiors), art);

            MarkStatic(interiors);
        }

        // =========================================================================
        // 1. GYMNASIUM
        // =========================================================================
        private static void BuildGymnasium(Transform parent, BurleySchoolArt art)
        {
            // Court dimensions: centered in the Gym (X: 82, Z: 44)
            Vector3 courtCenter = new(82f, 0.02f, 44f);
            float courtL = 36f, courtW = 20f;

            // Hardwood floor court boundary
            Box("CourtPerimeterStripe", parent, courtCenter, new Vector3(courtW, 0.015f, courtL), art.GymWood, false);
            // White Court Boundary Lines
            Box("Boundary_N", parent, courtCenter + Vector3.forward * (courtL * 0.5f), new Vector3(courtW, 0.025f, 0.12f), art.WhiteStripe, false);
            Box("Boundary_S", parent, courtCenter - Vector3.forward * (courtL * 0.5f), new Vector3(courtW, 0.025f, 0.12f), art.WhiteStripe, false);
            Box("Boundary_W", parent, courtCenter - Vector3.right * (courtW * 0.5f), new Vector3(0.12f, 0.025f, courtL), art.WhiteStripe, false);
            Box("Boundary_E", parent, courtCenter + Vector3.right * (courtW * 0.5f), new Vector3(0.12f, 0.025f, courtL), art.WhiteStripe, false);

            // Center Jump Circle with painted "BURLEY BEARS" in Gold
            Box("CenterLine", parent, courtCenter, new Vector3(courtW, 0.025f, 0.12f), art.WhiteStripe, false);
            Box("CenterCircleRing", parent, courtCenter, new Vector3(3.6f, 0.024f, 3.6f), art.GoldAccent, false);
            Box("CenterCircleFill", parent, courtCenter, new Vector3(3.4f, 0.026f, 3.4f), art.LockerGreen, false);
            Label(parent, "CenterCourtLogo", "BURLEY BEARS", courtCenter + Vector3.up * 0.03f,
                Quaternion.Euler(90f, 0f, 0f), 0.08f, art.GoldAccent.GetColor("_BaseColor"));

            // North and South Free-Throw Keys & Circles
            BuildBasketballKey(parent, courtCenter + Vector3.forward * (courtL * 0.5f - 5.8f), false, art);
            BuildBasketballKey(parent, courtCenter - Vector3.forward * (courtL * 0.5f - 5.8f), true, art);

            // Regulation Basketball Hoops
            BuildBasketballHoop(parent, "NorthHoop", new Vector3(82f, 0f, 65.5f), false, art);
            BuildBasketballHoop(parent, "SouthHoop", new Vector3(82f, 0f, 22.5f), true, art);

            // Multi-Tier Wooden Spectator Bleachers along west wall (X: 70)
            Transform bleachers = Group("SpectatorBleachers", parent);
            for (int tier = 0; tier < 6; tier++)
            {
                float bx = 70.2f + tier * 0.65f;
                float by = 0.25f + tier * 0.35f;
                Box($"BleacherTier_{tier}", bleachers, new Vector3(bx, by, 44f), new Vector3(0.6f, 0.15f, 32f), art.OakWood);
                Box($"BleacherFrame_{tier}", bleachers, new Vector3(bx, by * 0.5f, 44f), new Vector3(0.55f, by, 31.8f), art.MetalSteel);
            }

            // Electronic Scoreboard mounted on East Wall
            Transform scoreboard = Group("ElectronicScoreboard", parent);
            scoreboard.position = new Vector3(95.8f, 5.8f, 44f);
            Box("ScoreboardChassis", scoreboard, Vector3.zero, new Vector3(0.2f, 2.2f, 5.2f), art.MetalSteel);
            Box("ScoreboardFace", scoreboard, new Vector3(-0.11f, 0f, 0f), new Vector3(0.04f, 2.1f, 5.0f), art.BrickDark, false);
            Label(scoreboard, "ScoreHome", "BURLEY BEARS\n58", new Vector3(-0.14f, 0.35f, -1.3f),
                Quaternion.Euler(0f, -90f, 0f), 0.12f, Color.red);
            Label(scoreboard, "ScorePeriod", "PERIOD 4\nFOULS 2", new Vector3(-0.14f, -0.4f, 0f),
                Quaternion.Euler(0f, -90f, 0f), 0.08f, Color.yellow);
            Label(scoreboard, "ScoreGuest", "VISITORS\n52", new Vector3(-0.14f, 0.35f, 1.3f),
                Quaternion.Euler(0f, -90f, 0f), 0.12f, Color.green);

            // Wall Safety Impact Mats (Kelly Green) along baselines
            for (int m = 0; m < 5; m++)
            {
                float mx = 76f + m * 2.8f;
                Box($"WallMat_N_{m}", parent, new Vector3(mx, 1.2f, 67.8f), new Vector3(2.6f, 2.0f, 0.12f), art.LockerGreen, false);
                Box($"WallMat_S_{m}", parent, new Vector3(mx, 1.2f, 20.2f), new Vector3(2.6f, 2.0f, 0.12f), art.LockerGreen, false);
            }

            // Ball Rack with Basketballs in corner
            Transform rack = Group("BasketballRack", parent);
            rack.position = new Vector3(94.2f, 0.05f, 23.5f);
            Box("RackFrame", rack, new Vector3(0f, 0.6f, 0f), new Vector3(0.5f, 1.1f, 1.6f), art.MetalSteel);
            for (int b = 0; b < 6; b++)
            {
                float bz = -0.5f + (b % 3) * 0.5f;
                float by = (b < 3) ? 0.45f : 0.85f;
                Box($"Ball_{b}", rack, new Vector3(0f, by, bz), new Vector3(0.24f, 0.24f, 0.24f), art.GoldAccent, false);
            }
        }

        private static void BuildBasketballKey(Transform parent, Vector3 center, bool south, BurleySchoolArt art)
        {
            Transform key = Group(south ? "SouthKey" : "NorthKey", parent);
            key.position = center;
            // Key interior fill
            Box("KeyFill", key, Vector3.zero, new Vector3(4.8f, 0.024f, 5.8f), art.LockerGreen, false);
            // Free throw circle
            Box("FreeThrowCircle", key, new Vector3(0f, 0f, south ? 2.9f : -2.9f), new Vector3(3.6f, 0.025f, 3.6f), art.GoldAccent, false);
            // Three point line arc
            Box("ThreePointLine", key, new Vector3(0f, 0f, south ? 3.5f : -3.5f), new Vector3(8.5f, 0.025f, 0.12f), art.WhiteStripe, false);
        }

        private static void BuildBasketballHoop(Transform parent, string name, Vector3 basePos, bool south, BurleySchoolArt art)
        {
            Transform hoop = Group(name, parent);
            hoop.position = basePos;
            float rotY = south ? 0f : 180f;
            hoop.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Wall Extension Steel Truss
            Box("TrussTop", hoop, new Vector3(0f, 3.65f, 1.2f), new Vector3(0.12f, 0.12f, 2.4f), art.MetalSteel);
            Box("TrussBot", hoop, new Vector3(0f, 2.7f, 0.8f), new Vector3(0.12f, 0.12f, 1.6f), art.MetalSteel);
            Box("TrussStrut", hoop, new Vector3(0f, 3.1f, 1.4f), new Vector3(0.12f, 1.2f, 0.12f), art.MetalSteel);

            // Regulation Backboard (Glass with white square border)
            Box("Backboard", hoop, new Vector3(0f, 3.65f, 2.4f), new Vector3(1.8f, 1.05f, 0.04f), art.WindowGlass, false);
            Box("BackboardFrame", hoop, new Vector3(0f, 3.65f, 2.4f), new Vector3(1.84f, 1.09f, 0.03f), art.MetalSteel, false);
            Box("TargetSquare", hoop, new Vector3(0f, 3.45f, 2.42f), new Vector3(0.6f, 0.45f, 0.02f), art.WhiteStripe, false);

            // Orange Breakaway Rim & Net (Regulation 10ft / 3.05m height)
            Box("RimBase", hoop, new Vector3(0f, 3.05f, 2.55f), new Vector3(0.18f, 0.08f, 0.15f), art.GoldAccent, false);
            Box("RimRing", hoop, new Vector3(0f, 3.05f, 2.85f), new Vector3(0.48f, 0.05f, 0.48f), art.GoldAccent, false);
            Box("NetMesh", hoop, new Vector3(0f, 2.82f, 2.85f), new Vector3(0.36f, 0.45f, 0.36f), art.FloorTile, false);
        }

        // =========================================================================
        // 2. AUDITORIUM
        // =========================================================================
        private static void BuildAuditorium(Transform parent, BurleySchoolArt art)
        {
            // Proscenium Raised Stage (X: 2 to 22, Z: 46 to 54, Height: 0.9m)
            Transform stage = Group("ProsceniumStage", parent);
            Box("StageFloor", stage, new Vector3(12f, 0.45f, 50f), new Vector3(20f, 0.9f, 8f), art.StageWood);
            Box("StageApronStairs_L", stage, new Vector3(3f, 0.22f, 45.2f), new Vector3(2.5f, 0.45f, 1.4f), art.StageWood);
            Box("StageApronStairs_R", stage, new Vector3(21f, 0.22f, 45.2f), new Vector3(2.5f, 0.45f, 1.4f), art.StageWood);

            // Proscenium Arch Framing
            Box("ArchLeftPillar", stage, new Vector3(2.2f, 4.0f, 46f), new Vector3(1.2f, 8.0f, 0.4f), art.Brick);
            Box("ArchRightPillar", stage, new Vector3(21.8f, 4.0f, 46f), new Vector3(1.2f, 8.0f, 0.4f), art.Brick);
            Box("ArchLintelHeader", stage, new Vector3(12f, 7.6f, 46f), new Vector3(20.4f, 1.2f, 0.45f), art.Limestone);

            // Grand Velvet Stage Curtains (Forest Green with Gold Trim)
            Box("Curtain_Valance", stage, new Vector3(12f, 7.1f, 46.25f), new Vector3(18.5f, 0.9f, 0.12f), art.StageCurtain, false);
            Box("Curtain_ValanceGoldTrim", stage, new Vector3(12f, 6.62f, 46.26f), new Vector3(18.6f, 0.08f, 0.14f), art.GoldAccent, false);
            Box("CurtainDrape_Left", stage, new Vector3(3.8f, 3.8f, 46.25f), new Vector3(2.4f, 6.2f, 0.18f), art.StageCurtain, false);
            Box("CurtainDrape_Right", stage, new Vector3(20.2f, 3.8f, 46.25f), new Vector3(2.4f, 6.2f, 0.18f), art.StageCurtain, false);

            // Podium / Lectern on Stage
            Transform podium = Group("StagePodium", stage);
            podium.position = new Vector3(12f, 0.9f, 47.5f);
            Box("PodiumBase", podium, new Vector3(0f, 0.6f, 0f), new Vector3(0.75f, 1.2f, 0.6f), art.OakWood);
            Box("PodiumSlantTop", podium, new Vector3(0f, 1.22f, -0.05f), new Vector3(0.8f, 0.1f, 0.65f), art.OakWood, false);
            Box("MicrophoneGooseneck", podium, new Vector3(0f, 1.45f, 0.05f), new Vector3(0.03f, 0.35f, 0.03f), art.MetalSteel, false);

            // Grand Piano in Stage Right Wing
            Transform piano = Group("StageGrandPiano", stage);
            piano.position = new Vector3(19f, 0.9f, 51.5f);
            Box("PianoBody", piano, new Vector3(0f, 0.5f, 0f), new Vector3(1.6f, 0.5f, 2.2f), art.LabEpoxy);
            Box("PianoLid", piano, new Vector3(0f, 1.05f, 0f), new Vector3(1.5f, 0.65f, 2.0f), art.LabEpoxy, false);
            Box("PianoBench", piano, new Vector3(0f, 0.25f, -1.4f), new Vector3(0.75f, 0.5f, 0.35f), art.OakWood);

            // Auditorium Audience Seating (Rows of Theater Chairs with armrests)
            Transform seating = Group("AuditoriumSeating", parent);
            for (int row = 0; row < 6; row++)
            {
                float z = 41.5f - row * 2.8f;
                // Left seating bank (X: 3 to 10)
                for (int s = 0; s < 5; s++)
                {
                    float x = 4.2f + s * 1.3f;
                    BuildTheaterSeat(seating, $"Seat_L_{row}_{s}", new Vector3(x, 0f, z), art);
                }
                // Right seating bank (X: 14 to 21)
                for (int s = 0; s < 5; s++)
                {
                    float x = 14.8f + s * 1.3f;
                    BuildTheaterSeat(seating, $"Seat_R_{row}_{s}", new Vector3(x, 0f, z), art);
                }
            }

            // Sound & Lighting Tech Booth at rear (Z: 23f)
            Transform techBooth = Group("SoundTechBooth", parent);
            techBooth.position = new Vector3(12f, 0f, 22.5f);
            Box("TechDesk", techBooth, new Vector3(0f, 0.45f, 0f), new Vector3(3.6f, 0.9f, 0.8f), art.OakWood);
            Box("MixingConsole", techBooth, new Vector3(0f, 0.95f, 0f), new Vector3(1.8f, 0.15f, 0.6f), art.MetalSteel, false);
            Box("SoundRack", techBooth, new Vector3(1.4f, 0.9f, 0f), new Vector3(0.7f, 1.4f, 0.6f), art.MetalSteel);
        }

        private static void BuildTheaterSeat(Transform parent, string name, Vector3 pos, BurleySchoolArt art)
        {
            Transform chair = Group(name, parent);
            chair.position = pos;
            Box("Legs", chair, new Vector3(0f, 0.22f, 0f), new Vector3(0.5f, 0.44f, 0.4f), art.MetalSteel);
            Box("Cushion", chair, new Vector3(0f, 0.46f, 0f), new Vector3(0.55f, 0.12f, 0.45f), art.TheaterSeat);
            Box("Backrest", chair, new Vector3(0f, 0.78f, -0.18f), new Vector3(0.54f, 0.58f, 0.1f), art.TheaterSeat);
            Box("Armrest_L", chair, new Vector3(-0.3f, 0.6f, 0f), new Vector3(0.08f, 0.08f, 0.35f), art.OakWood, false);
            Box("Armrest_R", chair, new Vector3(0.3f, 0.6f, 0f), new Vector3(0.08f, 0.08f, 0.35f), art.OakWood, false);
        }

        // =========================================================================
        // 3. CAFETERIA & COMMERCIAL KITCHEN
        // =========================================================================
        private static void BuildCafeteriaAndKitchen(Transform parent, BurleySchoolArt art)
        {
            // Dining Hall (X: 26 to 52, Z: 54 to 78)
            Transform tables = Group("CafeteriaTables", parent);
            for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
            {
                float x = 28f + c * 7.5f;
                float z = 56f + r * 6.5f;
                BuildCafeteriaTable(tables, $"DiningTable_{r}_{c}", new Vector3(x, 0f, z), art);
            }

            // Cafeteria Menu Board
            Box("MenuBoardFrame", parent, new Vector3(44f, 2.2f, 52.2f), new Vector3(4.8f, 1.2f, 0.05f), art.OakWood, false);
            Box("MenuBoardBlack", parent, new Vector3(44f, 2.2f, 52.23f), new Vector3(4.6f, 1.05f, 0.03f), art.Chalkboard, false);
            Label(parent, "MenuText", "BURLEY CAFETERIA SPECIAL\nSALISBURY STEAK  //  MASHED POTATOES\nGREEN BEANS  //  CHILLED PEACHES  //  MILK",
                new Vector3(44f, 2.2f, 52.26f), Quaternion.identity, 0.065f, art.GoldAccent.GetColor("_BaseColor"));

            // Food Serving Counter & Tray Slide (Z: 70f, X: 52 to 64)
            Transform counter = Group("ServingCounter", parent);
            counter.position = new Vector3(58f, 0f, 70f);
            Box("CounterBase", counter, new Vector3(0f, 0.45f, 0f), new Vector3(8.5f, 0.9f, 0.9f), art.MetalSteel);
            Box("TraySlideRail", counter, new Vector3(0f, 0.85f, -0.55f), new Vector3(8.5f, 0.06f, 0.25f), art.MetalSteel, false);
            Box("SneezeGuardGlass", counter, new Vector3(0f, 1.25f, -0.4f), new Vector3(8.0f, 0.65f, 0.02f), art.WindowGlass, false);

            // Commercial Kitchen (X: 52 to 66, Z: 71 to 79)
            Transform kitchen = Group("CommercialKitchen", parent);
            // Industrial Cooking Range & Ovens
            Box("StoveRange", kitchen, new Vector3(56f, 0.5f, 76.5f), new Vector3(4.2f, 1.0f, 1.2f), art.MetalSteel);
            Box("ExhaustHood", kitchen, new Vector3(56f, 2.5f, 76.5f), new Vector3(4.6f, 0.8f, 1.6f), art.MetalSteel, false);
            // Triple-compartment Stainless Wash Sink
            Box("KitchenSink", kitchen, new Vector3(63f, 0.5f, 76.5f), new Vector3(2.6f, 1.0f, 0.9f), art.MetalSteel);
            // Commercial Refrigerator
            Box("CommercialFridge", kitchen, new Vector3(64.5f, 1.1f, 73f), new Vector3(1.2f, 2.2f, 1.0f), art.MetalSteel);
            // Prep Table
            Box("PrepTable", kitchen, new Vector3(59f, 0.45f, 73f), new Vector3(3.2f, 0.9f, 1.1f), art.MetalSteel);
        }

        private static void BuildCafeteriaTable(Transform parent, string name, Vector3 pos, BurleySchoolArt art)
        {
            Transform table = Group(name, parent);
            table.position = pos;
            // Center tabletop
            Box("Top", table, new Vector3(0f, 0.72f, 0f), new Vector3(5.0f, 0.06f, 1.1f), art.FloorTile);
            Box("LegFrame", table, new Vector3(0f, 0.35f, 0f), new Vector3(4.6f, 0.7f, 0.9f), art.MetalSteel);
            // Attached round stool seats along both sides
            for (int s = -2; s <= 2; s++)
            {
                float sx = s * 1.0f;
                Box($"Stool_N_{s + 2}", table, new Vector3(sx, 0.42f, 0.82f), new Vector3(0.35f, 0.06f, 0.35f), art.LockerGreen);
                Box($"Stool_S_{s + 2}", table, new Vector3(sx, 0.42f, -0.82f), new Vector3(0.35f, 0.06f, 0.35f), art.LockerGreen);
            }
        }

        // =========================================================================
        // 4. LIBRARY & MEDIA CENTER (Upper Floor)
        // =========================================================================
        private static void BuildLibraryAndMediaCenter(Transform parent, BurleySchoolArt art)
        {
            // Located Upper Floor (y: 4.0m, X: 26 to 64, Z: 54 to 78)
            const float y = L.StoryHeight;

            // Wooden Double-Sided Bookshelf Stacks
            Transform stacks = Group("BookcaseStacks", parent);
            for (int row = 0; row < 4; row++)
            {
                float z = 57f + row * 4.8f;
                Transform stack = Group($"BookStack_{row}", stacks);
                stack.position = new Vector3(36f, y, z);
                Box("WoodShelving", stack, new Vector3(0f, 1.25f, 0f), new Vector3(12.0f, 2.5f, 0.75f), art.OakWood);
                // Colorful book rows
                for (int b = -4; b <= 4; b++)
                {
                    float bx = b * 1.2f;
                    Box($"Books_N_{b + 4}", stack, new Vector3(bx, 1.2f, 0.4f), new Vector3(0.95f, 0.45f, 0.15f), (b % 2 == 0) ? art.LockerGreen : art.GoldAccent, false);
                    Box($"Books_S_{b + 4}", stack, new Vector3(bx, 1.2f, -0.4f), new Vector3(0.95f, 0.45f, 0.15f), (b % 2 == 0) ? art.TheaterSeat : art.BrickDark, false);
                }
            }

            // Study Tables with Library Chairs
            Transform study = Group("StudyTables", parent);
            for (int t = 0; t < 3; t++)
            {
                float z = 58f + t * 6.5f;
                Transform table = Group($"StudyTable_{t}", study);
                table.position = new Vector3(56f, y, z);
                Box("OakTop", table, new Vector3(0f, 0.72f, 0f), new Vector3(3.2f, 0.06f, 1.6f), art.OakWood);
                Box("OakLegs", table, new Vector3(0f, 0.35f, 0f), new Vector3(2.9f, 0.7f, 1.3f), art.OakWood);
            }

            // Computer Lab Row (Technology Station)
            Transform pcs = Group("ComputerLabRow", parent);
            pcs.position = new Vector3(48f, y, 77.2f);
            Box("ComputerDesk", pcs, new Vector3(0f, 0.38f, 0f), new Vector3(14.0f, 0.75f, 0.9f), art.OakWood);
            for (int pc = 0; pc < 6; pc++)
            {
                float cx = -5.0f + pc * 2.0f;
                Box($"Monitor_{pc}", pcs, new Vector3(cx, 0.95f, 0.15f), new Vector3(0.55f, 0.42f, 0.15f), art.MetalSteel, false);
                Box($"Keyboard_{pc}", pcs, new Vector3(cx, 0.78f, -0.2f), new Vector3(0.45f, 0.03f, 0.18f), art.FloorTile, false);
                Box($"Tower_{pc}", pcs, new Vector3(cx + 0.5f, 0.35f, 0.1f), new Vector3(0.22f, 0.45f, 0.42f), art.MetalSteel, false);
            }

            // Librarian Circulation Desk
            Transform circ = Group("CirculationDesk", parent);
            circ.position = new Vector3(28f, y, 74f);
            Box("CounterL", circ, new Vector3(0f, 0.5f, 0f), new Vector3(3.2f, 1.0f, 0.8f), art.OakWood);
            Box("CounterWing", circ, new Vector3(1.2f, 0.5f, -1.2f), new Vector3(0.8f, 1.0f, 1.6f), art.OakWood);
        }

        // =========================================================================
        // 5. CLASSROOMS
        // =========================================================================
        private static void BuildClassrooms(Transform parent, BurleySchoolArt art)
        {
            // Room 101 - Virginia History / Social Studies (Ground Floor, X: 26 to 42, Z: 22 to 32)
            BuildClassroomSuite(parent, "Classroom_101_History", new Vector3(26f, 0f, 22f), 16f, 10f,
                "MR. JACKSON  //  VIRGINIA HISTORY  //  ROOM 101\nCIVIL RIGHTS & PUBLIC EDUCATION IN CHARLOTTESVILLE", art);

            // Room 102 - Mathematics (Ground Floor, X: 50 to 66, Z: 22 to 32)
            BuildClassroomSuite(parent, "Classroom_102_Math", new Vector3(50f, 0f, 22f), 16f, 10f,
                "MRS. ROBINSON  //  ALGEBRA & GEOMETRY  //  ROOM 102\nPYTHAGOREAN THEOREM: a^2 + b^2 = c^2", art);

            // Room 203 - English Literature (Upper Floor, X: 50 to 66, Z: 22 to 32, Y: 4.0m)
            BuildClassroomSuite(parent, "Classroom_203_English", new Vector3(50f, L.StoryHeight, 22f), 16f, 10f,
                "ENGLISH LITERATURE  //  POETRY OF THE HARLEM RENAISSANCE  //  ROOM 203", art);
        }

        private static void BuildClassroomSuite(Transform parent, string name, Vector3 origin, float width, float depth, string chalkText, BurleySchoolArt art)
        {
            Transform room = Group(name, parent);
            room.position = origin;

            // Student Desks (Neat Rows facing the front wall)
            for (int row = 0; row < 3; row++)
            for (int col = 0; col < 4; col++)
            {
                float dx = 2.5f + col * 2.8f;
                float dz = 2.5f + row * 2.2f;
                Transform desk = Group($"Desk_{row}_{col}", room);
                desk.localPosition = new Vector3(dx, 0f, dz);
                // Desk top and book basket
                Box("DeskTop", desk, new Vector3(0f, 0.7f, 0f), new Vector3(0.7f, 0.04f, 0.5f), art.OakWood);
                Box("DeskFrame", desk, new Vector3(0f, 0.35f, 0f), new Vector3(0.6f, 0.68f, 0.4f), art.MetalSteel);
                // Chair attached
                Box("Seat", desk, new Vector3(0f, 0.42f, -0.45f), new Vector3(0.42f, 0.04f, 0.38f), art.OakWood);
                Box("Back", desk, new Vector3(0f, 0.72f, -0.62f), new Vector3(0.42f, 0.28f, 0.04f), art.OakWood);
            }

            // Teacher's Wooden Desk at Front
            Transform teacher = Group("TeacherDesk", room);
            teacher.localPosition = new Vector3(width * 0.5f, 0f, depth - 2.0f);
            Box("DeskBody", teacher, new Vector3(0f, 0.38f, 0f), new Vector3(1.6f, 0.76f, 0.85f), art.OakWood);
            Box("DeskDrawers", teacher, new Vector3(0.5f, 0.38f, 0f), new Vector3(0.45f, 0.7f, 0.8f), art.OakWood, false);
            Box("Chair", teacher, new Vector3(0f, 0.45f, 0.7f), new Vector3(0.55f, 0.9f, 0.55f), art.OakWood);

            // Wide Green Chalkboard with chalk lessons
            Transform chalkboard = Group("FrontChalkboard", room);
            chalkboard.localPosition = new Vector3(width * 0.5f, 1.8f, depth - 0.15f);
            Box("ChalkboardSlate", chalkboard, Vector3.zero, new Vector3(width - 4.0f, 1.35f, 0.04f), art.Chalkboard, false);
            Box("WoodChalkRail", chalkboard, new Vector3(0f, -0.68f, 0.05f), new Vector3(width - 3.8f, 0.05f, 0.12f), art.OakWood, false);
            Label(chalkboard, "ChalkText", chalkText, new Vector3(0f, 0f, 0.04f),
                Quaternion.Euler(0f, 180f, 0f), 0.07f, art.FloorTile.GetColor("_BaseColor"));

            // Bookshelf and globe
            Box("ClassroomBookshelf", room, new Vector3(1.5f, 1.0f, depth - 1.5f), new Vector3(0.9f, 2.0f, 0.4f), art.OakWood);
        }

        // =========================================================================
        // 6. SCIENCE WING & LABORATORY (Upper Floor)
        // =========================================================================
        private static void BuildScienceWing(Transform parent, BurleySchoolArt art)
        {
            // Upper Floor (y: 4.0m, X: 26 to 44, Z: 22 to 32)
            Transform lab = Group("ScienceLab_201", parent);
            lab.position = new Vector3(26f, L.StoryHeight, 22f);

            // Heavy chemical-resistant black epoxy lab benches with integral sinks
            for (int b = 0; b < 2; b++)
            {
                float z = 3.2f + b * 3.6f;
                Transform bench = Group($"LabBench_{b}", lab);
                bench.localPosition = new Vector3(9.0f, 0f, z);
                Box("EpoxyTop", bench, new Vector3(0f, 0.9f, 0f), new Vector3(11.0f, 0.08f, 1.4f), art.LabEpoxy);
                Box("CabinetBases", bench, new Vector3(0f, 0.44f, 0f), new Vector3(10.6f, 0.85f, 1.2f), art.OakWood);

                // Gas Burner Double Spigots & Sinks
                for (int sink = -2; sink <= 2; sink += 2)
                {
                    float sx = sink * 2.2f;
                    Box($"Sink_{sink}", bench, new Vector3(sx, 0.88f, 0f), new Vector3(0.6f, 0.06f, 0.5f), art.MetalSteel, false);
                    Box($"GooseneckFaucet_{sink}", bench, new Vector3(sx, 1.15f, 0.2f), new Vector3(0.04f, 0.35f, 0.04f), art.MetalSteel, false);
                    Box($"GasValve_{sink}", bench, new Vector3(sx + 0.6f, 0.98f, 0f), new Vector3(0.08f, 0.12f, 0.08f), art.GoldAccent, false);
                }
            }

            // Chemical Fume Hood with Glass Slide Sash
            Transform fumeHood = Group("ChemicalFumeHood", lab);
            fumeHood.localPosition = new Vector3(2.5f, 0f, 8.5f);
            Box("HoodBase", fumeHood, new Vector3(0f, 0.45f, 0f), new Vector3(2.2f, 0.9f, 1.2f), art.MetalSteel);
            Box("HoodChamber", fumeHood, new Vector3(0f, 1.75f, 0f), new Vector3(2.2f, 1.7f, 1.2f), art.MetalSteel);
            Box("GlassSash", fumeHood, new Vector3(0f, 1.55f, -0.58f), new Vector3(1.9f, 1.2f, 0.04f), art.WindowGlass, false);

            // Safety Eye-Wash Station & Emergency Shower
            Transform safety = Group("EyeWashStation", lab);
            safety.localPosition = new Vector3(16.5f, 0f, 8.5f);
            Box("ShowerPipe", safety, new Vector3(0f, 2.4f, 0f), new Vector3(0.06f, 1.2f, 0.06f), art.MetalSteel, false);
            Box("ShowerHead", safety, new Vector3(0f, 2.9f, -0.4f), new Vector3(0.35f, 0.08f, 0.35f), art.GoldAccent, false);
            Box("EyeWashBasin", safety, new Vector3(0f, 1.05f, -0.3f), new Vector3(0.42f, 0.2f, 0.42f), art.GoldAccent, false);

            // Periodic Table of Elements Wall Chart
            Box("PeriodicTableFrame", lab, new Vector3(9.0f, 2.2f, 9.85f), new Vector3(5.5f, 1.8f, 0.04f), art.OakWood, false);
            Box("PeriodicTableFace", lab, new Vector3(9.0f, 2.2f, 9.88f), new Vector3(5.3f, 1.65f, 0.02f), art.FloorTile, false);
            Label(lab, "PeriodicTitle", "PERIODIC TABLE OF THE ELEMENTS\nBURLEY SCIENCE DEPARTMENT",
                new Vector3(9.0f, 2.2f, 9.91f), Quaternion.Euler(0f, 180f, 0f), 0.085f, art.BrickDark.GetColor("_BaseColor"));
        }

        // =========================================================================
        // 7. ADMINISTRATION SUITE & CLINIC
        // =========================================================================
        private static void BuildAdminAndClinic(Transform parent, BurleySchoolArt art)
        {
            // Administration & Principal's Office (Ground Floor, X: 48 to 66, Z: 39 to 51)
            Transform admin = Group("PrincipalOffice", parent);
            admin.position = new Vector3(52f, 0f, 42f);

            // Executive Mahogany Desk & Chair
            Box("ExecutiveDesk", admin, new Vector3(0f, 0.4f, 0f), new Vector3(2.2f, 0.8f, 1.1f), art.OakWood);
            Box("LeatherChair", admin, new Vector3(0f, 0.55f, 0.85f), new Vector3(0.7f, 1.1f, 0.7f), art.LabEpoxy);
            Box("VisitorChair_1", admin, new Vector3(-0.6f, 0.45f, -0.9f), new Vector3(0.55f, 0.9f, 0.55f), art.OakWood);
            Box("VisitorChair_2", admin, new Vector3(0.6f, 0.45f, -0.9f), new Vector3(0.55f, 0.9f, 0.55f), art.OakWood);

            // Steel Filing Cabinets
            for (int f = 0; f < 3; f++)
                Box($"FileCabinet_{f}", admin, new Vector3(2.5f + f * 0.65f, 0.8f, 2.5f), new Vector3(0.55f, 1.6f, 0.65f), art.MetalSteel);

            // Attendance & Reception Counter with PA System Rack
            Transform recep = Group("ReceptionOffice", parent);
            recep.position = new Vector3(60f, 0f, 42f);
            Box("FrontCounter", recep, new Vector3(0f, 0.55f, 0f), new Vector3(4.8f, 1.1f, 0.8f), art.OakWood);
            Box("DividerGlass", recep, new Vector3(0f, 1.35f, 0f), new Vector3(4.4f, 0.65f, 0.04f), art.WindowGlass, false);
            // Public Address (PA) Amplifier Rack
            Box("PARack", recep, new Vector3(-1.8f, 1.15f, 0.2f), new Vector3(0.6f, 0.5f, 0.4f), art.MetalSteel, false);
            Box("PAMicrophone", recep, new Vector3(-1.8f, 1.45f, -0.1f), new Vector3(0.04f, 0.25f, 0.04f), art.MetalSteel, false);

            // School Nurse Clinic (X: 30 to 44, Z: 40 to 50)
            Transform clinic = Group("NurseClinic", parent);
            clinic.position = new Vector3(36f, 0f, 44f);
            // Examination Cot / Bed with sheets
            Box("ClinicCotFrame", clinic, new Vector3(0f, 0.3f, 0f), new Vector3(2.1f, 0.6f, 0.95f), art.MetalSteel);
            Box("ClinicMattress", clinic, new Vector3(0f, 0.65f, 0f), new Vector3(2.0f, 0.15f, 0.9f), art.FloorTile);
            Box("PrivacyCurtainScreen", clinic, new Vector3(0f, 1.2f, 0.8f), new Vector3(2.4f, 2.2f, 0.05f), art.FloorTile, false);
            // Medicine Cabinet & Height/Weight Scale
            Box("MedicineCabinet", clinic, new Vector3(-1.8f, 1.5f, 0f), new Vector3(0.8f, 1.0f, 0.35f), art.MetalSteel);
            Box("WeightScale", clinic, new Vector3(1.8f, 0.8f, 0f), new Vector3(0.45f, 1.6f, 0.45f), art.MetalSteel);
        }

        // =========================================================================
        // 8. BASEMENT BOILER ROOM & MAINTENANCE
        // =========================================================================
        private static void BuildBoilerRoom(Transform parent, BurleySchoolArt art)
        {
            // Utility Basement (y: -3.5m, X: 48 to 68, Z: 76 to 92)
            const float y = -3.5f;

            // Massive Industrial Cast-Iron Boilers
            for (int b = 0; b < 2; b++)
            {
                float x = 53f + b * 6.5f;
                Transform boiler = Group($"SteamBoiler_{b}", parent);
                boiler.position = new Vector3(x, y, 84f);
                // Boiler cylindrical/cubic furnace body
                Box("FurnaceBody", boiler, new Vector3(0f, 1.6f, 0f), new Vector3(3.6f, 3.2f, 4.2f), art.BrickDark);
                Box("CastIronFaceplate", boiler, new Vector3(0f, 1.6f, -2.15f), new Vector3(3.2f, 2.8f, 0.15f), art.MetalSteel);
                Box("FireDoor", boiler, new Vector3(0f, 0.8f, -2.25f), new Vector3(1.1f, 1.1f, 0.08f), art.BrickDark, false);
                Box("PressureGauge", boiler, new Vector3(0.8f, 2.6f, -2.25f), new Vector3(0.28f, 0.28f, 0.08f), art.GoldAccent, false);
            }

            // Overhead Steam Mains & Flue Breach to Smokestack
            Box("MainSteamHeaderPipe", parent, new Vector3(56f, y + 3.2f, 84f), new Vector3(14.0f, 0.45f, 0.45f), art.MetalSteel, false);
            Box("BoilerFlueBreach", parent, new Vector3(56f, y + 2.4f, 89f), new Vector3(1.8f, 1.8f, 6.0f), art.BrickDark);

            // Maintenance Workbench with bench vise and tool boxes
            Transform bench = Group("MaintenanceWorkbench", parent);
            bench.position = new Vector3(64f, y, 79f);
            Box("BenchTop", bench, new Vector3(0f, 0.45f, 0f), new Vector3(3.8f, 0.9f, 1.1f), art.OakWood);
            Box("BenchVise", bench, new Vector3(-1.2f, 1.0f, 0.3f), new Vector3(0.25f, 0.25f, 0.25f), art.MetalSteel, false);
            Box("ToolChest", bench, new Vector3(0.8f, 1.05f, 0f), new Vector3(0.7f, 0.35f, 0.35f), art.BrickDark, false);

            // Electrical Distribution Switchgear Panels
            for (int p = 0; p < 3; p++)
            {
                Box($"ElectricPanel_{p}", parent, new Vector3(50f + p * 1.2f, y + 1.4f, 77.2f),
                    new Vector3(0.95f, 1.8f, 0.3f), art.MetalSteel);
            }
        }

        private static void Label(Transform parent, string name, string text, Vector3 at, Quaternion rotation, float size, Color color)
        {
            var label = new GameObject(name).AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.localPosition = at;
            label.transform.localRotation = rotation;
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = size;
            label.fontSize = 48;
            label.color = color;
            WorldTextMaterial.Apply(label, AssetDatabase.LoadAssetAtPath<Font>(BurleySchoolArt.FontPath));
        }
    }
}

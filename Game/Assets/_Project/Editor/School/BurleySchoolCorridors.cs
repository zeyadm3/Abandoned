using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Authors the circulation spaces and double-loaded corridors of Jackson P. Burley Middle School:
    /// Kelly green and old gold student locker banks, lighted athletics trophy showcase,
    /// directional signage, drinking fountains, clocks, fire alarms, and notice boards.
    /// </summary>
    public static class BurleySchoolCorridors
    {
        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform corridors = Group("SchoolCorridors", root);
            Transform lockers = Group("LockerBanks", corridors);
            Transform trophies = Group("AthleticsTrophyCase", corridors);
            Transform signage = Group("DirectionalSignage", corridors);
            Transform amenities = Group("CorridorAmenities", corridors);

            // ---- 1. Student Locker Banks (Kelly Green with Gold Numbers) ----
            BuildLockers(lockers, art);

            // ---- 2. Athletics Trophy Showcase ----
            BuildTrophyCase(trophies, art);

            // ---- 3. Directional Wayfinding Signs ----
            BuildSignage(signage, art);

            // ---- 4. Corridor Amenities (Fountains, Clocks, Fire Alarms, Boards) ----
            BuildAmenities(amenities, art);

            MarkStatic(corridors);
        }

        private static void BuildLockers(Transform parent, BurleySchoolArt art)
        {
            // Ground Floor Corridors
            // South wall locker banks (facing north into hall)
            for (int bank = 0; bank < 3; bank++)
            {
                float startX = 26f + bank * 12f;
                BuildLockerBank(parent, $"LockerBank_G_S_{bank}", new Vector3(startX, 0.05f, 32.22f), 8, false, art);
            }

            // North wall locker banks (facing south into hall)
            for (int bank = 0; bank < 3; bank++)
            {
                float startX = 26f + bank * 12f;
                BuildLockerBank(parent, $"LockerBank_G_N_{bank}", new Vector3(startX, 0.05f, 37.78f), 8, true, art);
            }

            // Upper Floor Corridors
            for (int bank = 0; bank < 3; bank++)
            {
                float startX = 26f + bank * 12f;
                BuildLockerBank(parent, $"LockerBank_U_S_{bank}", new Vector3(startX, L.StoryHeight + 0.05f, 32.22f), 8, false, art);
                BuildLockerBank(parent, $"LockerBank_U_N_{bank}", new Vector3(startX, L.StoryHeight + 0.05f, 37.78f), 8, true, art);
            }
        }

        private static void BuildLockerBank(Transform parent, string name, Vector3 origin, int count, bool facingSouth, BurleySchoolArt art)
        {
            Transform bank = Group(name, parent);
            float width = count * 0.45f;
            float rotY = facingSouth ? 180f : 0f;
            bank.position = origin;
            bank.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Steel outer casing
            Box("Casing", bank, new Vector3(width * 0.5f, 0.95f, 0.25f), new Vector3(width, 1.9f, 0.5f), art.LockerGreen);
            Box("BasePlinth", bank, new Vector3(width * 0.5f, 0.06f, 0.25f), new Vector3(width, 0.12f, 0.52f), art.BrickDark);

            // Individual locker doors with air vents and combination dials
            for (int i = 0; i < count; i++)
            {
                float lx = (i + 0.5f) * 0.45f;
                // Door face
                Box($"Door_{i}", bank, new Vector3(lx, 0.96f, 0.51f), new Vector3(0.42f, 1.76f, 0.02f), art.LockerGreen, false);
                // Gold dial and handle
                Box($"Dial_{i}", bank, new Vector3(lx + 0.14f, 1.05f, 0.53f), new Vector3(0.04f, 0.04f, 0.03f), art.GoldAccent, false);
                Box($"Handle_{i}", bank, new Vector3(lx + 0.14f, 0.95f, 0.54f), new Vector3(0.02f, 0.12f, 0.03f), art.MetalSteel, false);
                // Ventilation louvers at top and bottom
                Box($"Louver_Top_{i}", bank, new Vector3(lx, 1.65f, 0.52f), new Vector3(0.24f, 0.08f, 0.015f), art.BrickDark, false);
                Box($"Louver_Bot_{i}", bank, new Vector3(lx, 0.35f, 0.52f), new Vector3(0.24f, 0.08f, 0.015f), art.BrickDark, false);
            }
        }

        private static void BuildTrophyCase(Transform parent, BurleySchoolArt art)
        {
            // Positioned prominently in the Central Academic Foyer (Ground Floor, facing entry)
            Vector3 center = new(46f, 1.6f, 31.85f);
            Transform caseRoot = Group("MainAthleticsShowcase", parent);
            caseRoot.position = center;

            // Wood display cabinet casing
            Box("CabinetBack", caseRoot, Vector3.zero, new Vector3(7.2f, 3.0f, 0.6f), art.OakWood);
            Box("GreenFeltBackdrop", caseRoot, new Vector3(0f, 0f, 0.25f), new Vector3(6.8f, 2.6f, 0.02f), art.LockerGreen, false);
            Box("GlassSlidingDoor", caseRoot, new Vector3(0f, 0f, 0.32f), new Vector3(6.9f, 2.7f, 0.04f), art.WindowGlass, false);

            // Display Shelves (Oak Wood with Gold Trim)
            for (int shelf = -1; shelf <= 1; shelf++)
            {
                Box($"Shelf_{shelf + 1}", caseRoot, new Vector3(0f, shelf * 0.75f, 0.15f), new Vector3(6.7f, 0.05f, 0.42f), art.OakWood, false);
                Box($"ShelfTrim_{shelf + 1}", caseRoot, new Vector3(0f, shelf * 0.75f, 0.36f), new Vector3(6.72f, 0.06f, 0.02f), art.GoldAccent, false);
            }

            // Trophies on display
            for (int i = 0; i < 6; i++)
            {
                float tx = -2.5f + i * 1.0f;
                float ty = (i % 2 == 0) ? -0.7f : 0.05f;
                Transform cup = Group($"Trophy_{i}", caseRoot);
                cup.localPosition = new Vector3(tx, ty, 0.18f);

                // Pedestal Base
                Box("Base", cup, Vector3.zero, new Vector3(0.22f, 0.12f, 0.22f), art.OakWood, false);
                // Gold Cup Column & Bowl
                Box("Column", cup, new Vector3(0f, 0.16f, 0f), new Vector3(0.08f, 0.2f, 0.08f), art.GoldAccent, false);
                Box("Bowl", cup, new Vector3(0f, 0.34f, 0f), new Vector3(0.24f, 0.18f, 0.24f), art.GoldAccent, false);
                Box("Handles", cup, new Vector3(0f, 0.34f, 0f), new Vector3(0.36f, 0.08f, 0.04f), art.GoldAccent, false);
            }

            // Large Centerpiece Championship Trophy (1951 State Champions)
            Transform champTrophy = Group("StateChampionship1951", caseRoot);
            champTrophy.localPosition = new Vector3(0f, 0.1f, 0.18f);
            Box("BigBase", champTrophy, Vector3.zero, new Vector3(0.38f, 0.18f, 0.32f), art.OakWood, false);
            Box("BigPlaque", champTrophy, new Vector3(0f, 0.02f, 0.17f), new Vector3(0.26f, 0.1f, 0.02f), art.GoldAccent, false);
            Box("BigChalice", champTrophy, new Vector3(0f, 0.32f, 0f), new Vector3(0.32f, 0.45f, 0.32f), art.GoldAccent, false);

            // Showcase Header Title
            Box("ShowcaseHeaderPlate", caseRoot, new Vector3(0f, 1.25f, 0.33f), new Vector3(6.8f, 0.38f, 0.03f), art.LockerGreen, false);
            Label(caseRoot, "TrophyTitle", "JACKSON P. BURLEY ATHLETICS  //  HOME OF THE BEARS",
                new Vector3(0f, 1.25f, 0.36f), Quaternion.identity, 0.10f, art.GoldAccent.GetColor("_BaseColor"));
        }

        private static void BuildSignage(Transform parent, BurleySchoolArt art)
        {
            // Main Corridor Wayfinding Signs suspended from ceiling
            Vector3[] signPositions =
            {
                new(30f, 3.1f, 35f),
                new(46f, 3.1f, 35f),
                new(62f, 3.1f, 35f),
                new(46f, 7.1f, 35f)
            };

            string[] texts =
            {
                "<-- AUDITORIUM & MUSIC    |    MAIN ENTRANCE -->",
                "<-- WEST WING    |    GYMNASIUM & ATHLETICS -->",
                "--> GYMNASIUM & ATHLETIC FIELDS    |    CAFETERIA ^",
                "SCIENCE LABS 201-202    |    LIBRARY & MEDIA CENTER ^"
            };

            for (int i = 0; i < signPositions.Length; i++)
            {
                Transform sign = Group($"WayfindingSign_{i}", parent);
                sign.position = signPositions[i];

                // Sign board (Green with Gold border)
                Box("SignBoard", sign, Vector3.zero, new Vector3(3.6f, 0.45f, 0.06f), art.LockerGreen, false);
                Box("SignTrim", sign, Vector3.zero, new Vector3(3.7f, 0.49f, 0.04f), art.GoldAccent, false);

                // Hanging chains
                Box("Hanger_L", sign, new Vector3(-1.4f, 0.45f, 0f), new Vector3(0.02f, 0.9f, 0.02f), art.MetalSteel, false);
                Box("Hanger_R", sign, new Vector3(1.4f, 0.45f, 0f), new Vector3(0.02f, 0.9f, 0.02f), art.MetalSteel, false);

                // Text on both sides
                Label(sign, "FrontText", texts[i], new Vector3(0f, 0f, -0.04f), Quaternion.identity, 0.07f, Color.white);
                Label(sign, "BackText", texts[i], new Vector3(0f, 0f, 0.04f), Quaternion.Euler(0f, 180f, 0f), 0.07f, Color.white);
            }
        }

        private static void BuildAmenities(Transform parent, BurleySchoolArt art)
        {
            // Drinking Water Fountains (recessed in hallway alcoves)
            Vector3[] fountainLocs = { new(28f, 0.9f, 32.15f), new(58f, 0.9f, 32.15f), new(38f, 4.9f, 32.15f) };
            for (int f = 0; f < fountainLocs.Length; f++)
            {
                Transform fountain = Group($"WaterFountain_{f}", parent);
                fountain.position = fountainLocs[f];
                Box("Basin", fountain, Vector3.zero, new Vector3(0.55f, 0.65f, 0.42f), art.MetalSteel);
                Box("Spigot", fountain, new Vector3(0f, 0.38f, 0.1f), new Vector3(0.06f, 0.12f, 0.15f), art.MetalSteel, false);
                Box("SplashTile", fountain, new Vector3(0f, 0.3f, -0.01f), new Vector3(0.7f, 1.1f, 0.02f), art.FloorTile, false);
            }

            // Hallway Cork Bulletin Boards
            Vector3[] boardLocs = { new(34f, 1.8f, 32.15f), new(52f, 1.8f, 32.15f), new(42f, 5.8f, 32.15f) };
            string[] boardTitles = { "BURLEY BEARS FALL SCHEDULE", "BURLEY MIDDLE SCHOOL NOTICES", "ACADEMIC EXCELLENCE" };
            for (int b = 0; b < boardLocs.Length; b++)
            {
                Transform board = Group($"BulletinBoard_{b}", parent);
                board.position = boardLocs[b];
                Box("CorkFace", board, Vector3.zero, new Vector3(2.4f, 1.2f, 0.04f), art.OakWood, false);
                Box("WoodFrame", board, Vector3.zero, new Vector3(2.52f, 1.32f, 0.02f), art.BrickDark, false);

                // Pinned paper announcements
                Box("Paper_1", board, new Vector3(-0.6f, 0.15f, 0.03f), new Vector3(0.4f, 0.5f, 0.01f), art.FloorTile, false);
                Box("Paper_2", board, new Vector3(0.1f, -0.1f, 0.03f), new Vector3(0.45f, 0.35f, 0.01f), art.FloorTile, false);
                Box("Paper_3", board, new Vector3(0.7f, 0.2f, 0.03f), new Vector3(0.38f, 0.45f, 0.01f), art.GoldAccent, false);

                Label(board, "BoardTitle", boardTitles[b], new Vector3(0f, 0.5f, 0.03f), Quaternion.identity, 0.065f, art.BrickDark.GetColor("_BaseColor"));
            }

            // Wall Clocks
            Vector3[] clockLocs = { new(36f, 2.7f, 32.15f), new(56f, 2.7f, 32.15f), new(46f, 6.7f, 32.15f) };
            for (int c = 0; c < clockLocs.Length; c++)
            {
                Transform clock = Group($"WallClock_{c}", parent);
                clock.position = clockLocs[c];
                Box("Bezel", clock, Vector3.zero, new Vector3(0.38f, 0.38f, 0.04f), art.BrickDark, false);
                Box("Face", clock, new Vector3(0f, 0f, 0.025f), new Vector3(0.32f, 0.32f, 0.02f), art.FloorTile, false);
                Box("Hands", clock, new Vector3(0f, 0f, 0.04f), new Vector3(0.12f, 0.12f, 0.01f), art.BrickDark, false);
            }

            // Fire Alarm Pull Stations & Emergency Bells
            Vector3[] alarmLocs = { new(26f, 1.4f, 32.15f), new(64f, 1.4f, 32.15f), new(46f, 5.4f, 32.15f) };
            for (int a = 0; a < alarmLocs.Length; a++)
            {
                Transform alarm = Group($"FireAlarm_{a}", parent);
                alarm.position = alarmLocs[a];
                Box("PullBox", alarm, Vector3.zero, new Vector3(0.16f, 0.22f, 0.06f), art.BrickDark, false);
                Box("PullHandle", alarm, new Vector3(0f, -0.03f, 0.04f), new Vector3(0.08f, 0.05f, 0.03f), art.FloorTile, false);
                Box("Bell", alarm, new Vector3(0f, 0.8f, 0.04f), new Vector3(0.25f, 0.25f, 0.08f), art.BrickDark, false);
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

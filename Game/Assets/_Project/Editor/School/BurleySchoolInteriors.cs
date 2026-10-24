using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.BurleySchoolFurniture;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Room-scale landmarks, usable furniture and abandoned facilities inside Burley.</summary>
    public static class BurleySchoolInteriors
    {
        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform interiors = Group("SchoolInteriors", root);
            BuildGymnasium(Group("Gymnasium", interiors), art);
            BuildAuditorium(Group("Auditorium", interiors), art);
            BuildCafeteriaAndKitchen(Group("CafeteriaAndKitchen", interiors), art);
            BuildLibraryAndMediaCenter(Group("LibraryAndMediaCenter", interiors), art);
            BurleySchoolRoomDetails.Build(interiors, art);
            MarkStatic(interiors);
        }

        private static void BuildGymnasium(Transform parent, BurleySchoolArt art)
        {
            Vector3 center = new(84f, 0.018f, 44f);
            foreach (float side in new[] { -1f, 1f })
            {
                Box($"Baseline_{side}", parent, center + Vector3.forward * side * 18f,
                    new Vector3(20f, 0.012f, 0.1f), art.WhiteStripe, false);
                Box($"Sideline_{side}", parent, center + Vector3.right * side * 10f,
                    new Vector3(0.1f, 0.012f, 36f), art.WhiteStripe, false);
                Vector3 key = center + Vector3.forward * side * 15.1f;
                Box($"KeyPaint_{side}", parent, key, new Vector3(3.6f, 0.01f, 5.8f), art.LockerGreen, false);
                F.Ring(parent, $"FreeThrowCircle_{side}", center + Vector3.forward * side * 12.2f,
                    1.8f, 0.065f, art.WhiteStripe);
                BuildBasketballHoop(parent, $"Hoop_{side}", new Vector3(84f, 0f, side < 0f ? 22.5f : 65.5f), side < 0f, art);
            }
            Box("HalfCourtLine", parent, center, new Vector3(20f, 0.012f, 0.08f), art.WhiteStripe, false);
            F.Ring(parent, "CenterCourtRing", center + Vector3.up * 0.004f, 2.2f, 0.1f, art.GoldAccent);
            Label(parent, "CenterCourtLettering", "PINE HOLLOW\nWOLVES", center + Vector3.up * 0.025f,
                Quaternion.Euler(90f, 0f, 0f), 0.13f, new Color(0.74f, 0.61f, 0.2f));

            // A break in the bleachers preserves the gym door's full carry route.
            foreach ((float z, float length) bank in new[] { (28f, 11f), (51f, 24f) })
            {
                Transform bleachers = Group($"RetractableBleachers_{bank.z}", parent);
                bleachers.position = new Vector3(70f, 0f, bank.z);
                for (int tier = 0; tier < 5; tier++)
                {
                    float x = 2.8f - tier * 0.58f;
                    float y = 0.35f + tier * 0.32f;
                    Box($"SeatPlank_{tier}", bleachers, new Vector3(x, y, 0f), new Vector3(0.5f, 0.085f, bank.length), art.OakWood);
                    Box($"FootDeck_{tier}", bleachers, new Vector3(x + 0.22f, y - 0.22f, 0f),
                        new Vector3(0.48f, 0.07f, bank.length), art.StageWood, false);
                    for (int leg = 0; leg < 5; leg++)
                        Box($"Frame_{tier}_{leg}", bleachers, new Vector3(x, y * 0.5f, -bank.length * 0.45f + leg * bank.length * 0.225f),
                            new Vector3(0.06f, y, 0.08f), art.MetalSteel, false);
                }
                var envelope = bleachers.gameObject.AddComponent<BoxCollider>();
                envelope.center = new Vector3(1.65f, 0.8f, 0f);
                envelope.size = new Vector3(3f, 1.6f, bank.length);
            }

            Transform scoreboard = Group("ElectronicScoreboard", parent);
            scoreboard.position = new Vector3(99.75f, 5.5f, 44f);
            Box("RivetedHousing", scoreboard, Vector3.zero, new Vector3(0.26f, 2.2f, 5.2f), art.MetalSteel, false);
            Box("DeadDisplay", scoreboard, new Vector3(-0.15f, 0f, 0f), new Vector3(0.025f, 2f, 5f), art.LabEpoxy, false);
            Label(scoreboard, "ScoreFace", "WOLVES     GUEST\n58      52\nPERIOD 4", new Vector3(-0.175f, 0f, 0f),
                Quaternion.Euler(0f, -90f, 0f), 0.12f, new Color(0.68f, 0.22f, 0.12f));
            F.Tube(scoreboard, "FeedConduit", new Vector3(0f, 1.1f, 0f), new Vector3(0f, 2.35f, 0f), 0.045f, art.MetalSteel);

            for (int i = 0; i < 5; i++)
            {
                float z = 25f + i * 9f;
                Box($"RoofTrussLower_{i}", parent, new Vector3(84f, 6.95f, z), new Vector3(31f, 0.14f, 0.14f), art.MetalSteel, false);
                for (int b = 0; b < 6; b++)
                    F.Tube(parent, $"TrussWeb_{i}_{b}", new Vector3(69f + b * 5f, 6.95f, z),
                        new Vector3(71.5f + b * 5f, 7.75f, z), 0.065f, art.MetalSteel);
                Box($"AcousticWallPanel_{i}", parent, new Vector3(99.7f, 3.5f, z),
                    new Vector3(0.14f, 2.4f, 3.5f), art.StageCurtain, false);
            }
            for (int i = 0; i < 3; i++)
            {
                float z = 32f + i * 11f;
                Box($"ChampionshipBanner_{i}", parent, new Vector3(69.2f, 5.8f, z),
                    new Vector3(0.035f, 1.75f, 1.2f), art.LockerGreen, false);
                Label(parent, $"BannerText_{i}", "WOLVES\n" + (1956 + i * 2), new Vector3(69.23f, 5.8f, z),
                    Quaternion.Euler(0f, 90f, 0f), 0.08f, new Color(0.75f, 0.62f, 0.24f));
            }
            for (int i = 0; i < 4; i++)
                Box($"ImpactPad_{i}", parent, new Vector3(99.75f, 1.1f, 33f + i * 6f),
                    new Vector3(0.18f, 1.9f, 2f), art.LockerGreen, false);
            F.Wear(parent, new Vector3(96.5f, 0f, 57f), new Vector2(3f, 4f), 23, art);
        }

        private static void BuildBasketballHoop(Transform parent, string name, Vector3 at, bool facingNorth, BurleySchoolArt art)
        {
            Transform hoop = Group(name, parent);
            hoop.position = at;
            hoop.rotation = Quaternion.Euler(0f, facingNorth ? 0f : 180f, 0f);
            F.Tube(hoop, "SuspensionA", new Vector3(-0.55f, 7.4f, -0.5f), new Vector3(-0.55f, 3.9f, 0f), 0.08f, art.MetalSteel);
            F.Tube(hoop, "SuspensionB", new Vector3(0.55f, 7.4f, -0.5f), new Vector3(0.55f, 3.9f, 0f), 0.08f, art.MetalSteel);
            Box("Backboard", hoop, new Vector3(0f, 3.45f, 0f), new Vector3(1.8f, 1.05f, 0.04f), art.WindowGlass, false);
            Box("FrameTop", hoop, new Vector3(0f, 3.98f, 0f), new Vector3(1.85f, 0.045f, 0.07f), art.WhiteStripe, false);
            Box("FrameBottom", hoop, new Vector3(0f, 2.93f, 0f), new Vector3(1.85f, 0.045f, 0.07f), art.WhiteStripe, false);
            F.Ring(hoop, "SteelRim", new Vector3(0f, 3.05f, 0.32f), 0.23f, 0.035f, art.TrackRubber, 20);
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * 0.2f;
                F.Tube(hoop, $"NetStrand_{i}", new Vector3(Mathf.Cos(a) * 0.22f, 3.03f, 0.32f + Mathf.Sin(a) * 0.22f),
                    new Vector3(Mathf.Cos(a + 0.3f) * 0.15f, 2.65f, 0.32f + Mathf.Sin(a + 0.3f) * 0.15f), 0.012f, art.WhiteStripe);
            }
        }

        private static void BuildAuditorium(Transform parent, BurleySchoolArt art)
        {
            Transform stage = Group("PerformanceStage", parent);
            stage.position = new Vector3(12f, 0f, 52f);
            Box("StageBase", stage, new Vector3(0f, 0.55f, 0f), new Vector3(23.6f, 1.1f, 7.8f), art.StageWood);
            Box("ProsceniumHeader", stage, new Vector3(0f, 6f, -3.9f), new Vector3(23.8f, 1.7f, 0.55f), art.PlasterWall, false);
            foreach (float side in new[] { -1f, 1f })
            {
                Box($"ProsceniumPier_{side}", stage, new Vector3(side * 10.9f, 3.25f, -3.85f),
                    new Vector3(0.65f, 6.5f, 0.55f), art.Limestone);
                for (int fold = 0; fold < 9; fold++)
                    Box($"VelvetFold_{side}_{fold}", stage, new Vector3(side * (7.2f + fold * 0.43f), 3.45f, -3.35f + Mathf.Sin(fold * 1.5f) * 0.12f),
                        new Vector3(0.46f, 4.6f - fold * 0.035f, 0.22f), art.StageCurtain, false);
                BuildStageSteps(stage, side, art);
            }
            for (int fold = 0; fold < 32; fold++)
                Box($"ValanceFold_{fold}", stage, new Vector3(-11.2f + fold * 0.72f, 5.45f, -3.4f),
                    new Vector3(0.75f, 0.72f + Mathf.Sin(fold * 0.5f) * 0.12f, 0.2f), art.StageCurtain, false);
            // The piano space is deliberately empty: the real jackpot is spawned here by gameplay.
            Box("PianoFootprintWear", stage, new Vector3(7.5f, 1.108f, -0.5f), new Vector3(2.3f, 0.009f, 2.7f), art.OakWood, false);
            F.Table(stage, "FixedProjectionBench", new Vector3(0f, 1.1f, -3f), 2.2f, 0.8f, 0.7f, art.StageWood, art);
            for (int i = 0; i < 4; i++)
            {
                Box($"SceneryFlat_{i}", stage, new Vector3(-6f + i * 3.7f, 2.5f, 3.3f),
                    new Vector3(2.8f, 2.8f, 0.12f), i % 2 == 0 ? art.PlasterWall : art.StageCurtain);
                F.Tube(stage, $"FlyBar_{i}", new Vector3(-10f, 7.25f, -2.5f + i * 1.5f),
                    new Vector3(10f, 7.25f, -2.5f + i * 1.5f), 0.08f, art.MetalSteel);
            }
            Transform seating = Group("AudienceSeating", parent);
            for (int row = 0; row < 7; row++)
            for (int col = 0; col < 16; col++)
            {
                if (col == 7 || col == 8 || (row == 3 && col == 2) || (row == 5 && col == 12)) continue;
                Transform seat = Group($"BoltedSeat_{row}_{col}", seating);
                seat.position = new Vector3(1.8f + col * 1.35f, 0f, 24f + row * 3f);
                Box("Pedestal", seat, new Vector3(0f, 0.23f, 0f), new Vector3(0.14f, 0.46f, 0.2f), art.MetalSteel, false);
                Box("FloorAnchor", seat, new Vector3(0f, 0.035f, 0f), new Vector3(0.4f, 0.07f, 0.38f), art.MetalSteel, false);
                Box("Cushion", seat, new Vector3(0f, 0.46f, 0f), new Vector3(0.63f, 0.13f, 0.56f), art.TheaterSeat, false);
                GameObject back = Box("Backrest", seat, new Vector3(0f, 0.82f, -0.23f), new Vector3(0.64f, 0.7f, 0.11f), art.TheaterSeat, false);
                back.transform.localRotation = Quaternion.Euler(row == 4 && col == 5 ? -28f : -8f, 0f, 0f);
                foreach (float side in new[] { -1f, 1f })
                    Box($"Arm_{side}", seat, new Vector3(side * 0.37f, 0.62f, -0.02f),
                        new Vector3(0.08f, 0.07f, 0.51f), art.OakWood, false);
                var solid = seat.gameObject.AddComponent<BoxCollider>();
                solid.center = new Vector3(0f, 0.54f, 0f);
                solid.size = new Vector3(0.83f, 1.08f, 0.68f);
            }
            for (int i = 0; i < 5; i++)
                Box($"WallAcousticPanel_{i}", parent, new Vector3(0.2f, 4.2f, 25f + i * 5f),
                    new Vector3(0.1f, 1.8f, 2.4f), art.StageCurtain, false);
            F.Wear(parent, new Vector3(20.8f, 0f, 29.2f), new Vector2(2f, 3f), 18, art);
        }

        private static void BuildStageSteps(Transform stage, float side, BurleySchoolArt art)
        {
            Transform stair = Group($"StageSteps_{side}", stage);
            stair.localPosition = new Vector3(side * 9.5f, 0f, -3.9f);
            const float run = 2.4f;
            for (int i = 0; i < 6; i++)
                Box($"Tread_{i}", stair, new Vector3(0f, (i + 1) * 1.1f / 6f - 0.05f, -run + (i + 0.5f) * 0.4f),
                    new Vector3(2.4f, 0.1f, 0.4f), art.StageWood, false);
            GameObject ramp = Box("Ramp", stair, new Vector3(0f, 0.55f, -run * 0.5f),
                new Vector3(2.4f, 0.12f, Mathf.Sqrt(run * run + 1.21f)), art.StageWood);
            ramp.transform.localRotation = Quaternion.Euler(-Mathf.Atan2(1.1f, run) * Mathf.Rad2Deg, 0f, 0f);
            var link = stair.gameObject.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            link.startPoint = new Vector3(0f, 0.05f, -2.9f);
            link.endPoint = new Vector3(0f, 1.15f, 0.5f);
            link.width = 2f;
            link.bidirectional = true;
        }

        private static void BuildCafeteriaAndKitchen(Transform parent, BurleySchoolArt art)
        {
            Transform dining = Group("CafeteriaDining", parent);
            for (int r = 0; r < 3; r++)
            for (int c = 0; c < 2; c++)
            {
                Transform table = F.Table(dining, $"FoldingTable_{r}_{c}", new Vector3(31f + c * 10f, 0f, 58f + r * 7.5f),
                    6.5f, 1.1f, 0.78f, art.OakWood, art);
                for (int s = -2; s <= 2; s++)
                foreach (float side in new[] { -1f, 1f })
                {
                    Primitive(PrimitiveType.Cylinder, $"AttachedStool_{s}_{side}", table,
                        new Vector3(s * 1.25f, 0.45f, side * 0.85f), new Vector3(0.38f, 0.035f, 0.38f), art.LockerGreen, false);
                    F.Tube(table, $"StoolStem_{s}_{side}", new Vector3(s * 1.25f, 0.12f, side * 0.85f),
                        new Vector3(s * 1.25f, 0.44f, side * 0.85f), 0.045f, art.MetalSteel);
                    F.Tube(table, $"StoolBracket_{s}_{side}", new Vector3(s * 1.25f, 0.12f, side * 0.85f),
                        new Vector3(s * 1.25f, 0.3f, side * 0.4f), 0.045f, art.MetalSteel);
                }
            }
            Transform counter = Group("ServiceCounter", parent);
            counter.position = new Vector3(53f, 0f, 68f);
            Box("CounterBase", counter, new Vector3(0f, 0.45f, 0f), new Vector3(0.85f, 0.9f, 14f), art.MetalSteel);
            Box("TraySlide", counter, new Vector3(-0.52f, 0.82f, 0f), new Vector3(0.4f, 0.04f, 13.8f), art.MetalSteel, false);
            Box("SneezeGuard", counter, new Vector3(0f, 1.25f, 0f), new Vector3(0.025f, 0.45f, 13.8f), art.WindowGlass, false);
            for (int i = 0; i < 7; i++)
            {
                Box($"PanInset_{i}", counter, new Vector3(0f, 0.908f, -5.6f + i * 1.8f),
                    new Vector3(0.62f, 0.012f, 1.1f), art.LabEpoxy, false);
                F.Tube(counter, $"GlassPost_{i}", new Vector3(0f, 0.92f, -6f + i * 2f),
                    new Vector3(0f, 1.55f, -6f + i * 2f), 0.025f, art.MetalSteel);
            }
            Transform kitchen = Group("CommercialKitchen", parent);
            F.Table(kitchen, "PrepTable_1", new Vector3(60f, 0f, 62f), 4.5f, 1.4f, 0.95f, art.MetalSteel, art);
            F.Table(kitchen, "PrepTable_2", new Vector3(60f, 0f, 72f), 4.5f, 1.4f, 0.95f, art.MetalSteel, art);
            Transform stove = Group("BuiltInRange", kitchen);
            stove.position = new Vector3(66.5f, 0f, 66f);
            Box("RangeBody", stove, new Vector3(0f, 0.45f, 0f), new Vector3(1.2f, 0.9f, 3.2f), art.MetalSteel);
            Box("ExhaustHood", stove, new Vector3(-0.1f, 2.7f, 0f), new Vector3(1.7f, 0.65f, 3.6f), art.MetalSteel, false);
            Box("Duct", stove, new Vector3(0.15f, 3.25f, 0f), new Vector3(0.6f, 0.6f, 1.4f), art.MetalSteel, false);
            for (int i = 0; i < 4; i++)
            {
                F.Ring(stove, $"Hob_{i}", new Vector3(0f, 0.91f, -1.05f + i * 0.7f), 0.22f, 0.065f, art.BrickDark, 16);
                Box($"Control_{i}", stove, new Vector3(-0.625f, 0.72f, -1.05f + i * 0.7f),
                    new Vector3(0.06f, 0.1f, 0.1f), art.BrickDark, false);
            }
            F.Cabinet(kitchen, "PantryBase", new Vector3(63f, 0f, 77.8f), 5f, 0.85f, 0.8f, art);
            for (int shelf = 0; shelf < 3; shelf++)
                Box($"PantryWallShelf_{shelf}", kitchen, new Vector3(63f, 1.3f + shelf * 0.55f, 78f),
                    new Vector3(5f, 0.055f, 0.6f), art.MetalSteel, false);
            Label(parent, "MenuBoard", "LUNCH SERVICE\nMONDAY — FRIDAY\n11:30 — 13:00", new Vector3(25f, 2.2f, 79.8f),
                Quaternion.identity, 0.1f, new Color(0.7f, 0.68f, 0.55f));
            F.Wear(parent, new Vector3(27f, 0f, 76f), new Vector2(3f, 3f), 43, art);
        }

        private static void BuildLibraryAndMediaCenter(Transform parent, BurleySchoolArt art)
        {
            Transform checkout = F.Table(parent, "CirculationDesk", new Vector3(31f, L.StoryHeight, 57.5f),
                5.5f, 1.2f, 0.85f, art.OakWood, art);
            Box("FrontPanel", checkout, new Vector3(0f, 0.42f, -0.55f), new Vector3(5.3f, 0.76f, 0.08f), art.OakWood, false);
            Box("BookReturnSlot", checkout, new Vector3(-1.6f, 0.62f, -0.6f), new Vector3(0.7f, 0.13f, 0.025f), art.LabEpoxy, false);
            for (int row = 0; row < 4; row++)
            {
                float z = 62f + row * 4.5f;
                BuildBookStack(parent, $"WestStack_{row}", new Vector3(32f, L.StoryHeight, z), 10.8f, row, art);
                BuildBookStack(parent, $"EastStack_{row}", new Vector3(59f, L.StoryHeight, z), 10.8f, row + 4, art);
            }
            // Open spine through the middle preserves sightlines to the library exit and a full carry route.
            foreach (float z in new[] { 57.5f, 68f, 78f })
            {
                Transform table = F.Table(parent, $"ReadingTable_{z}", new Vector3(58f, L.StoryHeight, z),
                    3.4f, 1.5f, 0.8f, art.OakWood, art);
                F.Chair(table, "ChairWest", new Vector3(-2.1f, 0f, 0f), 90f, art);
                F.Chair(table, "ChairEast", new Vector3(2.1f, 0f, 0f), -90f, art);
                Box("FixedCableGrommet", table, new Vector3(0f, 0.805f, 0.25f), new Vector3(0.13f, 0.008f, 0.08f), art.BrickDark, false);
            }
            F.Radiator(parent, new Vector3(24.4f, L.StoryHeight, 58f), 90f, art);
            F.Radiator(parent, new Vector3(24.4f, L.StoryHeight, 70f), 90f, art);
            Label(parent, "LibraryWallTitle", "LIBRARY & MEDIA CENTER", new Vector3(34f, 6.8f, 79.78f),
                Quaternion.identity, 0.15f, new Color(0.28f, 0.34f, 0.23f));
            F.Wear(parent, new Vector3(26f, L.StoryHeight, 75f), new Vector2(2f, 3f), 57, art);
        }

        private static void BuildBookStack(Transform parent, string name, Vector3 at, float width, int seed, BurleySchoolArt art)
        {
            Transform stack = Group(name, parent);
            stack.position = at;
            Box("BackSpine", stack, new Vector3(0f, 1.35f, 0f), new Vector3(width, 2.7f, 0.055f), art.OakWood);
            foreach (float side in new[] { -1f, 1f })
                Box($"EndPanel_{side}", stack, new Vector3(side * width * 0.5f, 1.35f, 0f), new Vector3(0.08f, 2.7f, 1.1f), art.OakWood);
            Box("Canopy", stack, new Vector3(0f, 2.73f, 0f), new Vector3(width + 0.14f, 0.07f, 1.14f), art.OakWood, false);
            for (int shelf = 0; shelf < 5; shelf++)
            {
                float y = 0.15f + shelf * 0.5f;
                Box($"Shelf_{shelf}", stack, new Vector3(0f, y, 0f), new Vector3(width, 0.055f, 1.08f), art.OakWood);
                for (int cluster = 0; cluster < 12; cluster++)
                foreach (float side in new[] { -1f, 1f })
                {
                    if ((cluster + shelf + seed) % 5 == 0 ||
                        (shelf == 2 && (cluster == 3 || cluster == 5 || cluster == 6 || cluster == 8))) continue;
                    float x = -width * 0.5f + 0.45f + cluster * (width - 0.9f) / 11f;
                    float height = 0.25f + Mathf.Abs(Mathf.Sin(cluster * 2.7f + shelf + seed)) * 0.16f;
                    Material cover = (cluster + shelf + seed) % 3 == 0 ? art.TheaterSeat : (cluster % 3 == 1 ? art.StageCurtain : art.StageWood);
                    Box($"BoundVolumes_{shelf}_{cluster}_{side}", stack, new Vector3(x, y + height * 0.5f + 0.03f, side * 0.3f),
                        new Vector3(0.52f, height, 0.35f), cover, false);
                    for (int spine = 0; spine < 2; spine++)
                        Box($"Spine_{shelf}_{cluster}_{side}_{spine}", stack,
                            new Vector3(x - 0.17f + spine * 0.17f, y + height * 0.5f + 0.03f, side * 0.482f),
                            new Vector3(0.008f, height * 0.9f, 0.008f), art.GoldAccent, false);
                }
            }
        }
    }
}

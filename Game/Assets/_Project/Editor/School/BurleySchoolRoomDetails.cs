using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.BurleySchoolFurniture;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Academic rooms and services, arranged around the school's protected circulation.</summary>
    public static class BurleySchoolRoomDetails
    {
        public static void Build(Transform interiors, BurleySchoolArt art)
        {
            Transform classrooms = Group("AcademicClassrooms", interiors);
            Classroom(classrooms, "Classroom_101", new Vector3(32f, 0f, 26f), 0, art);
            Classroom(classrooms, "Classroom_102", new Vector3(58f, 0f, 26f), 1, art);
            Classroom(classrooms, "Classroom_203", new Vector3(58f, L.StoryHeight, 26f), 2, art);
            Science(Group("ScienceLaboratory", interiors), art);
            Administration(Group("AdministrationAndClinic", interiors), art);
            UpperStudios(Group("UpperStudiosAndRecords", interiors), art);
            Boiler(Group("BasementBoilerRoom", interiors), art);
        }

        private static void Classroom(Transform parent, string name, Vector3 at, int subject, BurleySchoolArt art)
        {
            Transform room = Group(name, parent);
            room.position = at;
            string[] subjects = { "HISTORY 101", "MATHEMATICS 102", "ENGLISH 203" };
            Transform board = Group("FrontChalkboard", room);
            // Offset the board from the door; a full-width board used to cross the classroom entrance.
            board.localPosition = new Vector3(-4.6f, 1.95f, 5.75f);
            Box("WoodFrame", board, Vector3.zero, new Vector3(4.7f, 1.8f, 0.07f), art.OakWood, false);
            Box("Slate", board, new Vector3(0f, 0f, -0.045f), new Vector3(4.52f, 1.62f, 0.025f), art.Chalkboard, false);
            Box("Tray", board, new Vector3(0f, -0.87f, -0.1f), new Vector3(4.7f, 0.05f, 0.18f), art.MetalSteel, false);
            Label(board, "Lesson", subjects[subject], new Vector3(0f, 0.45f, -0.065f), Quaternion.identity,
                0.095f, new Color(0.7f, 0.72f, 0.64f));
            for (int line = 0; line < 4; line++)
                Box($"ErasedChalk_{line}", board, new Vector3(-0.2f + line * 0.12f, -0.05f - line * 0.16f, -0.064f),
                    new Vector3(2.7f - line * 0.37f, 0.006f, 0.004f), art.Concrete, false);

            Transform teacher = F.Table(room, "TeacherDesk", new Vector3(0f, 0f, 3.5f), 2.2f, 1.1f, 0.8f, art.OakWood, art);
            F.Cabinet(teacher, "PedestalDrawers", new Vector3(-0.72f, 0f, 0f), 0.65f, 0.7f, 0.85f, art);
            F.Chair(room, "TeacherChair", new Vector3(0f, 0f, 4.7f), 180f, art);
            for (int r = 0; r < 3; r++)
            for (int c = 0; c < 4; c++)
            {
                if (subject == 2 && r == 2 && c == 0) continue;
                float x = (c - 1.5f) * 2.7f;
                float z = -0.4f - r * 1.9f;
                Transform desk = F.Table(room, $"StudentDesk_{r}_{c}", new Vector3(x, 0f, z),
                    0.9f, 0.65f, 0.7f, art.OakWood, art);
                if ((r + c + subject) % 5 == 0) desk.localRotation = Quaternion.Euler(0f, 9f - c * 5f, 0f);
                Box("UnderDeskBasket", desk, new Vector3(0f, 0.48f, 0.02f), new Vector3(0.62f, 0.06f, 0.4f), art.MetalSteel, false);
                F.Chair(room, $"Chair_{r}_{c}", new Vector3(x + (r == 1 && c == 2 ? 0.35f : 0f), 0f, z - 0.7f),
                    (r + c) % 5 == 0 ? -14f : 0f, art);
            }
            F.Cabinet(room, "BuiltInTeachingCupboard", new Vector3(-6.5f, 0f, 2.2f), 1.1f, 2.35f, 0.6f, art);
            F.Radiator(room, new Vector3(-3f, 0f, -5.6f), 0f, art);
            F.Radiator(room, new Vector3(3f, 0f, -5.6f), 0f, art);
            Transform display = Group("SubjectWallDisplay", room);
            display.localPosition = new Vector3(subject == 0 ? 7.78f : 9.78f, 1.8f, 3.8f);
            display.localRotation = Quaternion.Euler(0f, -90f, 0f);
            Box("Backing", display, Vector3.zero, new Vector3(2.6f, 1.5f, 0.04f), art.StageCurtain, false);
            for (int i = 0; i < 6; i++)
                Box($"PinnedLesson_{i}", display, new Vector3(-0.9f + i % 3 * 0.9f, i < 3 ? 0.35f : -0.35f, -0.03f),
                    new Vector3(0.56f, 0.46f, 0.007f), i % 2 == 0 ? art.Limestone : art.Concrete, false);
            F.Wear(room, new Vector3(-6f, 0f, -3.6f), new Vector2(1.6f, 2.6f), 71 + subject, art);
        }

        private static void Science(Transform parent, BurleySchoolArt art)
        {
            foreach (float z in new[] { 23f, 28.5f })
            {
                Transform bench = F.Table(parent, $"EpoxyLabBench_{z}", new Vector3(34f, L.StoryHeight, z),
                    6.5f, 1.4f, 0.95f, art.LabEpoxy, art);
                F.Cabinet(bench, "UnderBenchStorage", new Vector3(-2.5f, 0f, 0f), 1.2f, 0.86f, 1.15f, art);
                Box("SinkRecess", bench, new Vector3(0f, 0.952f, 0f), new Vector3(0.6f, 0.015f, 0.46f), art.BrickDark, false);
                F.Tube(bench, "TapUpright", new Vector3(0f, 0.95f, 0.35f), new Vector3(0f, 1.38f, 0.35f), 0.035f, art.GoldAccent);
                F.Tube(bench, "TapSpout", new Vector3(0f, 1.38f, 0.35f), new Vector3(0f, 1.38f, 0.02f), 0.035f, art.GoldAccent);
                foreach (float side in new[] { -1f, 1f })
                {
                    F.Tube(bench, $"GasTap_{side}", new Vector3(side * 1.7f, 0.95f, 0.2f),
                        new Vector3(side * 1.7f, 1.13f, 0.2f), 0.045f, art.GoldAccent);
                    Box($"GasValve_{side}", bench, new Vector3(side * 1.7f, 1.13f, 0.2f),
                        new Vector3(0.16f, 0.025f, 0.04f), art.MetalSteel, false);
                }
            }
            F.Table(parent, "DemonstrationPlinth", new Vector3(34f, L.StoryHeight, 26f), 2.4f, 1.1f, 0.95f, art.LabEpoxy, art);
            Transform hood = Group("ChemicalFumeHood", parent);
            hood.position = new Vector3(39.2f, L.StoryHeight, 23f);
            Box("Base", hood, new Vector3(0f, 0.45f, 0f), new Vector3(1.15f, 0.9f, 2.5f), art.MetalSteel);
            Box("WorkSurface", hood, new Vector3(0f, 0.925f, 0f), new Vector3(1.2f, 0.05f, 2.5f), art.LabEpoxy);
            foreach (float side in new[] { -1f, 1f })
                Box($"Side_{side}", hood, new Vector3(0f, 1.7f, side * 1.2f), new Vector3(1.15f, 1.5f, 0.08f), art.MetalSteel);
            Box("Back", hood, new Vector3(0.53f, 1.7f, 0f), new Vector3(0.08f, 1.5f, 2.5f), art.MetalSteel, false);
            Box("Top", hood, new Vector3(0f, 2.52f, 0f), new Vector3(1.15f, 0.2f, 2.5f), art.MetalSteel, false);
            Box("RaisedGlassSash", hood, new Vector3(-0.59f, 2.02f, 0f), new Vector3(0.025f, 0.85f, 2.25f), art.WindowGlass, false);
            Box("ExtractionDuct", hood, new Vector3(0.2f, 3.15f, 0f), new Vector3(0.55f, 1.1f, 0.75f), art.MetalSteel, false);
            F.Cabinet(parent, "ChemicalWallStorage", new Vector3(26f, L.StoryHeight, 30.5f), 2.6f, 2.5f, 0.7f, art);
            F.Radiator(parent, new Vector3(29f, L.StoryHeight, 20.5f), 0f, art);
            F.Wear(parent, new Vector3(25.5f, L.StoryHeight, 24.5f), new Vector2(1.8f, 2f), 92, art);
        }

        private static void Administration(Transform parent, BurleySchoolArt art)
        {
            Transform admin = Group("MainAdminSuite", parent);
            F.Cabinet(admin, "ReceptionCounter", new Vector3(54f, 0f, 46.5f), 2.6f, 1.12f, 0.9f, art);
            Transform desk = F.Table(admin, "PrincipalDesk", new Vector3(61.5f, 0f, 46f), 2.4f, 1.3f, 0.85f, art.OakWood, art);
            F.Cabinet(desk, "DeskPedestal", new Vector3(-0.8f, 0f, 0f), 0.65f, 0.75f, 1.05f, art);
            F.Chair(admin, "PrincipalChair", new Vector3(61.5f, 0f, 47.4f), 180f, art);
            F.Chair(admin, "VisitorChairA", new Vector3(60.5f, 0f, 44.6f), 0f, art);
            F.Chair(admin, "VisitorChairB", new Vector3(62f, 0f, 44.6f), -8f, art);
            // The east edge belongs to the second stair, so records and the empty safe stay west of x64.5.
            F.Cabinet(admin, "RecordsSideboard", new Vector3(57f, 0f, 50.6f), 4.8f, 0.85f, 0.9f, art);
            Transform safe = Group("WallSafeRecess", admin);
            safe.position = new Vector3(62.5f, 0f, 50.6f);
            Box("RecessBack", safe, new Vector3(0f, 0.95f, 0.15f), new Vector3(1.3f, 1.9f, 0.3f), art.BrickDark);
            Box("OpenDoor", safe, new Vector3(0.8f, 0.95f, -0.25f), new Vector3(0.09f, 1.6f, 1.1f), art.MetalSteel);
            Box("EmptyShelf", safe, new Vector3(0f, 0.45f, -0.4f), new Vector3(1.1f, 0.06f, 0.8f), art.MetalSteel);
            for (int i = 0; i < 3; i++)
            {
                Box($"AwardFrame_{i}", admin, new Vector3(54f + i * 2f, 2.4f, 51.75f), new Vector3(0.85f, 0.65f, 0.04f), art.OakWood, false);
                Box($"FadedCertificate_{i}", admin, new Vector3(54f + i * 2f, 2.4f, 51.72f), new Vector3(0.73f, 0.53f, 0.012f), art.Limestone, false);
            }
            Transform clinic = Group("NurseClinic", parent);
            clinic.position = new Vector3(34f, 0f, 46f);
            Transform cot = F.Table(clinic, "MedicalCot", Vector3.zero, 1f, 2.1f, 0.53f, art.MetalSteel, art);
            Box("Mattress", cot, new Vector3(0f, 0.6f, 0f), new Vector3(0.94f, 0.14f, 2f), art.Concrete, false);
            Box("Pillow", cot, new Vector3(0f, 0.72f, 0.72f), new Vector3(0.72f, 0.12f, 0.36f), art.Limestone, false);
            F.Tube(cot, "HeadRail", new Vector3(-0.5f, 0.9f, 1.02f), new Vector3(0.5f, 0.9f, 1.02f), 0.04f, art.MetalSteel);
            F.Cabinet(clinic, "ClinicalCounter", new Vector3(0.5f, 0f, 2f), 2.5f, 1.15f, 0.6f, art);
            for (int i = 0; i < 3; i++)
            {
                Transform screen = Group($"PrivacyPanel_{i}", clinic);
                screen.localPosition = new Vector3(-1.5f + (i % 2) * 0.18f, 0f, -0.7f + i * 0.75f);
                screen.localRotation = Quaternion.Euler(0f, i % 2 == 0 ? -10f : 10f, 0f);
                Box("Fabric", screen, new Vector3(0f, 1.1f, 0f), new Vector3(0.035f, 1.4f, 0.7f), art.PlasterWall, false);
                F.Tube(screen, "Upright", new Vector3(0f, 0f, -0.35f), new Vector3(0f, 1.85f, -0.35f), 0.025f, art.MetalSteel);
                Box("Foot", screen, new Vector3(0f, 0.04f, 0f), new Vector3(0.4f, 0.06f, 0.6f), art.MetalSteel, false);
                var solid = screen.gameObject.AddComponent<BoxCollider>();
                solid.center = new Vector3(0f, 0.9f, 0f);
                solid.size = new Vector3(0.08f, 1.8f, 0.75f);
            }
            F.Radiator(clinic, new Vector3(-9.5f, 0f, 1f), 90f, art);
            F.Wear(clinic, new Vector3(-6f, 0f, 4.2f), new Vector2(2f, 1.5f), 103, art);
        }

        private static void UpperStudios(Transform parent, BurleySchoolArt art)
        {
            Transform studio = Group("ArtStudio", parent);
            F.Table(studio, "PrintmakingTable", new Vector3(30f, L.StoryHeight, 46f), 3.6f, 1.6f, 0.9f, art.OakWood, art);
            F.Table(studio, "DrawingTable", new Vector3(36f, L.StoryHeight, 44.5f), 2.8f, 1.4f, 0.9f, art.OakWood, art);
            F.Cabinet(studio, "ArtSupplyBase", new Vector3(30f, L.StoryHeight, 50.5f), 4.5f, 0.95f, 0.8f, art);
            for (int i = 0; i < 3; i++)
            {
                F.Chair(studio, $"StudioChair_{i}", new Vector3(28.8f + i * 1.3f, L.StoryHeight, 44.7f), 0f, art);
                Box($"DryingRail_{i}", studio, new Vector3(25f, 5f + i * 0.55f, 47f), new Vector3(0.5f, 0.04f, 4.2f), art.MetalSteel, false);
            }
            for (int i = 0; i < 8; i++)
                Box($"PinnedStudentArtwork_{i}", studio, new Vector3(25.8f + i % 4 * 3f, 6.2f + i / 4 * 0.7f, 51.75f),
                    new Vector3(0.7f, 0.5f, 0.015f), i % 2 == 0 ? art.Limestone : art.TheaterSeat, false);
            F.Wear(studio, new Vector3(29f, L.StoryHeight, 48f), new Vector2(2f, 1.8f), 119, art);

            Transform records = Group("StaffRecords", parent);
            F.Table(records, "StaffWorktable", new Vector3(57f, L.StoryHeight, 46f), 3.6f, 1.5f, 0.85f, art.OakWood, art);
            F.Chair(records, "StaffChairA", new Vector3(55.8f, L.StoryHeight, 44.7f), 0f, art);
            F.Chair(records, "StaffChairB", new Vector3(58.2f, L.StoryHeight, 47.3f), 180f, art);
            F.Cabinet(records, "RecordsCupboardA", new Vector3(55f, L.StoryHeight, 50.6f), 2.2f, 2.4f, 0.8f, art);
            F.Cabinet(records, "RecordsCupboardB", new Vector3(58f, L.StoryHeight, 50.6f), 2.2f, 2.4f, 0.8f, art);
            F.Cabinet(records, "StaffSideboard", new Vector3(62f, L.StoryHeight, 49.7f), 1.6f, 0.9f, 0.8f, art);
            Box("StaffNoticeboard", records, new Vector3(54.5f, 6.1f, 40.2f), new Vector3(2.5f, 1.2f, 0.06f), art.StageCurtain, false);
            F.Wear(records, new Vector3(60.5f, L.StoryHeight, 50f), new Vector2(1.8f, 1.5f), 121, art);
        }

        private static void Boiler(Transform parent, BurleySchoolArt art)
        {
            Transform boiler = Group("SteamBoiler", parent);
            boiler.position = new Vector3(56f, -3.5f, 86f);
            F.Tube(boiler, "PressureVessel", new Vector3(0f, 1.55f, -2.4f), new Vector3(0f, 1.55f, 2.4f), 2.6f, art.BrickDark, true);
            foreach (float z in new[] { -1.6f, 1.6f })
            {
                Box($"MountingSaddle_{z}", boiler, new Vector3(0f, 0.28f, z), new Vector3(2.7f, 0.56f, 0.38f), art.Concrete);
                Transform band = Group($"ClampingBand_{z}", boiler);
                band.localRotation = Quaternion.Euler(90f, 0f, 0f);
                F.Ring(band, "SteelBand", new Vector3(0f, z, -1.55f), 1.315f, 0.07f, art.MetalSteel, 28);
            }
            Box("BurnerFront", boiler, new Vector3(0f, 1.45f, -2.5f), new Vector3(2.2f, 2.35f, 0.15f), art.MetalSteel);
            Box("FireboxInspectionPort", boiler, new Vector3(0f, 1.2f, -2.59f), new Vector3(0.45f, 0.3f, 0.05f), art.BrickDark, false);
            for (int i = 0; i < 8; i++)
                Box($"FrontBolt_{i}", boiler, new Vector3((i % 2 == 0 ? -1f : 1f) * 0.95f, 0.5f + i / 2 * 0.62f, -2.6f),
                    new Vector3(0.065f, 0.065f, 0.045f), art.GoldAccent, false);
            foreach (float x in new[] { -0.7f, 0.7f })
            {
                F.Tube(boiler, $"SteamRiser_{x}", new Vector3(x, 2.45f, 0f), new Vector3(x, 3.05f, 0f), 0.18f, art.MetalSteel);
                F.Tube(boiler, $"CeilingMain_{x}", new Vector3(x, 3.05f, -5.2f), new Vector3(x, 3.05f, 4.6f), 0.18f, art.MetalSteel);
            }
            F.Table(parent, "MaintenanceWorkbench", new Vector3(63.5f, -3.5f, 85.5f), 3.8f, 1.2f, 0.9f, art.OakWood, art);
            Transform vice = Group("BoltedBenchVice", parent);
            vice.position = new Vector3(62.1f, -2.6f, 85.8f);
            Box("BasePlate", vice, new Vector3(0f, 0.025f, 0f), new Vector3(0.4f, 0.05f, 0.3f), art.MetalSteel, false);
            Box("Jaws", vice, new Vector3(0f, 0.16f, 0f), new Vector3(0.32f, 0.2f, 0.18f), art.MetalSteel, false);
            F.Tube(vice, "ScrewHandle", new Vector3(-0.3f, 0.12f, -0.15f), new Vector3(0.3f, 0.12f, -0.15f), 0.025f, art.MetalSteel);
            Transform rack = F.Table(parent, "FixedPipeCradle", new Vector3(63.5f, -3.5f, 89.5f), 2.8f, 1.4f, 0.35f, art.MetalSteel, art);
            foreach (float side in new[] { -1f, 1f })
                Box($"CradleStop_{side}", rack, new Vector3(side * 1.3f, 0.5f, 0f), new Vector3(0.08f, 0.3f, 1.4f), art.MetalSteel, false);
            F.Wear(parent, new Vector3(60f, -3.5f, 88.5f), new Vector2(2f, 2f), 142, art);
        }
    }
}

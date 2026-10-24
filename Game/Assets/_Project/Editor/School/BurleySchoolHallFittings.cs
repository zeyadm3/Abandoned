using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Readable school landmarks and wall-mounted fittings, outside the carrying routes.</summary>
    public static class BurleySchoolHallFittings
    {
        public static void Build(Transform parent, BurleySchoolArt art)
        {
            Transform signs = Group("DirectionalSignage", parent);
            HangingSign(signs, "ArrivalDirectory", new Vector3(46f, 3.26f, 30.7f), 0f,
                "< AUDITORIUM    |    GYMNASIUM >", "CAFETERIA  •  STRAIGHT AHEAD", art);
            HangingSign(signs, "ReturnDirectory", new Vector3(46f, 3.26f, 46.5f), 180f,
                "MAIN ENTRANCE  •  STRAIGHT AHEAD", "< GYMNASIUM    |    AUDITORIUM >", art);
            HangingSign(signs, "UpperDirectory", new Vector3(46f, L.StoryHeight + 3.26f, 46.5f), 0f,
                "LIBRARY  •  STRAIGHT AHEAD", "< SCIENCE LABS    |    EAST STAIR >", art);
            WallSign(signs, "GymPortal", new Vector3(67.7f, 2.94f, 36f), 90f, "GYMNASIUM", art);
            WallSign(signs, "TheatrePortal", new Vector3(24.3f, 2.94f, 36f), -90f, "AUDITORIUM", art);
            WallSign(signs, "CafeteriaPortal", new Vector3(46f, 2.94f, 51.8f), 0f, "CAFETERIA", art);
            WallSign(signs, "LibraryPortal", new Vector3(46f, L.StoryHeight + 2.94f, 51.8f), 0f, "LIBRARY & MEDIA", art);
            WallSign(signs, "BoilerAccess", new Vector3(50f, 2.92f, 79.77f), 0f, "BOILER  •  SERVICE STAIR", art);
            WallSign(signs, "EastStairGround", new Vector3(64.8f, 3.03f, 39.75f), 0f, "EAST STAIR  >", art);
            WallSign(signs, "EastStairUpper", new Vector3(64.8f, L.StoryHeight + 3.03f, 39.75f), 0f, "EAST STAIR  >", art);

            Transform amenities = Group("CorridorAmenities", parent);
            for (int floor = 0; floor < 2; floor++)
            {
                float y = floor * L.StoryHeight;
                Fountain(amenities, new Vector3(39.15f, y, 32.2f), art);
                Fountain(amenities, new Vector3(52.3f, y, 32.2f), art);
                Clock(amenities, new Vector3(32f, y + 3.3f, 32.24f), art);
                Clock(amenities, new Vector3(60f, y + 3.3f, 32.24f), art);
                FireStation(amenities, new Vector3(33.65f, y, 32.23f), art);
                FireStation(amenities, new Vector3(61.65f, y, 32.23f), art);
                Noticeboard(amenities, new Vector3(54.6f, y + 2.78f, 39.73f), floor == 0 ? "SCHOOL NOTICES" : "SCIENCE & EXPLORATION", art);
                MemorialPennants(amenities, y, art);
            }
            EntranceLedger(Group("EntranceLandmark", parent), art);
            Services(Group("ExposedHallServices", parent), art);
        }

        private static void HangingSign(Transform parent, string name, Vector3 at, float yaw, string top, string bottom, BurleySchoolArt art)
        {
            Transform sign = Group(name, parent);
            sign.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Box("GoldRim", sign, Vector3.zero, new Vector3(5.7f, .62f, .07f), art.GoldAccent, false);
            Box("EnamelFace", sign, new Vector3(0f, 0f, -.045f), new Vector3(5.58f, .51f, .03f), art.LockerGreen, false);
            Label(sign, "Destination", top, new Vector3(0f, .1f, -.067f), Quaternion.identity, .12f, new Color(.91f, .86f, .67f));
            Label(sign, "SecondaryDestination", bottom, new Vector3(0f, -.13f, -.067f), Quaternion.identity, .085f, new Color(.77f, .8f, .72f));
            foreach (float x in new[] { -2.25f, 2.25f })
                Box("Suspension", sign, new Vector3(x, .43f, 0f), new Vector3(.018f, .25f, .018f), art.MetalSteel, false);
        }

        private static void WallSign(Transform parent, string name, Vector3 at, float yaw, string text, BurleySchoolArt art)
        {
            Transform sign = Group(name, parent);
            sign.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Box("Frame", sign, Vector3.zero, new Vector3(2.7f, .38f, .07f), art.Limestone, false);
            Box("EnamelFace", sign, new Vector3(0f, 0f, -.045f), new Vector3(2.58f, .27f, .025f), art.LockerGreen, false);
            Label(sign, "Destination", text, new Vector3(0f, 0f, -.067f), Quaternion.identity, .095f, new Color(.9f, .82f, .55f));
        }

        private static void Fountain(Transform parent, Vector3 position, BurleySchoolArt art)
        {
            Transform root = Group("DrinkingFountain", parent);
            root.position = position;
            Box("WallMount", root, new Vector3(0f, .67f, .1f), new Vector3(.46f, .55f, .18f), art.Concrete);
            Box("BasinRim", root, new Vector3(0f, .9f, .27f), new Vector3(.54f, .1f, .46f), art.MetalSteel);
            Box("BasinRecess", root, new Vector3(0f, .953f, .27f), new Vector3(.42f, .012f, .32f), art.BrickDark, false);
            Box("Drain", root, new Vector3(0f, .961f, .27f), new Vector3(.08f, .008f, .08f), art.MetalSteel, false);
            Box("Tap", root, new Vector3(-.16f, 1.01f, .2f), new Vector3(.07f, .12f, .07f), art.MetalSteel, false);
            Box("Button", root, new Vector3(.12f, .87f, .506f), new Vector3(.08f, .045f, .012f), art.GoldAccent, false);
            Box("DrainPipe", root, new Vector3(0f, .43f, .18f), new Vector3(.045f, .45f, .045f), art.MetalSteel, false);
        }

        private static void Clock(Transform parent, Vector3 at, BurleySchoolArt art)
        {
            Transform clock = Group("StoppedHallClock", parent);
            clock.position = at;
            GameObject bezel = Primitive(PrimitiveType.Cylinder, "RoundSteelBezel", clock, Vector3.zero, new Vector3(.53f, .027f, .53f), art.MetalSteel, false);
            bezel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            GameObject dial = Primitive(PrimitiveType.Cylinder, "IvoryDial", clock, new Vector3(0f, 0f, .031f), new Vector3(.47f, .008f, .47f), art.Limestone, false);
            dial.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int tick = 0; tick < 12; tick++)
            {
                float a = tick * Mathf.PI / 6f;
                GameObject marker = Box("HourTick", clock, new Vector3(Mathf.Sin(a) * .193f, Mathf.Cos(a) * .193f, .044f),
                    new Vector3(.014f, tick % 3 == 0 ? .042f : .022f, .008f), art.BrickDark, false);
                marker.transform.localRotation = Quaternion.Euler(0f, 0f, -tick * 30f);
            }
            GameObject minute = Box("MinuteHand", clock, new Vector3(.067f, .039f, .051f), new Vector3(.012f, .17f, .009f), art.BrickDark, false);
            minute.transform.localRotation = Quaternion.Euler(0f, 0f, -60f);
            GameObject hour = Box("HourHand", clock, new Vector3(-.045f, -.031f, .057f), new Vector3(.021f, .115f, .009f), art.BrickDark, false);
            hour.transform.localRotation = Quaternion.Euler(0f, 0f, 125f);
        }

        private static void FireStation(Transform parent, Vector3 at, BurleySchoolArt art)
        {
            Transform station = Group("RecessedFireStation", parent);
            station.position = at;
            Box("SteelRecess", station, new Vector3(0f, 1.12f, .045f), new Vector3(.43f, .78f, .09f), art.MetalSteel, false);
            Box("RedCabinet", station, new Vector3(0f, 1.12f, .1f), new Vector3(.35f, .68f, .035f), art.TrackRubber, false);
            Box("WiredGlass", station, new Vector3(0f, 1.15f, .123f), new Vector3(.24f, .43f, .012f), art.WindowGlass, false);
            Box("PullStation", station, new Vector3(0f, 1.68f, .06f), new Vector3(.18f, .21f, .1f), art.TrackRubber, false);
            Box("PullBar", station, new Vector3(0f, 1.65f, .12f), new Vector3(.11f, .035f, .035f), art.Limestone, false);
            Box("BellBackplate", station, new Vector3(0f, 2.38f, .045f), new Vector3(.25f, .29f, .06f), art.MetalSteel, false);
            Primitive(PrimitiveType.Sphere, "AlarmBell", station, new Vector3(0f, 2.4f, .075f), new Vector3(.21f, .21f, .085f), art.TrackRubber, false);
        }

        private static void Noticeboard(Transform parent, Vector3 at, string title, BurleySchoolArt art)
        {
            Transform board = Group("FadedNoticeboard", parent);
            board.position = at;
            Box("OakFrame", board, Vector3.zero, new Vector3(3.45f, 1.16f, .08f), art.OakWood, false);
            Box("CorkBacking", board, new Vector3(0f, 0f, -.047f), new Vector3(3.3f, 1.02f, .025f), art.StageWood, false);
            Label(board, "BoardTitle", title, new Vector3(0f, .37f, -.065f), Quaternion.identity, .09f, new Color(.89f, .84f, .68f));
            for (int i = 0; i < 6; i++)
            {
                Transform sheet = Group("CurledNotice", board);
                sheet.localPosition = new Vector3(-1.25f + i * .5f, -.08f + (i % 2) * .065f, -.067f);
                sheet.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 4f : -6f);
                Box("Paper", sheet, Vector3.zero, new Vector3(.39f, .5f, .008f), i % 3 == 0 ? art.WhiteStripe : art.Limestone, false);
                for (int line = 0; line < 4; line++)
                    Box("FadedInk", sheet, new Vector3(-.015f, .12f - line * .065f, -.006f), new Vector3(.25f - (line % 2) * .05f, .013f, .004f), art.BrickDark, false);
                Box("DrawingPin", sheet, new Vector3(0f, .22f, -.012f), new Vector3(.025f, .025f, .008f), art.GoldAccent, false);
            }
        }

        private static void MemorialPennants(Transform parent, float floorY, BurleySchoolArt art)
        {
            foreach (float x in new[] { 27f, 36.5f, 54.5f, 64.8f })
            {
                Box("WallPennant", parent, new Vector3(x, floorY + 2.74f, 32.185f), new Vector3(1.2f, .82f, .018f), art.StageCurtain, false);
                Box("PennantPole", parent, new Vector3(x, floorY + 3.17f, 32.21f), new Vector3(1.32f, .035f, .035f), art.GoldAccent, false);
                Label(parent, "SchoolPennant", x < 40f ? "PINE HOLLOW\nEST. 1951" : "WOLVES\nGREEN & GOLD",
                    new Vector3(x, floorY + 2.74f, 32.2f), Quaternion.Euler(0f, 180f, 0f), .09f, new Color(.85f, .75f, .38f));
            }
        }

        private static void EntranceLedger(Transform parent, BurleySchoolArt art)
        {
            Transform directory = Group("CampusDirectory", parent);
            directory.position = new Vector3(40.3f, 1.7f, 27.6f);
            directory.rotation = Quaternion.Euler(0f, -90f, 0f);
            Box("OakFrame", directory, Vector3.zero, new Vector3(2.6f, 1.75f, .1f), art.OakWood, false);
            Box("DirectoryFace", directory, new Vector3(0f, 0f, -.065f), new Vector3(2.42f, 1.57f, .035f), art.LockerGreen, false);
            Label(directory, "Header", "PINE HOLLOW", new Vector3(0f, .57f, -.09f), Quaternion.identity, .13f, new Color(.87f, .77f, .43f));
            Label(directory, "Directory", "GROUND  •  CLASSROOMS / CLINIC\nWEST  •  AUDITORIUM\nEAST  •  GYMNASIUM\nNORTH  •  CAFETERIA\nUPPER  •  SCIENCE / LIBRARY", new Vector3(0f, -.1f, -.09f), Quaternion.identity, .09f, new Color(.85f, .84f, .75f));
        }

        private static void Services(Transform parent, BurleySchoolArt art)
        {
            for (int floor = 0; floor < 2; floor++)
            foreach (float z in new[] { 32.45f, 39.55f })
            {
                float y = floor * L.StoryHeight + 3.58f;
                foreach (var span in new[] { new Vector2(24.4f, 39.6f), new Vector2(52.4f, 67.6f) })
                {
                    Box("Conduit", parent, new Vector3((span.x + span.y) * .5f, y, z), new Vector3(span.y - span.x, .04f, .04f), art.MetalSteel, false);
                    for (float x = span.x + .7f; x < span.y; x += 2.4f)
                        Box("ConduitSaddle", parent, new Vector3(x, y, z), new Vector3(.075f, .075f, .07f), art.MetalSteel, false);
                }
            }
        }
    }
}

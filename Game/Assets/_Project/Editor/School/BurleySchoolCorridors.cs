using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Layered institutional halls with clear doorways, useful landmarks and hiding lockers.</summary>
    public static class BurleySchoolCorridors
    {
        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform halls = Group("SchoolCorridors", root);
            BuildWallFinishes(Group("HallWallFinishes", halls), art);
            BuildLockers(Group("LockerBanks", halls), art);
            BuildAwardCase(Group("AthleticsTrophyCase", halls), art);
            BurleySchoolHallFittings.Build(halls, art);
            MarkStatic(halls);

            // The hinged hiding doors must stay out of static batching and occlusion baking.
            BurleySchoolHallHiding.Build(Group("HidingLockers", halls), art);
        }

        private static void BuildWallFinishes(Transform parent, BurleySchoolArt art)
        {
            // Each run stops outside the 1.8m classroom portal; decorative trim must never seal a doorway.
            (float left, float right)[] runs =
            {
                (24.15f, 31.03f), (32.97f, 39.85f),
                (52.15f, 59.03f), (60.97f, 67.85f)
            };
            for (int floor = 0; floor < 2; floor++)
            foreach (float z in new[] { 32.15f, 39.85f })
            {
                float y = floor * L.StoryHeight;
                float face = z < 36f ? 1f : -1f;
                foreach (var run in runs)
                {
                    float length = run.right - run.left;
                    float x = (run.left + run.right) * .5f;
                    Box("GlazedWainscot", parent, new Vector3(x, y + .64f, z),
                        new Vector3(length, 1.23f, .035f), art.LockerGreen, false);
                    Box("TerrazzoSkirting", parent, new Vector3(x, y + .065f, z + face * .025f),
                        new Vector3(length, .13f, .07f), art.BrickDark, false);
                    Box("GoldDado", parent, new Vector3(x, y + 1.28f, z + face * .027f),
                        new Vector3(length, .045f, .06f), art.GoldAccent, false);
                    Box("PictureRail", parent, new Vector3(x, y + 3.12f, z + face * .018f),
                        new Vector3(length, .065f, .05f), art.Limestone, false);
                    // Uneven missing paint records years of damp without introducing floor obstacles.
                    for (int scar = 0; scar < 3; scar++)
                        Box("FlakedDado", parent, new Vector3(run.left + .6f + scar * 1.71f, y + .4f + scar * .18f, z + face * .022f),
                            new Vector3(.35f + scar * .08f, .11f, .008f), art.PlasterWall, false);
                }

                foreach (float x in new[] { 32f, 60f })
                {
                    Transform portal = Group($"ClassroomPortal_{floor}_{x}_{z}", parent);
                    portal.position = new Vector3(x, y, z);
                    portal.rotation = Quaternion.Euler(0f, face > 0 ? 0f : 180f, 0f);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Box("SteelJamb", portal, new Vector3(side * .965f, 1.24f, .015f),
                            new Vector3(.12f, 2.48f, .16f), art.MetalSteel, false);
                        Box("JambInset", portal, new Vector3(side * .935f, 1.22f, .1f),
                            new Vector3(.035f, 2.4f, .025f), art.Limestone, false);
                    }
                    Box("SteelHead", portal, new Vector3(0f, 2.48f, .015f),
                        new Vector3(2.05f, .15f, .16f), art.MetalSteel, false);
                    Box("RoomPlaque", portal, new Vector3(0f, 2.75f, .06f),
                        new Vector3(1.18f, .26f, .06f), art.LockerGreen, false);
                    string room = x < 40f ? (z < 36f ? (floor == 0 ? "101  HISTORY" : "201  SCIENCE") : (floor == 0 ? "HEALTH OFFICE" : "202  PREPARATION"))
                        : (z < 36f ? (floor == 0 ? "102  MATHEMATICS" : "203  GEOGRAPHY") : (floor == 0 ? "ADMINISTRATION" : "204  STAFF ROOM"));
                    Label(portal, "RoomNumber", room, new Vector3(0f, 2.75f, .097f),
                        Quaternion.Euler(0f, 180f, 0f), .065f, new Color(.86f, .78f, .5f));

                    // Leaves are secured inside the room, leaving the full classroom opening visible.
                    Transform leaf = Group("DoorSecuredOpen", portal);
                    leaf.localPosition = new Vector3(-.87f, 0f, -.08f);
                    leaf.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    Box("DoorLeaf", leaf, new Vector3(.84f, 1.19f, 0f), new Vector3(1.68f, 2.36f, .055f), art.OakWood, false);
                    Box("KickPlate", leaf, new Vector3(.84f, .19f, .033f), new Vector3(1.52f, .28f, .012f), art.MetalSteel, false);
                    Box("VisionPanelFrame", leaf, new Vector3(.84f, 1.76f, .033f), new Vector3(.38f, .52f, .025f), art.MetalSteel, false);
                    Box("VisionPanel", leaf, new Vector3(.84f, 1.76f, .05f), new Vector3(.29f, .43f, .015f), art.WindowGlass, false);
                    Box("Lever", leaf, new Vector3(1.47f, 1.05f, .08f), new Vector3(.19f, .045f, .06f), art.GoldAccent, false);
                }
            }
        }

        private static void BuildLockers(Transform parent, BurleySchoolArt art)
        {
            float[] centres = { 27f, 36.5f, 54.5f, 64.8f };
            for (int floor = 0; floor < 2; floor++)
            for (int side = 0; side < 2; side++)
            for (int bank = 0; bank < centres.Length; bank++)
            {
                // The ground-floor award display occupies one bank, giving the two floors different landmarks.
                if (floor == 0 && side == 1 && bank == 1) continue;
                BuildLockerBank(parent, $"StudentLockers_{floor}_{side}_{bank}",
                    new Vector3(centres[bank], floor * L.StoryHeight, side == 0 ? 32.2f : 39.8f),
                    side == 1, 100 + floor * 100 + side * 40 + bank * 8, art);
            }
        }

        private static void BuildLockerBank(Transform parent, string name, Vector3 position, bool facesSouth, int firstNumber, BurleySchoolArt art)
        {
            const int count = 8;
            const float width = count * .45f;
            Transform bank = Group(name, parent);
            bank.SetPositionAndRotation(position, Quaternion.Euler(0f, facesSouth ? 180f : 0f, 0f));
            var solid = bank.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, 1.0f, .23f);
            solid.size = new Vector3(width, 2f, .46f);
            Box("DarkInteriorBack", bank, new Vector3(0f, 1.0f, .025f), new Vector3(width, 1.9f, .04f), art.BrickDark, false);
            Box("RaisedPlinth", bank, new Vector3(0f, .08f, .23f), new Vector3(width + .04f, .16f, .46f), art.MetalSteel, false);
            Box("Cornice", bank, new Vector3(0f, 1.99f, .23f), new Vector3(width + .04f, .09f, .49f), art.LockerGreen, false);
            for (int i = 0; i <= count; i++)
                Box("VerticalPartition", bank, new Vector3(-width * .5f + i * .45f, 1.04f, .23f),
                    new Vector3(.025f, 1.82f, .43f), art.LockerGreen, false);

            for (int i = 0; i < count; i++)
            {
                Transform door = Group($"Door_{firstNumber + i}", bank);
                door.localPosition = new Vector3(-width * .5f + i * .45f + .02f, 0f, .455f);
                // A few hanging-open doors break the rhythm while staying within the bank's wall recess.
                if ((firstNumber + i) % 11 == 0) door.localRotation = Quaternion.Euler(0f, -28f, 0f);
                Box("PressedDoor", door, new Vector3(.2f, 1.04f, 0f), new Vector3(.4f, 1.78f, .024f), art.LockerGreen, false);
                Box("HandleRecess", door, new Vector3(.32f, 1.02f, .018f), new Vector3(.065f, .22f, .012f), art.BrickDark, false);
                Box("PullHandle", door, new Vector3(.32f, 1.02f, .043f), new Vector3(.02f, .15f, .025f), art.MetalSteel, false);
                Box("NumberPlate", door, new Vector3(.2f, 1.64f, .019f), new Vector3(.14f, .07f, .01f), art.Limestone, false);
                if (i % 2 == 0)
                    Label(door, "Number", (firstNumber + i).ToString(), new Vector3(.2f, 1.64f, .026f),
                        Quaternion.Euler(0f, 180f, 0f), .045f, new Color(.16f, .18f, .16f));
                for (int vent = 0; vent < 3; vent++)
                {
                    Box("UpperVent", door, new Vector3(.2f, 1.44f - vent * .055f, .018f), new Vector3(.24f, .018f, .014f), art.BrickDark, false);
                    Box("LowerVent", door, new Vector3(.2f, .43f - vent * .055f, .018f), new Vector3(.24f, .018f, .014f), art.BrickDark, false);
                }
                if (i % 3 == 0)
                    Box("ScrapedPaint", door, new Vector3(.12f, .56f, .019f), new Vector3(.12f, .018f, .007f), art.MetalSteel, false);
            }
        }

        private static void BuildAwardCase(Transform parent, BurleySchoolArt art)
        {
            Transform display = Group("OpenSalvageDisplay", parent);
            display.SetPositionAndRotation(new Vector3(36.7f, 0f, 39.78f), Quaternion.Euler(0f, 180f, 0f));
            Box("FeltBack", display, new Vector3(0f, 1.36f, .025f), new Vector3(4.1f, 2.42f, .05f), art.StageCurtain, false);
            foreach (float side in new[] { -1f, 1f })
                Box("OakUpright", display, new Vector3(side * 2.1f, 1.36f, .28f), new Vector3(.13f, 2.6f, .56f), art.OakWood);
            Box("Canopy", display, new Vector3(0f, 2.66f, .28f), new Vector3(4.32f, .16f, .58f), art.OakWood);
            Box("LowerSalvageShelf", display, new Vector3(0f, .68f, .28f), new Vector3(4.1f, .08f, .56f), art.OakWood);
            Box("UpperShelf", display, new Vector3(0f, 1.44f, .24f), new Vector3(4.1f, .08f, .48f), art.OakWood);
            Box("ShelfFoot", display, new Vector3(0f, .27f, .045f), new Vector3(4.1f, .48f, .09f), art.OakWood);
            Box("BrokenSlidingRail", display, new Vector3(0f, .79f, .55f), new Vector3(4.14f, .035f, .025f), art.MetalSteel, false);
            Box("FixedHeaderPlaque", display, new Vector3(0f, 2.38f, .065f), new Vector3(3.72f, .29f, .035f), art.Limestone, false);
            Label(display, "Header", "BURLEY  •  HONOUR & ACHIEVEMENT", new Vector3(0f, 2.38f, .09f),
                Quaternion.Euler(0f, 180f, 0f), .09f, new Color(.2f, .26f, .18f));
            for (int i = 0; i < 3; i++)
            {
                Box("MountedAward", display, new Vector3((i - 1) * 1.18f, 1.94f, .075f), new Vector3(.7f, .57f, .06f), art.OakWood, false);
                Box("EngravedPlate", display, new Vector3((i - 1) * 1.18f, 1.94f, .112f), new Vector3(.57f, .42f, .015f), art.GoldAccent, false);
                Label(display, "AwardYear", (1956 + i * 2).ToString(), new Vector3((i - 1) * 1.18f, 1.94f, .13f),
                    Quaternion.Euler(0f, 180f, 0f), .09f, new Color(.22f, .2f, .13f));
            }
            // The lower shelf is deliberately open for real spawned salvage, never scenery duplicates.
            Box("CaseLamp", display, new Vector3(0f, 2.56f, .31f), new Vector3(3.4f, .035f, .04f), art.FluorescentGlow, false);
        }
    }
}

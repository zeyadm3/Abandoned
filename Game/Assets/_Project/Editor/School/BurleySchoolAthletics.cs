using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the complete outdoor athletics complex of Jackson P. Burley Middle School
    /// as seen in the aerial campus photo: 6-lane oval running track with football/soccer field
    /// (end zones, yard lines, collegiate yellow goalposts, spectator bleachers, terraced hillside),
    /// outdoor paved basketball courts, baseball diamond with backstop fence, and the staff parking lot.
    /// </summary>
    public static class BurleySchoolAthletics
    {
        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform athletics = Group("AthleticsComplex", root);

            // 1. Oval Running Track & Football / Soccer Field (parallel to Madison Ave)
            BuildTrackAndFootballField(Group("TrackAndFootballField", athletics), art);

            // 2. Terraced Hillside Steps (between school plaza and track)
            BuildTerracedHillside(Group("TerracedHillside", athletics), art);

            // 3. Outdoor Basketball Courts
            BuildOutdoorBasketballCourt(Group("OutdoorBasketballCourt", athletics), art);

            // 4. Baseball Diamond & Backstop
            BuildBaseballDiamond(Group("BaseballDiamond", athletics), art);

            // 5. Staff & Visitor Northeast Parking Lot
            BuildParkingLot(Group("NortheastParkingLot", athletics), art);

            MarkStatic(athletics);
        }

        // =========================================================================
        // 1. RUNNING TRACK & FOOTBALL / SOCCER FIELD
        // =========================================================================
        private static void BuildTrackAndFootballField(Transform parent, BurleySchoolArt art)
        {
            Vector3 center = new(176f, -0.2f, 14f);
            float trackL = 104f, trackW = 76f;
            float fieldL = 72f, fieldW = 44f;

            // Red rubber oval track base slab
            Box("TrackOvalBase", parent, center, new Vector3(trackW, 0.22f, trackL), art.TrackRubber);

            // Track Oval Curves & Straightaway Lane Stripes (6 lanes)
            for (int lane = 0; lane <= 6; lane++)
            {
                float lw = fieldW + lane * 4.8f;
                float ll = fieldL + lane * 4.8f;
                // Straightaways
                Box($"Lane_W_{lane}", parent, center - Vector3.right * (lw * 0.5f), new Vector3(0.12f, 0.23f, fieldL), art.WhiteStripe, false);
                Box($"Lane_E_{lane}", parent, center + Vector3.right * (lw * 0.5f), new Vector3(0.12f, 0.23f, fieldL), art.WhiteStripe, false);
                // End turn curve arcs (approximated with angled segments)
                for (int arc = -2; arc <= 2; arc++)
                {
                    float angle = arc * 28f;
                    Vector3 nPos = center + Vector3.forward * (fieldL * 0.5f + (6 - lane) * 0.6f + Mathf.Cos(angle * Mathf.Deg2Rad) * (trackW * 0.24f));
                    nPos.x = center.x + Mathf.Sin(angle * Mathf.Deg2Rad) * (lw * 0.48f);
                    Box($"Lane_N_Arc_{lane}_{arc + 2}", parent, nPos, new Vector3(3.2f, 0.23f, 0.12f), art.WhiteStripe, false);

                    Vector3 sPos = center - Vector3.forward * (fieldL * 0.5f + (6 - lane) * 0.6f + Mathf.Cos(angle * Mathf.Deg2Rad) * (trackW * 0.24f));
                    sPos.x = center.x + Mathf.Sin(angle * Mathf.Deg2Rad) * (lw * 0.48f);
                    Box($"Lane_S_Arc_{lane}_{arc + 2}", parent, sPos, new Vector3(3.2f, 0.23f, 0.12f), art.WhiteStripe, false);
                }
            }

            // Finish Line across track straightaway
            Box("TrackFinishLine", parent, center + new Vector3(-fieldW * 0.5f - 7.0f, 0.02f, 0f), new Vector3(14.5f, 0.24f, 0.35f), art.WhiteStripe, false);

            // Interior Football & Soccer Grass Pitch
            Transform field = Group("FootballPitch", parent);
            field.position = center + Vector3.up * 0.02f;
            Box("TurfInfield", field, Vector3.zero, new Vector3(fieldW, 0.22f, fieldL), art.TurfGrass);

            // White Perimeter Boundary
            Box("FieldBoundary_W", field, new Vector3(-fieldW * 0.5f, 0.12f, 0f), new Vector3(0.15f, 0.03f, fieldL), art.WhiteStripe, false);
            Box("FieldBoundary_E", field, new Vector3(fieldW * 0.5f, 0.12f, 0f), new Vector3(0.15f, 0.03f, fieldL), art.WhiteStripe, false);
            Box("FieldBoundary_N", field, new Vector3(0f, 0.12f, fieldL * 0.5f), new Vector3(fieldW, 0.03f, 0.15f), art.WhiteStripe, false);
            Box("FieldBoundary_S", field, new Vector3(0f, 0.12f, -fieldL * 0.5f), new Vector3(fieldW, 0.03f, 0.15f), art.WhiteStripe, false);

            // Midfield 50-Yard Line & Center Circle
            Box("YardLine_50", field, new Vector3(0f, 0.12f, 0f), new Vector3(fieldW, 0.03f, 0.15f), art.WhiteStripe, false);
            Box("CenterCircle", field, new Vector3(0f, 0.12f, 0f), new Vector3(7.5f, 0.03f, 7.5f), art.WhiteStripe, false);

            // Football Yard Lines (every 6 metres ~ 10 yards)
            for (int y = 1; y <= 4; y++)
            {
                float zOffset = y * 6.5f;
                Box($"YardLine_N_{y * 10}", field, new Vector3(0f, 0.12f, zOffset), new Vector3(fieldW - 0.4f, 0.03f, 0.12f), art.WhiteStripe, false);
                Box($"YardLine_S_{y * 10}", field, new Vector3(0f, 0.12f, -zOffset), new Vector3(fieldW - 0.4f, 0.03f, 0.12f), art.WhiteStripe, false);
            }

            // North and South End Zones ("BURLEY BEARS")
            Transform endZoneN = Group("NorthEndZone", field);
            endZoneN.localPosition = new Vector3(0f, 0.12f, fieldL * 0.5f - 3.8f);
            Box("ZoneFillN", endZoneN, Vector3.zero, new Vector3(fieldW - 0.4f, 0.02f, 7.4f), art.LockerGreen, false);
            Label(endZoneN, "TextN", "BURLEY BEARS", Vector3.up * 0.02f,
                Quaternion.Euler(90f, 0f, 0f), 0.18f, art.GoldAccent.GetColor("_BaseColor"));

            Transform endZoneS = Group("SouthEndZone", field);
            endZoneS.localPosition = new Vector3(0f, 0.12f, -fieldL * 0.5f + 3.8f);
            Box("ZoneFillS", endZoneS, Vector3.zero, new Vector3(fieldW - 0.4f, 0.02f, 7.4f), art.LockerGreen, false);
            Label(endZoneS, "TextS", "BURLEY BEARS", Vector3.up * 0.02f,
                Quaternion.Euler(90f, 180f, 0f), 0.18f, art.GoldAccent.GetColor("_BaseColor"));

            // Yellow Collegiate Football Goalposts
            BuildGoalpost(parent, "NorthGoalpost", center + new Vector3(0f, 0f, fieldL * 0.5f), false, art);
            BuildGoalpost(parent, "SouthGoalpost", center - new Vector3(0f, 0f, fieldL * 0.5f), true, art);

            // Sideline Spectator Bleachers (West sideline facing east)
            Transform bleachers = Group("TrackSidelineBleachers", parent);
            bleachers.position = center + new Vector3(-fieldW * 0.5f - 11.5f, 0f, 0f);
            for (int tier = 0; tier < 6; tier++)
            {
                float bx = -tier * 0.65f;
                float by = 0.3f + tier * 0.35f;
                Box($"TrackBleacher_{tier}", bleachers, new Vector3(bx, by, 0f), new Vector3(0.6f, 0.15f, 36f), art.Limestone);
                Box($"TrackBleacherFrame_{tier}", bleachers, new Vector3(bx, by * 0.5f, 0f), new Vector3(0.55f, by, 35.8f), art.MetalSteel);
            }
        }

        private static void BuildGoalpost(Transform parent, string name, Vector3 pos, bool south, BurleySchoolArt art)
        {
            Transform goal = Group(name, parent);
            goal.position = pos;
            float rotY = south ? 0f : 180f;
            goal.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Goose-neck post base
            Box("BaseGooseneck", goal, new Vector3(0f, 1.5f, 1.2f), new Vector3(0.18f, 3.0f, 0.18f), art.YellowStripe);
            Box("ArmHorizontal", goal, new Vector3(0f, 3.05f, 0.6f), new Vector3(0.18f, 0.18f, 1.2f), art.YellowStripe);
            // Crossbar (Regulation 5.64m wide, 3.05m high)
            Box("Crossbar", goal, new Vector3(0f, 3.05f, 0f), new Vector3(5.64f, 0.15f, 0.15f), art.YellowStripe);
            // Uprights rising 6.1m
            Box("UprightLeft", goal, new Vector3(-2.82f, 6.1f, 0f), new Vector3(0.12f, 6.1f, 0.12f), art.YellowStripe);
            Box("UprightRight", goal, new Vector3(2.82f, 6.1f, 0f), new Vector3(0.12f, 6.1f, 0.12f), art.YellowStripe);
        }

        // =========================================================================
        // 2. TERRACED HILLSIDE STEPS
        // =========================================================================
        private static void BuildTerracedHillside(Transform parent, BurleySchoolArt art)
        {
            // Distinctive parallel retaining terraces between school plaza and track (visible in aerial photo)
            for (int t = 0; t < 5; t++)
            {
                float x = 126f + t * 4.5f;
                float y = 0.4f - t * 0.15f;
                // Grass terrace strip
                Box($"TerraceLawn_{t}", parent, new Vector3(x, y, 14f), new Vector3(4.2f, 0.25f, 84f), art.TurfGrass);
                // Concrete/stone retaining curb
                Box($"RetainingCurb_{t}", parent, new Vector3(x + 2.1f, y + 0.1f, 14f), new Vector3(0.25f, 0.35f, 84f), art.Limestone);
            }
        }

        // =========================================================================
        // 3. OUTDOOR BASKETBALL COURTS
        // =========================================================================
        private static void BuildOutdoorBasketballCourt(Transform parent, BurleySchoolArt art)
        {
            Vector3 center = new(112f, -0.05f, 28f);
            float courtL = 26f, courtW = 16f;

            // Dark paved asphalt court slab
            Box("CourtAsphalt", parent, center, new Vector3(courtW, 0.2f, courtL), art.Asphalt);

            // Painted Boundary Lines
            Box("LineN", parent, center + Vector3.forward * (courtL * 0.5f), new Vector3(courtW, 0.22f, 0.12f), art.WhiteStripe, false);
            Box("LineS", parent, center - Vector3.forward * (courtL * 0.5f), new Vector3(courtW, 0.22f, 0.12f), art.WhiteStripe, false);
            Box("LineW", parent, center - Vector3.right * (courtW * 0.5f), new Vector3(0.12f, 0.22f, courtL), art.WhiteStripe, false);
            Box("LineE", parent, center + Vector3.right * (courtW * 0.5f), new Vector3(0.12f, 0.22f, courtL), art.WhiteStripe, false);
            Box("MidCourt", parent, center, new Vector3(courtW, 0.22f, 0.12f), art.WhiteStripe, false);
            Box("CenterCircle", parent, center, new Vector3(3.6f, 0.22f, 3.6f), art.WhiteStripe, false);

            // Keys & Free Throw Lines
            Box("KeyN", parent, center + new Vector3(0f, 0.01f, courtL * 0.5f - 3.2f), new Vector3(4.2f, 0.22f, 5.8f), art.LockerGreen, false);
            Box("KeyS", parent, center - new Vector3(0f, -0.01f, courtL * 0.5f - 3.2f), new Vector3(4.2f, 0.22f, 5.8f), art.LockerGreen, false);

            // Outdoor Steel Gooseneck Basketball Hoops
            BuildOutdoorHoop(parent, "OutdoorHoop_N", center + new Vector3(0f, 0f, courtL * 0.5f), false, art);
            BuildOutdoorHoop(parent, "OutdoorHoop_S", center - new Vector3(0f, 0f, courtL * 0.5f), true, art);

            // Perimeter Chain-Link Fence
            BuildFence(parent, "CourtFence", center, courtW + 2.5f, courtL + 2.5f, 3.2f, art);
        }

        private static void BuildOutdoorHoop(Transform parent, string name, Vector3 pos, bool south, BurleySchoolArt art)
        {
            Transform hoop = Group(name, parent);
            hoop.position = pos;
            float rotY = south ? 0f : 180f;
            hoop.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Heavy duty steel pipe standard
            Box("Pole", hoop, new Vector3(0f, 1.8f, 1.1f), new Vector3(0.14f, 3.6f, 0.14f), art.MetalSteel);
            Box("GooseneckArm", hoop, new Vector3(0f, 3.2f, 0.55f), new Vector3(0.12f, 0.12f, 1.1f), art.MetalSteel);
            // Steel fan backboard
            Box("Backboard", hoop, new Vector3(0f, 3.35f, 0f), new Vector3(1.5f, 1.0f, 0.04f), art.FloorTile, false);
            Box("TargetBox", hoop, new Vector3(0f, 3.15f, -0.03f), new Vector3(0.5f, 0.4f, 0.02f), art.GoldAccent, false);
            // Orange Rim & Chain Net
            Box("Rim", hoop, new Vector3(0f, 3.05f, -0.3f), new Vector3(0.48f, 0.05f, 0.48f), art.GoldAccent, false);
        }

        // =========================================================================
        // 4. BASEBALL DIAMOND
        // =========================================================================
        private static void BuildBaseballDiamond(Transform parent, BurleySchoolArt art)
        {
            Vector3 home = L.BaseballHomePlate;
            float baseDist = 22f; // Scaled high school diamond base path

            // Red clay dirt infield area
            Box("InfieldClayDirt", parent, home + new Vector3(0f, -0.1f, baseDist * 0.7f), new Vector3(32f, 0.2f, 32f), art.InfieldClay);

            // Home Plate (Five-sided rubber slab)
            Box("HomePlate", parent, home + Vector3.up * 0.02f, new Vector3(0.45f, 0.03f, 0.45f), art.FloorTile, false);

            // 1st Base, 2nd Base, 3rd Base canvas bags
            Vector3 firstBase = home + new Vector3(baseDist * 0.7f, 0.02f, baseDist * 0.7f);
            Vector3 secondBase = home + new Vector3(0f, 0.02f, baseDist * 1.4f);
            Vector3 thirdBase = home + new Vector3(-baseDist * 0.7f, 0.02f, baseDist * 0.7f);

            Box("Base_1st", parent, firstBase, new Vector3(0.4f, 0.06f, 0.4f), art.FloorTile);
            Box("Base_2nd", parent, secondBase, new Vector3(0.4f, 0.06f, 0.4f), art.FloorTile);
            Box("Base_3rd", parent, thirdBase, new Vector3(0.4f, 0.06f, 0.4f), art.FloorTile);

            // Pitcher's Mound & Pitching Rubber
            Vector3 mound = home + new Vector3(0f, 0.15f, baseDist * 0.7f);
            Box("PitchersMound", parent, mound, new Vector3(4.2f, 0.3f, 4.2f), art.InfieldClay);
            Box("PitchingRubber", parent, mound + Vector3.up * 0.16f, new Vector3(0.6f, 0.04f, 0.15f), art.FloorTile, false);

            // Outfield Turf Grass expanding towards Booker T. Washington Park
            Box("OutfieldGrass", parent, home + new Vector3(0f, -0.1f, baseDist * 1.8f), new Vector3(56f, 0.2f, 44f), art.TurfGrass);

            // Backstop Chain-Link Fence behind Home Plate
            Transform backstop = Group("BackstopFence", parent);
            backstop.position = home - new Vector3(0f, 0f, 5.0f);
            Box("BackstopCenter", backstop, new Vector3(0f, 2.5f, 0f), new Vector3(12.0f, 5.0f, 0.08f), art.FenceChainlink);
            Box("BackstopWingL", backstop, new Vector3(-6.8f, 2.5f, 2.5f), new Vector3(6.0f, 5.0f, 0.08f), art.FenceChainlink);
            Box("BackstopWingR", backstop, new Vector3(6.8f, 2.5f, 2.5f), new Vector3(6.0f, 5.0f, 0.08f), art.FenceChainlink);
        }

        // =========================================================================
        // 5. NORTHEAST PARKING LOT
        // =========================================================================
        private static void BuildParkingLot(Transform parent, BurleySchoolArt art)
        {
            Vector3 center = new(110f, -0.1f, 70f);
            float lotW = 44f, lotL = 40f;

            // Paved Asphalt Parking Surface
            Box("ParkingLotAsphalt", parent, center, new Vector3(lotW, 0.22f, lotL), art.Asphalt);

            // White Painted Parking Stalls
            Transform stalls = Group("ParkingStalls", parent);
            stalls.position = center + Vector3.up * 0.12f;

            // Row 1 (North stalls)
            for (int s = 0; s < 12; s++)
            {
                float sx = -lotW * 0.5f + 3.0f + s * 3.2f;
                Box($"StallLine_N_{s}", stalls, new Vector3(sx, 0f, 14f), new Vector3(0.12f, 0.02f, 5.5f), art.WhiteStripe, false);
            }
            // Row 2 & 3 (Central double stalls)
            for (int s = 0; s < 12; s++)
            {
                float sx = -lotW * 0.5f + 3.0f + s * 3.2f;
                Box($"StallLine_C1_{s}", stalls, new Vector3(sx, 0f, 0f), new Vector3(0.12f, 0.02f, 5.2f), art.WhiteStripe, false);
                Box($"StallLine_C2_{s}", stalls, new Vector3(sx, 0f, -5.2f), new Vector3(0.12f, 0.02f, 5.2f), art.WhiteStripe, false);
            }

            // High-Mast Parking Lot Light Poles
            Vector3[] poleLocs = { new(94f, 0f, 54f), new(126f, 0f, 54f), new(94f, 0f, 84f), new(126f, 0f, 84f) };
            for (int p = 0; p < poleLocs.Length; p++)
            {
                Transform pole = Group($"ParkingLightPole_{p}", parent);
                pole.position = poleLocs[p];
                Box("SteelPole", pole, new Vector3(0f, 4.5f, 0f), new Vector3(0.24f, 9.0f, 0.24f), art.MetalSteel);
                Box("LampArm", pole, new Vector3(0f, 8.8f, 0.6f), new Vector3(0.12f, 0.12f, 1.2f), art.MetalSteel);
                Box("LampHead", pole, new Vector3(0f, 8.7f, 1.2f), new Vector3(0.6f, 0.25f, 0.8f), art.LampSodium, false);
            }

            // Green Industrial Dumpster Enclosure
            Transform dumpster = Group("DumpsterEnclosure", parent);
            dumpster.position = new Vector3(88f, 0f, 86f);
            Box("EnclosureWallBack", dumpster, new Vector3(0f, 1.1f, 2.5f), new Vector3(6.5f, 2.2f, 0.25f), art.BrickDark);
            Box("EnclosureWallLeft", dumpster, new Vector3(-3.2f, 1.1f, 0f), new Vector3(0.25f, 2.2f, 5.0f), art.BrickDark);
            Box("DumpsterBin", dumpster, new Vector3(0f, 0.75f, 0f), new Vector3(3.2f, 1.5f, 1.8f), art.LockerGreen);
        }

        private static void BuildFence(Transform parent, string name, Vector3 center, float width, float length, float height, BurleySchoolArt art)
        {
            Transform fence = Group(name, parent);
            // Four perimeter chain-link fence sides
            Box("FenceN", fence, center + Vector3.forward * (length * 0.5f) + Vector3.up * (height * 0.5f), new Vector3(width, height, 0.06f), art.FenceChainlink);
            Box("FenceS", fence, center - Vector3.forward * (length * 0.5f) + Vector3.up * (height * 0.5f), new Vector3(width, height, 0.06f), art.FenceChainlink);
            Box("FenceW", fence, center - Vector3.right * (width * 0.5f) + Vector3.up * (height * 0.5f), new Vector3(0.06f, height, length), art.FenceChainlink);
            Box("FenceE", fence, center + Vector3.right * (width * 0.5f) + Vector3.up * (height * 0.5f), new Vector3(0.06f, height, length), art.FenceChainlink);
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

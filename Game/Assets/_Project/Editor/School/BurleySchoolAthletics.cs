using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A = Abandoned.EditorTools.BurleySchoolArchitecture;
using G = Abandoned.EditorTools.BurleySchoolAthleticsGeometry;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Winter athletics with a true oval track, framed grandstand and open diamond wire fences.</summary>
    public static class BurleySchoolAthletics
    {
        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform athletics = Group("AthleticsComplex", root);
            // The track, the field and their terraces were authored round z 14, where the oval's south curve ran under
            // Rose Hill Drive; they stand TrackShift further north, clear of the road and its sidewalk (2026-10-24).
            Transform track = Group("TrackAndFootballField", athletics);
            BuildTrackAndFootballField(track, art);
            track.localPosition = Vector3.forward * L.TrackShift;
            Transform terraces = Group("TerracedHillside", athletics);
            BuildTerracedHillside(terraces, art);
            terraces.localPosition = Vector3.forward * L.TrackShift;
            BuildOutdoorBasketballCourt(Group("OutdoorBasketballCourt", athletics), art);
            BuildBaseballDiamond(Group("BaseballDiamond", athletics), art);
            BuildParkingLot(Group("NortheastParkingLot", athletics), art);
            MarkStatic(athletics);
        }

        private static void BuildTrackAndFootballField(Transform parent, BurleySchoolArt art)
        {
            Vector3 centre = new(176f, 0.025f, 14f);
            const float inner = 31f;
            const float laneWidth = 1.15f;
            const float straight = 28f;
            G.StadiumBand(parent, "RubberOval", centre, inner, inner + 6f * laneWidth, straight, art.TrackRubber);
            for (int lane = 0; lane <= 6; lane++)
            {
                float radius = inner + lane * laneWidth;
                G.StadiumBand(parent, $"LaneStripe_{lane}", centre + Vector3.up * 0.012f, radius - 0.045f, radius + 0.045f, straight, art.WhiteStripe);
            }
            G.StadiumBand(parent, "OuterSnowRidge", centre + Vector3.up * 0.016f, 38.15f, 38.8f, straight, art.Snow);
            A.PaintLine(parent, "FinishLine", centre + new Vector3(-37.9f, 0.022f, -4f), centre + new Vector3(-31f, 0.022f, -4f), 0.25f, art.WhiteStripe);
            for (int lane = 0; lane < 6; lane++)
                Label(parent, $"LaneNumber_{lane}", (lane + 1).ToString(), centre + new Vector3(-31.58f - lane * laneWidth, 0.032f, -6f),
                    Quaternion.Euler(90f, 0f, 0f), 0.18f, Color.white);

            Transform field = Group("FootballPitch", parent);
            field.localPosition = new Vector3(176f, 0f, 14f);
            const float width = 42f;
            const float length = 70f;
            Box("TurfBase", field, new Vector3(0f, -0.08f, 0f), new Vector3(width, 0.16f, length), art.TurfGrass);
            for (int i = 0; i < 9; i++)
            {
                float x = -14f + i % 3 * 13f;
                float z = -26f + i / 3 * 24f;
                Primitive(PrimitiveType.Sphere, $"WindblownSnow_{i}", field, new Vector3(x, 0.012f, z),
                    new Vector3(14f + i % 2 * 3f, 0.035f, 15f), art.Snow, false);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                A.PaintLine(field, side < 0f ? "WestSideline" : "EastSideline", new Vector3(side * width * 0.5f, 0.045f, -length * 0.5f),
                    new Vector3(side * width * 0.5f, 0.045f, length * 0.5f), 0.12f, art.WhiteStripe);
                BuildGoalpost(field, side < 0f ? "SouthGoalpost" : "NorthGoalpost", new Vector3(0f, 0f, side * (length * 0.5f + 0.8f)), side > 0f, art);
            }
            for (int line = -4; line <= 4; line++)
            {
                float z = line * 6.5f;
                A.PaintLine(field, $"YardLine_{line}", new Vector3(-width * 0.5f, 0.045f, z), new Vector3(width * 0.5f, 0.045f, z), 0.1f, art.WhiteStripe);
                if (line == 0) continue;
                for (int hash = -1; hash <= 1; hash += 2)
                    A.PaintLine(field, $"Hash_{line}_{hash}", new Vector3(hash * 6f, 0.048f, z - 0.7f), new Vector3(hash * 6f, 0.048f, z + 0.7f), 0.1f, art.WhiteStripe);
            }
            Box("NorthEndZone", field, new Vector3(0f, 0.04f, 31.5f), new Vector3(width, 0.015f, 7f), art.LockerGreen, false);
            Box("SouthEndZone", field, new Vector3(0f, 0.04f, -31.5f), new Vector3(width, 0.015f, 7f), art.LockerGreen, false);
            Label(field, "NorthEndZoneName", "PINE HOLLOW WOLVES", new Vector3(0f, 0.055f, 31.5f), Quaternion.Euler(90f, 0f, 0f), 0.28f, art.GoldAccent.GetColor("_BaseColor"));
            Label(field, "SouthEndZoneName", "PINE HOLLOW", new Vector3(0f, 0.055f, -31.5f), Quaternion.Euler(90f, 180f, 0f), 0.32f, art.GoldAccent.GetColor("_BaseColor"));
            BuildGrandstand(Group("StadiumBleachers", parent), art);
        }

        private static void BuildGrandstand(Transform parent, BurleySchoolArt art)
        {
            // East-side seating keeps the courts and the school's service drive clear.
            parent.localPosition = new Vector3(220f, 0f, 14f);
            for (int tier = 0; tier < 7; tier++)
            {
                float x = tier * 0.72f;
                float y = 0.42f + tier * 0.4f;
                Box($"Seat_{tier}", parent, new Vector3(x, y, 0f), new Vector3(0.46f, 0.1f, 38f), art.MetalSteel);
                Box($"Tread_{tier}", parent, new Vector3(x - 0.3f, y - 0.23f, 0f), new Vector3(0.56f, 0.06f, 38f), art.MetalSteel);
                Box($"SeatSnow_{tier}", parent, new Vector3(x, y + 0.07f, 0f), new Vector3(0.42f, 0.055f, 37.6f), art.Snow, false);
                for (int frame = -3; frame <= 3; frame++)
                {
                    float z = frame * 6f;
                    Box($"Support_{tier}_{frame}", parent, new Vector3(x, y * 0.5f, z), new Vector3(0.085f, y, 0.085f), art.MetalSteel);
                    Box($"Footing_{tier}_{frame}", parent, new Vector3(x, 0.025f, z), new Vector3(0.34f, 0.05f, 0.34f), art.Concrete);
                }
            }
            for (int frame = -3; frame <= 3; frame++)
                A.Rod(parent, $"DiagonalFrame_{frame}", new Vector3(-0.25f, 0.1f, frame * 6f), new Vector3(4.65f, 2.55f, frame * 6f), 0.045f, art.MetalSteel);
            A.Rod(parent, "RearGuardrail", new Vector3(4.75f, 3.8f, -19f), new Vector3(4.75f, 3.8f, 19f), 0.045f, art.MetalSteel, true);
            for (int post = -5; post <= 5; post++)
                A.Rod(parent, $"RearPost_{post}", new Vector3(4.75f, 2.55f, post * 3.8f), new Vector3(4.75f, 3.85f, post * 3.8f), 0.04f, art.MetalSteel, true);
            for (int step = 0; step < 14; step++)
            {
                float x = -0.25f + step * 0.36f;
                float y = 0.14f + step * 0.2f;
                Box($"EndAccessStep_{step}", parent, new Vector3(x, y, -20.2f), new Vector3(0.36f, 0.2f, 1.8f), art.MetalSteel);
            }
            Box("GroundSnowbank", parent, new Vector3(4.2f, 0.2f, 20f), new Vector3(6f, 0.4f, 1.5f), art.SnowDrift, false);
        }

        private static void BuildGoalpost(Transform parent, string name, Vector3 at, bool north, BurleySchoolArt art)
        {
            Transform goal = Group(name, parent);
            goal.localPosition = at;
            goal.localRotation = Quaternion.Euler(0f, north ? 180f : 0f, 0f);
            A.Cylinder(goal, "BasePad", new Vector3(0f, 0.6f, -0.8f), 0.25f, 1.2f, art.LockerGreen, true);
            A.Cylinder(goal, "Mast", new Vector3(0f, 1.55f, -0.8f), 0.12f, 3.1f, art.YellowStripe, true);
            A.Rod(goal, "Gooseneck", new Vector3(0f, 3.05f, -0.8f), new Vector3(0f, 3.05f, 0f), 0.11f, art.YellowStripe);
            A.Rod(goal, "Crossbar", new Vector3(-2.82f, 3.05f, 0f), new Vector3(2.82f, 3.05f, 0f), 0.09f, art.YellowStripe);
            A.Rod(goal, "UprightL", new Vector3(-2.82f, 3.05f, 0f), new Vector3(-2.82f, 9.05f, 0f), 0.075f, art.YellowStripe);
            A.Rod(goal, "UprightR", new Vector3(2.82f, 3.05f, 0f), new Vector3(2.82f, 9.05f, 0f), 0.075f, art.YellowStripe);
        }

        private static void BuildTerracedHillside(Transform parent, BurleySchoolArt art)
        {
            // Thin edge terraces replace the broad concrete slabs that used to run through the gym and courts.
            for (int tier = 0; tier < 3; tier++)
            {
                float x = 134f + tier * 0.85f;
                Box($"RetainingEdge_{tier}", parent, new Vector3(x, -0.03f - tier * 0.025f, 16f), new Vector3(0.24f, 0.25f, 96f), art.Limestone);
                Box($"SnowEdge_{tier}", parent, new Vector3(x, 0.13f - tier * 0.025f, 16f), new Vector3(0.38f, 0.06f, 95.8f), art.Snow, false);
            }
        }

        private static void BuildOutdoorBasketballCourt(Transform parent, BurleySchoolArt art)
        {
            Rect rect = L.OutdoorBasketballCourt;
            Vector3 centre = new(rect.center.x, 0f, rect.center.y);
            parent.localPosition = centre;
            float w = rect.width;
            float l = rect.height;
            Box("CourtAsphalt", parent, new Vector3(0f, -0.07f, 0f), new Vector3(w, 0.14f, l), art.SnowyAsphalt);
            Primitive(PrimitiveType.Sphere, "IcePuddleW", parent, new Vector3(-5.5f, 0.013f, 4.5f), new Vector3(6.2f, 0.02f, 3.1f), art.FrostIce, false);
            Primitive(PrimitiveType.Sphere, "IcePuddleE", parent, new Vector3(7f, 0.013f, -3.5f), new Vector3(4.4f, 0.02f, 3.4f), art.FrostIce, false);
            float end = w * 0.5f - 1f;
            float side = l * 0.5f - 1f;
            foreach (float sign in new[] { -1f, 1f })
            {
                A.PaintLine(parent, $"EndLine_{sign}", new Vector3(sign * end, 0.032f, -side), new Vector3(sign * end, 0.032f, side), 0.08f, art.WhiteStripe);
                A.PaintLine(parent, $"SideLine_{sign}", new Vector3(-end, 0.032f, sign * side), new Vector3(end, 0.032f, sign * side), 0.08f, art.WhiteStripe);
                BuildOutdoorHoop(parent, sign < 0f ? "WestHoop" : "EastHoop", new Vector3(sign * (end - 0.6f), 0f, 0f), sign < 0f, art);
                float freeThrow = sign * (end - 4.8f);
                A.Arc(parent, $"FreeThrowCircle_{sign}", new Vector3(freeThrow, 0.034f, 0f), 1.8f, 0f, 360f, 0.075f, art.WhiteStripe, 32);
                A.Arc(parent, $"ThreePointArc_{sign}", new Vector3(sign * (end - 1.2f), 0.034f, 0f), 6.25f, sign < 0f ? 0f : 180f, 180f, 0.075f, art.WhiteStripe, 32);
                foreach (float keySide in new[] { -1f, 1f })
                    A.PaintLine(parent, $"KeySide_{sign}_{keySide}", new Vector3(sign * end, 0.034f, keySide * 2.4f), new Vector3(freeThrow, 0.034f, keySide * 2.4f), 0.075f, art.WhiteStripe);
                A.PaintLine(parent, $"FreeThrowLine_{sign}", new Vector3(freeThrow, 0.034f, -2.4f), new Vector3(freeThrow, 0.034f, 2.4f), 0.075f, art.WhiteStripe);
            }
            A.PaintLine(parent, "HalfCourtLine", new Vector3(0f, 0.034f, -side), new Vector3(0f, 0.034f, side), 0.08f, art.WhiteStripe);
            A.Arc(parent, "CentreCircle", new Vector3(0f, 0.034f, 0f), 1.8f, 0f, 360f, 0.08f, art.WhiteStripe, 36);
            float fx = w * 0.5f + 1f;
            float fz = l * 0.5f + 1f;
            G.FenceRun(parent, "FenceNorth", new Vector3(-fx, 0f, fz), new Vector3(fx, 0f, fz), 3.2f, art);
            G.FenceRun(parent, "FenceWest", new Vector3(-fx, 0f, -fz), new Vector3(-fx, 0f, fz), 3.2f, art);
            G.FenceRun(parent, "FenceEast", new Vector3(fx, 0f, fz), new Vector3(fx, 0f, -fz), 3.2f, art);
            G.FenceRun(parent, "FenceSouthL", new Vector3(-fx, 0f, -fz), new Vector3(-1.3f, 0f, -fz), 3.2f, art);
            G.FenceRun(parent, "FenceSouthR", new Vector3(1.3f, 0f, -fz), new Vector3(fx, 0f, -fz), 3.2f, art);
            Box("DriftAtNorthFence", parent, new Vector3(0f, 0.18f, fz), new Vector3(w * 0.7f, 0.36f, 0.65f), art.SnowDrift, false);
        }

        private static void BuildOutdoorHoop(Transform parent, string name, Vector3 at, bool east, BurleySchoolArt art)
        {
            Transform hoop = Group(name, parent);
            hoop.localPosition = at;
            hoop.localRotation = Quaternion.Euler(0f, east ? 90f : -90f, 0f);
            A.Cylinder(hoop, "SteelPole", new Vector3(0f, 1.95f, -0.65f), 0.09f, 3.9f, art.MetalSteel, true);
            A.Rod(hoop, "SupportArm", new Vector3(0f, 3.3f, -0.65f), new Vector3(0f, 3.3f, 0f), 0.07f, art.MetalSteel);
            Box("Backboard", hoop, new Vector3(0f, 3.3f, 0f), new Vector3(1.8f, 1.05f, 0.07f), art.Concrete, false);
            foreach (float sign in new[] { -1f, 1f })
            {
                Box($"TargetSide_{sign}", hoop, new Vector3(sign * 0.3f, 3.23f, 0.045f), new Vector3(0.04f, 0.42f, 0.02f), art.WhiteStripe, false);
                Box($"TargetHorizontal_{sign}", hoop, new Vector3(0f, 3.23f + sign * 0.21f, 0.045f), new Vector3(0.64f, 0.04f, 0.02f), art.WhiteStripe, false);
            }
            A.Arc(hoop, "RoundRim", new Vector3(0f, 3.05f, 0.33f), 0.23f, 0f, 360f, 0.045f, art.YellowStripe, 24);
            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.PI / 5f;
                Vector3 radial = new(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                A.Rod(hoop, $"ChainNet_{i}", new Vector3(0f, 3.03f, 0.33f) + radial * 0.22f,
                    new Vector3(0f, 2.7f, 0.33f) + radial * 0.15f, 0.008f, art.MetalSteel);
            }
        }

        private static void BuildBaseballDiamond(Transform parent, BurleySchoolArt art)
        {
            parent.localPosition = L.BaseballHomePlate;
            float b = L.BaseballBaseDistance;
            Vector3[] bases = { new(0f, 0.04f, 0f), new(b, 0.04f, 0f), new(b, 0.04f, b), new(0f, 0.04f, b) };
            for (int i = 0; i < bases.Length; i++)
            {
                A.PaintLine(parent, $"ClayBasePath_{i}", bases[i], bases[(i + 1) % bases.Length], 1.15f, art.InfieldClay);
                GameObject bag = Box(i == 0 ? "HomePlate" : $"BaseBag_{i}", parent, bases[i] + Vector3.up * 0.02f, new Vector3(0.48f, 0.06f, 0.48f), art.WhiteStripe, false);
                bag.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            }
            Primitive(PrimitiveType.Sphere, "PitchersMound", parent, new Vector3(b * 0.5f, 0.1f, b * 0.5f), new Vector3(4f, 0.3f, 4f), art.InfieldClay, false);
            Box("PitchersRubber", parent, new Vector3(b * 0.5f, 0.28f, b * 0.5f), new Vector3(0.6f, 0.025f, 0.14f), art.WhiteStripe, false);
            G.FenceRun(parent, "BackstopMain", new Vector3(-5.5f, 0f, -3f), new Vector3(4.5f, 0f, -3f), 5.5f, art);
            G.FenceRun(parent, "BackstopWestWing", new Vector3(-5.5f, 0f, -3f), new Vector3(-5.5f, 0f, 3f), 5.5f, art);
            G.FenceRun(parent, "BackstopEastWing", new Vector3(4.5f, 0f, -3f), new Vector3(7f, 0f, 1.5f), 5.5f, art);
            Transform dugout = Group("HomeDugout", parent);
            dugout.localPosition = new Vector3(-8.5f, 0f, 10f);
            Box("Floor", dugout, new Vector3(0f, -0.03f, 0f), new Vector3(3.2f, 0.1f, 8.5f), art.Concrete);
            Box("Roof", dugout, new Vector3(0f, 2.4f, 0f), new Vector3(3.6f, 0.15f, 8.9f), art.MetalSteel);
            Box("RoofSnow", dugout, new Vector3(0f, 2.53f, 0f), new Vector3(3.5f, 0.12f, 8.8f), art.Snow, false);
            foreach (float x in new[] { -1.4f, 1.4f })
            foreach (float z in new[] { -4f, 4f })
                A.Cylinder(dugout, $"RoofPost_{x}_{z}", new Vector3(x, 1.2f, z), 0.06f, 2.4f, art.MetalSteel, true);
            Box("BenchSeat", dugout, new Vector3(-0.85f, 0.48f, 0f), new Vector3(0.48f, 0.08f, 7.5f), art.OakWood);
            for (int leg = -2; leg <= 2; leg++)
                Box($"BenchSupport_{leg}", dugout, new Vector3(-0.85f, 0.24f, leg * 1.6f), new Vector3(0.08f, 0.48f, 0.08f), art.MetalSteel);
            G.FenceRun(dugout, "RearWireWall", new Vector3(-1.4f, 0f, -4f), new Vector3(-1.4f, 0f, 4f), 2.3f, art);
        }

        private static void BuildParkingLot(Transform parent, BurleySchoolArt art)
        {
            Rect r = L.ParkingLot;
            Vector3 centre = new(r.center.x, 0f, r.center.y);
            Box("ParkingLotAsphalt", parent, centre + Vector3.down * 0.07f, new Vector3(r.width, 0.14f, r.height), art.SnowyAsphalt);
            int stalls = Mathf.FloorToInt((r.height - 8f) / 3.1f);
            for (int bank = -1; bank <= 1; bank += 2)
            for (int stall = 0; stall <= stalls; stall++)
            {
                float z = r.yMin + 4f + stall * 3.1f;
                float edge = bank < 0 ? r.xMin + 1.6f : r.xMax - 1.6f;
                float aisle = edge - bank * 5.2f;
                A.PaintLine(parent, $"StallDivider_{bank}_{stall}", new Vector3(edge, 0.028f, z), new Vector3(aisle, 0.028f, z), 0.09f, art.YellowStripe);
                if (stall < stalls)
                    Box($"WheelStop_{bank}_{stall}", parent, new Vector3(edge - bank * 0.4f, 0.09f, z + 1.55f), new Vector3(0.18f, 0.18f, 1.8f), art.Concrete);
            }
            Box("NorthSnowbank", parent, new Vector3(centre.x, 0.35f, r.yMax), new Vector3(r.width - 2f, 0.7f, 1.5f), art.SnowDrift, false);
            Box("EastSnowbank", parent, new Vector3(r.xMax, 0.25f, centre.z), new Vector3(0.85f, 0.5f, r.height - 2f), art.SnowDrift, false);
            foreach (float x in new[] { r.xMin + 0.5f, r.xMax - 0.5f })
            foreach (float z in new[] { r.yMin + 1f, r.yMax - 1f })
            {
                Transform pole = Group($"LotLight_{x}_{z}", parent);
                pole.localPosition = new Vector3(x, 0f, z);
                A.Cylinder(pole, "Footing", new Vector3(0f, 0.25f, 0f), 0.28f, 0.5f, art.Concrete, true);
                A.Cylinder(pole, "Mast", new Vector3(0f, 4f, 0f), 0.095f, 8f, art.MetalSteel, true);
                Box("LampHousing", pole, new Vector3(0f, 8.05f, 0f), new Vector3(0.8f, 0.22f, 0.65f), art.MetalSteel, false);
                Box("LampLens", pole, new Vector3(0f, 7.93f, 0f), new Vector3(0.65f, 0.025f, 0.5f), art.LampSodium, false);
                Box("SnowCap", pole, new Vector3(0f, 8.21f, 0f), new Vector3(0.78f, 0.1f, 0.63f), art.Snow, false);
            }
        }
    }
}

using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the scenic surroundings of Pine Hollow Middle School in winter at 3:00 PM:
    /// Distant snow-capped Blue Ridge mountain backdrop, Hollow Creek Park with a frozen ice pond
    /// and wooden footbridge, snow-covered pine trees, the front loop and flagpoles. The streets, the houses round
    /// the campus, the campus grounds and the two outer rings are BurleySchoolTown (2026-10-24).
    /// </summary>
    public static class BurleySchoolSurroundings
    {
        public const string GroupName = "CampusSurroundings";

        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform surroundings = Group(GroupName, root);

            BurleySchoolLandscape.Ground(surroundings, art);

            // 1. Distant Snow-Capped Blue Ridge Mountain Backdrop
            BuildMountainBackdrop(Group("BlueRidgeMountainPanorama", surroundings), art);

            // 2. Hollow Creek Park with Frozen Pond & Footbridge
            BuildHollowCreekPark(Group("HollowCreekPark", surroundings), art);

            // 3-4. The neighbourhood: streets, houses, the campus grounds and two outer rings (BurleySchoolTown).
            BurleySchoolTown.Build(surroundings, art);

            // 5. Winter Pine Trees & Campus Foliage
            BuildWinterFoliage(Group("WinterFoliage", surroundings), art);

            // 6. Front Semicircular Driveway Drop-off Loop & Flagpoles
            BuildFrontDrivewayLoop(Group("FrontDrivewayLoop", surroundings), art);

            // 7. Abandoned Snow-Covered Vehicles (School Buses & Cars)
            BuildWinterVehicles(Group("WinterVehicles", surroundings), art);

            MarkStatic(surroundings);
            StationRings.Settle(StationSite.Exterior(surroundings));
        }

        // =========================================================================
        // 1. DISTANT BLUE RIDGE MOUNTAIN BACKDROP
        // =========================================================================
        private static void BuildMountainBackdrop(Transform parent, BurleySchoolArt art)
        {
            // Monumental snow-capped mountain ridges encircling the town
            (Vector3 pos, Vector3 size, float rotY)[] ridges =
            {
                // North Ridge (visible behind Hollow Creek Park)
                (new Vector3(120f, 65f, 420f), new Vector3(550f, 130f, 120f), -8f),
                (new Vector3(-180f, 85f, 460f), new Vector3(480f, 170f, 140f), 15f),
                // West Ridge (visible past Henry Ave)
                (new Vector3(-380f, 75f, 60f), new Vector3(140f, 150f, 520f), 4f),
                (new Vector3(-420f, 95f, -220f), new Vector3(160f, 190f, 460f), -12f),
                // East Ridge (visible past Madison Ave & athletic track)
                (new Vector3(480f, 80f, 40f), new Vector3(140f, 160f, 560f), -5f),
                (new Vector3(520f, 110f, 280f), new Vector3(160f, 220f, 480f), 18f),
                // South Foothills (visible down Concord & Rose Hill)
                (new Vector3(60f, 55f, -380f), new Vector3(580f, 110f, 130f), 6f)
            };

            for (int i = 0; i < ridges.Length; i++)
            {
                var (pos, size, rotY) = ridges[i];
                BurleySchoolLandscape.Ridge(parent, $"MountainRidge_{i}", pos, size, rotY, 31 + i * 19, art);
            }
        }

        // =========================================================================
        // 2. HOLLOW CREEK PARK WITH FROZEN POND & FOOTBRIDGE
        // =========================================================================
        private static void BuildHollowCreekPark(Transform parent, BurleySchoolArt art)
        {
            Rect r = L.ParkBounds;
            Vector3 center = new(r.center.x, -0.1f, r.center.y);
            Vector3 pond = center + new Vector3(15f, 0.05f, 10f);
            const float pondWidth = 42f;
            const float pondLength = 28f;
            float west = pond.x - pondWidth * 0.5f - 1f;
            float east = pond.x + pondWidth * 0.5f + 1f;
            float south = pond.z - pondLength * 0.5f - 1f;
            float north = pond.z + pondLength * 0.5f + 1f;
            // Leave a real opening in the snow mantle so it cannot hide the ice.
            Box("ParkSnowWest", parent, new Vector3((r.xMin + west) * 0.5f, -0.1f, r.center.y),
                new Vector3(west - r.xMin, 0.25f, r.height), art.Snow);
            Box("ParkSnowEast", parent, new Vector3((east + r.xMax) * 0.5f, -0.1f, r.center.y),
                new Vector3(r.xMax - east, 0.25f, r.height), art.Snow);
            Box("ParkSnowSouth", parent, new Vector3(pond.x, -0.1f, (r.yMin + south) * 0.5f),
                new Vector3(east - west, 0.25f, south - r.yMin), art.Snow);
            Box("ParkSnowNorth", parent, new Vector3(pond.x, -0.1f, (north + r.yMax) * 0.5f),
                new Vector3(east - west, 0.25f, r.yMax - north), art.Snow);
            Box("PondBasin", parent, pond - Vector3.up * 0.15f, new Vector3(pondWidth + 2f, 0.2f, pondLength + 2f), art.SnowDrift);
            Box("FrozenPondIce", parent, pond, new Vector3(pondWidth, 0.04f, pondLength), art.FrostIce);
            foreach (float sign in new[] { -1f, 1f })
            {
                Box($"SnowShoreEW_{sign}", parent, pond + new Vector3(sign * (pondWidth * 0.5f + 0.5f), 0.025f, 0f),
                    new Vector3(1f, 0.14f, pondLength + 2f), art.SnowDrift, false);
                Box($"SnowShoreNS_{sign}", parent, pond + new Vector3(0f, 0.025f, sign * (pondLength * 0.5f + 0.5f)),
                    new Vector3(pondWidth, 0.14f, 1f), art.SnowDrift, false);
            }
            BurleySchoolCampusProps.Bridge(parent, pond, pondLength + 4f, art);
            Box("NorthParkWalk", parent, new Vector3(center.x + 5f, -0.015f, pond.z + 20f), new Vector3(115f, 0.08f, 2.4f), art.Sidewalk);
            for (int i = 0; i < 4; i++)
            {
                Transform bench = Group($"ParkBench_{i}", parent);
                bench.localPosition = new Vector3(pond.x - 22f + i * 14f, 0f, pond.z - 18f);
                for (int slat = 0; slat < 4; slat++)
                    Box($"SeatSlat_{slat}", bench, new Vector3(0f, 0.45f, -0.2f + slat * 0.13f), new Vector3(2.2f, 0.065f, 0.1f), art.OakWood);
                for (int slat = 0; slat < 3; slat++)
                    Box($"BackSlat_{slat}", bench, new Vector3(0f, 0.69f + slat * 0.15f, -0.27f), new Vector3(2.2f, 0.11f, 0.07f), art.OakWood);
                foreach (float sign in new[] { -1f, 1f })
                {
                    BurleySchoolArchitecture.Rod(bench, $"LegF_{sign}", new Vector3(sign * 0.85f, -0.08f, 0.2f), new Vector3(sign * 0.85f, 0.48f, 0.15f), 0.04f, art.MetalSteel, true);
                    BurleySchoolArchitecture.Rod(bench, $"LegB_{sign}", new Vector3(sign * 0.85f, -0.08f, -0.22f), new Vector3(sign * 0.85f, 1.02f, -0.3f), 0.04f, art.MetalSteel, true);
                }
                Box("SeatSnow", bench, new Vector3(-0.3f, 0.52f, 0.01f), new Vector3(1.5f, 0.055f, 0.5f), art.Snow, false);
            }
            for (int i = 0; i < 5; i++)
            {
                Transform lamp = Group($"ParkLamp_{i}", parent);
                lamp.localPosition = new Vector3(center.x - 45f + i * 25f, 0f, pond.z + 20f);
                BurleySchoolArchitecture.Cylinder(lamp, "IronBase", new Vector3(0f, 0.2f, 0f), 0.19f, 0.5f, art.MetalSteel, true);
                BurleySchoolArchitecture.Cylinder(lamp, "IronPost", new Vector3(0f, 2.2f, 0f), 0.065f, 4.4f, art.MetalSteel, true);
                Box("LanternGlow", lamp, new Vector3(0f, 4.5f, 0f), new Vector3(0.45f, 0.55f, 0.45f), art.LampSodium, false);
                Box("LanternCap", lamp, new Vector3(0f, 4.82f, 0f), new Vector3(0.62f, 0.12f, 0.62f), art.MetalSteel, false);
                Box("CapSnow", lamp, new Vector3(0f, 4.91f, 0f), new Vector3(0.58f, 0.07f, 0.58f), art.Snow, false);
            }
        }

        // =========================================================================
        // 5. WINTER PINE TREES & MATURE CAMPUS FOLIAGE
        // =========================================================================
        private static void BuildWinterFoliage(Transform parent, BurleySchoolArt art)
        {
            // Clustered Stands of Winter Pine Trees with tiered snow-laden needle boughs
            Vector3[] pineLocations =
            {
                // Front lawn & Entrance perimeter (clear of the walks to the west door and round the gym)
                new(6f, 0f, 4f), new(14f, 0f, 3f), new(76f, 0f, 8f), new(82f, 0f, 12f), new(-6f, 0f, 8f),
                // Behind the portables, by the service drive
                new(12f, 0f, 93f),
                // Parking Lot border (clear of the drive in from the service yard)
                new(97f, 0f, 95f), new(132f, 0f, 88f),
                // Hollow Creek Park Grove
                new(95f, 0f, 142f), new(110f, 0f, 155f), new(135f, 0f, 148f), new(150f, 0f, 160f),
                new(85f, 0f, 165f), new(125f, 0f, 175f), new(165f, 0f, 150f),
                // Athletics perimeter & track banks
                new(132f, 0f, 65f), new(225f, 0f, 25f), new(225f, 0f, 60f)
            };

            for (int i = 0; i < pineLocations.Length; i++)
            {
                float scale = 0.85f + (i % 5) * 0.12f;
                BuildWinterPineTree(parent, $"PineTree_{i}", pineLocations[i], scale, art);
            }
        }

        private static void BuildWinterPineTree(Transform parent, string name, Vector3 pos, float scale, BurleySchoolArt art)
        {
            BurleySchoolLandscape.Pine(parent, name, pos, scale, art);
        }

        // =========================================================================
        // 6. FRONT DRIVEWAY LOOP & CAMPUS LAWN
        // =========================================================================
        private static void BuildFrontDrivewayLoop(Transform parent, BurleySchoolArt art)
        {
            Vector3 loopCenter = new(46f, -0.05f, 4f);

            // Semicircular driveway asphalt loop surface
            Box("DrivewayLoopAsphalt", parent, loopCenter, new Vector3(48f, 0.2f, 22f), art.SnowyAsphalt);

            // Front Campus Lawn under deep snow blanket
            Transform lawn = Group("FrontCampusLawn", parent);
            lawn.position = new Vector3(31f, -.03f, 3f);
            Box("LawnSnow", lawn, Vector3.zero, new Vector3(14f, 0.22f, 10f), art.Snow);
            // Concrete curbing around lawn
            Box("LawnCurb_N", lawn, new Vector3(0f, 0.15f, 5f), new Vector3(14.4f, 0.25f, 0.35f), art.Limestone);
            Box("LawnCurb_S", lawn, new Vector3(0f, 0.15f, -5f), new Vector3(14.4f, 0.25f, 0.35f), art.Limestone);
            Box("LawnCurb_W", lawn, new Vector3(-7f, 0.15f, 0f), new Vector3(0.35f, 0.25f, 10.4f), art.Limestone);
            Box("LawnCurb_E", lawn, new Vector3(7f, 0.15f, 0f), new Vector3(0.35f, 0.25f, 10.4f), art.Limestone);

            Box("ClearedEntranceWalk", parent, new Vector3(46f, -.035f, 13.5f), new Vector3(7f, .12f, 13f), art.Sidewalk);

            // Flagpole Plaza on Front Lawn
            Transform flagPlaza = Group("FlagpolePlaza", lawn);
            flagPlaza.localPosition = new Vector3(0f, 0.1f, 1.5f);
            Box("PlazaSlab", flagPlaza, Vector3.zero, new Vector3(6.5f, 0.15f, 4.5f), art.Concrete);

            // American Flag (Center)
            BuildFlagpole(flagPlaza, "MainFlagpole", Vector3.zero, 11f, art.Limestone, art.GoldAccent, art.WhiteStripe);
            // Pine Hollow Wolves School Flag (Kelly Green and Old Gold)
            BuildFlagpole(flagPlaza, "SchoolFlagpole", new Vector3(-2.2f, 0f, 0f), 8.5f, art.Limestone, art.GoldAccent, art.LockerGreen);
        }

        private static void BuildFlagpole(Transform parent, string name, Vector3 pos, float height, Material poleMat, Material ballMat, Material flagMat)
        {
            Transform pole = Group(name, parent);
            pole.localPosition = pos;
            Box("SteelPole", pole, new Vector3(0f, height * 0.5f, 0f), new Vector3(0.12f, height, 0.12f), poleMat);
            Box("GoldBallFinial", pole, new Vector3(0f, height + 0.15f, 0f), new Vector3(0.35f, 0.35f, 0.35f), ballMat, false);
            Box("FlagBanner", pole, new Vector3(1.1f, height - 1.1f, 0f), new Vector3(2.1f, 1.35f, 0.03f), flagMat, false);
        }

        // =========================================================================
        // 7. ABANDONED SNOW-COVERED VEHICLES (SCHOOL BUSES & CARS)
        // =========================================================================
        private static void BuildWinterVehicles(Transform parent, BurleySchoolArt art)
        {
            // Abandoned Yellow School Buses parked in turnaround & parking lot
            BuildSchoolBus(parent, "SchoolBus_FrontTurnaround", new Vector3(18f, 0f, 4f), 0f, art);
            BuildSchoolBus(parent, "SchoolBus_ParkingLot", new Vector3(118f, 0f, 65f), 0f, art);

            // The cars along the streets are the town's (BurleySchoolTownStreet); the buses in the bus lane are the campus's.
        }

        private static void BuildSchoolBus(Transform parent, string name, Vector3 pos, float rotY, BurleySchoolArt art)
        {
            BurleySchoolCampusProps.Bus(parent, name, pos, rotY, art);
        }
    }
}

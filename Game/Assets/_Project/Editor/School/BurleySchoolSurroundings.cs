using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the scenic surroundings of Jackson P. Burley Middle School in winter at 3:00 PM:
    /// Distant snow-capped Blue Ridge mountain backdrop, Booker T. Washington Park with a frozen ice pond
    /// and wooden footbridge, Charlottesville residential neighborhood houses with smoking brick chimneys
    /// and snow-laden roofs, snow-covered pine trees, winter roads with plow banks, and parked snow-dusted vehicles.
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

            // 2. Booker T. Washington Park with Frozen Pond & Footbridge
            BuildBookerTPark(Group("BookerTWashingtonPark", surroundings), art);

            // 3. Charlottesville Residential Neighborhood Houses
            BuildResidentialNeighborhood(Group("ResidentialNeighborhood", surroundings), art);

            // 4. Snowy Road Networks & Utility Infrastructure
            BuildRoadNetworks(Group("RoadNetworks", surroundings), art);

            // 5. Winter Pine Trees & Campus Foliage
            BuildWinterFoliage(Group("WinterFoliage", surroundings), art);

            // 6. Front Semicircular Driveway Drop-off Loop & Flagpoles
            BuildFrontDrivewayLoop(Group("FrontDrivewayLoop", surroundings), art);

            // 7. Abandoned Snow-Covered Vehicles (School Buses & Cars)
            BuildWinterVehicles(Group("WinterVehicles", surroundings), art);

            MarkStatic(surroundings);
        }

        // =========================================================================
        // 1. DISTANT BLUE RIDGE MOUNTAIN BACKDROP
        // =========================================================================
        private static void BuildMountainBackdrop(Transform parent, BurleySchoolArt art)
        {
            // Monumental snow-capped mountain ridges encircling Charlottesville
            (Vector3 pos, Vector3 size, float rotY)[] ridges =
            {
                // North Ridge (visible behind Booker T. Park)
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
        // 2. BOOKER T. WASHINGTON PARK WITH FROZEN POND & FOOTBRIDGE
        // =========================================================================
        private static void BuildBookerTPark(Transform parent, BurleySchoolArt art)
        {
            Rect r = L.BookerTParkBounds;
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
        // 3. CHARLOTTESVILLE RESIDENTIAL NEIGHBORHOOD HOUSES
        // =========================================================================
        private static void BuildResidentialNeighborhood(Transform parent, BurleySchoolArt art)
        {
            // Homes along south side of Rose Hill Drive (Z: -32f to -44f)
            for (int i = 0; i < 5; i++)
            {
                float hx = 10f + i * 22f;
                BuildCraftsmanHouse(parent, $"House_RoseHill_{i}", new Vector3(hx, 0f, -38f), 0f, art);
            }

            // Homes along east side of Madison Avenue (X: 254f)
            for (int i = 0; i < 4; i++)
            {
                float hz = -15f + i * 24f;
                BuildCraftsmanHouse(parent, $"House_Madison_{i}", new Vector3(258f, 0f, hz), -90f, art);
            }

            // Homes along west side of Henry Avenue (X: -36f)
            for (int i = 0; i < 3; i++)
            {
                float hz = 20f + i * 24f;
                BuildCraftsmanHouse(parent, $"House_Henry_{i}", new Vector3(-38f, 0f, hz), 90f, art);
            }
        }

        private static void BuildCraftsmanHouse(Transform parent, string name, Vector3 pos, float rotY, BurleySchoolArt art)
        {
            Transform house = Group(name, parent);
            house.position = pos;
            house.rotation = Quaternion.Euler(0f, rotY, 0f);

            // House Main Body (2 stories, clapboard/brick)
            Box("MainBody", house, new Vector3(0f, 3.2f, 0f), new Vector3(10.5f, 6.4f, 8.5f), art.PlasterWall);

            BurleySchoolLandscape.Roof(house, art);

            // Brick Chimney with cold-weather smoke
            Transform chimney = Group("BrickChimney", house);
            chimney.localPosition = new Vector3(3.2f, 7.8f, 1.8f);
            Box("ChimneyShaft", chimney, Vector3.zero, new Vector3(1.1f, 3.2f, 1.1f), art.Brick);
            Box("ChimneyCapSnow", chimney, new Vector3(0f, 1.7f, 0f), new Vector3(1.2f, 0.2f, 1.2f), art.Snow, false);

            // Front Covered Porch
            Transform porch = Group("FrontPorch", house);
            porch.localPosition = new Vector3(0f, 0f, 4.8f);
            Box("PorchDeck", porch, new Vector3(0f, 0.25f, 0f), new Vector3(7.5f, 0.5f, 2.2f), art.Concrete);
            Box("PorchRoof", porch, new Vector3(0f, 3.2f, 0f), new Vector3(7.8f, 0.4f, 2.4f), art.PlasterWall);
            Box("PorchRoofSnow", porch, new Vector3(0f, 3.5f, 0f), new Vector3(7.9f, 0.25f, 2.5f), art.Snow, false);
            // Porch Columns
            Box("PorchCol_L", porch, new Vector3(-3.4f, 1.7f, 0.8f), new Vector3(0.35f, 2.6f, 0.35f), art.Limestone);
            Box("PorchCol_R", porch, new Vector3(3.4f, 1.7f, 0.8f), new Vector3(0.35f, 2.6f, 0.35f), art.Limestone);

            // Frosted Windows with warm interior light
            Box("Window_1", house, new Vector3(-2.8f, 2.2f, 4.28f), new Vector3(1.4f, 1.6f, 0.04f), art.WindowGlass, false);
            Box("Window_2", house, new Vector3(2.8f, 2.2f, 4.28f), new Vector3(1.4f, 1.6f, 0.04f), art.WindowGlass, false);
            Box("UpperWindow_1", house, new Vector3(-2.8f, 4.8f, 4.28f), new Vector3(1.4f, 1.4f, 0.04f), art.WindowGlass, false);
            Box("UpperWindow_2", house, new Vector3(2.8f, 4.8f, 4.28f), new Vector3(1.4f, 1.4f, 0.04f), art.WindowGlass, false);

            // White Picket Fence along front yard with snow caps
            Transform fence = Group("FrontYardPicketFence", house);
            fence.localPosition = new Vector3(0f, 0f, 7.5f);
            Box("FenceRail", fence, new Vector3(0f, 0.55f, 0f), new Vector3(12.0f, 0.08f, 0.04f), art.WhiteStripe);
            Box("FenceSnowCap", fence, new Vector3(0f, 0.65f, 0f), new Vector3(12.1f, 0.12f, 0.12f), art.Snow, false);
            for (float px = -5.8f; px <= 5.8f; px += 0.45f)
                Box($"Picket_{px:0.0}", fence, new Vector3(px, 0.5f, 0f), new Vector3(0.08f, 1.0f, 0.02f), art.WhiteStripe, false);

            // Mailbox with snow pile
            Transform mailbox = Group("Mailbox", house);
            mailbox.localPosition = new Vector3(5.5f, 0f, 8.2f);
            Box("Post", mailbox, new Vector3(0f, 0.55f, 0f), new Vector3(0.1f, 1.1f, 0.1f), art.OakWood);
            Box("Box", mailbox, new Vector3(0f, 1.15f, 0f), new Vector3(0.35f, 0.35f, 0.55f), art.MetalSteel, false);
            Box("MailboxSnow", mailbox, new Vector3(0f, 1.38f, 0f), new Vector3(0.38f, 0.14f, 0.58f), art.Snow, false);
        }

        // =========================================================================
        // 4. ROAD NETWORKS & UTILITY INFRASTRUCTURE
        // =========================================================================
        private static void BuildRoadNetworks(Transform parent, BurleySchoolArt art)
        {
            const float roadW = L.RoadWidth;

            // 1. Rose Hill Drive (Across front of school: Z: -14f, X: -50 to 260)
            Transform roseHill = Group("RoseHillDrive", parent);
            roseHill.position = new Vector3(105f, -0.06f, L.RoseHillZ);
            Box("AsphaltSlab", roseHill, Vector3.zero, new Vector3(310f, 0.2f, roadW), art.SnowyAsphalt);
            // Plowed snow ridges on North and South curbs
            Box("PlowRidge_N", roseHill, new Vector3(0f, 0.25f, roadW * 0.5f), new Vector3(310f, 0.5f, 1.2f), art.SnowDrift);
            Box("PlowRidge_S", roseHill, new Vector3(0f, 0.25f, -roadW * 0.5f), new Vector3(310f, 0.5f, 1.2f), art.SnowDrift);

            // 2. Madison Avenue (Running along eastern edge past track: X: 240f, Z: -60 to 180)
            Transform madison = Group("MadisonAvenue", parent);
            madison.position = new Vector3(L.MadisonAveX, -0.06f, 60f);
            Box("AsphaltSlab", madison, Vector3.zero, new Vector3(roadW, 0.2f, 240f), art.SnowyAsphalt);
            Box("PlowRidge_W", madison, new Vector3(-roadW * 0.5f, 0.25f, 0f), new Vector3(1.2f, 0.5f, 240f), art.SnowDrift);
            Box("PlowRidge_E", madison, new Vector3(roadW * 0.5f, 0.25f, 0f), new Vector3(1.2f, 0.5f, 240f), art.SnowDrift);

            // 3. Henry Avenue (Northwest side: X: -18f, Z: -20 to 90)
            Transform henry = Group("HenryAvenue", parent);
            henry.position = new Vector3(L.HenryAveX, -0.06f, 35f);
            Box("AsphaltSlab", henry, Vector3.zero, new Vector3(roadW, 0.2f, 110f), art.SnowyAsphalt);

            // 4. Concord Avenue (Intersecting Rose Hill Dr: X: 26f, Z: -60 to -14)
            Transform concord = Group("ConcordAvenue", parent);
            concord.position = new Vector3(L.ConcordAveX, -0.06f, -37f);
            Box("AsphaltSlab", concord, Vector3.zero, new Vector3(roadW, 0.2f, 46f), art.SnowyAsphalt);

            // Utility Telephone Poles with crossbars and transformers along Rose Hill Dr
            for (int p = 0; p < 7; p++)
            {
                float px = -30f + p * 45f;
                Transform pole = Group($"UtilityPole_{p}", parent);
                pole.position = new Vector3(px, 0f, L.RoseHillZ - roadW * 0.5f - 1.2f);
                Box("WoodMast", pole, new Vector3(0f, 5.0f, 0f), new Vector3(0.25f, 10.0f, 0.25f), art.OakWood);
                Box("Crossbar", pole, new Vector3(0f, 9.4f, 0f), new Vector3(2.4f, 0.15f, 0.15f), art.OakWood, false);
                Box("SnowCap", pole, new Vector3(0f, 9.55f, 0f), new Vector3(2.45f, 0.15f, 0.2f), art.Snow, false);
                // Heavy cylindrical transformer on every second pole
                if (p % 2 == 1)
                    Box("Transformer", pole, new Vector3(0.45f, 8.2f, 0f), new Vector3(0.55f, 0.85f, 0.55f), art.MetalSteel, false);
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
                // Front lawn & Entrance perimeter
                new(12f, 0f, 8f), new(16f, 0f, 12f), new(76f, 0f, 8f), new(82f, 0f, 12f),
                // West Wing light court
                new(-8f, 0f, 32f), new(-8f, 0f, 48f),
                // North Wing garden court
                new(16f, 0f, 68f), new(18f, 0f, 76f),
                // Parking Lot border
                new(102f, 0f, 94f), new(132f, 0f, 88f),
                // Booker T. Washington Park Grove
                new(95f, 0f, 142f), new(110f, 0f, 155f), new(135f, 0f, 148f), new(150f, 0f, 160f),
                new(85f, 0f, 165f), new(125f, 0f, 175f), new(165f, 0f, 150f),
                // Athletics perimeter & track banks
                new(132f, 0f, 65f), new(132f, 0f, -25f), new(225f, 0f, 25f), new(225f, 0f, -15f)
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
            Box("ServiceWalkEast", parent, new Vector3(101.4f, -.035f, 46f), new Vector3(2.4f, .12f, 52f), art.Sidewalk);

            // Flagpole Plaza on Front Lawn
            Transform flagPlaza = Group("FlagpolePlaza", lawn);
            flagPlaza.localPosition = new Vector3(0f, 0.1f, 1.5f);
            Box("PlazaSlab", flagPlaza, Vector3.zero, new Vector3(6.5f, 0.15f, 4.5f), art.Concrete);

            // American Flag (Center)
            BuildFlagpole(flagPlaza, "MainFlagpole", Vector3.zero, 11f, art.Limestone, art.GoldAccent, art.WhiteStripe);
            // Burley Bears School Flag (Kelly Green and Old Gold)
            BuildFlagpole(flagPlaza, "SchoolFlagpole", new Vector3(-2.2f, 0f, 0f), 8.5f, art.Limestone, art.GoldAccent, art.LockerGreen);

            // Covered Bus Stop Shelter along curb with frosted glass & snow roof
            Transform busStop = Group("BusStopShelter", parent);
            busStop.position = new Vector3(25f, 0f, -7.5f);
            Box("ShelterPad", busStop, new Vector3(0f, 0.05f, 0f), new Vector3(4.5f, 0.1f, 2.5f), art.Concrete);
            Box("Post_L", busStop, new Vector3(-2.0f, 1.3f, 1.0f), new Vector3(0.1f, 2.6f, 0.1f), art.MetalSteel);
            Box("Post_R", busStop, new Vector3(2.0f, 1.3f, 1.0f), new Vector3(0.1f, 2.6f, 0.1f), art.MetalSteel);
            Box("GlassBack", busStop, new Vector3(0f, 1.3f, 1.0f), new Vector3(3.9f, 2.2f, 0.04f), art.WindowGlass, false);
            Box("RoofCanopy", busStop, new Vector3(0f, 2.65f, 0f), new Vector3(4.8f, 0.15f, 2.8f), art.MetalSteel);
            Box("RoofSnowMantle", busStop, new Vector3(0f, 2.85f, 0f), new Vector3(4.9f, 0.25f, 2.9f), art.Snow, false);
            Box("Bench", busStop, new Vector3(0f, 0.45f, 0.6f), new Vector3(3.2f, 0.08f, 0.4f), art.OakWood);
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

            // Abandoned snow-dusted sedans along roads
            BuildWinterSedan(parent, "Sedan_RoseHill_1", new Vector3(58f, 0f, L.RoseHillZ + 2.5f), 5f, art);
            BuildWinterSedan(parent, "Sedan_RoseHill_2", new Vector3(-8f, 0f, L.RoseHillZ - 2.5f), 180f, art);
            BuildWinterSedan(parent, "Sedan_Madison", new Vector3(L.MadisonAveX - 2.5f, 0f, 25f), 90f, art);
        }

        private static void BuildSchoolBus(Transform parent, string name, Vector3 pos, float rotY, BurleySchoolArt art)
        {
            BurleySchoolCampusProps.Bus(parent, name, pos, rotY, art);
        }

        private static void BuildWinterSedan(Transform parent, string name, Vector3 pos, float rotY, BurleySchoolArt art)
        {
            BurleySchoolCampusProps.Sedan(parent, name, pos, rotY, art);
        }
    }
}

using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the surrounding Charlottesville neighborhood, road networks (Rose Hill Dr,
    /// Henry Ave, Concord Ave, Madison Ave), residential homes, Booker T. Washington Park,
    /// and the front entrance driveway drop-off loop with flagpoles and bus stop.
    /// </summary>
    public static class BurleySchoolSurroundings
    {
        public static void Build(Transform root, BurleySchoolArt art)
        {
            Transform surroundings = Group("CampusSurroundings", root);

            // 1. Front Semicircular Driveway Drop-off Loop & Campus Lawn
            BuildFrontDrivewayLoop(Group("FrontDrivewayLoop", surroundings), art);

            // 2. Road Network (Rose Hill Dr, Madison Ave, Henry Ave, Concord Ave)
            BuildRoadNetworks(Group("RoadNetworks", surroundings), art);

            // 3. Charlottesville Residential Neighborhood Houses
            BuildResidentialNeighborhood(Group("ResidentialNeighborhood", surroundings), art);

            // 4. Booker T. Washington Park & Landscape
            BuildBookerTPark(Group("BookerTWashingtonPark", surroundings), art);

            // 5. Mature Trees & Campus Foliage
            BuildCampusTrees(Group("CampusTrees", surroundings), art);

            MarkStatic(surroundings);
        }

        // =========================================================================
        // 1. FRONT DRIVEWAY LOOP & CAMPUS LAWN
        // =========================================================================
        private static void BuildFrontDrivewayLoop(Transform parent, BurleySchoolArt art)
        {
            // Semicircular asphalt driveway sweeping off Rose Hill Drive past front portico
            Vector3 loopCenter = new(46f, -0.05f, 4f);

            // Driveway asphalt loop surface (spans from Z: -8 to Z: 18, X: 22 to X: 70)
            Box("DrivewayLoopAsphalt", parent, loopCenter, new Vector3(48f, 0.2f, 22f), art.RoadAsphalt);

            // Inside front island grass lawn
            Transform lawn = Group("FrontCampusLawn", parent);
            lawn.position = loopCenter + new Vector3(0f, 0.02f, -2.5f);
            Box("LawnTurf", lawn, Vector3.zero, new Vector3(32f, 0.22f, 13f), art.TurfGrass);
            // Concrete curbing around lawn
            Box("LawnCurb_N", lawn, new Vector3(0f, 0.15f, 6.5f), new Vector3(32.4f, 0.25f, 0.35f), art.Limestone);
            Box("LawnCurb_S", lawn, new Vector3(0f, 0.15f, -6.5f), new Vector3(32.4f, 0.25f, 0.35f), art.Limestone);
            Box("LawnCurb_W", lawn, new Vector3(-16f, 0.15f, 0f), new Vector3(0.35f, 0.25f, 13.4f), art.Limestone);
            Box("LawnCurb_E", lawn, new Vector3(16f, 0.15f, 0f), new Vector3(0.35f, 0.25f, 13.4f), art.Limestone);

            // Flagpole Plaza on Front Lawn
            Transform flagPlaza = Group("FlagpolePlaza", lawn);
            flagPlaza.localPosition = new Vector3(0f, 0.1f, 1.5f);
            Box("PlazaSlab", flagPlaza, Vector3.zero, new Vector3(6.5f, 0.15f, 4.5f), art.Concrete);

            // American Flag (Center)
            BuildFlagpole(flagPlaza, "MainFlagpole", Vector3.zero, 11f, art.Limestone, art.GoldAccent, art.WhiteStripe);
            // Burley Bears School Flag (Kelly Green and Old Gold)
            BuildFlagpole(flagPlaza, "SchoolFlagpole", new Vector3(-2.2f, 0f, 0f), 8.5f, art.Limestone, art.GoldAccent, art.LockerGreen);

            // Front Covered Bus Stop Shelter along curb
            Transform busStop = Group("BusStopShelter", parent);
            busStop.position = new Vector3(25f, 0f, -7.5f);
            Box("ShelterPad", busStop, new Vector3(0f, 0.05f, 0f), new Vector3(4.5f, 0.1f, 2.5f), art.Concrete);
            Box("Post_L", busStop, new Vector3(-2.0f, 1.3f, 1.0f), new Vector3(0.1f, 2.6f, 0.1f), art.MetalSteel);
            Box("Post_R", busStop, new Vector3(2.0f, 1.3f, 1.0f), new Vector3(0.1f, 2.6f, 0.1f), art.MetalSteel);
            Box("GlassBack", busStop, new Vector3(0f, 1.3f, 1.0f), new Vector3(3.9f, 2.2f, 0.04f), art.WindowGlass, false);
            Box("RoofCanopy", busStop, new Vector3(0f, 2.65f, 0f), new Vector3(4.8f, 0.15f, 2.8f), art.MetalSteel);
            Box("Bench", busStop, new Vector3(0f, 0.45f, 0.6f), new Vector3(3.2f, 0.08f, 0.4f), art.OakWood);
            // Bus Stop Sign
            Box("BusSignPole", busStop, new Vector3(-2.4f, 1.1f, -1.0f), new Vector3(0.06f, 2.2f, 0.06f), art.MetalSteel);
            Box("BusSignFace", busStop, new Vector3(-2.4f, 1.95f, -1.0f), new Vector3(0.45f, 0.6f, 0.03f), art.GoldAccent, false);
            Label(busStop, "BusSignText", "BUS\nSTOP", new Vector3(-2.4f, 1.95f, -1.03f),
                Quaternion.Euler(0f, 180f, 0f), 0.08f, art.LockerGreen.GetColor("_BaseColor"));
        }

        private static void BuildFlagpole(Transform parent, string name, Vector3 pos, float height, Material poleMat, Material ballMat, Material flagMat)
        {
            Transform pole = Group(name, parent);
            pole.localPosition = pos;
            Box("SteelPole", pole, new Vector3(0f, height * 0.5f, 0f), new Vector3(0.12f, height, 0.12f), poleMat);
            Box("GoldBallFinial", pole, new Vector3(0f, height + 0.15f, 0f), new Vector3(0.35f, 0.35f, 0.35f), ballMat, false);
            // Flag banner waving in wind
            Box("FlagBanner", pole, new Vector3(1.1f, height - 1.1f, 0f), new Vector3(2.1f, 1.35f, 0.03f), flagMat, false);
        }

        // =========================================================================
        // 2. ROAD NETWORKS
        // =========================================================================
        private static void BuildRoadNetworks(Transform parent, BurleySchoolArt art)
        {
            const float roadW = L.RoadWidth;

            // --- 1. Rose Hill Drive (Runs East-West along front, Z: -14) ---
            Transform roseHill = Group("RoseHillDrive", parent);
            roseHill.position = new Vector3(100f, -0.15f, L.RoseHillZ);
            // 2-lane asphalt roadway (spanning 320 metres across entire front)
            Box("AsphaltPavement", roseHill, Vector3.zero, new Vector3(320f, 0.2f, roadW), art.RoadAsphalt);
            // Double yellow centerlines
            Box("YellowCenter_1", roseHill, new Vector3(0f, 0.12f, 0.1f), new Vector3(320f, 0.02f, 0.12f), art.YellowStripe, false);
            Box("YellowCenter_2", roseHill, new Vector3(0f, 0.12f, -0.1f), new Vector3(320f, 0.02f, 0.12f), art.YellowStripe, false);
            // White edge lines
            Box("WhiteEdge_N", roseHill, new Vector3(0f, 0.12f, roadW * 0.5f - 0.2f), new Vector3(320f, 0.02f, 0.12f), art.WhiteStripe, false);
            Box("WhiteEdge_S", roseHill, new Vector3(0f, 0.12f, -roadW * 0.5f + 0.2f), new Vector3(320f, 0.02f, 0.12f), art.WhiteStripe, false);
            // Concrete Sidewalks & Curbs on both sides
            Box("Sidewalk_N", roseHill, new Vector3(0f, 0.1f, roadW * 0.5f + 1.25f), new Vector3(320f, 0.25f, 2.5f), art.Sidewalk);
            Box("Sidewalk_S", roseHill, new Vector3(0f, 0.1f, -roadW * 0.5f - 1.25f), new Vector3(320f, 0.25f, 2.5f), art.Sidewalk);

            // Striped Crosswalk at front entrance
            for (int stripe = -4; stripe <= 4; stripe++)
            {
                Box($"CrosswalkStripe_{stripe}", roseHill, new Vector3(stripe * 1.5f - 54f, 0.12f, 0f), new Vector3(0.65f, 0.025f, roadW - 0.8f), art.WhiteStripe, false);
            }

            // --- 2. Madison Avenue (Runs North-South along east boundary, X: 240) ---
            Transform madison = Group("MadisonAvenue", parent);
            madison.position = new Vector3(L.MadisonAveX, -0.15f, 50f);
            Box("MadisonAsphalt", madison, Vector3.zero, new Vector3(roadW, 0.2f, 240f), art.RoadAsphalt);
            Box("MadisonYellow_1", madison, new Vector3(0.1f, 0.12f, 0f), new Vector3(0.12f, 0.02f, 240f), art.YellowStripe, false);
            Box("MadisonYellow_2", madison, new Vector3(-0.1f, 0.12f, 0f), new Vector3(0.12f, 0.02f, 240f), art.YellowStripe, false);
            Box("MadisonSidewalk_W", madison, new Vector3(-roadW * 0.5f - 1.25f, 0.1f, 0f), new Vector3(2.5f, 0.25f, 240f), art.Sidewalk);
            Box("MadisonSidewalk_E", madison, new Vector3(roadW * 0.5f + 1.25f, 0.1f, 0f), new Vector3(2.5f, 0.25f, 240f), art.Sidewalk);

            // --- 3. Henry Avenue (Runs North-South along northwest boundary, X: -18) ---
            Transform henry = Group("HenryAvenue", parent);
            henry.position = new Vector3(L.HenryAveX, -0.15f, 60f);
            Box("HenryAsphalt", henry, Vector3.zero, new Vector3(roadW, 0.2f, 180f), art.RoadAsphalt);
            Box("HenrySidewalk_E", henry, new Vector3(roadW * 0.5f + 1.25f, 0.1f, 0f), new Vector3(2.5f, 0.25f, 180f), art.Sidewalk);

            // --- 4. Concord Avenue (Intersecting Rose Hill Dr heading south, X: 26) ---
            Transform concord = Group("ConcordAvenue", parent);
            concord.position = new Vector3(L.ConcordAveX, -0.15f, -48f);
            Box("ConcordAsphalt", concord, Vector3.zero, new Vector3(roadW, 0.2f, 60f), art.RoadAsphalt);
            Box("ConcordSidewalk_W", concord, new Vector3(-roadW * 0.5f - 1.25f, 0.1f, 0f), new Vector3(2.5f, 0.25f, 60f), art.Sidewalk);
            Box("ConcordSidewalk_E", concord, new Vector3(roadW * 0.5f + 1.25f, 0.1f, 0f), new Vector3(2.5f, 0.25f, 60f), art.Sidewalk);
        }

        // =========================================================================
        // 3. RESIDENTIAL NEIGHBORHOOD HOUSES
        // =========================================================================
        private static void BuildResidentialNeighborhood(Transform parent, BurleySchoolArt art)
        {
            // Array of authentic mid-century residential homes lining the streets (Charlottesville architecture)
            // Houses along South side of Rose Hill Drive
            for (int h = 0; h < 6; h++)
            {
                float hx = -30f + h * 42f;
                BuildResidentialHouse(parent, $"House_RoseHill_S_{h}", new Vector3(hx, 0f, -34f), 0f, (h % 2 == 0) ? art.Brick : art.PlasterWall, art);
            }

            // Houses along Madison Avenue (East side, facing west towards the athletic track)
            for (int h = 0; h < 4; h++)
            {
                float hz = -20f + h * 38f;
                BuildResidentialHouse(parent, $"House_Madison_E_{h}", new Vector3(258f, 0f, hz), -90f, (h % 2 == 0) ? art.PlasterWall : art.Brick, art);
            }

            // Houses along Henry Avenue (West side, facing east towards the school)
            for (int h = 0; h < 3; h++)
            {
                float hz = 30f + h * 38f;
                BuildResidentialHouse(parent, $"House_Henry_W_{h}", new Vector3(-36f, 0f, hz), 90f, art.PlasterWall, art);
            }
        }

        private static void BuildResidentialHouse(Transform parent, string name, Vector3 pos, float rotY, Material wallMat, BurleySchoolArt art)
        {
            Transform house = Group(name, parent);
            house.position = pos;
            house.rotation = Quaternion.Euler(0f, rotY, 0f);

            // House yard lawn
            Box("YardLawn", house, new Vector3(0f, -0.05f, 0f), new Vector3(28f, 0.2f, 26f), art.TurfGrass);

            // Main House Structure (Two-story / Split-level Charlottesville home)
            Box("HouseBody", house, new Vector3(0f, 2.8f, 0f), new Vector3(14.0f, 5.6f, 10.5f), wallMat);

            // Pitched Gabled Roof (Dark shingles)
            Transform roof = Group("PitchedRoof", house);
            roof.localPosition = new Vector3(0f, 5.6f, 0f);
            Box("RoofRidge", roof, new Vector3(0f, 1.6f, 0f), new Vector3(14.8f, 0.2f, 0.4f), art.RoofGravel, false);
            // Roof Slopes
            Box("RoofSlopeFront", roof, new Vector3(0f, 0.8f, 2.7f), new Vector3(14.8f, 0.15f, 5.8f), art.RoofGravel, false);
            Box("RoofSlopeBack", roof, new Vector3(0f, 0.8f, -2.7f), new Vector3(14.8f, 0.15f, 5.8f), art.RoofGravel, false);

            // Brick Chimney
            Box("HouseChimney", house, new Vector3(4.5f, 6.2f, 1.5f), new Vector3(1.2f, 3.2f, 1.2f), art.BrickDark);

            // Front Porch with wooden railing and steps
            Transform porch = Group("FrontPorch", house);
            porch.localPosition = new Vector3(0f, 0f, 6.0f);
            Box("PorchDeck", porch, new Vector3(0f, 0.35f, 0f), new Vector3(5.2f, 0.3f, 2.2f), art.Limestone);
            Box("PorchRoof", porch, new Vector3(0f, 2.8f, 0f), new Vector3(5.4f, 0.2f, 2.4f), art.RoofGravel, false);
            Box("Pillar_L", porch, new Vector3(-2.3f, 1.5f, 0.9f), new Vector3(0.18f, 2.4f, 0.18f), art.Limestone);
            Box("Pillar_R", porch, new Vector3(2.3f, 1.5f, 0.9f), new Vector3(0.18f, 2.4f, 0.18f), art.Limestone);
            Box("Steps", porch, new Vector3(0f, 0.15f, 1.5f), new Vector3(2.4f, 0.3f, 0.8f), art.Limestone);

            // Windows and Front Door
            Box("FrontDoor", house, new Vector3(0f, 1.3f, 5.3f), new Vector3(1.1f, 2.2f, 0.08f), art.OakWood, false);
            Box("Window_L", house, new Vector3(-3.5f, 1.6f, 5.3f), new Vector3(1.6f, 1.5f, 0.08f), art.WindowGlass, false);
            Box("Window_R", house, new Vector3(3.5f, 1.6f, 5.3f), new Vector3(1.6f, 1.5f, 0.08f), art.WindowGlass, false);

            // Driveway with Mailbox
            Box("HouseDriveway", house, new Vector3(8.5f, -0.02f, 3.5f), new Vector3(3.5f, 0.15f, 16f), art.Asphalt);
            Box("MailboxPost", house, new Vector3(6.2f, 0.55f, 11.5f), new Vector3(0.08f, 1.1f, 0.08f), art.OakWood);
            Box("MailboxBox", house, new Vector3(6.2f, 1.15f, 11.5f), new Vector3(0.22f, 0.25f, 0.45f), art.MetalSteel, false);
        }

        // =========================================================================
        // 4. BOOKER T. WASHINGTON PARK
        // =========================================================================
        private static void BuildBookerTPark(Transform parent, BurleySchoolArt art)
        {
            Rect bounds = L.BookerTParkBounds;
            Vector3 parkCenter = new(bounds.center.x, -0.1f, bounds.center.y);

            // Rolling green park terrain slab
            Box("ParkGroundTurf", parent, parkCenter, new Vector3(bounds.width, 0.2f, bounds.height), art.TurfGrass);

            // Paved pedestrian walking trail meandering through the park
            for (int t = 0; t < 12; t++)
            {
                float tx = bounds.xMin + 15f + t * 10f;
                float tz = bounds.yMin + 20f + Mathf.Sin(t * 0.6f) * 12f;
                Box($"ParkTrail_{t}", parent, new Vector3(tx, 0.02f, tz), new Vector3(10.5f, 0.15f, 2.8f), art.Sidewalk);
            }

            // Park Amenities: Wooden Picnic Tables & Benches
            Vector3[] picnicSpots =
            {
                new(bounds.xMin + 30f, 0f, bounds.yMin + 35f),
                new(bounds.xMin + 70f, 0f, bounds.yMin + 45f),
                new(bounds.xMin + 110f, 0f, bounds.yMin + 30f)
            };

            for (int p = 0; p < picnicSpots.Length; p++)
            {
                Transform table = Group($"PicnicTable_{p}", parent);
                table.position = picnicSpots[p];
                Box("TableTop", table, new Vector3(0f, 0.75f, 0f), new Vector3(2.4f, 0.08f, 1.1f), art.OakWood);
                Box("TableLegs", table, new Vector3(0f, 0.38f, 0f), new Vector3(2.1f, 0.72f, 0.9f), art.OakWood);
                Box("Bench_N", table, new Vector3(0f, 0.45f, 0.85f), new Vector3(2.4f, 0.08f, 0.35f), art.OakWood);
                Box("Bench_S", table, new Vector3(0f, 0.45f, -0.85f), new Vector3(2.4f, 0.08f, 0.35f), art.OakWood);
            }
        }

        // =========================================================================
        // 5. CAMPUS & NEIGHBORHOOD TREES
        // =========================================================================
        private static void BuildCampusTrees(Transform parent, BurleySchoolArt art)
        {
            // Trees lining Rose Hill Drive sidewalk
            for (int i = 0; i < 9; i++)
            {
                float tx = -20f + i * 26f;
                BuildDeciduousTree(parent, $"StreetTree_RoseHill_{i}", new Vector3(tx, 0f, -7.5f), 6.5f + (i % 3) * 0.8f, art);
            }

            // Trees lining Madison Avenue
            for (int i = 0; i < 7; i++)
            {
                float tz = -30f + i * 25f;
                BuildDeciduousTree(parent, $"StreetTree_Madison_{i}", new Vector3(232f, 0f, tz), 7.0f + (i % 2) * 1.0f, art);
            }

            // Trees in Booker T. Washington Park
            for (int i = 0; i < 10; i++)
            {
                float tx = 95f + (i % 5) * 28f;
                float tz = 150f + (i / 5) * 32f + (i % 2) * 8f;
                BuildDeciduousTree(parent, $"ParkTree_{i}", new Vector3(tx, 0f, tz), 8.5f + (i % 3) * 1.2f, art);
            }
        }

        private static void BuildDeciduousTree(Transform parent, string name, Vector3 pos, float height, BurleySchoolArt art)
        {
            Transform tree = Group(name, parent);
            tree.position = pos;
            float trunkH = height * 0.38f;
            float crownH = height * 0.62f;
            float crownW = height * 0.55f;

            // Wooden Tree Trunk (Solid collider)
            Box("Trunk", tree, new Vector3(0f, trunkH * 0.5f, 0f), new Vector3(0.55f, trunkH, 0.55f), art.StageWood);

            // Lush Foliage Canopy (Multi-tier leafy crown)
            Box("CanopyTier_1", tree, new Vector3(0f, trunkH + crownH * 0.35f, 0f), new Vector3(crownW, crownH * 0.6f, crownW), art.TurfGrass, false);
            Box("CanopyTier_2", tree, new Vector3(0f, trunkH + crownH * 0.75f, 0f), new Vector3(crownW * 0.72f, crownH * 0.55f, crownW * 0.72f), art.TurfGrass, false);
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

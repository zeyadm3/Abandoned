using UnityEngine;
using F = Abandoned.EditorTools.StationPlan.Finish;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Millbrook Fire Station No. 4's floor plan as data (Docs/FIRE_STATION_PLAN.md): a brick firehouse 64 x 40 m on the
    /// 4 m grid (16 x 10 cells). The west half is the four-lane apparatus bay (two engines and the ladder truck in; the
    /// second lane empty and kept clear: the haul lane), double height at the front, with the bunk rooms over its back
    /// third and a hose mezzanine along its east side. The east half is two storeys of quarters over a basement. The hose
    /// tower (stairs to a 16 m bell deck) stands on the north-west corner as its own structure (<see cref="FireStationTower"/>).
    /// South is the street; the crew's van backs up to the haul lane's door.
    /// </summary>
    public static class FireStationLayout
    {
        public const string SceneName = "FireStation";
        public const string ScenePath = "Assets/_Project/Scenes/FireStation.unity";
        public const string DisplayName = "Millbrook Fire Station No. 4";
        public const string ArtFolder = "Assets/_Project/Art/FireStation";
        public const string LootFolder = "Assets/_Project/Data/Loot/FireStation";
        public const string LootPrefix = "fs_";

        public const string Apparatus = "fire_apparatus", Quarters = "fire_quarters", Tower = "fire_tower", Watch = "fire_watch";

        public const int Width = 16, Depth = 10;
        public const float BayDoorWidth = 4.2f, BayDoorHeight = 4.6f;
        /// <summary>The four bay doors' centres (x). Lane 2 (x 12) is the haul lane: empty, its door the way out.</summary>
        public static readonly float[] BayDoors = { 4f, 12f, 20f, 28f };
        public static readonly Vector3 HaulDoor = new(12f, 0f, 0f);
        /// <summary>Where the van parks: its rear doors 11 m out from the haul lane's door, cab to the street.</summary>
        public static readonly Vector3 VanRear = new(12f, 0f, -11f);
        public static readonly Vector3 VanForward = Vector3.back;
        public const float WatchDoorX = 38f, RearDoorX = 34f;

        /// <summary>The hose tower: cells x 0-1, z 7-9 (8 x 12 m), landings every 4 m up to the bell deck at 16 m.</summary>
        public static readonly RectInt TowerCells = new(0, 7, 2, 3);
        public const float TowerDeck = 16f, TowerTop = 20f;

        /// <summary>The fire pole: through the slab of bunk room B at cell (5, 5), down to the back of the bay.</summary>
        public static readonly Vector2Int PoleCell = new(5, 5);
        public const float PoleHole = 1.4f;

        private static P cached;
        public static P Plan => cached ??= Create();

        private static P Create()
        {
            var p = new P
            {
                SceneName = SceneName, ScenePath = ScenePath, DisplayName = DisplayName, ArtFolder = ArtFolder, LogTag = "FireStation",
                Width = Width, Depth = Depth, Stability = .68f, Seed = 4141,
            };
            p.FillFloor(0, new RectInt(0, 0, Width, Depth));
            p.FillFloor(1, new RectInt(0, 4, 8, 3));     // the bunk rooms, over the back of the bay
            p.FillFloor(1, new RectInt(7, 0, 1, 4));     // the hose mezzanine along the bay's east side
            p.FillFloor(1, new RectInt(2, 7, 6, 3));     // the storage loft over the workshop and the SCBA room
            p.FillFloor(1, new RectInt(8, 0, 8, Depth)); // the quarters
            p.FillFloor(-1, new RectInt(9, 6, 7, 4));
            p.FillFloor(-1, new RectInt(11, 5, 5, 1));
            p.Hole(0, new Vector2Int(10, 6), new Vector2Int(10, 7));
            p.Hole(1, new Vector2Int(9, 6), new Vector2Int(9, 7));
            for (int x = TowerCells.xMin; x < TowerCells.xMax; x++)
            for (int z = TowerCells.yMin; z < TowerCells.yMax; z++) p.Custom.Add(new Vector2Int(x, z));
            p.Pierced[P.Key(1, PoleCell)] = PoleHole;

            Ground(p);
            Upper(p);
            Basement(p);
            p.Stairs.Add(new P.Flight("MainStair", 0, new Vector3(38f, 0f, 24f), Vector3.forward));
            p.Stairs.Add(new P.Flight("BasementStair", -1, new Vector3(42f, -P.Story, 32f), Vector3.back));
            p.Open(0, 10, 5, 10, 6);
            p.Open(1, 9, 7, 9, 8);
            for (int z = 0; z < 4; z++) p.Rail(1, 6, z, 7, z);   // the mezzanine looks over the bay

            foreach (float x in BayDoors)
                p.Facade.Add(new P.Opening("BayDoor", P.Side.South, x, BayDoorWidth, 0f, BayDoorHeight, false));
            p.Facade.Add(new P.Opening("WatchDoor", P.Side.South, WatchDoorX, P.DoorWidth, 0f, 2.5f, false));
            p.Facade.Add(new P.Opening("RearDoor", P.Side.North, RearDoorX, 1.4f, 0f, 2.4f, false));
            Strength(p);
            return p;
        }

        private static void Ground(P p)
        {
            p.AddRoom("Apparatus_Bay", 0, 0, 0, 8, 7, Apparatus, F.Brick);
            p.AddRoom("Hose_Tower", 0, 0, 7, 2, 3, Tower, F.Concrete, windows: false);
            p.AddRoom("Workshop", 0, 2, 7, 3, 3, Apparatus, F.Block);
            p.AddRoom("SCBA_Room", 0, 5, 7, 3, 3, Apparatus, F.Tile);
            p.AddRoom("Watch_Office", 0, 8, 0, 3, 2, Watch, F.Panel);
            p.AddRoom("Captain_Office", 0, 11, 0, 2, 2, Watch, F.Panel);
            p.AddRoom("Records_Room", 0, 13, 0, 3, 2, Watch, F.Block);
            p.AddRoom("Turnout_Room", 0, 9, 2, 2, 3, Apparatus, F.Block);
            p.AddRoom("Day_Room", 0, 11, 2, 5, 3, Quarters);
            p.AddRoom("Kitchen", 0, 12, 5, 4, 3, Quarters, F.Tile);
            p.AddRoom("Laundry", 0, 9, 8, 4, 2, Quarters, F.Tile);
            p.AddRoom("Restroom", 0, 13, 8, 3, 2, null, F.Tile);

            p.Door(0, 7, 1, 8, 1);
            p.Door(0, 8, 1, 8, 2);
            p.Door(0, 10, 0, 11, 0);
            p.Door(0, 12, 1, 12, 2);
            p.Door(0, 14, 1, 14, 2);
            p.Door(0, 7, 2, 8, 2, wide: true);    // the bay to the hall at the front
            p.Door(0, 7, 5, 8, 5, wide: true);    // and at the back, by the pole
            p.Door(0, 8, 3, 9, 3, wide: true);
            p.Door(0, 11, 4, 11, 5, wide: true);  // the day room's jukebox goes out here
            p.Door(0, 13, 4, 13, 5);
            p.Door(0, 11, 6, 12, 6);
            p.Door(0, 11, 7, 11, 8);
            p.Door(0, 8, 8, 9, 8);
            p.Door(0, 13, 7, 13, 8);
            p.Door(0, 3, 6, 3, 7, wide: true);    // the workshop to the bay
            p.Door(0, 6, 6, 6, 7, wide: true);    // the SCBA room to the bay
            p.Door(0, 7, 8, 8, 8);
        }

        private static void Upper(P p)
        {
            p.AddRoom("Bunk_Room_A", 1, 0, 4, 4, 3, Quarters);
            p.AddRoom("Bunk_Room_B", 1, 4, 4, 4, 3, Quarters);
            p.AddRoom("Storage_Loft", 1, 2, 7, 6, 3, Watch, F.Panel);
            p.AddRoom("Officers_Quarters", 1, 9, 0, 3, 3, Quarters, F.Panel);
            p.AddRoom("Bathroom", 1, 12, 0, 4, 3, null, F.Tile);
            p.AddRoom("Training_Room", 1, 9, 3, 3, 3, Watch);
            p.AddRoom("Fitness_Room", 1, 12, 3, 4, 3, Quarters, F.Concrete);
            p.AddRoom("Radio_Room", 1, 10, 6, 2, 2, Watch, F.Panel);
            p.AddRoom("Chief_Quarters", 1, 12, 6, 4, 2, Quarters, F.Panel);
            p.AddRoom("Attic_Storage", 1, 10, 8, 6, 2, Watch, F.Panel);

            p.Door(1, 3, 5, 4, 5);
            p.Door(1, 7, 5, 8, 5);
            p.Door(1, 7, 3, 7, 4);
            p.Door(1, 2, 6, 2, 7);
            p.Door(1, 7, 8, 8, 8);
            p.Door(1, 8, 1, 9, 1);
            p.Door(1, 11, 1, 12, 1);
            p.Door(1, 13, 2, 13, 3);
            p.Door(1, 8, 4, 9, 4);
            p.Door(1, 11, 4, 12, 4);
            p.Door(1, 10, 7, 10, 8);
            p.Door(1, 11, 6, 12, 6);
            p.Door(1, 9, 8, 10, 8);
            p.Door(1, 13, 7, 13, 8);
        }

        private static void Basement(P p)
        {
            p.AddRoom("Foam_Store", -1, 9, 6, 1, 2, Apparatus, F.Concrete, windows: false);
            p.AddRoom("Utility_Room", -1, 11, 5, 3, 3, null, F.Concrete, windows: false);
            p.AddRoom("Old_Store", -1, 14, 5, 2, 3, Apparatus, F.Block, windows: false);
            p.AddRoom("Archive", -1, 9, 9, 7, 1, Watch, F.Block, windows: false);
            p.Door(-1, 9, 7, 9, 8);
            p.Door(-1, 12, 7, 12, 8);
            p.Door(-1, 14, 7, 14, 8, wide: true);
            p.Door(-1, 12, 8, 12, 9, wide: true);
        }

        private static void Strength(P p)
        {
            p.Weak(1, new RectInt(0, 4, 8, 3), .65f, .55f);  // the bunk rooms over the bay: they come down onto it
            p.Weak(1, new RectInt(7, 0, 1, 4), .6f, .5f);    // the hose mezzanine, over Engine 9
            p.Weak(1, new RectInt(2, 7, 6, 3), .7f, .6f);    // the storage loft, old timber
            p.Weak(1, new RectInt(12, 3, 4, 3), .75f, .65f); // the fitness room
            p.Weak(0, new RectInt(14, 5, 2, 3), .75f, .65f); // the kitchen's east end, over the old store
            p.UpperRoomsCollapse(1);

            p.Clear(0, new RectInt(2, 0, 2, 7));   // the haul lane
            p.Clear(0, new RectInt(6, 4, 2, 3));   // the back of the bay by the hall door
            p.Clear(0, new RectInt(5, 5, 1, 1));   // where the pole lands
            p.Clear(0, new RectInt(8, 2, 1, 8));   // the hall
            p.Clear(0, new RectInt(9, 5, 3, 1));
            p.Clear(0, new RectInt(11, 5, 1, 3));
            p.Clear(0, new RectInt(9, 6, 1, 2));   // the main stair
            p.Clear(-1, new RectInt(9, 8, 7, 1));
            p.Clear(-1, new RectInt(10, 6, 1, 2));
        }

        public static P.Room Room(string name) => Plan.FindRoom(name);
    }
}

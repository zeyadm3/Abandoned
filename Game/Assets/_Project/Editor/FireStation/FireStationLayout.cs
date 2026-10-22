using UnityEngine;
using F = Abandoned.EditorTools.StationPlan.Finish;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Millbrook Fire Station No. 4's floor plan as data (Docs/FIRE_STATION_PLAN.md): a brick firehouse, 48 x 36 m on the
    /// 4 m grid (12 x 9 cells), a double-height apparatus bay on the west, two storeys of quarters on the east, the hose
    /// tower on the north-west corner (its own structure, <see cref="FireStationTower"/>). South is the street: three
    /// open bay doors, the truck backed up to the middle one.
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

        public const int Width = 12, Depth = 9;
        public const float BayDoorWidth = 4.2f, BayDoorHeight = 4.6f;
        /// <summary>The three bay doors' centres (x); the middle one is the haul lane.</summary>
        public static readonly float[] BayDoors = { 4f, 12f, 20f };
        public static readonly Vector3 HaulDoor = new(12f, 0f, 0f);
        public static readonly Vector3 TruckRear = new(12f, 0f, -11f);
        public static readonly Vector3 TruckForward = Vector3.back;

        /// <summary>The hose tower's cells and its platform heights (north half, south half, north half).</summary>
        public static readonly RectInt TowerCells = new(0, 7, 2, 2);
        public static readonly float[] Platforms = { 5f, 10f, 15f };
        public const float TowerTop = 18f;

        /// <summary>The fire pole: through the upper hall at cell (6, 2), down to the hall by the bay.</summary>
        public static readonly Vector2Int PoleCell = new(6, 2);
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
            p.FillFloor(1, new RectInt(6, 0, 6, Depth));       // the quarters wing and the hall
            p.FillFloor(1, new RectInt(5, 5, 1, 2));           // the hose gallery over the bay
            p.FillFloor(1, new RectInt(2, 7, 4, 2));           // the storage loft over the workshop
            for (int x = TowerCells.xMin; x < TowerCells.xMax; x++)
            for (int z = TowerCells.yMin; z < TowerCells.yMax; z++) p.Custom.Add(new Vector2Int(x, z));
            p.Pierced[P.Key(1, PoleCell)] = PoleHole;

            p.AddRoom("Apparatus_Bay", 0, 0, 0, 6, 7, Apparatus, F.Brick);
            p.AddRoom("Watch_Office", 0, 6, 0, 3, 2, Watch, F.Panel);
            p.AddRoom("Station_Office", 0, 9, 0, 3, 2, Watch, F.Panel);
            p.AddRoom("Day_Room", 0, 7, 2, 5, 3, Quarters);
            p.AddRoom("Kitchen", 0, 7, 5, 5, 2, Quarters, F.Tile);
            p.AddRoom("Turnout_Room", 0, 7, 7, 2, 2, Tower, F.Block);
            p.AddRoom("Laundry", 0, 9, 7, 3, 2, Quarters, F.Tile);
            p.AddRoom("Hose_Workshop", 0, 2, 7, 4, 2, Apparatus, F.Block);
            p.AddRoom("Hose_Tower", 0, 0, 7, 2, 2, Tower, F.Concrete, windows: false);

            p.AddRoom("Bunk_Room", 1, 7, 0, 5, 4, Quarters);
            p.AddRoom("Captain_Quarters", 1, 7, 4, 2, 2, Quarters, F.Panel);
            p.AddRoom("Bathroom", 1, 9, 4, 3, 2, null, F.Tile);
            p.AddRoom("Fitness_Room", 1, 7, 6, 5, 3, Quarters, F.Concrete);
            p.AddRoom("Storage_Loft", 1, 2, 7, 4, 2, Watch, F.Panel);

            p.Door(0, 5, 1, 6, 1);
            p.Door(0, 6, 1, 6, 2);
            p.Door(0, 5, 2, 6, 2, wide: true);    // the pole landing, straight into the bay
            p.Door(0, 5, 5, 6, 5, wide: true);
            p.Door(0, 6, 3, 7, 3, wide: true);    // the day room's jukebox goes out here
            p.Door(0, 6, 5, 7, 5);
            p.Door(0, 9, 4, 9, 5);
            p.Door(0, 10, 1, 10, 2);
            p.Door(0, 8, 0, 9, 0);
            p.Door(0, 6, 7, 7, 7);
            p.Door(0, 10, 6, 10, 7);
            p.Door(0, 3, 6, 3, 7, wide: true);    // the workshop to the bay
            p.Door(0, 5, 7, 6, 7);

            p.Door(1, 6, 1, 7, 1);
            p.Door(1, 6, 3, 7, 3);
            p.Door(1, 6, 4, 7, 4);
            p.Door(1, 10, 3, 10, 4);
            p.Door(1, 10, 5, 10, 6);
            p.Door(1, 6, 7, 7, 7);
            p.Door(1, 5, 6, 5, 7);
            p.Door(1, 5, 8, 6, 8);
            // The hose gallery looks over the bay; the bay stair arrives on it from the south.
            p.Rail(1, 4, 5, 5, 5);
            p.Rail(1, 4, 6, 5, 6);
            p.Open(1, 5, 4, 5, 5);
            p.Stairs.Add(new P.Flight("BayStair", 0, new Vector3(22f, 0f, 12f), Vector3.forward));

            foreach (float x in BayDoors)
                p.Facade.Add(new P.Opening("BayDoor", P.Side.South, x, BayDoorWidth, 0f, BayDoorHeight, false));
            p.Facade.Add(new P.Opening("WatchDoor", P.Side.South, 30f, P.DoorWidth, 0f, 2.5f, false));
            p.Facade.Add(new P.Opening("RearDoor", P.Side.North, 26f, 1.4f, 0f, 2.4f, false));

            p.Weak(1, new RectInt(2, 7, 4, 2), .6f, .5f);     // the storage loft: old timber
            p.Weak(1, new RectInt(7, 6, 5, 3), .7f, .6f);     // the fitness room
            p.Weak(1, new RectInt(9, 4, 3, 2), .8f, .7f);     // the bathroom
            p.UpperRoomsCollapse(1);
            p.Clear(0, new RectInt(2, 0, 2, 7));              // the bay's middle lane: the haul route
            p.Clear(0, new RectInt(6, 2, 1, 7));              // the hall
            p.Clear(0, new RectInt(5, 3, 1, 4));              // the stair and under the gallery
            return p;
        }

        public static P.Room Room(string name) => Plan.FindRoom(name);
    }
}

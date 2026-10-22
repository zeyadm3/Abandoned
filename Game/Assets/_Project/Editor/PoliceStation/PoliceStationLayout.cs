using UnityEngine;
using F = Abandoned.EditorTools.StationPlan.Finish;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The Millbrook 3rd Precinct's floor plan as data (Docs/POLICE_STATION_PLAN.md): a two-storey brick precinct with
    /// a basement, 56 x 40 m on the 4 m grid (14 x 10 cells). South is the street and the public lobby; east is the sally
    /// port, where the truck backs up to the open vehicle door: the carry route. Everything that places something reads
    /// its numbers here.
    /// </summary>
    public static class PoliceStationLayout
    {
        public const string SceneName = "PoliceStation";
        public const string ScenePath = "Assets/_Project/Scenes/PoliceStation.unity";
        public const string DisplayName = "the Millbrook 3rd Precinct";
        public const string ArtFolder = "Assets/_Project/Art/PoliceStation";
        public const string LootFolder = "Assets/_Project/Data/Loot/PoliceStation";
        public const string LootPrefix = "ps_";

        /// <summary>The station's own spawn tags: no other location's loot turns up here, and none of this elsewhere.</summary>
        public const string Office = "police_office", Cells = "police_cells", Evidence = "police_evidence", Armoury = "police_armoury";

        public const int Width = 14, Depth = 10;
        /// <summary>The sally port's vehicle door in the east wall (centre), and the truck's rear, cab to the east.</summary>
        public static readonly Vector3 SallyDoor = new(Width * P.Tile, 0f, 10f);
        public static readonly Vector3 TruckRear = new(Width * P.Tile + 11f, 0f, 10f);
        public static readonly Vector3 TruckForward = Vector3.right;
        public const float SallyDoorWidth = 6f, SallyDoorHeight = 3.4f;
        public const float LobbyDoorX = 20f;

        private static P cached;

        public static P Plan => cached ??= Create();

        private static P Create()
        {
            var p = new P
            {
                SceneName = SceneName, ScenePath = ScenePath, DisplayName = DisplayName, ArtFolder = ArtFolder, LogTag = "PoliceStation",
                Width = Width, Depth = Depth, Stability = .7f, Seed = 7313,
            };
            p.FillFloor(0, new RectInt(0, 0, Width, Depth));
            p.FillFloor(1, new RectInt(0, 0, Width, Depth));
            p.FillFloor(-1, new RectInt(0, 4, 8, 6));
            // The basement stair comes up through the ground floor; the main and rear stairs go up through the upper one.
            p.Hole(0, new Vector2Int(1, 4), new Vector2Int(1, 5));
            p.Hole(1, new Vector2Int(2, 4), new Vector2Int(2, 5), new Vector2Int(13, 6), new Vector2Int(13, 7));
            Ground(p);
            Upper(p);
            Basement(p);

            p.Stairs.Add(new P.Flight("MainStair", 0, new Vector3(10f, 0f, 16f), Vector3.forward));
            p.Stairs.Add(new P.Flight("BasementStair", -1, new Vector3(6f, -P.Story, 24f), Vector3.back));
            p.Stairs.Add(new P.Flight("RearStair", 0, new Vector3(54f, 0f, 24f), Vector3.forward));
            p.Open(0, 1, 3, 1, 4);
            p.Open(1, 2, 5, 2, 6);
            p.Open(1, 13, 7, 13, 8);

            p.Facade.Add(new P.Opening("LobbyDoors", P.Side.South, LobbyDoorX, P.WideDoor, 0f, P.WideDoorHeight, false));
            p.Facade.Add(new P.Opening("SallyPortDoor", P.Side.East, SallyDoor.z, SallyDoorWidth, 0f, SallyDoorHeight, false));
            Strength(p);
            return p;
        }

        private static void Ground(P p)
        {
            p.AddRoom("Records", 0, 0, 0, 3, 3, Office, F.Block);
            p.AddRoom("Lobby", 0, 3, 0, 4, 3, Office);
            p.AddRoom("Squad_Room", 0, 7, 0, 3, 3, Office);
            p.AddRoom("Sally_Port", 0, 10, 0, 4, 5, Evidence, F.Brick, windows: false);
            p.AddRoom("Booking", 0, 7, 4, 3, 3, Cells, F.Tile);
            p.AddRoom("Holding_Cells", 0, 3, 5, 4, 5, Cells, F.Block, windows: false);
            p.AddRoom("Interview", 0, 7, 7, 2, 3, Cells, F.Block);
            p.AddRoom("Observation", 0, 9, 7, 1, 3, Cells, F.Panel, windows: false);
            p.AddRoom("Dispatch", 0, 10, 5, 3, 2, Office, F.Panel);
            p.AddRoom("Briefing_Room", 0, 10, 7, 3, 3, Office);
            p.AddRoom("Janitor_Closet", 0, 13, 8, 1, 2, null, F.Block, windows: false);
            p.AddRoom("Locker_Room", 0, 0, 6, 3, 4, Armoury, F.Tile);

            p.Door(0, 1, 2, 1, 3);
            p.Door(0, 5, 2, 5, 3, wide: true);
            p.Door(0, 8, 2, 8, 3);
            p.Door(0, 9, 1, 10, 1);
            p.Door(0, 9, 3, 10, 3, wide: true);   // the haul door: main hall to the sally port
            p.Door(0, 8, 3, 8, 4, wide: true);
            p.Door(0, 9, 4, 10, 4);
            p.Door(0, 6, 5, 7, 5);
            p.Door(0, 5, 4, 5, 5);
            p.Door(0, 9, 5, 10, 5);
            p.Door(0, 7, 6, 7, 7);
            p.Door(0, 9, 6, 9, 7);
            p.Door(0, 11, 6, 11, 7);
            p.Door(0, 12, 5, 13, 5);
            p.Door(0, 13, 4, 13, 5);
            p.Door(0, 12, 8, 13, 8);
            p.Door(0, 0, 5, 0, 6);
        }

        private static void Upper(P p)
        {
            p.AddRoom("Records_Archive", 1, 0, 0, 3, 4, Office, F.Block);
            p.AddRoom("Detective_Bureau", 1, 3, 0, 7, 6, Office, F.Panel);
            p.AddRoom("Gym", 1, 10, 0, 4, 6, Armoury, F.Concrete);
            p.AddRoom("Evidence_Room", 1, 0, 7, 4, 3, Evidence, F.Block, windows: false);
            p.AddRoom("Captain_Office", 1, 4, 7, 3, 3, Office, F.Panel);
            p.AddRoom("Break_Room", 1, 7, 7, 4, 3, Office, F.Tile);

            p.Door(1, 0, 3, 0, 4);
            p.Door(1, 6, 5, 6, 6, wide: true);
            p.Door(1, 4, 5, 4, 6);
            p.Door(1, 9, 2, 10, 2);
            p.Door(1, 11, 5, 11, 6);
            p.Door(1, 1, 6, 1, 7, wide: true);    // the evidence haul goes out here and down the main stair
            p.Door(1, 5, 6, 5, 7, wide: true);
            p.Door(1, 8, 6, 8, 7);
            p.Door(1, 10, 8, 11, 8);
        }

        private static void Basement(P p)
        {
            p.AddRoom("Boiler_Room", -1, 0, 4, 1, 2, null, F.Concrete, windows: false);
            p.AddRoom("Property_Storage", -1, 2, 4, 3, 2, Evidence, F.Block, windows: false);
            p.AddRoom("Generator_Room", -1, 5, 4, 3, 2, null, F.Concrete, windows: false);
            p.AddRoom("Firing_Range", -1, 0, 7, 5, 3, Armoury, F.Concrete, windows: false);
            p.AddRoom("Armoury", -1, 5, 7, 3, 3, Armoury, F.Block, windows: false);

            p.Door(-1, 0, 5, 0, 6);
            p.Door(-1, 3, 5, 3, 6, wide: true);
            p.Door(-1, 6, 5, 6, 6);
            p.Door(-1, 0, 6, 0, 7);
            p.Door(-1, 6, 6, 6, 7, wide: true);
            p.Door(-1, 4, 8, 5, 8);
        }

        private static void Strength(P p)
        {
            // Upstairs every room may fall; the hall and the landings never do.
            p.Weak(1, new RectInt(0, 7, 4, 3), .6f, .5f);     // the evidence room: the headline
            p.Weak(1, new RectInt(10, 0, 4, 6), .7f, .6f);    // the gym over the sally port
            p.Weak(1, new RectInt(0, 0, 3, 4), .8f, .7f);     // the records archive
            p.UpperRoomsCollapse(1);
            // The locker room's shower corner is water-rotted and drops into the firing range.
            p.Weak(0, new RectInt(0, 8, 2, 2), .7f, .6f);

            // Rubble never piles on the carry route, the stair cells or the cell corridor.
            p.Clear(0, new RectInt(0, 3, 10, 1));
            p.Clear(0, new RectInt(10, 0, 4, 5));
            p.Clear(0, new RectInt(3, 4, 4, 1));
            p.Clear(0, new RectInt(0, 4, 1, 2));
            p.Clear(0, new RectInt(2, 4, 1, 2));
            p.Clear(0, new RectInt(13, 5, 1, 3));
            p.Clear(0, new RectInt(7, 4, 3, 1));
            p.Clear(-1, new RectInt(0, 6, 8, 1));
            p.Clear(-1, new RectInt(1, 4, 1, 2));
        }

        public static P.Room Room(string name) => Plan.FindRoom(name);
    }
}

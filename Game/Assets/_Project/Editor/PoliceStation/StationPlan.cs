using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A civic station's floor plan as data (police station, fire station): a grid of 4 m cells per storey, the rooms
    /// on it, the doors between them, the stairs, the facade openings and how strong each slab is. The station kit
    /// (<c>Station*.cs</c>) reads only this, so moving a room moves its walls, lights, loot and floor strength together.
    /// Storeys are 4 m apart; floor -1 is a basement. A cell with a slab on a storey but no room is open hall.
    /// </summary>
    public sealed class StationPlan
    {
        public const float Tile = 4f, Story = 4f, Slab = .25f;
        public const float WallThickness = .2f, ShellThickness = .36f;
        public const float DoorWidth = 1.8f, WideDoor = 3.2f, DoorHeight = 2.7f, WideDoorHeight = 3.1f;

        public string SceneName, ScenePath, DisplayName, ArtFolder, LogTag;
        public int Width, Depth;
        /// <summary>Top of the facade (parapet). The roof slab's underside is at <see cref="RoofY"/>.</summary>
        public float ParapetTop = 8.9f;
        public float RoofY = 2f * Story - Slab;
        public float Stability = .7f;
        public int Seed = 1;

        public readonly List<Room> Rooms = new();
        /// <summary>Cells that carry a slab, per storey (key: floor index).</summary>
        public readonly Dictionary<int, HashSet<Vector2Int>> Floors = new();
        /// <summary>Openings in a storey's floor (stairwells): the edges round them get railings, not walls.</summary>
        public readonly Dictionary<int, HashSet<Vector2Int>> Holes = new();
        public readonly List<Doorway> Doors = new();
        /// <summary>Edges (floor, a, b) with nothing on them: where a flight leaves its stairwell.</summary>
        public readonly HashSet<(int floor, Vector2Int a, Vector2Int b)> OpenEdges = new();
        /// <summary>Edges that get a railing whatever is either side (a gallery over a tall room).</summary>
        public readonly HashSet<(int floor, Vector2Int a, Vector2Int b)> Rails = new();
        public readonly List<Flight> Stairs = new();
        public readonly List<Opening> Facade = new();
        /// <summary>Slabs that may fall, with (health, capacity) multipliers; everything else never collapses.</summary>
        public readonly Dictionary<Vector3Int, Vector2> Collapsible = new();
        /// <summary>Slabs rubble never piles on (the haul route, stair cells, landings).</summary>
        public readonly HashSet<Vector3Int> KeepClear = new();
        /// <summary>Slabs with a small square opening in the middle (side in metres): a fire pole's hole.</summary>
        public readonly Dictionary<Vector3Int, float> Pierced = new();
        /// <summary>Roof cells with a hatch (a ladder comes up through it from the top floor's hall).</summary>
        public readonly HashSet<Vector2Int> RoofHatches = new();
        public const float HatchSize = 1.3f;
        /// <summary>Cells outside the generic shell and walls: a special structure builds its own (a hose tower).</summary>
        public readonly HashSet<Vector2Int> Custom = new();

        public static float FloorY(int floor) => floor * Story;
        public static Vector3 CellCentre(Vector2Int cell, int floor = 0) => new((cell.x + .5f) * Tile, FloorY(floor), (cell.y + .5f) * Tile);
        public static Vector3Int Key(int floor, Vector2Int cell) => new(cell.x, floor, cell.y);

        public Vector3 SizeMeters => new(Width * Tile, ParapetTop, Depth * Tile);
        public bool InFootprint(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < Width && c.y < Depth;
        public bool HasFloor(int floor, Vector2Int c) => Floors.TryGetValue(floor, out HashSet<Vector2Int> s) && s.Contains(c);
        public bool IsHole(int floor, Vector2Int c) => Holes.TryGetValue(floor, out HashSet<Vector2Int> s) && s.Contains(c);

        /// <summary>The room a slab belongs to, or null for open hall (or no slab).</summary>
        public Room RoomAt(int floor, Vector2Int c)
        {
            foreach (Room r in Rooms)
                if (r.Floor == floor && r.Cells.Contains(c)) return r;
            return null;
        }

        public Room FindRoom(string name) => Rooms.Find(r => r.Name == name);

        public HashSet<Vector2Int> FloorSet(int floor)
        {
            if (!Floors.TryGetValue(floor, out HashSet<Vector2Int> s)) Floors[floor] = s = new HashSet<Vector2Int>();
            return s;
        }

        // ---- Authoring helpers (the station layouts call these) ----------------------------------------------

        public void FillFloor(int floor, RectInt area)
        {
            HashSet<Vector2Int> s = FloorSet(floor);
            for (int x = area.xMin; x < area.xMax; x++)
            for (int z = area.yMin; z < area.yMax; z++) s.Add(new Vector2Int(x, z));
        }

        public void Hole(int floor, params Vector2Int[] cells)
        {
            FloorSet(floor).ExceptWith(cells);
            if (!Holes.TryGetValue(floor, out HashSet<Vector2Int> s)) Holes[floor] = s = new HashSet<Vector2Int>();
            s.UnionWith(cells);
        }

        public Room AddRoom(string name, int floor, int x, int z, int w, int d, string lootTag = null,
            Finish finish = Finish.Plaster, bool windows = true)
        {
            var room = new Room(name, floor, new RectInt(x, z, w, d), lootTag, finish, windows);
            Rooms.Add(room);
            return room;
        }

        public void Door(int floor, int ax, int az, int bx, int bz, bool wide = false) =>
            Doors.Add(new Doorway(floor, new Vector2Int(ax, az), new Vector2Int(bx, bz), wide));

        public void Open(int floor, int ax, int az, int bx, int bz) => OpenEdges.Add(Edge(floor, new Vector2Int(ax, az), new Vector2Int(bx, bz)));
        public void Rail(int floor, int ax, int az, int bx, int bz) => Rails.Add(Edge(floor, new Vector2Int(ax, az), new Vector2Int(bx, bz)));

        public void Weak(int floor, RectInt area, float health, float capacity)
        {
            for (int x = area.xMin; x < area.xMax; x++)
            for (int z = area.yMin; z < area.yMax; z++) Collapsible[new Vector3Int(x, floor, z)] = new Vector2(health, capacity);
        }

        public void Clear(int floor, RectInt area)
        {
            for (int x = area.xMin; x < area.xMax; x++)
            for (int z = area.yMin; z < area.yMax; z++) KeepClear.Add(new Vector3Int(x, floor, z));
        }

        /// <summary>Every upper-floor slab that is not open hall may fall (with the given strength) unless it is a landing.</summary>
        public void UpperRoomsCollapse(int floor, float health = 1f, float capacity = 1f)
        {
            foreach (Vector2Int c in FloorSet(floor))
            {
                var key = Key(floor, c);
                if (RoomAt(floor, c) == null || Collapsible.ContainsKey(key)) continue;
                Collapsible[key] = new Vector2(health, capacity);
            }
        }

        /// <summary>The canonical form of an edge between two neighbouring cells (lower cell first).</summary>
        public static (int floor, Vector2Int a, Vector2Int b) Edge(int floor, Vector2Int a, Vector2Int b) =>
            a.x < b.x || (a.x == b.x && a.y < b.y) ? (floor, a, b) : (floor, b, a);

        public Doorway DoorOn(int floor, Vector2Int a, Vector2Int b)
        {
            var e = Edge(floor, a, b);
            foreach (Doorway d in Doors)
                if (Edge(d.Floor, d.A, d.B) == e) return d;
            return null;
        }

        public enum Finish { Plaster, Block, Tile, Brick, Concrete, Panel }

        public sealed class Room
        {
            public readonly string Name, LootTag;
            public readonly int Floor;
            public readonly RectInt Cells;
            public readonly Finish Finish;
            public readonly bool Windows;

            public Room(string name, int floor, RectInt cells, string lootTag, Finish finish, bool windows)
            { Name = name; Floor = floor; Cells = cells; LootTag = lootTag; Finish = finish; Windows = windows; }

            public Rect Meters => new(Cells.x * StationPlan.Tile, Cells.y * StationPlan.Tile, Cells.width * StationPlan.Tile, Cells.height * StationPlan.Tile);
            public float Y => StationPlan.FloorY(Floor);
            public Vector3 Centre => new(Meters.center.x, Y, Meters.center.y);
        }

        public sealed class Doorway
        {
            public readonly int Floor;
            public readonly Vector2Int A, B;
            public readonly bool Wide;
            public Doorway(int floor, Vector2Int a, Vector2Int b, bool wide) { Floor = floor; A = a; B = b; Wide = wide; }
            public float Width => Wide ? StationPlan.WideDoor : StationPlan.DoorWidth;
            public float Height => Wide ? StationPlan.WideDoorHeight : StationPlan.DoorHeight;
        }

        /// <summary>A straight flight: 8 m long, 3 m wide, one storey up from <see cref="Floor"/>, starting at the foot.</summary>
        public sealed class Flight
        {
            public const float Length = 8f, Width = 3f;
            public readonly string Name;
            public readonly int Floor;
            public readonly Vector3 Foot, Direction;
            public Flight(string name, int floor, Vector3 foot, Vector3 direction)
            { Name = name; Floor = floor; Foot = foot; Direction = direction.normalized; }
            public float Rise => StationPlan.Story;
            public Vector3 Top => Foot + Direction * Length + Vector3.up * Rise;
        }

        public enum Side { South, North, West, East }

        /// <summary>An opening in the outer wall: a door, a vehicle door or a window (world metres).</summary>
        public sealed class Opening
        {
            public readonly Side Side;
            public readonly float Centre, Width, Bottom, Top;
            public readonly bool Glazed;
            public readonly string Name;
            public Opening(string name, Side side, float centre, float width, float bottom, float top, bool glazed)
            { Name = name; Side = side; Centre = centre; Width = width; Bottom = bottom; Top = top; Glazed = glazed; }
        }
    }
}

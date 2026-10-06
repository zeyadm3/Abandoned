using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The Abandoned Mall's fixed layout (GDD 8: hand-built layout, randomized contents), as data the
    /// builder and populator read. A 12 x 10 grid of 4 m tiles, three 4 m stories, south (z = 0) is the
    /// street front. A 4 x 4 atrium rises through all floors, ringed by walkways on 1 and 2, with a bridge
    /// across it on floor 1; escalators climb inside the atrium; a service stairwell in the stockroom
    /// corner is the route that never collapses (GDD 6.4). Every floor tile becomes a StructuralSection.
    /// </summary>
    public static class MallLayout
    {
        public const float Tile = 4f;
        public const int TilesX = 12, TilesZ = 10, Floors = 3;
        public const float StoryHeight = 4f;

        public enum Kind { Store, Walkway, Bridge, Concourse }

        public readonly struct Zone
        {
            public readonly string Name, Tag;
            public readonly RectInt Area;
            public readonly Kind Kind;

            public Zone(string name, string tag, Kind kind, int x, int z, int w, int d)
            {
                Name = name;
                Tag = tag;
                Kind = kind;
                Area = new RectInt(x, z, w, d);
            }

            public bool Contains(Vector2Int c) => Area.Contains(c);
        }

        /// <summary>A doorway in the wall on the edge between <see cref="A"/> and <see cref="B"/> (neighbouring tiles).</summary>
        public readonly struct Door
        {
            public readonly int Floor;
            public readonly Vector2Int A, B;
            public readonly bool Wide;

            public Door(int floor, int ax, int az, int bx, int bz, bool wide = false)
            {
                Floor = floor;
                A = new Vector2Int(ax, az);
                B = new Vector2Int(bx, bz);
                Wide = wide;
            }
        }

        /// <summary>A sloped flight: starts at the edge of <see cref="Start"/> at <see cref="Floor"/>, climbs one story over two tiles along <see cref="Dir"/>.</summary>
        public readonly struct Flight
        {
            public readonly string Name;
            public readonly int Floor;
            public readonly Vector2Int Start, Dir;
            public readonly bool CanCollapse;

            public Flight(string name, int floor, int x, int z, Vector2Int dir, bool canCollapse)
            {
                Name = name;
                Floor = floor;
                Start = new Vector2Int(x, z);
                Dir = dir;
                CanCollapse = canCollapse;
            }

            public IEnumerable<Vector2Int> Tiles => new[] { Start, Start + Dir };
        }

        public static readonly RectInt Atrium = new(4, 3, 4, 4);

        /// <summary>The ring of walkway tiles around the atrium on the upper floors.</summary>
        public static readonly RectInt Ring = new(3, 2, 6, 6);

        public static readonly Flight[] Flights =
        {
            // Escalators: collapse showcase (GDD 8). One up from the atrium floor onto the floor-1 bridge,
            // one from the bridge's east end up to the floor-2 ring.
            new("Escalator_G1", 0, 4, 3, Vector2Int.up, true),
            new("Escalator_12", 1, 7, 4, Vector2Int.down, true),
            // Service stairs in the stockroom corner: the way down that never falls.
            // G->1 climbs south from the loading bay's back wall; 1->2 climbs north from the stockroom.
            new("Stairs_G1", 0, 11, 8, Vector2Int.down, false),
            new("Stairs_12", 1, 10, 7, Vector2Int.up, false),
        };

        public static readonly Zone[][] ZonesByFloor =
        {
            new[] // Ground
            {
                new Zone("Concourse", "concourse", Kind.Concourse, 3, 0, 6, 8),
                new Zone("Electronics", "electronics", Kind.Store, 0, 0, 3, 5),
                new Zone("Clothing", "clothing", Kind.Store, 0, 5, 3, 5),
                new Zone("FoodCourt", "food", Kind.Concourse, 3, 8, 6, 2),
                new Zone("Jewelry", "jewelry", Kind.Store, 9, 0, 3, 4),
                new Zone("Security", "office", Kind.Store, 9, 4, 3, 2),
                new Zone("LoadingBay", "stock", Kind.Store, 9, 6, 3, 4),
            },
            new[] // Floor 1
            {
                new Zone("Walkway1", "walkway", Kind.Walkway, 3, 2, 6, 6),
                new Zone("Furniture", "furniture", Kind.Store, 0, 0, 3, 10),
                new Zone("Toys", "toys", Kind.Store, 3, 0, 6, 2),
                new Zone("FoodTerrace", "food", Kind.Walkway, 3, 8, 6, 2),
                new Zone("Electronics2", "electronics", Kind.Store, 9, 0, 3, 6),
                new Zone("Stockroom", "stock", Kind.Store, 9, 6, 3, 4),
            },
            new[] // Floor 2
            {
                new Zone("Walkway2", "walkway", Kind.Walkway, 3, 2, 6, 6),
                new Zone("Cinema", "cinema", Kind.Store, 0, 0, 3, 10),
                new Zone("Boutique", "jewelry", Kind.Store, 3, 0, 6, 2),
                new Zone("Gallery", "gallery", Kind.Store, 3, 8, 6, 2),
                new Zone("Offices", "office", Kind.Store, 9, 0, 3, 6),
                new Zone("StairTop", "stock", Kind.Store, 9, 6, 3, 4),
            },
        };

        public static readonly Door[] Doors =
        {
            new(0, 2, 2, 3, 2), new(0, 2, 6, 3, 6), new(0, 9, 1, 8, 1), new(0, 9, 4, 8, 4),
            new(0, 9, 7, 8, 7, wide: true), // loading bay <-> concourse: big items come through here
            new(1, 2, 3, 3, 3), new(1, 2, 6, 3, 6), new(1, 5, 1, 5, 2), new(1, 9, 3, 8, 3),
            new(1, 9, 7, 8, 7, wide: true),
            new(2, 2, 5, 3, 5), new(2, 6, 1, 6, 2), new(2, 9, 2, 8, 2), new(2, 9, 7, 8, 7),
            new(2, 5, 8, 5, 7, wide: true), new(2, 6, 8, 6, 7, wide: true), // gallery: the statue's way out
        };

        /// <summary>Exterior openings: (floor, side 'S'/'N'/'W'/'E', column along that side, wide, window).</summary>
        public static readonly (int floor, char side, int column, bool wide, bool window)[] Exterior =
        {
            (0, 'S', 5, true, false), (0, 'S', 6, true, false), // main entrance
            (0, 'E', 8, true, false),                           // loading bay door to the truck
            (0, 'W', 2, false, false),                          // electronics side exit
            (1, 'W', 5, false, true), (1, 'E', 2, false, true), // fallback rope windows (GDD 6.4)
            (2, 'S', 2, false, true), (2, 'N', 10, false, true),
        };

        public static float FloorY(int floor) => floor * StoryHeight;

        /// <summary>
        /// Edges a flight opens onto (its foot on its own floor, its head on the floor above): no railing
        /// there, even where one side is a void.
        /// </summary>
        public static bool IsFlightOpening(Vector2Int a, Vector2Int b, int floor)
        {
            foreach (Flight f in Flights)
            {
                Vector2Int foot = f.Start - f.Dir, head = f.Start + f.Dir * 2, top = f.Start + f.Dir;
                if (f.Floor == floor && Matches(a, b, foot, f.Start)) return true;
                if (f.Floor + 1 == floor && Matches(a, b, top, head)) return true;
            }
            return false;

            static bool Matches(Vector2Int a, Vector2Int b, Vector2Int p, Vector2Int q) => (a == p && b == q) || (a == q && b == p);
        }

        public static Vector3 TileTopCenter(Vector2Int c, int floor) => new((c.x + 0.5f) * Tile, FloorY(floor), (c.y + 0.5f) * Tile);

        public static bool InGrid(Vector2Int c) => c.x >= 0 && c.x < TilesX && c.y >= 0 && c.y < TilesZ;

        public static bool IsBridge(Vector2Int c, int floor) => floor == 1 && c.y == 5 && c.x >= Atrium.xMin && c.x < Atrium.xMax;

        /// <summary>No floor here: the atrium above the ground, and where a flight climbs through.</summary>
        public static bool IsVoid(Vector2Int c, int floor)
        {
            if (floor == 0) return false;
            if (Atrium.Contains(c) && !IsBridge(c, floor)) return true;
            // A flight from floor f-1 passes up through floor f's slab.
            return Flights.Any(f => f.Floor == floor - 1 && f.Tiles.Contains(c));
        }

        public static Zone? ZoneAt(Vector2Int c, int floor)
        {
            if (!InGrid(c) || IsVoid(c, floor)) return null;
            foreach (Zone z in ZonesByFloor[floor]) if (z.Contains(c)) return z;
            return null;
        }

        /// <summary>Upper-floor tiles next to the atrium void are walkways: lighter build, weaker (Balcony sections).</summary>
        public static bool IsWalkwayTile(Vector2Int c, int floor)
        {
            if (floor == 0 || IsVoid(c, floor)) return false;
            if (IsBridge(c, floor)) return true;
            Zone? z = ZoneAt(c, floor);
            return z.HasValue && z.Value.Kind == Kind.Walkway && Ring.Contains(c);
        }

        public static Door? DoorBetween(Vector2Int a, Vector2Int b, int floor)
        {
            foreach (Door d in Doors)
                if (d.Floor == floor && ((d.A == a && d.B == b) || (d.A == b && d.B == a))) return d;
            return null;
        }

        /// <summary>Two tiles belong to the same open space (no wall between them).</summary>
        public static bool SameSpace(Zone a, Zone b) =>
            a.Name == b.Name || (a.Kind != Kind.Store && b.Kind != Kind.Store);
    }
}

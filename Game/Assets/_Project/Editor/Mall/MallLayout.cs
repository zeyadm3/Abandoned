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
        public const int TilesX = 14, TilesZ = 12, Floors = 3;
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

        public static readonly RectInt Atrium = new(5, 4, 4, 4);

        /// <summary>The ring of walkway tiles around the atrium on the upper floors.</summary>
        public static readonly RectInt Ring = new(4, 3, 6, 6);

        public static readonly Flight[] Flights =
        {
            new("Escalator_G1", 0, 5, 4, Vector2Int.up, true),
            new("Escalator_12", 1, 8, 5, Vector2Int.down, true),
            new("Stairs_G1", 0, 13, 10, Vector2Int.down, false),
            new("Stairs_12", 1, 12, 9, Vector2Int.up, false),
        };

        public static readonly Zone[][] ZonesByFloor =
        {
            new[]
            {
                new Zone("GrandAtrium", "concourse", Kind.Concourse, 4, 0, 6, 10),
                new Zone("DeadCircuit", "electronics", Kind.Store, 0, 0, 4, 5),
                new Zone("Threadbare", "clothing", Kind.Store, 0, 5, 4, 5),
                new Zone("FoodCourt", "food", Kind.Concourse, 0, 10, 10, 2),
                new Zone("ServicePassage", "stock", Kind.Store, 10, 0, 1, 12),
                new Zone("Jewelry", "jewelry", Kind.Store, 11, 0, 3, 4),
                new Zone("Security", "office", Kind.Store, 11, 4, 3, 3),
                new Zone("LoadingBay", "stock", Kind.Store, 11, 7, 3, 5),
            },
            new[]
            {
                new Zone("Walkway1", "walkway", Kind.Walkway, 4, 3, 6, 6),
                new Zone("Furniture", "furniture", Kind.Store, 0, 0, 4, 12),
                new Zone("ToyArchive", "toys", Kind.Store, 4, 0, 6, 3),
                new Zone("FoodTerrace", "food", Kind.Walkway, 4, 9, 6, 3),
                new Zone("ServicePassage", "stock", Kind.Store, 10, 0, 1, 12),
                new Zone("Electronics2", "electronics", Kind.Store, 11, 0, 3, 7),
                new Zone("Stockroom", "stock", Kind.Store, 11, 7, 3, 5),
            },
            new[]
            {
                new Zone("Walkway2", "walkway", Kind.Walkway, 4, 3, 6, 6),
                new Zone("Cinema", "cinema", Kind.Store, 0, 0, 4, 12),
                new Zone("Quarantine", "jewelry", Kind.Store, 4, 0, 6, 3),
                new Zone("CollapsedGallery", "gallery", Kind.Store, 4, 9, 6, 3),
                new Zone("ServicePassage", "stock", Kind.Store, 10, 0, 1, 12),
                new Zone("Offices", "office", Kind.Store, 11, 0, 3, 7),
                new Zone("StairTop", "stock", Kind.Store, 11, 7, 3, 5),
            },
        };

        public static readonly Door[] Doors =
        {
            new(0,3,2,4,2,true), new(0,3,7,4,7,true),
            new(0,9,2,10,2), new(0,9,5,10,5), new(0,9,8,10,8,true),
            new(0,10,1,11,1), new(0,10,5,11,5), new(0,10,8,11,8,true),
            new(1,3,4,4,4), new(1,3,8,4,8,true), new(1,6,2,6,3),
            new(1,9,4,10,4), new(1,9,8,10,8,true), new(1,10,3,11,3), new(1,10,8,11,8,true),
            new(2,3,6,4,6), new(2,6,2,6,3), new(2,6,9,6,8,true),
            new(2,9,4,10,4), new(2,9,8,10,8,true), new(2,10,2,11,2), new(2,10,8,11,8,true),
        };

        public static readonly (int floor, char side, int column, bool wide, bool window)[] Exterior =
        {
            (0,'S',6,true,false), (0,'S',7,true,false),
            (0,'E',8,true,false), (0,'W',2,false,false),
            (1,'W',6,false,true), (1,'E',3,false,true),
            (2,'W',5,false,true), (2,'N',12,false,true),
        };

        public static float FloorY(int floor) => floor * StoryHeight;

        /// <summary>
        /// Edges a flight opens onto (its foot on its own floor, its head on the floor above): no railing
        /// there, even where one side is a void.
        /// </summary>
        public static bool IsFlightOpening(Vector2Int a, Vector2Int b, int floor)
        {
            if (floor == 0 && ((a == new Vector2Int(13,9) && b == new Vector2Int(13,10)) || (b == new Vector2Int(13,9) && a == new Vector2Int(13,10)))) return true;
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

        public static bool IsBridge(Vector2Int c, int floor) => floor == 1 && c.y == 6 && c.x >= Atrium.xMin && c.x < Atrium.xMax;

        /// <summary>No floor here: the atrium above the ground, and where a flight climbs through.</summary>
        public static bool IsVoid(Vector2Int c, int floor)
        {
            if (floor == 0) return c.x == 13 && (c.y == 8 || c.y == 9);
            // The ruined gallery leaves a visible wound while its south carry lane survives.
            if (floor == 2 && c.y >= 10 && c.x >= 7 && c.x <= 9) return true;
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

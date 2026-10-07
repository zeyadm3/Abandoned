using System.Linq;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Exterior walls, store partitions with doors, and railings along every drop into a void.</summary>
    public static class MallWalls
    {
        public const float Thickness = 0.2f;
        private const float RailingHeight = 1.05f;
        private static readonly WallOpening Door = new(1.4f, 0f, 2.3f);
        private static readonly WallOpening WideDoor = new(3.5f, 0f, 3.4f);   // the piano/statue/rack route
        private static readonly WallOpening Window = new(1.4f, 1.0f, 1.3f);   // rope-point fallback (GDD 6.4)

        private static float WallHeight => StoryHeight - TestBuildingBuilder.SlabThickness;

        public static int Build(Transform parent, int floor, Material wall, Material frame, Material railing)
        {
            Transform walls = Group($"Walls_{floor}", parent);
            Transform rails = Group($"Railings_{floor}", parent);
            int count = 0;
            float y = FloorY(floor);

            for (int x = 0; x < TilesX; x++)
            for (int z = 0; z < TilesZ; z++)
            {
                var c = new Vector2Int(x, z);
                // Each interior edge once: east and north neighbours.
                count += Edge(c, c + Vector2Int.right);
                count += Edge(c, c + Vector2Int.up);
            }
            count += Perimeter(walls, floor, y, wall, frame);
            MarkStatic(walls);
            MarkStatic(rails);
            return count;

            int Edge(Vector2Int a, Vector2Int b)
            {
                if (!InGrid(b)) return 0;
                Zone? za = ZoneAt(a, floor), zb = ZoneAt(b, floor);
                bool voidA = IsVoid(a, floor), voidB = IsVoid(b, floor);
                if (voidA && voidB) return 0;
                if (voidA || voidB)
                {
                    if (IsFlightOpening(a, b, floor)) return FlightSideRails(rails, a, b, floor, y, railing);
                    Railing(rails, $"Rail_{floor}_{a.x}_{a.y}_{b.x}_{b.y}", a, b, y, railing);
                    return 1;
                }
                if (!za.HasValue || !zb.HasValue || SameSpace(za.Value, zb.Value)) return 0;
                Door? door = DoorBetween(a, b, floor);
                WallOpening? hole = door.HasValue ? (door.Value.Wide ? WideDoor : Door) : null;
                InteriorPanel(walls, $"Wall_{floor}_{a.x}_{a.y}_{b.x}_{b.y}", a, b, y, hole, wall, frame);
                return 1;
            }
        }

        // The panel runs along the shared edge of two neighbouring tiles.
        private static void InteriorPanel(Transform parent, string name, Vector2Int a, Vector2Int b, float y,
            WallOpening? hole, Material wall, Material frame)
        {
            bool east = b.x != a.x;
            Vector3 start = east ? new Vector3(b.x * Tile, y, a.y * Tile) : new Vector3(a.x * Tile, y, b.y * Tile);
            Vector3 dir = east ? Vector3.forward : Vector3.right;
            GreyboxWall.Panel(parent, name, start, dir, Tile, WallHeight, Thickness, hole, wall, frame);
        }

        private static void Railing(Transform parent, string name, Vector2Int a, Vector2Int b, float y, Material material)
        {
            bool east = b.x != a.x;
            Vector3 center = east
                ? new Vector3(b.x * Tile, y + RailingHeight / 2f, (a.y + 0.5f) * Tile)
                : new Vector3((a.x + 0.5f) * Tile, y + RailingHeight / 2f, b.y * Tile);
            Vector3 size = east ? new Vector3(0.1f, RailingHeight, Tile) : new Vector3(Tile, RailingHeight, 0.1f);
            Box(name, parent, center, size, material);
        }

        // A flight is narrower than the 4 m edge it opens onto: rail off what's left on either side.
        private static int FlightSideRails(Transform parent, Vector2Int a, Vector2Int b, int floor, float y, Material material)
        {
            float width = Flights.Where(f => f.Tiles.Contains(a) || f.Tiles.Contains(b) || f.Start - f.Dir == a || f.Start - f.Dir == b)
                .Select(MallFlights.WidthOf).DefaultIfEmpty(Tile).Max();
            float side = (Tile - width) / 2f;
            if (side <= 0.05f) return 0;
            bool east = b.x != a.x;
            for (int s = -1; s <= 1; s += 2)
            {
                float offset = s * (Tile / 2f - side / 2f);
                Vector3 center = east
                    ? new Vector3(b.x * Tile, y + RailingHeight / 2f, (a.y + 0.5f) * Tile + offset)
                    : new Vector3((a.x + 0.5f) * Tile + offset, y + RailingHeight / 2f, b.y * Tile);
                Vector3 size = east ? new Vector3(0.1f, RailingHeight, side) : new Vector3(side, RailingHeight, 0.1f);
                Box($"Rail_{floor}_{a.x}_{a.y}_{b.x}_{b.y}_{(s < 0 ? "L" : "R")}", parent, center, size, material);
            }
            return 2;
        }

        private static int Perimeter(Transform walls, int floor, float y, Material wall, Material frame)
        {
            float h = Thickness / 2f, width = TilesX * Tile, depth = TilesZ * Tile;
            int count = 0;
            for (int x = 0; x < TilesX; x++)
            {
                Panel($"Wall_{floor}_S_{x}", new Vector3(x * Tile, y, -h), Vector3.right, Opening(floor, 'S', x));
                Panel($"Wall_{floor}_N_{x}", new Vector3(x * Tile, y, depth + h), Vector3.right, Opening(floor, 'N', x));
            }
            for (int z = 0; z < TilesZ; z++)
            {
                Panel($"Wall_{floor}_W_{z}", new Vector3(-h, y, z * Tile), Vector3.forward, Opening(floor, 'W', z));
                Panel($"Wall_{floor}_E_{z}", new Vector3(width + h, y, z * Tile), Vector3.forward, Opening(floor, 'E', z));
            }
            foreach (Vector3 corner in new[] { new Vector3(-h, 0, -h), new Vector3(width + h, 0, -h),
                         new Vector3(-h, 0, depth + h), new Vector3(width + h, 0, depth + h) })
                Box("Corner", walls, corner + Vector3.up * (y + StoryHeight / 2f), new Vector3(Thickness, StoryHeight, Thickness), wall);
            return count;

            void Panel(string name, Vector3 start, Vector3 dir, WallOpening? hole)
            {
                GreyboxWall.Panel(walls, name, start, dir, Tile, StoryHeight, Thickness, hole, wall, frame);
                count++;
            }
        }

        private static WallOpening? Opening(int floor, char side, int column)
        {
            foreach (var e in Exterior)
                if (e.floor == floor && e.side == side && e.column == column)
                    return e.window ? Window : e.wide ? WideDoor : Door;
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The Iron Factory's floor plan as data (Docs/IRON_FACTORY_PLAN.md): a 120 x 100 m plant on the mall's
    /// 4 m grid, split north-south by a 20 x 60 m molten channel. Everything that places something reads its
    /// numbers here, so moving a room or the channel moves its walls, floor hole, lights and markers together.
    /// </summary>
    public static class IronFactoryLayout
    {
        public const string SceneName = "Factory";
        public const float Tile = 4f;
        public const int Width = 30;   // 120 m, west -> east (x)
        public const int Depth = 25;   // 100 m, dock (south, z = 0) -> locker rooms (north)
        public const float Slab = .25f;
        public const float WallHeight = 20f;
        public const float WallThickness = .4f;

        /// <summary>Catwalk tier heights (step 2). Room roofs sit at the first, so they join that tier.</summary>
        public static readonly float[] Tiers = { 5f, 10f, 15f };
        public static float RoomHeight => Tiers[0];

        /// <summary>The molten channel in cells: x 52-72 m, z 20-80 m. The dock is reachable from both banks
        /// around either end without crossing it.</summary>
        public static readonly RectInt Channel = new(13, 5, 5, 15);
        public const float LavaSurfaceY = -1.4f;
        public const float ChannelBedY = -2.2f;
        public const float ChannelFloorY = -2.6f;   // bottom of the lining walls

        /// <summary>The rail loading dock: the plant's one way in and out, centred on the truck.</summary>
        public const float DockX = 60f;
        public const float DockWidth = 10f;
        public const float DockHeight = 7f;
        /// <summary>Cells straight in from the dock: never a rubble pile, so the haul route stays open.</summary>
        public static readonly RectInt DockLane = new(14, 0, 2, 5);

        [Flags]
        public enum Door { South = 1, North = 2, West = 4, East = 8 }

        public readonly struct Room
        {
            public readonly string Name;
            public readonly RectInt Cells;
            public readonly Door Doors;
            public Room(string name, int x, int z, int width, int depth, Door doors)
            { Name = name; Cells = new RectInt(x, z, width, depth); Doors = doors; }
            public Rect Meters => new(Cells.x * Tile, Cells.y * Tile, Cells.width * Tile, Cells.height * Tile);
        }

        /// <summary>Enclosed ground-floor rooms, 5 m tall with walkable roofs. The north concourse
        /// (cells x 11-18) between the locker block and the offices stays open floor.</summary>
        public static readonly Room[] Rooms =
        {
            new("Locker_Room", 0, 21, 7, 4, Door.South | Door.East),
            new("Washroom", 7, 21, 4, 4, Door.West | Door.South),
            new("Foremen_Offices", 19, 21, 6, 4, Door.South | Door.West),
            new("Admin", 25, 21, 5, 4, Door.West | Door.South),
            new("Breaker_Hall", 0, 8, 4, 4, Door.East),
            new("Pattern_Shop", 26, 8, 4, 5, Door.West),
            new("Foundry_Stores", 26, 14, 4, 4, Door.West),
            new("Weighbridge_Office", 0, 0, 4, 3, Door.North | Door.East),
        };

        public static Vector3 SizeMeters => new(Width * Tile, WallHeight, Depth * Tile);

        /// <summary>"Inside the building" for the horror director and the total collapse: down to the channel bed.</summary>
        public static Bounds Building => new(new Vector3(Width * Tile * .5f, 8f, Depth * Tile * .5f),
            new Vector3(Width * Tile, 24f, Depth * Tile));

        public static Rect ChannelMeters => new(Channel.x * Tile, Channel.y * Tile, Channel.width * Tile, Channel.height * Tile);

        /// <summary>The kill box: from under the bed up to just under the floor, so walking the edge is safe.</summary>
        public static Bounds LavaKill
        {
            get
            {
                Rect c = ChannelMeters;
                const float top = -.8f, bottom = ChannelFloorY - .6f;
                return new Bounds(new Vector3(c.center.x, (top + bottom) * .5f, c.center.y),
                    new Vector3(c.width, top - bottom, c.height));
            }
        }

        public static bool InChannel(Vector2Int cell) => Channel.Contains(cell);

        public static IEnumerable<Vector2Int> GroundCells()
        {
            for (int z = 0; z < Depth; z++)
            for (int x = 0; x < Width; x++)
            {
                var cell = new Vector2Int(x, z);
                if (!InChannel(cell)) yield return cell;
            }
        }

        public static Vector3 CellCentre(Vector2Int cell, float y = 0f) => new((cell.x + .5f) * Tile, y, (cell.y + .5f) * Tile);

        public static readonly Vector3 TruckRear = new(DockX, 0f, -11f);

        public static IReadOnlyList<Vector3> CrewSpawns => new[]
        {
            new Vector3(DockX - 4f, .05f, -6.5f), new Vector3(DockX - 2.5f, .05f, -6.5f),
            new Vector3(DockX + 2.5f, .05f, -6.5f), new Vector3(DockX + 4f, .05f, -6.5f),
        };

        // Loot points: IronFactoryLootPlan. Threat spawns: IronFactoryThreatPlan.
    }
}

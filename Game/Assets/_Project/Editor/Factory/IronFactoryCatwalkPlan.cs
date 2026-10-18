using System.Collections.Generic;
using System.Text;
using UnityEngine;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The catwalk maze as data: straight runs of 4 m cells on each tier, plus the stairs and ladders between
    /// tiers. Level 0 is the machinery floor; levels 1-3 are the tiers at 5 / 10 / 15 m. Runs overlap at
    /// junctions; a later kind wins over an earlier one where they share a cell, so spines are listed last.
    /// <see cref="Problems"/> checks the plan (stairs and ladders need clear cells); the build logs them.
    /// </summary>
    public static class IronFactoryCatwalkPlan
    {
        public enum Kind : byte { Catwalk, Weak, Narrow, Spine }

        public readonly struct Run
        {
            public readonly int Level;
            public readonly Vector2Int From, To;
            public readonly Kind Kind;
            public Run(int level, int x0, int z0, int x1, int z1, Kind kind = Kind.Catwalk)
            { Level = level; From = new Vector2Int(x0, z0); To = new Vector2Int(x1, z1); Kind = kind; }
        }

        /// <summary>A straight flight: you stand on <see cref="Foot"/> (lower level), climb over the next two
        /// cells and arrive on the third (one level up).</summary>
        public readonly struct Stair
        {
            public readonly string Name;
            public readonly int FromLevel;
            public readonly Vector2Int Foot, Direction;
            public Stair(string name, int fromLevel, int x, int z, Vector2Int direction)
            { Name = name; FromLevel = fromLevel; Foot = new Vector2Int(x, z); Direction = direction; }
            public Vector2Int Top => Foot + Direction * 3;
            public IEnumerable<Vector2Int> RunCells { get { yield return Foot + Direction; yield return Foot + Direction * 2; } }
        }

        /// <summary>A ladder against the <see cref="Side"/> edge of a cell on <see cref="Level"/>, its foot in the
        /// neighbouring cell one level down.</summary>
        public readonly struct LadderSpot
        {
            public readonly int Level;
            public readonly Vector2Int Cell, Side;
            public LadderSpot(int level, int x, int z, Vector2Int side) { Level = level; Cell = new Vector2Int(x, z); Side = side; }
            public Vector2Int Below => Cell + Side;
        }

        private static readonly Vector2Int N = Vector2Int.up, S = Vector2Int.down, E = Vector2Int.right, W = Vector2Int.left;
        public static readonly Vector2Int[] Sides = { N, E, S, W };

        // Channel columns are x 13-17 (rows 5-19); anything crossing them is a bridge over the lava.
        public static readonly Run[] Runs =
        {
            // ---- Tier 1, 5 m: the main maze over the machines --------------------------------------
            new(1, 2, 4, 8, 4), new(1, 10, 4, 12, 4), new(1, 3, 3, 3, 7), new(1, 4, 7, 8, 7),
            new(1, 2, 12, 2, 16), new(1, 3, 16, 8, 16),
            new(1, 12, 4, 12, 18), new(1, 12, 9, 12, 10, Kind.Weak),           // the rusted stretch over the west edge
            new(1, 10, 18, 19, 18), new(1, 13, 18, 17, 18, Kind.Weak),         // the north bridge: it can fall in
            new(1, 10, 20, 19, 20),
            new(1, 18, 4, 18, 18), new(1, 18, 4, 19, 4), new(1, 21, 4, 27, 4),
            new(1, 21, 10, 25, 10), new(1, 21, 15, 25, 15), new(1, 22, 16, 22, 16),
            // Spines: dock stairs -> north rooms on each bank, and the one bridge that never falls.
            new(1, 9, 4, 9, 20, Kind.Spine), new(1, 20, 4, 20, 20, Kind.Spine), new(1, 10, 12, 19, 12, Kind.Spine),

            // ---- Tier 2, 10 m: crane rails, furnace tops, the control pulpit -------------------------
            new(2, 8, 10, 8, 19), new(2, 4, 19, 8, 19), new(2, 4, 13, 4, 19), new(2, 4, 13, 8, 13),
            new(2, 5, 16, 6, 16), new(2, 5, 17, 6, 17),                          // furnace-top platform
            new(2, 9, 10, 20, 10), new(2, 9, 14, 20, 14), new(2, 14, 14, 16, 14, Kind.Weak),
            new(2, 20, 10, 20, 14), new(2, 21, 13, 21, 19),
            new(2, 22, 17, 25, 17), new(2, 24, 18, 25, 18),                      // control pulpit

            // ---- Tier 3, 15 m: the roof-truss gantry and one narrow bridge ---------------------------
            new(3, 11, 8, 11, 19), new(3, 18, 8, 18, 19), new(3, 12, 19, 17, 19, Kind.Narrow),
        };

        public static readonly Stair[] Stairs =
        {
            new("DockStair_West", 0, 9, 1, N), new("DockStair_East", 0, 20, 1, N),
            new("FurnaceStair", 1, 8, 7, N), new("PulpitStair", 1, 21, 10, N),
            new("GantryStair_West", 2, 8, 19, E), new("GantryStair_East", 2, 21, 19, W),
        };

        public static readonly LadderSpot[] Ladders =
        {
            new(1, 12, 6, W), new(1, 18, 7, E), new(1, 5, 16, N), new(1, 25, 15, N),
            new(2, 8, 16, W), new(2, 21, 16, E),
            new(3, 11, 10, W), new(3, 18, 10, E),
        };

        /// <summary>Every catwalk cell per level, resolved from the runs.</summary>
        public static Dictionary<Vector2Int, Kind>[] Cells()
        {
            var levels = new Dictionary<Vector2Int, Kind>[4];
            for (int i = 0; i < levels.Length; i++) levels[i] = new Dictionary<Vector2Int, Kind>();
            foreach (Run run in Runs)
            {
                Vector2Int step = new(System.Math.Sign(run.To.x - run.From.x), System.Math.Sign(run.To.y - run.From.y));
                for (Vector2Int c = run.From; ; c += step)
                {
                    levels[run.Level][c] = run.Kind;
                    if (c == run.To || step == Vector2Int.zero) break;
                }
            }
            return levels;
        }

        public static float Height(int level) => level == 0 ? 0f : L.Tiers[level - 1];

        public static bool IsRoom(Vector2Int cell)
        {
            foreach (L.Room room in L.Rooms) if (room.Cells.Contains(cell)) return true;
            return false;
        }

        /// <summary>Sides of a cell that open onto something walkable at its own height: another catwalk, a room
        /// roof (tier 1 only), the top or foot of a stair, or the top of a ladder.</summary>
        public static List<Vector2Int> OpenSides(Dictionary<Vector2Int, Kind>[] cells, int level, Vector2Int cell)
        {
            var open = new List<Vector2Int>();
            foreach (Vector2Int side in Sides)
            {
                Vector2Int next = cell + side;
                bool joined = cells[level].ContainsKey(next) || level == 1 && IsRoom(next);
                foreach (Stair s in Stairs)
                    joined |= s.FromLevel == level && s.Foot == cell && s.Direction == side
                           || s.FromLevel + 1 == level && s.Top == cell && s.Direction == -side;
                foreach (LadderSpot l in Ladders) joined |= l.Level == level && l.Cell == cell && l.Side == side;
                if (joined) open.Add(side);
            }
            return open;
        }

        public static List<string> Problems(Dictionary<Vector2Int, Kind>[] cells)
        {
            var problems = new List<string>();
            bool Inside(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < L.Width && c.y < L.Depth;
            for (int level = 1; level < cells.Length; level++)
                foreach (Vector2Int c in cells[level].Keys)
                {
                    if (!Inside(c)) problems.Add($"tier {level} cell {c} is outside the building");
                    if (level == 1 && IsRoom(c)) problems.Add($"tier 1 cell {c} sits on a room roof");
                }
            foreach (Stair s in Stairs)
            {
                bool footOk = s.FromLevel == 0 ? !L.InChannel(s.Foot) && !IsRoom(s.Foot) : cells[s.FromLevel].ContainsKey(s.Foot);
                if (!footOk) problems.Add($"{s.Name}: nothing to stand on at its foot {s.Foot}");
                if (!cells[s.FromLevel + 1].ContainsKey(s.Top)) problems.Add($"{s.Name}: no catwalk at its top {s.Top}");
                foreach (Vector2Int r in s.RunCells)
                    if (cells[s.FromLevel + 1].ContainsKey(r) || s.FromLevel > 0 && cells[s.FromLevel].ContainsKey(r)
                        || s.FromLevel == 0 && (L.InChannel(r) || IsRoom(r)))
                        problems.Add($"{s.Name}: cell {r} under the flight is not clear");
            }
            foreach (LadderSpot l in Ladders)
            {
                if (!cells[l.Level].ContainsKey(l.Cell)) problems.Add($"ladder at tier {l.Level} {l.Cell}: no catwalk at its top");
                if (cells[l.Level].ContainsKey(l.Below)) problems.Add($"ladder at tier {l.Level} {l.Cell}: its climb space {l.Below} is a catwalk");
                bool floor = l.Level == 1 ? !L.InChannel(l.Below) && !IsRoom(l.Below) : cells[l.Level - 1].ContainsKey(l.Below);
                if (!floor) problems.Add($"ladder at tier {l.Level} {l.Cell}: nothing to stand on at its foot {l.Below}");
            }
            return problems;
        }

        /// <summary>Plan view of one tier for the Console (north at the top): S spine, # catwalk, w weak,
        /// n narrow, R room roof, ~ lava below.</summary>
        public static string Map(Dictionary<Vector2Int, Kind>[] cells, int level)
        {
            var text = new StringBuilder();
            for (int z = L.Depth - 1; z >= 0; z--)
            {
                for (int x = 0; x < L.Width; x++)
                {
                    var c = new Vector2Int(x, z);
                    char ch = cells[level].TryGetValue(c, out Kind k)
                        ? k switch { Kind.Spine => 'S', Kind.Weak => 'w', Kind.Narrow => 'n', _ => '#' }
                        : level == 1 && IsRoom(c) ? 'R' : L.InChannel(c) ? '~' : '.';
                    text.Append(ch);
                }
                text.Append('\n');
            }
            return text.ToString();
        }
    }
}

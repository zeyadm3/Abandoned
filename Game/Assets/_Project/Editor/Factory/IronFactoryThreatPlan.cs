using System.Collections.Generic;
using UnityEngine;
using Kind = Abandoned.EditorTools.IronFactoryCatwalkPlan.Kind;
using L = Abandoned.EditorTools.IronFactoryLayout;
using Machines = Abandoned.EditorTools.IronFactoryMachinePlan;
using Walks = Abandoned.EditorTools.IronFactoryCatwalkPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Where monsters may appear in the Iron Factory: deep on both banks of the lava, in the back rooms, and up
    /// on catwalk tiers 1 and 2 (so the maze overhead is never safe), always well away from the dock the crew
    /// comes in by. Tier 3 and the narrow bridge stay monster-free at the start. Every ground point is checked
    /// clear of machines, hiding places, stairs and the lava; every catwalk point must be a real walkway cell.
    /// </summary>
    public static class IronFactoryThreatPlan
    {
        private const float MinFromDock = 30f;

        private static readonly Vector2[] Ground =
        {
            new(10f, 52f), new(36f, 66f), new(20f, 14f), new(60f, 90f), new(16f, 92f),
            new(92f, 56f), new(110f, 40f), new(116f, 62f), new(88f, 90f), new(80f, 74f), new(100f, 12f),
        };

        private static readonly (int level, int x, int z)[] Catwalk =
        {
            (1, 2, 14), (1, 25, 10), (1, 12, 16), (1, 18, 15), (1, 14, 20),
            (2, 4, 16), (2, 21, 17), (2, 8, 12),
        };

        public static List<Vector3> Points(Dictionary<Vector2Int, Kind>[] cells, out List<string> problems)
        {
            problems = new List<string>();
            var blockers = new List<Rect>();
            foreach (Machines.Machine m in Machines.Machines)
            {
                Vector3 s = Machines.Size(m);
                blockers.Add(new Rect(m.At.x - s.x * .5f - 1f, m.At.y - s.z * .5f - 1f, s.x + 2f, s.z + 2f));
            }
            foreach (Machines.Spot spot in Machines.Spots)
            {
                Bounds b = Machines.SpotBounds(spot);
                blockers.Add(new Rect(b.min.x - .8f, b.min.z - .8f, b.size.x + 1.6f, b.size.z + 1.6f));
            }
            Rect c = L.ChannelMeters;
            blockers.Add(new Rect(c.x - 1.5f, c.y - 1.5f, c.width + 3f, c.height + 3f));
            foreach (Walks.Stair st in Walks.Stairs)
                if (st.FromLevel == 0)
                    foreach (Vector2Int r in st.RunCells) blockers.Add(new Rect(r.x * L.Tile, r.y * L.Tile, L.Tile, L.Tile));

            var dock = new Vector2(L.DockX, 0f);
            var points = new List<Vector3>();
            foreach (Vector2 p in Ground)
            {
                string why = null;
                foreach (Rect b in blockers) if (b.Contains(p)) { why = "it stands in a machine, hiding place, stair or the lava"; break; }
                if (why == null && Vector2.Distance(p, dock) < MinFromDock) why = "it is too near the dock";
                if (why == null && OnRoomWall(p)) why = "it is on a room wall";
                if (why != null) { problems.Add($"ground spawn {p}: {why}"); continue; }
                points.Add(new Vector3(p.x, 0f, p.y));
            }
            foreach ((int level, int x, int z) in Catwalk)
            {
                var cell = new Vector2Int(x, z);
                if (!cells[level].TryGetValue(cell, out Kind kind) || kind == Kind.Narrow)
                {
                    problems.Add($"tier {level} spawn {cell}: no walkway there");
                    continue;
                }
                points.Add(L.CellCentre(cell, Walks.Height(level)));
            }
            return points;
        }

        private static bool OnRoomWall(Vector2 p)
        {
            foreach (L.Room room in L.Rooms)
            {
                Rect r = room.Meters;
                Rect inner = new(r.x + .6f, r.y + .6f, r.width - 1.2f, r.height - 1.2f);
                if (r.Contains(p) && !inner.Contains(p)) return true;
            }
            return false;
        }
    }
}

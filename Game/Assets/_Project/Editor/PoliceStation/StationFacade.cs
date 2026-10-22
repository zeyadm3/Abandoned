using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using P = Abandoned.EditorTools.StationPlan;
using Hole = Abandoned.EditorTools.StationWalls.Hole;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The outer shell: brick from the ground to the parapet on every side of the footprint (custom cells such as a
    /// hose tower are left to their own builder), cut by the plan's doors and vehicle openings and by a window per bay
    /// and storey where the room inside wants one. A stone band marks the upper floor, a coping caps the parapet, and
    /// the flat roof covers it all. Windows are glazed and solid (some boarded up): the way in is through the doors.
    /// </summary>
    public static class StationFacade
    {
        private const float WindowWidth = 1.8f, Sill = 1f, Head = 2.4f;

        public static int Build(Transform root, StationPlan plan, StationArt art)
        {
            Transform shell = Group("Shell", root);
            int boxes = 0;
            foreach (P.Side side in new[] { P.Side.South, P.Side.North, P.Side.West, P.Side.East })
                foreach ((float from, float to, List<Vector2Int> cells) in Runs(plan, side))
                {
                    bool alongX = side is P.Side.South or P.Side.North;
                    float line = side switch
                    {
                        P.Side.South => 0f,
                        P.Side.North => plan.Depth * P.Tile,
                        P.Side.West => 0f,
                        _ => plan.Width * P.Tile,
                    };
                    List<Hole> holes = Openings(plan, side, from, to, cells);
                    string name = $"{side}_{from:0}";
                    // At the building's corners the south and north walls run on over the ends of the side walls, which
                    // stop short, so the corner is closed and no two outer faces share a plane.
                    float half = P.ShellThickness * .5f, length = alongX ? plan.Width * P.Tile : plan.Depth * P.Tile;
                    float a = from, b = to;
                    if (alongX) { if (from <= 0f) a -= half; if (to >= length) b += half; }
                    else { if (from <= 0f) a += half; if (to >= length) b -= half; }
                    boxes += StationWalls.Run(shell, "Facade_" + name, alongX, line, a, b, 0f, plan.ParapetTop, P.ShellThickness, art.Brick, holes, art);
                    // The band at the upper floor and the coping, both standing proud of the brick outside.
                    float outward = side is P.Side.South or P.Side.West ? -1f : 1f;
                    float proud = P.ShellThickness * .5f + .04f;
                    boxes += StationWalls.Run(shell, "Band_" + name, alongX, line, from, to, P.Story - .15f, P.Story + .2f, .1f, art.Trim,
                        holes.Where(h => h.Top > P.Story - .15f && h.Bottom < P.Story + .2f).ToList(), art, outward * proud);
                    boxes += StationWalls.Run(shell, "Plinth_" + name, alongX, line, from, to, -.05f, .45f, .08f, art.Trim,
                        holes.Where(h => h.Bottom < .45f).ToList(), art, outward * proud);
                    float lip = (P.ShellThickness + .14f) * .5f;
                    boxes += StationWalls.Run(shell, "Coping_" + name, alongX, line, alongX ? a - .07f : a + lip - half, alongX ? b + .07f : b - lip + half,
                        plan.ParapetTop, plan.ParapetTop + .12f, P.ShellThickness + .14f, art.Trim, new List<Hole>(), art);
                    boxes += Sills(shell, holes, alongX, line, outward, art);
                }
            boxes += Roof(shell, plan, art);
            MarkStatic(shell);
            return boxes;
        }

        // Contiguous stretches of a side whose inside cell is not custom.
        private static IEnumerable<(float from, float to, List<Vector2Int> cells)> Runs(StationPlan plan, P.Side side)
        {
            int count = side is P.Side.South or P.Side.North ? plan.Width : plan.Depth;
            var cells = new List<Vector2Int>();
            float from = 0f;
            for (int i = 0; i <= count; i++)
            {
                Vector2Int c = side switch
                {
                    P.Side.South => new Vector2Int(i, 0),
                    P.Side.North => new Vector2Int(i, plan.Depth - 1),
                    P.Side.West => new Vector2Int(0, i),
                    _ => new Vector2Int(plan.Width - 1, i),
                };
                bool usable = i < count && !plan.Custom.Contains(c);
                if (usable)
                {
                    if (cells.Count == 0) from = i * P.Tile;
                    cells.Add(c);
                    continue;
                }
                if (cells.Count > 0) yield return (from, i * P.Tile, new List<Vector2Int>(cells));
                cells.Clear();
            }
        }

        private static List<Hole> Openings(StationPlan plan, P.Side side, float from, float to, List<Vector2Int> cells)
        {
            var holes = plan.Facade.Where(o => o.Side == side).Select(o => new Hole(o.Centre, o.Width, o.Bottom, o.Top, o.Glazed)).ToList();
            bool alongX = side is P.Side.South or P.Side.North;
            foreach (Vector2Int c in cells)
            {
                float a = alongX ? c.x * P.Tile : c.y * P.Tile, mid = a + P.Tile * .5f;
                for (int f = 0; f <= 1; f++)
                {
                    if (!plan.HasFloor(f, c) && !(f == 1 && plan.HasFloor(0, c))) continue;
                    P.Room room = plan.RoomAt(f, c) ?? (plan.HasFloor(f, c) ? null : plan.RoomAt(0, c));
                    if (room != null && !room.Windows) continue;
                    float bottom = P.FloorY(f) + Sill, top = P.FloorY(f) + Head;
                    bool clash = holes.Any(h => h.Centre + h.Width * .5f > a - .3f && h.Centre - h.Width * .5f < a + P.Tile + .3f &&
                                                h.Top > bottom - .2f && h.Bottom < top + .2f);
                    if (!clash) holes.Add(new Hole(mid, WindowWidth, bottom, top, true));
                }
            }
            return holes;
        }

        // A stone sill under each window, outside.
        private static int Sills(Transform shell, List<Hole> holes, bool alongX, float line, float outward, StationArt art)
        {
            int made = 0;
            Transform sills = Group("Sills", shell);
            foreach (Hole h in holes)
            {
                if (!h.Glazed || h.Bottom < .2f) continue;
                float off = outward * (P.ShellThickness * .5f + .06f);
                Vector3 c = alongX ? new Vector3(h.Centre, h.Bottom - .05f, line + off) : new Vector3(line + off, h.Bottom - .05f, h.Centre);
                Vector3 s = alongX ? new Vector3(h.Width + .2f, .1f, .14f) : new Vector3(.14f, .1f, h.Width + .2f);
                Box("Sill", sills, c, s, art.Trim, false);
                made++;
            }
            return made;
        }

        // The flat roof over every non-custom cell, one slab per row run.
        private static int Roof(Transform shell, StationPlan plan, StationArt art)
        {
            Transform roof = Group("Roof", shell);
            int made = 0;
            for (int z = 0; z < plan.Depth; z++)
            {
                int start = -1;
                for (int x = 0; x <= plan.Width; x++)
                {
                    bool use = x < plan.Width && !plan.Custom.Contains(new Vector2Int(x, z));
                    if (use && start < 0) start = x;
                    if (use || start < 0) continue;
                    float x0 = start * P.Tile, x1 = x * P.Tile;
                    Box($"Roof_{z}_{start}", roof, new Vector3((x0 + x1) * .5f, plan.RoofY + P.Slab * .5f, (z + .5f) * P.Tile),
                        new Vector3(x1 - x0, P.Slab, P.Tile), art.Roof);
                    // The ceiling under the roof, a hand's width below so the two never share a plane.
                    Box($"Ceiling_{z}_{start}", roof, new Vector3((x0 + x1) * .5f, plan.RoofY - .012f, (z + .5f) * P.Tile),
                        new Vector3(x1 - x0, .02f, P.Tile), art.Ceiling, false);
                    made += 2;
                    start = -1;
                }
            }
            return made;
        }
    }
}

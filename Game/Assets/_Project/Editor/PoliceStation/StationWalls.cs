using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Walls from the plan's grid. Every edge between two cells gets at most one wall: one where two different rooms (or
    /// a room and the hall) meet, a railing where a hall meets a stairwell, nothing between hall and hall. Doors are cut
    /// where the plan lists them. Straight neighbouring pieces of the same kind merge into one run, so a 40 m corridor
    /// wall is a few boxes, not ten. The outer brick shell, its windows and openings are <see cref="StationFacade"/>.
    /// </summary>
    public static class StationWalls
    {
        public readonly struct Hole
        {
            public readonly float Centre, Width, Bottom, Top;
            public readonly bool Glazed;
            public Hole(float centre, float width, float bottom, float top, bool glazed = false)
            { Centre = centre; Width = width; Bottom = bottom; Top = top; Glazed = glazed; }
        }

        private sealed class Segment
        {
            public int Floor;
            public bool AlongX;     // the wall runs along x (an edge between z rows)
            public float Line, Start, End;
            public Material Material;
            public bool Rail;
            public readonly List<Hole> Holes = new();
        }

        public static int BuildInterior(Transform root, StationPlan plan, StationArt art)
        {
            Transform walls = Group("Walls", root);
            Transform rails = Group("Railings", root);
            var segments = new List<Segment>();
            int[] floors = plan.Floors.Keys.OrderBy(f => f).ToArray();
            foreach (int f in floors)
            for (int x = -1; x < plan.Width; x++)
            for (int z = -1; z < plan.Depth; z++)
            {
                var c = new Vector2Int(x, z);
                foreach (Vector2Int step in new[] { Vector2Int.right, Vector2Int.up })
                {
                    Vector2Int n = c + step;
                    Segment s = Edge(plan, art, f, c, n, step == Vector2Int.up);
                    if (s != null) segments.Add(s);
                }
            }
            int pieces = 0;
            foreach (Segment run in Merge(segments))
            {
                float y = P.FloorY(run.Floor);
                Transform parent = run.Rail ? rails : walls;
                if (run.Rail) pieces += Railing(parent, run, y, art);
                else pieces += Run(parent, $"Wall_{run.Floor}_{(run.AlongX ? "X" : "Z")}_{run.Line:0}_{run.Start:0}", run.AlongX, run.Line,
                    run.Start, run.End, y, y + P.Story - P.Slab, P.WallThickness, run.Material, run.Holes, art);
            }
            MarkStatic(walls);
            return pieces;
        }

        private static Segment Edge(StationPlan plan, StationArt art, int f, Vector2Int a, Vector2Int b, bool alongX)
        {
            bool aIn = plan.InFootprint(a), bIn = plan.InFootprint(b);
            if (!aIn && !bIn) return null;
            if (plan.Custom.Contains(a) || plan.Custom.Contains(b)) return null;
            var key = P.Edge(f, a, b);
            if (plan.OpenEdges.Contains(key)) return null;
            bool aFloor = plan.HasFloor(f, a), bFloor = plan.HasFloor(f, b);
            if (!aFloor && !bFloor) return null;
            // Outer edges above ground belong to the facade; the basement's are retaining walls.
            if ((!aIn || !bIn) && f >= 0) return null;

            float line = alongX ? (a.y + 1) * P.Tile : (a.x + 1) * P.Tile;
            float start = alongX ? a.x * P.Tile : a.y * P.Tile;
            var s = new Segment { Floor = f, AlongX = alongX, Line = line, Start = start, End = start + P.Tile };
            P.Room ra = aFloor ? plan.RoomAt(f, a) : null, rb = bFloor ? plan.RoomAt(f, b) : null;

            if (plan.Rails.Contains(key)) { s.Rail = true; s.Material = art.Steel; return s; }
            if (aFloor && bFloor)
            {
                if (ra == rb) return null;
                P.Room owner = ra ?? rb;
                s.Material = f < 0 && owner.Finish == P.Finish.Plaster ? art.Block : art.For(owner.Finish);
                P.Doorway door = plan.DoorOn(f, a, b);
                if (door != null)
                    s.Holes.Add(new Hole(start + P.Tile * .5f, door.Width, P.FloorY(f), P.FloorY(f) + door.Height));
                return s;
            }
            // One side has no slab on this storey: a stairwell (railing beside a hall, wall beside a room), the void
            // of a tall room, or outside the basement.
            Vector2Int open = aFloor ? b : a;
            P.Room room = aFloor ? ra : rb;
            bool hole = plan.InFootprint(open) && plan.IsHole(f, open);
            if (hole && room == null) { s.Rail = true; s.Material = art.Steel; return s; }
            s.Material = f < 0 ? art.Concrete : room != null ? art.For(room.Finish) : art.Plaster;
            return s;
        }

        // Neighbouring segments on one line, same storey, kind and material, become one run.
        private static IEnumerable<Segment> Merge(List<Segment> segments)
        {
            foreach (var group in segments.GroupBy(x => (x.Floor, x.AlongX, x.Line, x.Material, x.Rail)))
            {
                Segment current = null;
                foreach (Segment seg in group.OrderBy(x => x.Start))
                {
                    if (current != null && Mathf.Abs(current.End - seg.Start) < .01f)
                    {
                        current.End = seg.End;
                        current.Holes.AddRange(seg.Holes);
                        continue;
                    }
                    if (current != null) yield return current;
                    current = new Segment { Floor = seg.Floor, AlongX = seg.AlongX, Line = seg.Line, Start = seg.Start, End = seg.End, Material = seg.Material, Rail = seg.Rail };
                    current.Holes.AddRange(seg.Holes);
                }
                if (current != null) yield return current;
            }
        }

        /// <summary>
        /// A straight wall from <paramref name="start"/> to <paramref name="end"/> along one axis, between two heights, with
        /// holes cut in it: piers between them, a sill under and a lintel over each. Glazed holes get a solid pane.
        /// Returns how many boxes it made.
        /// </summary>
        public static int Run(Transform parent, string name, bool alongX, float line, float start, float end, float bottom, float top,
            float thickness, Material material, IList<Hole> holes, StationArt art, float offset = 0f)
        {
            int made = 0;
            Transform run = Group(name, parent);
            List<Hole> cuts = holes.Where(h => h.Centre + h.Width * .5f > start + .01f && h.Centre - h.Width * .5f < end - .01f &&
                                               h.Top > bottom + .01f && h.Bottom < top - .01f).ToList();
            void Piece(string n, float from, float to, float y0, float y1, Material m, bool solid = true, float depth = -1f)
            {
                if (to - from < .02f || y1 - y0 < .02f) return;
                float mid = (from + to) * .5f, my = (y0 + y1) * .5f, d = depth > 0f ? depth : thickness;
                Vector3 centre = alongX ? new Vector3(mid, my, line + offset) : new Vector3(line + offset, my, mid);
                Vector3 size = alongX ? new Vector3(to - from, y1 - y0, d) : new Vector3(d, y1 - y0, to - from);
                Box(n, run, centre, size, m, solid);
                made++;
            }
            float L(Hole h) => Mathf.Max(start, h.Centre - h.Width * .5f);
            float R(Hole h) => Mathf.Min(end, h.Centre + h.Width * .5f);
            // Split the run into columns at every hole's edge; in each column fill what no hole covers, bottom to top.
            var edges = new SortedSet<float> { start, end };
            foreach (Hole h in cuts) { edges.Add(L(h)); edges.Add(R(h)); }
            float[] xs = edges.ToArray();
            float pierFrom = start;
            for (int i = 0; i + 1 < xs.Length; i++)
            {
                float x0 = xs[i], x1 = xs[i + 1];
                if (x1 - x0 < .005f) continue;
                List<Hole> over = cuts.Where(h => L(h) <= x0 + .001f && R(h) >= x1 - .001f).OrderBy(h => h.Bottom).ToList();
                if (over.Count == 0) continue;
                // Full-height stretch before this column: one pier.
                Piece("Pier", pierFrom, x0, bottom, top, material);
                pierFrom = x1;
                float y = bottom;
                foreach (Hole h in over)
                {
                    float hb = Mathf.Clamp(h.Bottom, bottom, top), ht = Mathf.Clamp(h.Top, bottom, top);
                    if (hb > y) Piece("Infill", x0, x1, y, hb, material);
                    y = Mathf.Max(y, ht);
                }
                if (top > y) Piece("Infill", x0, x1, y, top, material);
            }
            Piece("Pier", pierFrom, end, bottom, top, material);
            // One pane (or boards) per glazed hole, across its whole width.
            foreach (Hole h in cuts)
            {
                if (!h.Glazed) continue;
                float l = L(h), r = R(h), y0 = Mathf.Max(bottom, h.Bottom), y1 = Mathf.Min(top, h.Top), mid = (l + r) * .5f;
                bool boarded = Mathf.Abs(Mathf.RoundToInt(mid * 7f + y0 * 13f + line * 3f)) % 5 == 0;
                Piece(boarded ? "Boards" : "Pane", l, r, y0, y1, boarded ? art.Board : art.Glass, true, boarded ? .06f : .03f);
            }
            return made;
        }

        private static int Railing(Transform parent, Segment s, float floorY, StationArt art)
        {
            Transform rail = Group($"Rail_{s.Floor}_{s.Line:0}_{s.Start:0}", parent);
            float length = s.End - s.Start, mid = (s.Start + s.End) * .5f;
            Vector3 Along(float t, float y) => s.AlongX ? new Vector3(t, floorY + y, s.Line) : new Vector3(s.Line, floorY + y, t);
            Vector3 Size(float len, float h, float w) => s.AlongX ? new Vector3(len, h, w) : new Vector3(w, h, len);
            Box("TopRail", rail, Along(mid, 1.05f), Size(length, .06f, .07f), art.Safety, false);
            Box("MidRail", rail, Along(mid, .55f), Size(length, .04f, .04f), art.Steel, false);
            Box("Kick", rail, Along(mid, .07f), Size(length, .14f, .03f), art.Steel, false);
            int posts = Mathf.Max(2, Mathf.CeilToInt(length / 1.3f) + 1);
            for (int i = 0; i < posts; i++)
                Box("Post", rail, Along(s.Start + length * i / (posts - 1), .53f), Size(.06f, 1.06f, .06f), art.Steel, false);
            // One thin solid for the whole run: nobody walks off the edge, loot doesn't roll off.
            var solid = rail.gameObject.AddComponent<BoxCollider>();
            solid.center = rail.InverseTransformPoint(Along(mid, .6f));
            solid.size = Size(length, 1.2f, .1f);
            return 1;
        }
    }
}

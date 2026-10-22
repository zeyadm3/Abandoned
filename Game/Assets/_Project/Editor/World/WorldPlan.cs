using System.Collections.Generic;
using Abandoned.World;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Where everything in the open world is (world metres; the HQ's own origin is the world's origin). Six kilometres
    /// square of wooded hills; each place sits on its own level ground about two kilometres from its neighbours, and a
    /// road leaves the HQ's district on the side facing each place. Places whose scenes aren't built yet get their site
    /// and road now, and a flat stand-in until their scene exists.
    /// </summary>
    public static class WorldPlan
    {
        public const float Size = 6144f, Half = Size / 2f;
        public const int Tiles = 6;
        public const float TileSize = Size / Tiles;
        public const float RoadHalfWidth = 4.2f;
        /// <summary>Lowest and highest the land may go (terrain base and height).</summary>
        public const float Floor = -60f, Height = 260f;

        /// <summary>The places, in atlas order (0 is the HQ).</summary>
        public static List<WorldPoi> Places()
        {
            var places = new List<WorldPoi>
            {
                // The HQ's company yard and its vacant district stand on one flat plane (DistrictGround, y -0.47).
                new()
                {
                    scene = "HQ", worldPosition = Vector3.zero, ground = Rect.MinMaxRect(-302f, -328f, 338f, 312f), groundHeight = -.47f,
                    hideInWorld = new[] { "HQ/IndustrialEstateSurroundings/IndustrialEstateLandscape/EstateScenery" },
                    parking = new Vector3(10f, 0f, -22f), built = true,
                },
                Planned("Mall", new Vector3(2000f, 0f, 600f)),
                Planned("School", new Vector3(-1900f, 0f, 1300f)),
                Planned("Factory", new Vector3(900f, 0f, -2100f)),
                Planned("Prison", new Vector3(-2250f, 0f, -2100f)),
                Planned("Hospital", new Vector3(1750f, 0f, 2300f)),
                Planned("Motel", new Vector3(-700f, 0f, -2300f)),
                Planned("PoliceStation", new Vector3(-2300f, 0f, -300f)),
                Planned("FireStation", new Vector3(-200f, 0f, 2300f)),
                Planned("Skyscraper", new Vector3(2400f, 0f, -1500f)),
            };
            return places;
        }

        // A place not yet fitted into the world: a 400 m square of level ground round its origin.
        private static WorldPoi Planned(string scene, Vector3 at) => new()
        {
            scene = scene, worldPosition = at, ground = Rect.MinMaxRect(-200f, -200f, 200f, 200f), groundHeight = 0f,
            parking = new Vector3(0f, 0f, -150f), built = false,
        };

        /// <summary>World rectangle of a place's ground.</summary>
        public static Rect GroundRect(WorldPoi p) =>
            new(p.ground.xMin + p.worldPosition.x, p.ground.yMin + p.worldPosition.z, p.ground.width, p.ground.height);

        public static float GroundY(WorldPoi p) => p.worldPosition.y + p.groundHeight;

        /// <summary>
        /// Each road as world points: out of the HQ's district on the side facing the place, a bend on the way (seeded
        /// from the place's name, so rebuilds match), into the middle of the place's nearest side.
        /// </summary>
        public static List<Vector2[]> Roads(IReadOnlyList<WorldPoi> places)
        {
            var roads = new List<Vector2[]>();
            WorldPoi home = places[0];
            Rect hq = GroundRect(home);
            for (int i = 1; i < places.Count; i++)
            {
                Rect site = GroundRect(places[i]);
                Vector2 toSite = (site.center - hq.center).normalized;
                Vector2 start = EdgeToward(hq, toSite), end = EdgeToward(site, -toSite);
                Vector2 outward = Outward(hq, start), inward = Outward(site, end);
                var rng = new System.Random(places[i].scene.GetHashCode() & 0x7fffffff);
                Vector2 side = new(-toSite.y, toSite.x);
                Vector2 mid = (start + end) * .5f + side * (float)(rng.NextDouble() * 2 - 1) * 260f;
                roads.Add(new[]
                {
                    start - outward * 40f, start + outward * 90f, Vector2.Lerp(start, mid, .55f) + side * 60f, mid,
                    Vector2.Lerp(mid, end, .45f) - side * 60f, end + inward * 90f, end - inward * 40f,
                });
            }
            return roads;
        }

        // The middle of the rectangle's side that faces a direction.
        private static Vector2 EdgeToward(Rect r, Vector2 dir)
        {
            if (Mathf.Abs(dir.x) * r.height > Mathf.Abs(dir.y) * r.width)
                return new Vector2(dir.x > 0 ? r.xMax : r.xMin, r.center.y);
            return new Vector2(r.center.x, dir.y > 0 ? r.yMax : r.yMin);
        }

        private static Vector2 Outward(Rect r, Vector2 edge)
        {
            if (Mathf.Approximately(edge.x, r.xMax)) return Vector2.right;
            if (Mathf.Approximately(edge.x, r.xMin)) return Vector2.left;
            return edge.y >= r.center.y ? Vector2.up : Vector2.down;
        }

        /// <summary>Catmull-Rom through the points, one sample every <paramref name="step"/> metres.</summary>
        public static List<Vector2> Smooth(Vector2[] points, float step)
        {
            var dense = new List<Vector2>();
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 p0 = points[Mathf.Max(0, i - 1)], p1 = points[i], p2 = points[i + 1], p3 = points[Mathf.Min(points.Length - 1, i + 2)];
                int n = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(p1, p2) / (step * .5f)));
                for (int k = 0; k < n; k++)
                {
                    float t = k / (float)n, t2 = t * t, t3 = t2 * t;
                    dense.Add(.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3));
                }
            }
            dense.Add(points[^1]);
            // Even spacing along the curve.
            var even = new List<Vector2> { dense[0] };
            float carried = 0f;
            for (int i = 1; i < dense.Count; i++)
            {
                Vector2 a = dense[i - 1], b = dense[i];
                float len = Vector2.Distance(a, b);
                while (carried + len >= step)
                {
                    float t = (step - carried) / len;
                    a = Vector2.Lerp(a, b, t);
                    even.Add(a);
                    len = Vector2.Distance(a, b);
                    carried = 0f;
                }
                carried += len;
            }
            if (Vector2.Distance(even[^1], points[^1]) > step * .3f) even.Add(points[^1]);
            return even;
        }

        /// <summary>The land's own height at world (x, z), before places and roads are levelled into it.</summary>
        public static float Natural(float x, float z)
        {
            float hills = (ShowcaseSitePlan.Fbm(x * .0012f + 13.7f, z * .0012f - 4.1f, 5) - .45f) * 150f;
            float rolling = (ShowcaseSitePlan.Fbm(x * .006f + 3.3f, z * .006f + 9.1f, 4) - .5f) * 22f;
            float detail = (ShowcaseSitePlan.Fbm(x * .04f - 7f, z * .04f + 2f, 3) - .5f) * 2.2f;
            // The far edge climbs into a ring of ridges, so the world ends in hills, not a cliff.
            float edge = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
            float rim = 140f * ShowcaseSitePlan.Smooth(Half - 700f, Half - 60f, edge);
            return hills + rolling + detail + rim;
        }

        /// <summary>How much forest stands at world (x, z), 0..1.</summary>
        public static float Forest(float x, float z) =>
            ShowcaseSitePlan.Smooth(.36f, .56f, ShowcaseSitePlan.Fbm(x * .004f + 11f, z * .004f - 5f, 4));
    }
}

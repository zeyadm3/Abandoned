using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The drawn city plan as data (Docs/CITY_PLAN.md): the coast, the river, the Circus, the parks, the districts
    /// and the slot each salvage location stands on. Step 1 of the city build — no geometry at all, so the whole
    /// layout can be looked at (<see cref="CityPlanPreview"/>) and argued with before anything is built from it.
    ///
    /// Everything here is world metres (x east, z north), traced from the user's drawing at
    /// <see cref="MetresPerPixel"/> with the drawing's centre at the world origin. The drawing is the template, not
    /// the final size: its blocks are read as two-by-two, so a drawn block of about 240 m becomes four real
    /// <see cref="Block"/>-metre blocks and the city carries the ~50 blocks across that the user asked for.
    ///
    /// The traced numbers live in CityPlan.Data.cs.
    /// </summary>
    public static partial class CityPlan
    {
        /// <summary>The drawing's scale. 1160 drawn pixels of city across become the 6 km band in the world.</summary>
        public const float MetresPerPixel = 5.2f;
        /// <summary>A city block before its streets are cut out (the user's answer, 2026-10-24).</summary>
        public const float Block = 120f;
        /// <summary>Side of a streaming cell. One cell is about two blocks, and merges to ~4 renderers.</summary>
        public const float Cell = 256f;
        /// <summary>Half-width of an arterial (the drawing's yellow roads) and of a minor street.</summary>
        public const float ArterialHalfWidth = 7f, StreetHalfWidth = 4f;
        /// <summary>The city's ground sits here; the sea is below it and the river is cut down into it.</summary>
        public const float GroundY = 0f, SeaY = -4.5f, RiverY = -3.2f;
        /// <summary>How wide the river runs between its banks.</summary>
        public const float RiverWidth = 90f;
        /// <summary>An arterial every so many metres inside a district; minor streets fill in at <see cref="Block"/>.</summary>
        public const float ArterialSpacing = 600f;

        /// <summary>One quarter of the city: its ground, what it is made of, and the angle its streets run at.</summary>
        public readonly struct District
        {
            public readonly string Name;
            public readonly CityKind Kind;
            /// <summary>Its ground as a polygon in world metres (x, z).</summary>
            public readonly Vector2[] Shape;
            /// <summary>Degrees its streets are turned from east-west. Negative turns the east end south.</summary>
            public readonly float GridAngle;
            /// <summary>Lowest and highest a building here may stand, in metres.</summary>
            public readonly Vector2 Storeys;

            public District(string name, CityKind kind, Vector2[] shape, float gridAngle, Vector2 storeys)
            {
                Name = name; Kind = kind; Shape = shape; GridAngle = gridAngle; Storeys = storeys;
            }

            /// <summary>World rectangle it fits inside, for cheap rejection before the polygon test.</summary>
            public Rect Bounds()
            {
                float xMin = float.MaxValue, xMax = float.MinValue, zMin = float.MaxValue, zMax = float.MinValue;
                foreach (Vector2 p in Shape)
                {
                    if (p.x < xMin) xMin = p.x;
                    if (p.x > xMax) xMax = p.x;
                    if (p.y < zMin) zMin = p.y;
                    if (p.y > zMax) zMax = p.y;
                }
                return Rect.MinMaxRect(xMin, zMin, xMax, zMax);
            }

            /// <summary>Is this world point inside? Crossing count, so a concave quarter works.</summary>
            public bool Contains(Vector2 p)
            {
                bool inside = false;
                for (int i = 0, j = Shape.Length - 1; i < Shape.Length; j = i++)
                {
                    Vector2 a = Shape[i], b = Shape[j];
                    if (a.y > p.y == b.y > p.y) continue;
                    if (p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
                }
                return inside;
            }
        }

        /// <summary>A road the drawing names rather than one a district's grid generates: the shore road, the two
        /// embankments, the Circus arms and the highway out of the south-east corner.</summary>
        public readonly struct Arterial
        {
            public readonly string Name;
            public readonly Vector2[] Points;
            public readonly float HalfWidth;

            public Arterial(string name, Vector2[] points, float halfWidth = ArterialHalfWidth)
            {
                Name = name; Points = points; HalfWidth = halfWidth;
            }
        }

        /// <summary>Where a salvage location stands, and which way its ground faces the road that reaches it.</summary>
        public readonly struct Site
        {
            /// <summary>Scene name, matching <c>WorldPoi.scene</c> so step 6 can hand these to the atlas.</summary>
            public readonly string Scene;
            public readonly Vector2 Centre;
            /// <summary>Compass bearing in degrees the approach road comes in on (0 = from the north).</summary>
            public readonly float Approach;
            public readonly string District;

            public Site(string scene, Vector2 centre, float approach, string district)
            {
                Scene = scene; Centre = centre; Approach = approach; District = district;
            }
        }

        /// <summary>World point of a pixel in the drawing, so a retrace can be checked against the image.</summary>
        public static Vector2 FromDrawing(float px, float py) =>
            new((px - DrawingCentre.x) * MetresPerPixel, (DrawingCentre.y - py) * MetresPerPixel);

        /// <summary>Is this world point out at sea? The coast runs north to south and the water lies west of it.</summary>
        public static bool AtSea(Vector2 p)
        {
            Vector2[] c = Coast;
            for (int i = 0; i < c.Length - 1; i++)
            {
                if (p.y > c[i].y || p.y < c[i + 1].y) continue;
                float t = Mathf.InverseLerp(c[i].y, c[i + 1].y, p.y);
                return p.x < Mathf.Lerp(c[i].x, c[i + 1].x, t);
            }
            return p.x < c[p.y > c[0].y ? 0 : c.Length - 1].x;
        }

        /// <summary>Metres from the river's centreline, so banks, bridges and embankments can be placed off it.</summary>
        public static float FromRiver(Vector2 p)
        {
            float best = float.MaxValue;
            for (int i = 0; i < River.Length - 1; i++)
            {
                Vector2 a = River[i], ab = River[i + 1] - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, .001f));
                best = Mathf.Min(best, Vector2.Distance(p, a + ab * t));
            }
            return best;
        }

        /// <summary>The district a world point stands in, or null for water, park edges and the open outskirts.</summary>
        public static District? At(Vector2 p)
        {
            foreach (District d in Districts)
                if (d.Contains(p)) return d;
            return null;
        }

        /// <summary>Cell the point falls in, as whole-cell steps from the world origin.</summary>
        public static Vector2Int CellOf(Vector2 p) =>
            new(Mathf.FloorToInt(p.x / Cell), Mathf.FloorToInt(p.y / Cell));
    }
}

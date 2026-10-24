using System.IO;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Draws <see cref="CityPlan"/> as a top-down image, so the layout can be held next to the user's drawing and
    /// argued with before a single mesh is built. Nothing in the project is changed: it reads the plan, writes one
    /// PNG under Docs/renders/city and says where it put it.
    /// </summary>
    public static class CityPlanPreview
    {
        // The whole world the city sits in, not just its band: the prison island and the east highway live outside it.
        private static readonly Rect View = Rect.MinMaxRect(-4150f, -1900f, 3750f, 1900f);
        private const float PixelsPerMetre = .2f;

        private static readonly Color Sea = new(.56f, .78f, .91f);
        private static readonly Color Outskirts = new(.77f, .80f, .70f);
        private static readonly Color Water = new(.50f, .74f, .90f);
        private static readonly Color Park = new(.66f, .85f, .66f);
        private static readonly Color Road = new(.93f, .86f, .36f);
        private static readonly Color Ink = new(.18f, .17f, .16f);
        private static readonly Color SiteMark = new(.85f, .25f, .20f);

        [MenuItem(ToolMenu.OpenWorld + "Preview City Plan", priority = ToolMenu.OpenWorldP + 2)]
        public static void Render()
        {
            int w = Mathf.RoundToInt(View.width * PixelsPerMetre), h = Mathf.RoundToInt(View.height * PixelsPerMetre);
            var px = new Color[w * h];

            Ground(px, w, h);
            foreach (CityPlan.Arterial a in CityPlan.Named) Stroke(px, w, h, a.Points, a.HalfWidth * 2f, Road);
            Grids(px, w, h);
            Circus(px, w, h);
            Sites(px, w, h);
            Frame(px, w, h);

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels(px);
            tex.Apply();
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Docs/renders/city"));
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, "city-plan.png");
            File.WriteAllBytes(file, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Debug.Log($"[CityPlan] {w} x {h} px over {View.width:0} x {View.height:0} m -> {file}\n" +
                      $"{CityPlan.Districts.Length} districts, {CityPlan.Named.Length} named roads, " +
                      $"{CityPlan.Sites.Length} sites, blocks {CityPlan.Block:0} m, cells {CityPlan.Cell:0} m.");
        }

        // Sea, river, parks and a tint per district kind: the base the roads are drawn over.
        private static void Ground(Color[] px, int w, int h)
        {
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Vector2 p = World(x, y, w, h);
                    Color c;
                    if (CityPlan.AtSea(p)) c = Sea;
                    else if (CityPlan.FromRiver(p) < CityPlan.RiverWidth * .5f) c = Water;
                    else if (InPark(p)) c = Park;
                    else
                    {
                        CityPlan.District? d = CityPlan.At(p);
                        c = d.HasValue ? Tint(d.Value.Kind) : Outskirts;
                    }
                    px[y * w + x] = c;
                }
        }

        private static bool InPark(Vector2 p)
        {
            foreach (Rect r in CityPlan.Parks)
                if (r.Contains(p)) return true;
            return false;
        }

        private static Color Tint(CityKind kind) => kind switch
        {
            CityKind.Downtown => new Color(.62f, .60f, .62f),
            CityKind.Retail => new Color(.82f, .76f, .66f),
            CityKind.Civic => new Color(.76f, .74f, .78f),
            CityKind.Industrial => new Color(.70f, .64f, .58f),
            CityKind.Waterfront => new Color(.72f, .70f, .64f),
            CityKind.Suburb => new Color(.86f, .85f, .74f),
            CityKind.Park => Park,
            _ => new Color(.84f, .81f, .75f),
        };

        // Each district's own street grid, at its own angle: arterials solid, block streets faint.
        private static void Grids(Color[] px, int w, int h)
        {
            foreach (CityPlan.District d in CityPlan.Districts)
            {
                Rect b = d.Bounds();
                float reach = Mathf.Max(b.width, b.height);
                Vector2 centre = b.center;
                float rad = d.GridAngle * Mathf.Deg2Rad;
                Vector2 along = new(Mathf.Cos(rad), Mathf.Sin(rad)), across = new(-along.y, along.x);
                Lines(px, w, h, d, centre, along, across, reach);
                Lines(px, w, h, d, centre, across, along, reach);
            }
        }

        private static void Lines(Color[] px, int w, int h, CityPlan.District d, Vector2 centre,
                                  Vector2 along, Vector2 across, float reach)
        {
            var faint = new Color(.55f, .53f, .50f, .5f);
            for (float o = -reach; o <= reach; o += CityPlan.Block)
            {
                bool arterial = Mathf.Repeat(o, CityPlan.ArterialSpacing) < CityPlan.Block * .5f;
                Vector2 a = centre + across * o - along * reach, bEnd = centre + across * o + along * reach;
                for (float t = 0f; t <= 1f; t += 2f / reach)
                {
                    Vector2 p = Vector2.Lerp(a, bEnd, t);
                    if (!d.Contains(p) || CityPlan.AtSea(p) || CityPlan.FromRiver(p) < CityPlan.RiverWidth * .5f) continue;
                    Dot(px, w, h, p, arterial ? CityPlan.ArterialHalfWidth * 2f : CityPlan.StreetHalfWidth,
                        arterial ? Road : faint);
                }
            }
        }

        // The ring road, its park island and the five arms that leave it.
        private static void Circus(Color[] px, int w, int h)
        {
            for (float a = 0f; a < 360f; a += .4f)
            {
                float r = a * Mathf.Deg2Rad;
                var on = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
                Dot(px, w, h, CityPlan.CircusCentre + on * CityPlan.CircusRing, CityPlan.ArterialHalfWidth * 2f, Road);
                for (float d = 0f; d < CityPlan.CircusIsland; d += 8f)
                    Dot(px, w, h, CityPlan.CircusCentre + on * d, 8f, Park);
            }
            foreach (float bearing in CityPlan.CircusArms)
            {
                float r = (90f - bearing) * Mathf.Deg2Rad;
                var on = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
                for (float d = CityPlan.CircusRing; d < 700f; d += 3f)
                    Dot(px, w, h, CityPlan.CircusCentre + on * d, CityPlan.ArterialHalfWidth * 2f, Road);
            }
        }

        // Every salvage location, as a square on its ground with a stroke toward the road that reaches it.
        private static void Sites(Color[] px, int w, int h)
        {
            foreach (CityPlan.Site s in CityPlan.Sites)
            {
                for (float dx = -90f; dx <= 90f; dx += 3f)
                    for (float dz = -90f; dz <= 90f; dz += 3f)
                        Dot(px, w, h, s.Centre + new Vector2(dx, dz), 3f, SiteMark);
                float r = (90f - s.Approach) * Mathf.Deg2Rad;
                var on = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
                for (float d = 90f; d < 260f; d += 3f) Dot(px, w, h, s.Centre + on * d, 6f, Ink);
            }
        }

        // The city band, so it is obvious what is city and what is the outskirts it already had.
        private static void Frame(Color[] px, int w, int h)
        {
            Rect b = CityPlan.Bounds;
            Stroke(px, w, h, new[]
            {
                new Vector2(b.xMin, b.yMin), new Vector2(b.xMax, b.yMin),
                new Vector2(b.xMax, b.yMax), new Vector2(b.xMin, b.yMax), new Vector2(b.xMin, b.yMin),
            }, 14f, Ink);
            Stroke(px, w, h, CityPlan.Coast, 10f, Ink);
        }

        private static void Stroke(Color[] px, int w, int h, Vector2[] points, float width, Color c)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                float len = Vector2.Distance(points[i], points[i + 1]);
                for (float t = 0f; t <= len; t += 2f)
                    Dot(px, w, h, Vector2.Lerp(points[i], points[i + 1], t / Mathf.Max(len, .001f)), width, c);
            }
        }

        private static void Dot(Color[] px, int w, int h, Vector2 world, float metres, Color c)
        {
            int cx = Mathf.RoundToInt((world.x - View.xMin) * PixelsPerMetre);
            int cy = Mathf.RoundToInt((world.y - View.yMin) * PixelsPerMetre);
            int r = Mathf.Max(0, Mathf.RoundToInt(metres * .5f * PixelsPerMetre));
            for (int y = cy - r; y <= cy + r; y++)
                for (int x = cx - r; x <= cx + r; x++)
                {
                    if (x < 0 || y < 0 || x >= w || y >= h) continue;
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) > r * r) continue;
                    int i = y * w + x;
                    px[i] = c.a >= 1f ? c : Color.Lerp(px[i], c, c.a);
                }
        }

        private static Vector2 World(int x, int y, int w, int h) => new(
            Mathf.Lerp(View.xMin, View.xMax, (x + .5f) / w),
            Mathf.Lerp(View.yMin, View.yMax, (y + .5f) / h));
    }
}

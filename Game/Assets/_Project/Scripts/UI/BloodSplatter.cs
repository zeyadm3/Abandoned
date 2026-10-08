using System;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Original blood-on-the-lens pictures, painted in code (no art pack) and the same on every machine:
    /// medium splatter in the four corners, a thin smeared film, heavy splatter round the whole edge, and drips.
    /// Each is a coverage map of soft discs and tapered strokes: thin blood is red and see-through, pooled
    /// blood nearly black. Built once and kept; <see cref="WarmStep"/> spreads the work over four frames.
    /// </summary>
    public static class BloodSplatter
    {
        private const int W = 640, H = 360;

        private static Texture2D corners, film, heavy, drips;

        public static Texture2D Corners => corners != null ? corners : corners = Build("Blood corners", 4101, PaintCorners);
        public static Texture2D Film => film != null ? film : film = Build("Blood film", 7723, PaintFilm);
        public static Texture2D Heavy => heavy != null ? heavy : heavy = Build("Blood heavy", 1289, PaintHeavy);
        public static Texture2D Drips => drips != null ? drips : drips = Build("Blood drips", 9340, PaintDrips);

        public static bool Ready => corners != null && film != null && heavy != null && drips != null;

        /// <summary>Builds the next missing picture, one per call.</summary>
        public static void WarmStep()
        {
            if (corners == null) _ = Corners;
            else if (film == null) _ = Film;
            else if (heavy == null) _ = Heavy;
            else if (drips == null) _ = Drips;
        }

        // ---- The four pictures (x runs right, y runs DOWN the screen) ----

        // Severe trauma: medium density, the corners only.
        private static void PaintCorners(float[] a, System.Random r)
        {
            float reachMax = W * 0.17f;
            for (int corner = 0; corner < 4; corner++)
            {
                float cx = (corner & 1) == 0 ? 0f : W, cy = (corner & 2) == 0 ? 0f : H;
                float inX = cx == 0f ? 1f : -1f, inY = cy == 0f ? 1f : -1f;
                for (int i = 0; i < 7; i++)
                {
                    float reach = Skew(r, 1.7f) * reachMax, angle = Range(r, 0.08f, 1.49f);
                    float radius = Mathf.Lerp(34f, 12f, reach / reachMax) * Range(r, 0.75f, 1.2f);
                    Splat(a, r, cx + inX * Mathf.Cos(angle) * reach, cy + inY * Mathf.Sin(angle) * reach * 0.8f, radius, 2.2f, 0.95f);
                }
                for (int i = 0; i < 3; i++)
                {
                    // Thrown streaks, pointing in from the corner.
                    float angle = Range(r, 0.2f, 1.37f), start = Range(r, 0.02f, 0.08f) * W, length = Range(r, 26f, 64f), width = Range(r, 2.4f, 5f);
                    float dx = inX * Mathf.Cos(angle), dy = inY * Mathf.Sin(angle);
                    Stamp(a, cx + dx * start, cy + dy * start, cx + dx * (start + length), cy + dy * (start + length), width, width * 0.35f, 1.6f, 0.9f);
                }
                for (int i = 0; i < 34; i++)
                {
                    float reach = Skew(r, 1.3f) * W * 0.27f, angle = Range(r, 0.03f, 1.54f);
                    Dot(a, cx + inX * Mathf.Cos(angle) * reach, cy + inY * Mathf.Sin(angle) * reach * 0.85f, Range(r, 1.2f, 4.6f), 1.2f, 0.9f);
                }
            }
        }

        // Near death, underneath: a few wide, soft, thin smears reaching well into the view (the blur in the spec).
        private static void PaintFilm(float[] a, System.Random r)
        {
            for (int i = 0; i < 16; i++)
            {
                Rim(r, 0.36f, 1.2f, out float x, out float y);
                float radius = Range(r, 40f, 90f);
                Dot(a, x, y, radius, radius * 0.55f, 0.5f);
                Dot(a, x + Range(r, -30f, 30f), y + Range(r, -22f, 22f), radius * 0.6f, radius * 0.4f, 0.4f);
            }
            for (int i = 0; i < 70; i++)
            {
                Rim(r, 0.45f, 1f, out float x, out float y);
                Dot(a, x, y, Range(r, 1.2f, 4.2f), 1.5f, 0.8f);
            }
        }

        // Near death, on top: heavy splatter all the way round the edge.
        private static void PaintHeavy(float[] a, System.Random r)
        {
            for (int i = 0; i < 30; i++)
            {
                Rim(r, 0.24f, 1.5f, out float x, out float y);
                Splat(a, r, x, y, Range(r, 18f, 52f), 4.5f, 0.95f);
            }
            for (int i = 0; i < 10; i++)
            {
                Rim(r, 0.1f, 1f, out float x, out float y);
                // Streaks thrown toward the middle of the screen.
                float dx = W * 0.5f - x, dy = H * 0.5f - y, length = Mathf.Sqrt(dx * dx + dy * dy);
                float run = Range(r, 40f, 95f), width = Range(r, 3f, 6.5f);
                Stamp(a, x, y, x + dx / length * run, y + dy / length * run, width, width * 0.3f, 2.2f, 0.9f);
            }
            for (int i = 0; i < 120; i++)
            {
                Rim(r, 0.32f, 1.2f, out float x, out float y);
                Dot(a, x, y, Range(r, 1.3f, 5.2f), 1.6f, 0.92f);
            }
        }

        // Near death: runs of blood from the top edge and from the splatter on the upper half.
        private static void PaintDrips(float[] a, System.Random r)
        {
            for (int i = 0; i < 15; i++)
            {
                float x = Range(r, 0.03f, 0.97f) * W;
                float top = r.NextDouble() < 0.6 ? -4f : Range(r, 0.02f, 0.3f) * H;
                float length = Range(r, 0.08f, 0.3f) * H, width = Range(r, 2.2f, 5.2f), drift = Range(r, -3f, 3f);
                Dot(a, x, top + width, width * 2.3f, 2f, 0.95f);                                          // where it gathered
                Stamp(a, x, top, x + drift, top + length, width, width * 0.55f, 1.6f, 0.95f);            // the run
                Dot(a, x + drift, top + length, width * 1.25f, 1.6f, 0.98f);                              // the bead at its end
            }
        }

        // ---- Brushes ----

        // One impact: a body, lobes that break up its outline, and a ring of flung droplets.
        private static void Splat(float[] a, System.Random r, float x, float y, float radius, float soft, float strength)
        {
            Dot(a, x, y, radius, soft, strength);
            int lobes = 5 + r.Next(5);
            for (int i = 0; i < lobes; i++)
            {
                float angle = Range(r, 0f, Mathf.PI * 2f), away = radius * Range(r, 0.55f, 1.05f);
                Dot(a, x + Mathf.Cos(angle) * away, y + Mathf.Sin(angle) * away, radius * Range(r, 0.22f, 0.5f), soft, strength);
            }
            int flung = 3 + r.Next(6);
            for (int i = 0; i < flung; i++)
            {
                float angle = Range(r, 0f, Mathf.PI * 2f), away = radius * Range(r, 1.25f, 2.4f);
                Dot(a, x + Mathf.Cos(angle) * away, y + Mathf.Sin(angle) * away, radius * Range(r, 0.05f, 0.14f), 1.2f, strength);
            }
        }

        private static void Dot(float[] a, float x, float y, float radius, float soft, float strength) =>
            Stamp(a, x, y, x, y, radius, radius, soft, strength);

        // A tapered stroke from A to B (a disc when they're the same point), soft over its outer `soft` pixels.
        private static void Stamp(float[] a, float ax, float ay, float bx, float by, float ra, float rb, float soft, float strength)
        {
            float reach = Mathf.Max(ra, rb) + 1f;
            int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ax, bx) - reach)), x1 = Mathf.Min(W - 1, Mathf.CeilToInt(Mathf.Max(ax, bx) + reach));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ay, by) - reach)), y1 = Mathf.Min(H - 1, Mathf.CeilToInt(Mathf.Max(ay, by) + reach));
            if (x0 > x1 || y0 > y1) return;
            float abx = bx - ax, aby = by - ay, length2 = abx * abx + aby * aby;
            soft = Mathf.Max(0.5f, soft);
            for (int y = y0; y <= y1; y++)
            {
                float py = y + 0.5f - ay;
                int row = y * W;
                for (int x = x0; x <= x1; x++)
                {
                    float px = x + 0.5f - ax;
                    float t = length2 > 0.0001f ? Mathf.Clamp01((px * abx + py * aby) / length2) : 0f;
                    float dx = px - abx * t, dy = py - aby * t;
                    float inside = ra + (rb - ra) * t - Mathf.Sqrt(dx * dx + dy * dy);
                    if (inside <= 0f) continue;
                    float v = inside >= soft ? 1f : inside / soft;
                    v = v * v * (3f - 2f * v) * strength;
                    a[row + x] += (1f - a[row + x]) * v;
                }
            }
        }

        // A point within `depth` (a fraction of the screen's height) of a random edge; a higher bias hugs the edge.
        private static void Rim(System.Random r, float depth, float bias, out float x, out float y)
        {
            float inward = Skew(r, bias) * depth * H, along = (float)r.NextDouble();
            switch (r.Next(4))
            {
                case 0: x = along * W; y = inward; break;
                case 1: x = along * W; y = H - inward; break;
                case 2: x = inward; y = along * H; break;
                default: x = W - inward; y = along * H; break;
            }
        }

        private static float Range(System.Random r, float min, float max) => min + (float)r.NextDouble() * (max - min);

        private static float Skew(System.Random r, float power) => Mathf.Pow((float)r.NextDouble(), power);

        private static Texture2D Build(string name, int seed, Action<float[], System.Random> paint)
        {
            var coverage = new float[W * H];
            paint(coverage, new System.Random(seed));
            var pixels = new Color32[W * H];
            // Clear pixels carry the thin-blood red, so filtering at an edge never blends toward black.
            var clear = new Color32(150, 8, 12, 0);
            for (int y = 0; y < H; y++)
            {
                int from = y * W, to = (H - 1 - y) * W; // the map's y runs down the screen; a texture's rows run up
                for (int x = 0; x < W; x++)
                {
                    float c = Mathf.Clamp01(coverage[from + x]);
                    pixels[to + x] = c <= 0.003f ? clear : new Color32(
                        (byte)Mathf.Lerp(150f, 62f, c), (byte)Mathf.Lerp(8f, 0f, c), (byte)Mathf.Lerp(12f, 3f, c),
                        (byte)(Mathf.Clamp01(c * 1.15f) * 235f));
                }
            }
            var texture = new Texture2D(W, H, TextureFormat.RGBA32, false, false)
            {
                name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => corners = film = heavy = drips = null;
    }
}

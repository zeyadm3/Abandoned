using UnityEngine;

namespace Abandoned.EditorTools
{
    // The island prison's ground, made in code like the factory's maps (no art pack): wind-burnt turf, bare earth,
    // sea-worn rock and shingle. Each tiles seamlessly; the island's materials give them their real-world repeat.
    public static partial class IronFactoryTextures
    {
        // Short coastal turf: patchy greens and straws, darker in the hollows, blades as fine streaks.
        private static Map Turf()
        {
            const int n = 1024;
            var m = new Map(n, n) { NormalStrength = 2.2f };
            var rng = new System.Random(911);
            var blades = new float[n * n];
            for (int k = 0; k < 90000; k++)
            {
                int x = rng.Next(n), y = rng.Next(n), len = 3 + rng.Next(7);
                float lean = (float)rng.NextDouble() * 2f - 1f;
                for (int s = 0; s < len; s++)
                {
                    int px = (x + Mathf.RoundToInt(lean * s * .5f) + n) % n, py = (y + s) % n;
                    blades[py * n + px] = Mathf.Max(blades[py * n + px], 1f - s / (float)len);
                }
            }
            Fill(m, (x, y, i) =>
            {
                float patch = Noise(x, y, n, n, 4f, 901) * .6f + Noise(x, y, n, n, 13f, 902) * .4f;
                float dry = Mathf.Clamp01((Noise(x, y, n, n, 3f, 903) - .45f) * 2.4f);
                Color green = Color.Lerp(new Color(.17f, .21f, .1f), new Color(.27f, .31f, .15f), patch);
                Color straw = Color.Lerp(new Color(.33f, .3f, .18f), new Color(.42f, .37f, .22f), patch);
                Color soil = new(.17f, .14f, .1f);
                Color c = Color.Lerp(green, straw, dry);
                float b = blades[i];
                c = Color.Lerp(soil, c, .55f + b * .45f) * (.85f + b * .25f);
                m.Colour[i] = c;
                m.Height[i] = b * .6f + patch * .3f;
                m.Smooth[i] = .08f + b * .06f;
                m.Occlusion[i] = .7f + b * .3f;
            });
            return m;
        }

        // Packed earth with grit and small stones: paths, worn ground, the shelves on the slopes.
        private static Map Earth()
        {
            const int n = 1024;
            var m = new Map(n, n) { NormalStrength = 2.6f };
            Fill(m, (x, y, i) =>
            {
                float a = Noise(x, y, n, n, 5f, 911), b = Noise(x, y, n, n, 21f, 912), grit = Noise(x, y, n, n, 90f, 913);
                bool stone = Noise(x, y, n, n, 48f, 914) > .74f;
                Color c = Color.Lerp(new Color(.2f, .17f, .13f), new Color(.31f, .27f, .21f), a * .7f + b * .3f);
                c *= .9f + grit * .2f;
                if (stone) c = Color.Lerp(c, new Color(.42f, .4f, .37f), .7f);
                m.Colour[i] = c;
                m.Height[i] = a * .3f + grit * .25f + (stone ? .5f : 0f);
                m.Smooth[i] = stone ? .25f : .1f;
                m.Occlusion[i] = stone ? 1f : .85f;
            });
            return m;
        }

        // Grey-brown cliff rock: angular blocks split by fractures (cells of a tiling Voronoi pattern), each block its
        // own shade with bedding lines, fine grit, lichen and salt crust. Never contour swirls.
        private static Map SeaRock()
        {
            const int n = 1024, cells = 22;
            var m = new Map(n, n) { NormalStrength = 4f };
            var rng = new System.Random(921);
            var px = new float[cells]; var py = new float[cells]; var shade = new float[cells];
            for (int i = 0; i < cells; i++) { px[i] = (float)rng.NextDouble() * n; py[i] = (float)rng.NextDouble() * n; shade[i] = (float)rng.NextDouble(); }
            Fill(m, (x, y, i) =>
            {
                // Nearest and second-nearest site, wrapping so the pattern tiles.
                float d1 = float.MaxValue, d2 = float.MaxValue; int near = 0;
                for (int k = 0; k < cells; k++)
                {
                    float dx = Mathf.Abs(x - px[k]), dy = Mathf.Abs(y - py[k]);
                    dx = Mathf.Min(dx, n - dx); dy = Mathf.Min(dy, n - dy);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d < d1) { d2 = d1; d1 = d; near = k; } else if (d < d2) d2 = d;
                }
                float edge = d2 - d1;
                float crack = 1f - Mathf.Clamp01(edge / 7f);
                float grit = Noise(x, y, n, n, 60f, 925), mid = Noise(x, y, n, n, 11f, 922);
                float bedding = Mathf.Abs(Mathf.Sin((y / (float)n * 26f + shade[near] * 4f + mid) * Mathf.PI)) * .12f;
                float lichen = Mathf.Clamp01((Noise(x, y, n, n, 6f, 924) - .66f) * 6f);
                Color c = Color.Lerp(new Color(.27f, .26f, .24f), new Color(.43f, .41f, .38f), shade[near] * .7f + mid * .3f);
                c *= .9f + grit * .18f - bedding;
                c = Color.Lerp(c, new Color(.36f, .38f, .26f), lichen * .5f);
                c = Color.Lerp(c, c * .35f, crack * crack);
                m.Colour[i] = c;
                m.Height[i] = Mathf.Clamp01(edge / 20f) * .7f + grit * .2f + mid * .1f;
                m.Smooth[i] = .12f + grit * .08f;
                m.Occlusion[i] = 1f - crack * .55f;
            });
            return m;
        }

        // Rounded beach pebbles in greys and browns, wet-dark in the gaps.
        private static Map Shingle()
        {
            const int n = 512;
            var m = new Map(n, n) { NormalStrength = 3f };
            var rng = new System.Random(931);
            var height = new float[n * n];
            var tint = new float[n * n];
            for (int k = 0; k < 2600; k++)
            {
                int cx = rng.Next(n), cy = rng.Next(n), r = 4 + rng.Next(9);
                float t = (float)rng.NextDouble();
                for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    float d = Mathf.Sqrt(dx * dx + dy * dy * 1.4f) / r;
                    if (d > 1f) continue;
                    int px = (cx + dx + n) % n, py = (cy + dy + n) % n, idx = py * n + px;
                    float h = Mathf.Sqrt(1f - d * d);
                    if (h > height[idx]) { height[idx] = h; tint[idx] = t; }
                }
            }
            Fill(m, (x, y, i) =>
            {
                Color stone = Color.Lerp(new Color(.32f, .31f, .29f), new Color(.5f, .46f, .4f), tint[i]);
                m.Colour[i] = Color.Lerp(new Color(.08f, .08f, .08f), stone, Mathf.Clamp01(height[i] * 3f));
                m.Height[i] = height[i];
                m.Smooth[i] = .2f + height[i] * .3f;
                m.Occlusion[i] = .5f + height[i] * .5f;
            });
            return m;
        }
    }
}

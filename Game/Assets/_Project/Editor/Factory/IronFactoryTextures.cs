using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The Iron Factory's own surface maps, generated in code (no art pack): industrial brick, fire brick, riveted
    /// steel plate, catwalk grating, mesh guard panels, worn safety paint, cinder ground, an oil-stain decal and the
    /// soot veil that darkens the walls toward the roof. Each set is a tileable BaseColor (+alpha where it is cut
    /// out), a Normal built from a height field, and a Mask (R metallic, G occlusion, A smoothness) like the mall's.
    /// Made once; delete a set's PNGs to make it again.
    /// </summary>
    public static class IronFactoryTextures
    {
        public const string Folder = "Assets/_Project/Art/Factory/Textures";

        private sealed class Map
        {
            public readonly int W, H;
            public readonly Color[] Colour;
            public readonly float[] Height, Metal, Smooth, Occlusion;
            public float NormalStrength = 2f;
            public Map(int w, int h)
            {
                W = w; H = h;
                Colour = new Color[w * h];
                Height = new float[w * h]; Metal = new float[w * h]; Smooth = new float[w * h]; Occlusion = new float[w * h];
                for (int i = 0; i < Occlusion.Length; i++) Occlusion[i] = 1f;
            }
        }

        public static void EnsureAll()
        {
            Make("SootBrick", () => Brick(1024, 1024, 8, 16, new Color(.29f, .16f, .12f), new Color(.2f, .19f, .18f), 31));
            Make("FireBrick", () => Brick(1024, 1024, 8, 17, new Color(.62f, .48f, .3f), new Color(.36f, .2f, .12f), 47));
            Make("RivetedPlate", RivetedPlate);
            Make("Grating", Grating);
            Make("MeshGuard", MeshGuard);
            Make("WornPaint", WornPaint);
            Make("Cinder", Cinder);
            Make("OilStain", OilStain);
            Make("SootVeil", SootVeil);
            Make("LightShaft", LightShaft);
        }

        // Moonlight falling through dusty air: brightest under the hole, fading toward the floor, soft at the sides.
        private static Map LightShaft()
        {
            const int w = 256, h = 512;
            var m = new Map(w, h) { NormalStrength = 0f };
            Fill(m, (x, y, i) =>
            {
                float u = x / (float)(w - 1) - .5f, v = y / (float)(h - 1);
                float side = Mathf.Clamp01(1f - Mathf.Abs(u) * 2f);
                float drift = .75f + Noise(x, y, w, h, 6f, 141) * .5f;
                float a = side * side * Mathf.Lerp(.05f, .32f, v) * drift;
                m.Colour[i] = new Color(.62f, .72f, .9f, a);
            });
            return m;
        }

        public static Texture2D Get(string set, string channel) =>
            AssetDatabase.LoadAssetAtPath<Texture2D>($"{Folder}/{set}_{channel}.png");

        // ---- Recipes ------------------------------------------------------------------------------

        // Industrial blocks in running bond: each a slightly different burnt tint, sunk mortar, grime and chips.
        private static Map Brick(int w, int h, int across, int rows, Color brick, Color mortar, int seed)
        {
            var m = new Map(w, h) { NormalStrength = 3f };
            var rng = new System.Random(seed);
            float[] tint = new float[across * rows * 2];
            for (int i = 0; i < tint.Length; i++) tint[i] = (float)rng.NextDouble();
            float bw = w / (float)across, bh = h / (float)rows, joint = Mathf.Max(3f, bh * .09f);
            Fill(m, (x, y, i) =>
            {
                int row = (int)(y / bh);
                float shift = row % 2 == 0 ? 0f : bw * .5f;
                float bx = (x + shift) % w;
                int col = (int)(bx / bw);
                float lx = bx - col * bw, ly = y - row * bh;
                float edge = Mathf.Min(Mathf.Min(lx, bw - lx), Mathf.Min(ly, bh - ly));
                bool isMortar = edge < joint;
                float t = tint[(row * across + col) % tint.Length];
                float grime = Noise(x, y, w, h, 6f, seed) * .6f + Noise(x, y, w, h, 23f, seed + 5) * .4f;
                Color c = isMortar ? mortar : Color.Lerp(brick, brick * .62f, t * .7f);
                c = Color.Lerp(c, c * .55f, Mathf.Clamp01(grime - .35f) * 1.3f);
                bool chip = !isMortar && Noise(x, y, w, h, 40f, seed + 9) > .78f;
                if (chip) c *= .8f;
                m.Colour[i] = c;
                m.Height[i] = isMortar ? 0f : Mathf.Clamp01(edge / (joint * 2.5f)) * (chip ? .6f : 1f) - grime * .08f;
                m.Smooth[i] = isMortar ? .05f : .18f + (1f - grime) * .1f;
                m.Occlusion[i] = isMortar ? .55f : 1f;
            });
            return m;
        }

        // 1 m x 0.5 m plates (the set tiles every 2 m), rivets along every seam, rust where the paint went.
        private static Map RivetedPlate()
        {
            const int n = 1024;
            var m = new Map(n, n) { NormalStrength = 2.5f };
            float pw = n / 2f, ph = n / 4f, pitch = n / 16f;
            Fill(m, (x, y, i) =>
            {
                float lx = x % pw, ly = y % ph;
                float seam = Mathf.Min(Mathf.Min(lx, pw - lx), Mathf.Min(ly, ph - ly));
                float rust = Noise(x, y, n, n, 5f, 71) * .55f + Noise(x, y, n, n, 19f, 72) * .45f;
                bool rusty = rust > .6f;
                // Rivets: a row just inside each seam.
                float rx = Mathf.Abs(((x + pitch * .5f) % pitch) - pitch * .5f), ry = Mathf.Abs(((y + pitch * .5f) % pitch) - pitch * .5f);
                bool rowNearSeam = Mathf.Abs(ly - 14f) < 6f || Mathf.Abs(ph - ly - 14f) < 6f || Mathf.Abs(lx - 14f) < 6f || Mathf.Abs(pw - lx - 14f) < 6f;
                float rivet = rowNearSeam ? Mathf.Clamp01(1f - Mathf.Sqrt(rx * rx + ry * ry) / 7f) : 0f;
                Color steel = Color.Lerp(new Color(.24f, .25f, .25f), new Color(.17f, .18f, .19f), Noise(x, y, n, n, 3f, 73));
                Color c = rusty ? Color.Lerp(steel, new Color(.34f, .17f, .08f), Mathf.Clamp01((rust - .6f) * 4f)) : steel;
                if (seam < 3f) c *= .5f;
                m.Colour[i] = c;
                m.Height[i] = (seam < 3f ? 0f : .5f) + rivet * .5f - (rusty ? rust * .1f : 0f);
                m.Metal[i] = rusty ? .2f : .75f;
                m.Smooth[i] = rusty ? .12f : .42f - Noise(x, y, n, n, 30f, 74) * .15f;
                m.Occlusion[i] = seam < 3f ? .5f : 1f;
            });
            return m;
        }

        // Expanded-metal grating: diamond holes (cut out) in worn steel strands. The set tiles every 0.5 m.
        private static Map Grating()
        {
            const int n = 512;
            var m = new Map(n, n) { NormalStrength = 4f };
            const float cw = 64f, ch = 32f;
            Fill(m, (x, y, i) =>
            {
                int row = (int)(y / ch);
                float lx = (x + (row % 2) * cw * .5f) % cw - cw * .5f, ly = y % ch - ch * .5f;
                float d = Mathf.Abs(lx) / (cw * .5f) + Mathf.Abs(ly) / (ch * .5f);
                bool strand = d > .68f;
                float wear = Noise(x, y, n, n, 8f, 81);
                Color c = Color.Lerp(new Color(.3f, .3f, .29f), new Color(.31f, .19f, .11f), wear);
                c.a = strand ? 1f : 0f;
                m.Colour[i] = c;
                m.Height[i] = strand ? Mathf.Clamp01((d - .68f) * 4f) : 0f;
                m.Metal[i] = .6f;
                m.Smooth[i] = .5f - wear * .3f;
            });
            return m;
        }

        // Welded square mesh on the guard panels: thin wires, mostly see-through. Tiles every 0.5 m.
        private static Map MeshGuard()
        {
            const int n = 512;
            var m = new Map(n, n) { NormalStrength = 3f };
            const int cell = 32, wire = 4;
            Fill(m, (x, y, i) =>
            {
                bool on = x % cell < wire || y % cell < wire;
                float wear = Noise(x, y, n, n, 6f, 91);
                Color c = Color.Lerp(new Color(.36f, .36f, .34f), new Color(.33f, .2f, .12f), wear);
                c.a = on ? 1f : 0f;
                m.Colour[i] = c;
                m.Height[i] = on ? 1f : 0f;
                m.Metal[i] = .6f;
                m.Smooth[i] = .4f;
            });
            return m;
        }

        // Safety yellow, chipped to dark steel at the edges of wear. Tiles every 1 m.
        private static Map WornPaint()
        {
            const int n = 512;
            var m = new Map(n, n) { NormalStrength = 1.5f };
            Fill(m, (x, y, i) =>
            {
                float wear = Noise(x, y, n, n, 7f, 101) * .7f + Noise(x, y, n, n, 31f, 102) * .3f;
                bool bare = wear > .62f;
                m.Colour[i] = bare ? new Color(.18f, .16f, .14f) : Color.Lerp(new Color(.72f, .55f, .1f), new Color(.5f, .38f, .1f), wear);
                m.Height[i] = bare ? 0f : .4f;
                m.Metal[i] = bare ? .6f : 0f;
                m.Smooth[i] = bare ? .35f : .25f;
            });
            return m;
        }

        // Black cinder and slag grit for the yard. Tiles every 4 m.
        private static Map Cinder()
        {
            const int n = 512;
            var m = new Map(n, n) { NormalStrength = 2f };
            Fill(m, (x, y, i) =>
            {
                float grit = Noise(x, y, n, n, 48f, 111), lumps = Noise(x, y, n, n, 9f, 112);
                m.Colour[i] = Color.Lerp(new Color(.07f, .065f, .06f), new Color(.2f, .18f, .16f), grit * .6f + lumps * .4f);
                m.Height[i] = grit * .6f + lumps * .4f;
                m.Smooth[i] = .1f + grit * .1f;
            });
            return m;
        }

        // A spreading oil stain for the floor: dark, glossy, ragged at the edge. A decal (alpha cut).
        private static Map OilStain()
        {
            const int n = 512;
            var m = new Map(n, n) { NormalStrength = 0f };
            Fill(m, (x, y, i) =>
            {
                float u = x / (float)n - .5f, v = y / (float)n - .5f;
                float r = Mathf.Sqrt(u * u + v * v) * 2f;
                float edge = .78f + (Noise(x, y, n, n, 5f, 121) - .5f) * .45f;
                float a = Mathf.Clamp01((edge - r) * 6f) * (.6f + Noise(x, y, n, n, 18f, 122) * .4f);
                m.Colour[i] = new Color(.03f, .028f, .025f, a);
                m.Smooth[i] = .75f;
            });
            return m;
        }

        // Soot on the walls: clear at the bottom, nearly black under the roof, with streaks run down by rain leaks.
        private static Map SootVeil()
        {
            const int w = 512, h = 1024;
            var m = new Map(w, h) { NormalStrength = 0f };
            Fill(m, (x, y, i) =>
            {
                float v = y / (float)(h - 1);
                float streak = Noise(x, 0, w, 1, 40f, 131);
                float a = Mathf.Clamp01(Mathf.SmoothStep(.05f, 1f, v) * .9f + (streak - .5f) * .35f * v);
                m.Colour[i] = new Color(.025f, .022f, .02f, a);
                m.Smooth[i] = .05f;
            });
            return m;
        }

        // ---- Plumbing -------------------------------------------------------------------------------

        private static void Fill(Map m, Action<int, int, int> pixel)
        {
            for (int y = 0; y < m.H; y++)
            for (int x = 0; x < m.W; x++)
                pixel(x, y, y * m.W + x);
        }

        // Perlin noise that repeats exactly across the texture, so every set tiles without seams.
        private static float Noise(int x, int y, int w, int h, float scale, int seed)
        {
            float u = x / (float)w, v = y / (float)h;
            float N(float a, float b) => Mathf.PerlinNoise(a * scale + seed * 1.37f, b * scale + seed * .71f);
            return N(u, v) * (1 - u) * (1 - v) + N(u - 1, v) * u * (1 - v) + N(u, v - 1) * (1 - u) * v + N(u - 1, v - 1) * u * v;
        }

        private static void Make(string set, Func<Map> recipe)
        {
            if (File.Exists($"{Folder}/{set}_BaseColor.png")) return;
            Directory.CreateDirectory(Folder);
            Map m = recipe();
            Write($"{set}_BaseColor", m.W, m.H, m.Colour);
            var normal = new Color[m.Colour.Length];
            var mask = new Color[m.Colour.Length];
            for (int y = 0; y < m.H; y++)
            for (int x = 0; x < m.W; x++)
            {
                int i = y * m.W + x;
                float dx = m.Height[y * m.W + (x + 1) % m.W] - m.Height[y * m.W + (x - 1 + m.W) % m.W];
                float dy = m.Height[((y + 1) % m.H) * m.W + x] - m.Height[((y - 1 + m.H) % m.H) * m.W + x];
                Vector3 n = new Vector3(-dx * m.NormalStrength, -dy * m.NormalStrength, 1f).normalized;
                normal[i] = new Color(n.x * .5f + .5f, n.y * .5f + .5f, n.z * .5f + .5f, 1f);
                mask[i] = new Color(m.Metal[i], m.Occlusion[i], 0f, m.Smooth[i]);
            }
            Write($"{set}_Normal", m.W, m.H, normal);
            Write($"{set}_Mask", m.W, m.H, mask);
            Import($"{set}_BaseColor", TextureImporterType.Default, true);
            Import($"{set}_Normal", TextureImporterType.NormalMap, false);
            Import($"{set}_Mask", TextureImporterType.Default, false);
        }

        private static void Write(string name, int w, int h, Color[] pixels)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes($"{Folder}/{name}.png", tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void Import(string name, TextureImporterType type, bool srgb)
        {
            string path = $"{Folder}/{name}.png";
            AssetDatabase.ImportAsset(path);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            importer.textureType = type;
            importer.sRGBTexture = srgb;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.alphaIsTransparency = srgb;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }
    }
}

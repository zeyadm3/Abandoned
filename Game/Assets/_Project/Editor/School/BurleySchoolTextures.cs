using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Generates complete winter PBR surface texture sets (BaseColor, Normal, Mask) for
    /// Pine Hollow Middle School: crystalline winter snow, wind-blown snowdrifts,
    /// frozen ice sheets, frost-dusted colonial brick, polished hardwood gymnasium court,
    /// institutional linoleum tiles, frosted multi-pane window glass, snowy asphalt, and winter pine foliage.
    /// </summary>
    public static class BurleySchoolTextures
    {
        public const string Folder = "Assets/_Project/Art/School/Textures";

        private sealed class Map
        {
            public readonly int W, H;
            public readonly Color[] Colour;
            public readonly float[] Height, Metal, Smooth, Occlusion;
            public float NormalStrength = 2.0f;

            public Map(int w, int h)
            {
                W = w; H = h;
                Colour = new Color[w * h];
                Height = new float[w * h];
                Metal = new float[w * h];
                Smooth = new float[w * h];
                Occlusion = new float[w * h];
                for (int i = 0; i < Occlusion.Length; i++) Occlusion[i] = 1f;
            }
        }

        public static void EnsureAll()
        {
            Make("WinterSnow", WinterSnow);
            Make("SnowDrift", SnowDrift);
            Make("FrostIce", FrostIce);
            Make("WinterBrick", WinterBrick);
            Make("WinterBrickDark", WinterBrickDark);
            Make("SchoolTile", SchoolTile);
            Make("GymMaple", GymMaple);
            Make("SchoolPlaster", SchoolPlaster);
            Make("FrostedGlass", FrostedGlass);
            Make("Chalkboard", Chalkboard);
            Make("SnowyAsphalt", SnowyAsphalt);
            Make("PineSnow", PineSnow);
            Make("LockerMetal", LockerMetal);
            Make("AcousticCeiling", AcousticCeiling);
        }

        public static Texture2D Get(string set, string channel)
        {
            EnsureAll();
            string path = $"{Folder}/{set}_{channel}.png";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                AssetDatabase.ImportAsset(path);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return texture;
        }

        // 1. Pristine Winter Snow with micro-crystalline glitter & soft powdery height
        private static Map WinterSnow()
        {
            const int w = 512, h = 512;
            var m = new Map(w, h) { NormalStrength = 1.4f };
            Fill(m, (x, y, i) =>
            {
                float n1 = Noise(x, y, w, h, 6.0f, 101);
                float n2 = Noise(x, y, w, h, 24.0f, 202);
                float n3 = Noise(x, y, w, h, 64.0f, 303);
                float hVal = n1 * 0.65f + n2 * 0.25f + n3 * 0.10f;
                m.Height[i] = hVal;

                // Crisp winter white with subtle cool blue shadow tone
                float c = Mathf.Lerp(0.92f, 0.99f, hVal);
                m.Colour[i] = new Color(c * 0.96f, c * 0.98f, c * 1.0f, 1f);
                m.Metal[i] = 0.0f;
                m.Occlusion[i] = Mathf.Lerp(0.85f, 1.0f, hVal);
                // Sparkling crystalline specular sheen
                m.Smooth[i] = Mathf.Lerp(0.18f, 0.38f, n3);
            });
            return m;
        }

        // 2. Sculpted Wind-Swept Snow Drift with directional wave ripples
        private static Map SnowDrift()
        {
            const int w = 512, h = 512;
            var m = new Map(w, h) { NormalStrength = 2.4f };
            Fill(m, (x, y, i) =>
            {
                float u = x / (float)w, v = y / (float)h;
                float wave = Mathf.Sin((u + v * 0.35f) * Mathf.PI * 8f) * 0.5f + 0.5f;
                float n = Noise(x, y, w, h, 4.0f, 404);
                float hVal = Mathf.Lerp(wave, n, 0.4f);
                m.Height[i] = hVal;

                float c = Mathf.Lerp(0.88f, 0.98f, hVal);
                m.Colour[i] = new Color(c * 0.94f, c * 0.97f, c * 1.0f, 1f);
                m.Metal[i] = 0.0f;
                m.Occlusion[i] = Mathf.Lerp(0.80f, 1.0f, hVal);
                m.Smooth[i] = 0.25f;
            });
            return m;
        }

        // 3. Slick Reflective Ice with internal fracture stress lines
        private static Map FrostIce()
        {
            const int w = 256, h = 256;
            var m = new Map(w, h) { NormalStrength = 0.8f };
            Fill(m, (x, y, i) =>
            {
                float n = Noise(x, y, w, h, 12.0f, 505);
                float crack = Mathf.Abs(Noise(x, y, w, h, 28.0f, 606) - 0.5f) < 0.04f ? 0.35f : 0f;
                m.Height[i] = n * 0.1f + crack;
                m.Colour[i] = new Color(0.78f + crack * 0.2f, 0.88f + crack * 0.1f, 0.96f, 0.88f);
                m.Metal[i] = 0.0f;
                m.Occlusion[i] = 1.0f;
                m.Smooth[i] = 0.92f - crack * 0.4f; // Extremely slick and reflective
            });
            return m;
        }

        // 4. 1951 Colonial Red Brick with Frost & Snow in Mortar Joints
        private static Map WinterBrick()
        {
            const int w = 512, h = 512;
            const int rows = 16, cols = 8;
            int rh = h / rows, cw = w / cols;
            var m = new Map(w, h) { NormalStrength = 2.5f };
            Fill(m, (x, y, i) =>
            {
                int r = y / rh;
                int ox = (r % 2 == 1) ? cw / 2 : 0;
                int c = ((x + ox) % w) / cw;
                int localY = y % rh;
                int localX = (x + ox) % cw;

                bool mortar = localY < 4 || localY > rh - 4 || localX < 4 || localX > cw - 4;
                float n = Noise(x, y, w, h, 20.0f, 707);

                if (mortar)
                {
                    // Mortar joint filled with white snow/frost
                    m.Height[i] = 0.15f;
                    m.Colour[i] = new Color(0.88f + n * 0.08f, 0.90f + n * 0.06f, 0.92f, 1f);
                    m.Metal[i] = 0f;
                    m.Occlusion[i] = 0.75f;
                    m.Smooth[i] = 0.15f;
                }
                else
                {
                    // Weathered red brick body with subtle frost dusting
                    m.Height[i] = 0.8f + n * 0.2f;
                    float frost = n > 0.65f ? (n - 0.65f) * 0.6f : 0f;
                    Color brickColor = new Color(0.68f, 0.30f, 0.20f) * Mathf.Lerp(0.85f, 1.15f, n);
                    m.Colour[i] = Color.Lerp(brickColor, new Color(0.92f, 0.94f, 0.98f), frost);
                    m.Metal[i] = 0.02f;
                    m.Occlusion[i] = 0.95f;
                    m.Smooth[i] = 0.18f;
                }
            });
            return m;
        }

        // 5. Dark Foundation Brick with heavier frost and moisture
        private static Map WinterBrickDark()
        {
            const int w = 512, h = 512;
            const int rows = 16, cols = 8;
            int rh = h / rows, cw = w / cols;
            var m = new Map(w, h) { NormalStrength = 2.2f };
            Fill(m, (x, y, i) =>
            {
                int r = y / rh;
                int ox = (r % 2 == 1) ? cw / 2 : 0;
                int localY = y % rh;
                int localX = (x + ox) % cw;

                bool mortar = localY < 4 || localY > rh - 4 || localX < 4 || localX > cw - 4;
                float n = Noise(x, y, w, h, 16.0f, 808);

                if (mortar)
                {
                    m.Height[i] = 0.2f;
                    m.Colour[i] = new Color(0.75f, 0.78f, 0.82f, 1f);
                    m.Metal[i] = 0f;
                    m.Occlusion[i] = 0.70f;
                    m.Smooth[i] = 0.20f;
                }
                else
                {
                    m.Height[i] = 0.75f + n * 0.25f;
                    Color brickColor = new Color(0.42f, 0.22f, 0.16f) * Mathf.Lerp(0.8f, 1.1f, n);
                    m.Colour[i] = brickColor;
                    m.Metal[i] = 0.02f;
                    m.Occlusion[i] = 0.90f;
                    m.Smooth[i] = 0.25f;
                }
            });
            return m;
        }

        // 6. Institutional Linoleum 12x12 Checked Floor Tiles with Wax Sheen
        private static Map SchoolTile()
        {
            const int w = 512, h = 512;
            const int tileSize = 64;
            var m = new Map(w, h) { NormalStrength = 1.2f };
            Fill(m, (x, y, i) =>
            {
                int tx = x / tileSize, ty = y / tileSize;
                int lx = x % tileSize, ly = y % tileSize;
                bool grout = lx < 2 || ly < 2;
                bool alt = (tx + ty) % 2 == 0;
                float fleck = Noise(x, y, w, h, 64.0f, 909);
                float patch = Noise(x, y, w, h, 8.0f, 910);

                if (grout)
                {
                    m.Height[i] = 0.2f;
                    m.Colour[i] = new Color(0.45f, 0.44f, 0.42f, 1f);
                    m.Metal[i] = 0f;
                    m.Occlusion[i] = 0.8f;
                    m.Smooth[i] = 0.2f;
                }
                else
                {
                    m.Height[i] = 0.8f;
                    Color baseCol = alt ? new Color(0.78f, 0.76f, 0.71f) : new Color(0.68f, 0.67f, 0.63f);
                    baseCol += new Color(fleck * 0.05f - 0.025f, fleck * 0.05f - 0.025f, fleck * 0.05f - 0.025f);
                    m.Colour[i] = baseCol;
                    m.Metal[i] = 0f;
                    m.Occlusion[i] = 1.0f;
                    m.Smooth[i] = Mathf.Lerp(0.45f, 0.62f, patch); // Institutional wax coat
                }
            });
            return m;
        }

        // 7. Polished Hardwood Maple Gymnasium Planks
        private static Map GymMaple()
        {
            const int w = 512, h = 512;
            const int plankH = 16;
            var m = new Map(w, h) { NormalStrength = 1.6f };
            Fill(m, (x, y, i) =>
            {
                int p = y / plankH;
                int py = y % plankH;
                bool groove = py == 0;
                float grain = Noise(x, y * 4, w, h * 4, 12.0f, 1001 + p);
                float plankTint = (Mathf.Sin(p * 37.1f) * 0.5f + 0.5f) * 0.12f;

                if (groove)
                {
                    m.Height[i] = 0.3f;
                    m.Colour[i] = new Color(0.42f, 0.28f, 0.16f, 1f);
                    m.Smooth[i] = 0.3f;
                }
                else
                {
                    m.Height[i] = 0.85f + grain * 0.15f;
                    Color maple = new Color(0.88f + plankTint, 0.72f + plankTint * 0.8f, 0.48f + plankTint * 0.5f);
                    m.Colour[i] = maple * Mathf.Lerp(0.92f, 1.05f, grain);
                    m.Smooth[i] = 0.82f; // High gloss polyurethane finish
                }
                m.Metal[i] = 0f;
                m.Occlusion[i] = groove ? 0.75f : 1.0f;
            });
            return m;
        }

        // 8. Institutional Two-Tone Plaster Wall with Kelly Green Dado Wainscot
        private static Map SchoolPlaster()
        {
            const int w = 512, h = 512;
            var m = new Map(w, h) { NormalStrength = 1.1f };
            Fill(m, (x, y, i) =>
            {
                float n = Noise(x, y, w, h, 32.0f, 1101);
                m.Height[i] = n;
                // Upper: cream ivory plaster; lower: warm institution beige
                m.Colour[i] = new Color(0.85f, 0.84f, 0.80f) + new Color(n * 0.04f, n * 0.04f, n * 0.03f);
                m.Metal[i] = 0f;
                m.Occlusion[i] = 1.0f;
                m.Smooth[i] = 0.18f;
            });
            return m;
        }

        // 9. Frosted Window Glass with Ice Crystal Ferns
        private static Map FrostedGlass()
        {
            const int w = 256, h = 256;
            var m = new Map(w, h) { NormalStrength = 1.8f };
            Fill(m, (x, y, i) =>
            {
                float n1 = Noise(x, y, w, h, 8.0f, 1201);
                float n2 = Noise(x, y, w, h, 32.0f, 1202);
                float frost = Mathf.Clamp01(n1 * 0.6f + n2 * 0.6f);
                m.Height[i] = frost;
                m.Colour[i] = new Color(0.88f, 0.94f, 0.98f, Mathf.Lerp(0.35f, 0.85f, frost));
                m.Metal[i] = 0f;
                m.Occlusion[i] = 1.0f;
                m.Smooth[i] = Mathf.Lerp(0.88f, 0.22f, frost);
            });
            return m;
        }

        // 10. Dark Green Slate Chalkboard with Smudged White Chalk
        private static Map Chalkboard()
        {
            const int w = 256, h = 256;
            var m = new Map(w, h) { NormalStrength = 0.9f };
            Fill(m, (x, y, i) =>
            {
                float n = Noise(x, y, w, h, 14.0f, 1301);
                float smudge = Mathf.Clamp01((n - 0.45f) * 1.8f);
                m.Height[i] = n * 0.2f;
                Color slate = new Color(0.16f, 0.25f, 0.18f);
                m.Colour[i] = Color.Lerp(slate, new Color(0.75f, 0.78f, 0.75f), smudge * 0.35f);
                m.Metal[i] = 0f;
                m.Occlusion[i] = 1.0f;
                m.Smooth[i] = 0.12f;
            });
            return m;
        }

        // 11. Coarse Road Asphalt Dusted with Crisp White Snow in Crevices
        private static Map SnowyAsphalt()
        {
            const int w = 512, h = 512;
            var m = new Map(w, h) { NormalStrength = 2.4f };
            Fill(m, (x, y, i) =>
            {
                float n = Noise(x, y, w, h, 40.0f, 1401);
                float snowDust = n > 0.62f ? (n - 0.62f) * 2.5f : 0f;
                m.Height[i] = n;
                Color asphalt = new Color(0.20f, 0.21f, 0.22f) * Mathf.Lerp(0.85f, 1.15f, n);
                m.Colour[i] = Color.Lerp(asphalt, new Color(0.92f, 0.94f, 0.98f), snowDust);
                m.Metal[i] = 0.05f;
                m.Occlusion[i] = 0.85f;
                m.Smooth[i] = Mathf.Lerp(0.18f, 0.35f, snowDust);
            });
            return m;
        }

        // 12. Dense Winter Pine Foliage with Clumped Snow Caps
        private static Map PineSnow()
        {
            const int w = 256, h = 256;
            var m = new Map(w, h) { NormalStrength = 2.0f };
            Fill(m, (x, y, i) =>
            {
                float n = Noise(x, y, w, h, 16.0f, 1501);
                bool isSnow = n > 0.55f;
                m.Height[i] = n;
                if (isSnow)
                {
                    m.Colour[i] = new Color(0.94f, 0.96f, 0.98f, 1f);
                    m.Smooth[i] = 0.30f;
                }
                else
                {
                    m.Colour[i] = new Color(0.11f, 0.26f, 0.13f, 1f);
                    m.Smooth[i] = 0.18f;
                }
                m.Metal[i] = 0f;
                m.Occlusion[i] = 0.9f;
            });
            return m;
        }

        // 13. Pressed Metal Locker Steel with Louvers
        private static Map LockerMetal()
        {
            const int w = 256, h = 256;
            var m = new Map(w, h) { NormalStrength = 1.2f };
            Fill(m, (x, y, i) =>
            {
                float n = Noise(x, y, w, h, 20.0f, 1601);
                m.Height[i] = n * 0.1f;
                m.Colour[i] = new Color(0.11f, 0.35f, 0.21f) * Mathf.Lerp(0.95f, 1.05f, n);
                m.Metal[i] = 0.45f;
                m.Occlusion[i] = 1.0f;
                m.Smooth[i] = 0.65f;
            });
            return m;
        }

        // 14. 1950s Pitted Acoustic Ceiling Tile Grid
        private static Map AcousticCeiling()
        {
            const int w = 256, h = 256;
            var m = new Map(w, h) { NormalStrength = 1.4f };
            Fill(m, (x, y, i) =>
            {
                int lx = x % 64, ly = y % 64;
                bool border = lx < 2 || ly < 2;
                float pits = Noise(x, y, w, h, 64.0f, 1701);
                float isPit = pits < 0.28f ? 0.3f : 0f;

                if (border)
                {
                    m.Height[i] = 0.3f;
                    m.Colour[i] = new Color(0.72f, 0.71f, 0.68f, 1f);
                    m.Smooth[i] = 0.2f;
                }
                else
                {
                    m.Height[i] = 0.8f - isPit;
                    float c = 0.88f - isPit * 0.25f;
                    m.Colour[i] = new Color(c, c * 0.99f, c * 0.96f, 1f);
                    m.Smooth[i] = 0.12f;
                }
                m.Metal[i] = 0f;
                m.Occlusion[i] = border ? 0.8f : 1.0f;
            });
            return m;
        }

        // ---- Plumbing Helpers ----
        private static void Fill(Map m, Action<int, int, int> pixel)
        {
            for (int y = 0; y < m.H; y++)
            for (int x = 0; x < m.W; x++)
                pixel(x, y, y * m.W + x);
        }

        private static float Noise(int x, int y, int w, int h, float scale, int seed)
        {
            float u = x / (float)w, v = y / (float)h;
            float N(float a, float b) => Mathf.PerlinNoise(a * scale + seed * 1.37f, b * scale + seed * 0.71f);
            return N(u, v) * (1 - u) * (1 - v) + N(u - 1, v) * u * (1 - v) + N(u, v - 1) * (1 - u) * v + N(u - 1, v - 1) * u * v;
        }

        private static void Make(string set, Func<Map> recipe)
        {
            if (File.Exists($"{Folder}/{set}_BaseColor.png") && File.Exists($"{Folder}/{set}_Normal.png") && File.Exists($"{Folder}/{set}_Mask.png"))
                return;

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
                normal[i] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
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

using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The 0.24.12 decal set, drawn in code (no image pack): a 2 x 2 atlas of floor cracks (Structure.StructureCrackDecals
    /// loads it from Resources), and grime at the foot of walls, water stains, soot, rust runs and floor stains for
    /// DecalScatter. URP's own decal shader; colour and alpha only.
    /// </summary>
    public static class DecalAssets
    {
        public const string Folder = "Assets/_Project/Art/Decals";
        public const string ResourcesFolder = Folder + "/Resources";
        private const string Shader = "Shader Graphs/Decal";

        public static Material Cracks => Ensure("AV_Cracks", ResourcesFolder, 1024, 1024, CrackAtlas);
        public static Material Grime => Ensure("AV_Grime", Folder, 512, 512, WallGrime);
        public static Material WaterStain => Ensure("AV_WaterStain", Folder, 512, 512, Stain);
        public static Material Soot => Ensure("AV_Soot", Folder, 512, 512, SootBlot);
        public static Material RustRun => Ensure("AV_RustRun", Folder, 256, 512, Rust);
        public static Material FloorStain => Ensure("AV_FloorStain", Folder, 512, 512, Puddle);

        [MenuItem(ToolMenu.Art + "Make Decal Set (0.24.12)", priority = ToolMenu.ArtP + 62)]
        public static void MakeAll()
        {
            _ = Cracks; _ = Grime; _ = WaterStain; _ = Soot; _ = RustRun; _ = FloorStain;
            AssetDatabase.SaveAssets();
        }

        private static Material Ensure(string name, string folder, int w, int h, System.Func<float, float, Color> paint)
        {
            string matPath = $"{folder}/{name}.mat", texPath = $"{Folder}/{name}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null) return existing;
            PolishAssets.EnsureFolder(folder);
            PolishAssets.EnsureFolder(Folder);
            if (!System.IO.File.Exists(texPath))
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                var px = new Color[w * h];
                for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = paint((x + 0.5f) / w, (y + 0.5f) / h);
                tex.SetPixels(px);
                System.IO.File.WriteAllBytes(texPath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(texPath);
                if (AssetImporter.GetAtPath(texPath) is TextureImporter ti)
                {
                    ti.alphaIsTransparency = true;
                    ti.wrapMode = TextureWrapMode.Clamp;
                    ti.SaveAndReimport();
                }
            }
            var m = new Material(UnityEngine.Shader.Find(Shader)) { name = name, enableInstancing = true };
            m.SetTexture("Base_Map", AssetDatabase.LoadAssetAtPath<Texture2D>(texPath));
            m.SetFloat("Normal_Blend", 0f);
            AssetDatabase.CreateAsset(m, matPath);
            return m;
        }

        // ---- painters (u, v in 0..1) ----

        private static float Fbm(float x, float y, int octaves = 4)
        {
            float sum = 0f, amp = 0.5f, f = 1f;
            for (int i = 0; i < octaves; i++) { sum += amp * Mathf.PerlinNoise(x * f + 17.3f * i, y * f + 9.1f * i); amp *= 0.5f; f *= 2f; }
            return sum;
        }

        private static Color WallGrime(float u, float v)
        {
            // Dark at the floor, rising unevenly, fading out by mid-height; soft at the sides.
            float edge = Mathf.SmoothStep(0f, 0.15f, u) * Mathf.SmoothStep(0f, 0.15f, 1f - u);
            float reach = 0.25f + 0.35f * Fbm(u * 6f, 2f);
            float a = Mathf.Clamp01(1f - v / reach) * edge * (0.55f + 0.45f * Fbm(u * 18f, v * 18f));
            return new Color(0.11f, 0.095f, 0.075f, a * 0.85f);
        }

        private static Color Stain(float u, float v)
        {
            float dx = u - 0.5f, dy = v - 0.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy) * 2f + (Fbm(u * 5f, v * 5f) - 0.5f) * 0.35f;
            if (r > 1f) return Color.clear;
            float ring = Mathf.Exp(-Mathf.Pow((r - 0.85f) / 0.06f, 2f));   // the tide line
            float body = (1f - r) * 0.35f * Fbm(u * 9f, v * 9f);
            return new Color(0.36f, 0.28f, 0.16f, Mathf.Clamp01(ring * 0.75f + body));
        }

        private static Color SootBlot(float u, float v)
        {
            float dx = u - 0.5f, dy = (v - 0.35f) * 0.8f;
            float r = Mathf.Sqrt(dx * dx + dy * dy) * 2.2f;
            float a = Mathf.Clamp01(1f - r + (Fbm(u * 7f, v * 7f) - 0.5f) * 0.8f) * Mathf.SmoothStep(0f, 0.3f, v + 0.2f);
            return new Color(0.03f, 0.03f, 0.03f, a * 0.9f);
        }

        private static Color Rust(float u, float v)
        {
            // Streaks from the top edge (v = 1) running down.
            float streak = Mathf.Pow(Mathf.PerlinNoise(u * 22f, 3.3f), 3f) * 2.4f;
            float length = 0.4f + 0.6f * Mathf.PerlinNoise(u * 9f, 7.7f);
            float a = Mathf.Clamp01(streak * Mathf.Clamp01((v - (1f - length)) / length)) * Mathf.SmoothStep(0f, 0.2f, u) * Mathf.SmoothStep(0f, 0.2f, 1f - u);
            return new Color(0.34f, 0.15f, 0.06f, a * 0.8f);
        }

        private static Color Puddle(float u, float v)
        {
            float dx = u - 0.5f, dy = v - 0.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy) * 2f + (Fbm(u * 3.5f, v * 3.5f) - 0.5f) * 0.9f;
            float a = Mathf.Clamp01((1f - r) * 2f) * (0.5f + 0.5f * Fbm(u * 14f, v * 14f));
            return new Color(0.09f, 0.08f, 0.06f, a * 0.7f);
        }

        // Four fractures, one per quadrant: a wandering main crack with branches.
        private static Color[] crackCache;
        private static Color CrackAtlas(float u, float v)
        {
            if (crackCache == null) crackCache = DrawCracks(1024);
            int x = Mathf.Clamp((int)(u * 1024), 0, 1023), y = Mathf.Clamp((int)(v * 1024), 0, 1023);
            return crackCache[y * 1024 + x];
        }

        private static Color[] DrawCracks(int size)
        {
            var px = new Color[size * size];
            var rng = new System.Random(4242);
            int half = size / 2;
            for (int q = 0; q < 4; q++)
            {
                int ox = q % 2 * half, oy = q / 2 * half;
                for (int branch = 0; branch < 7; branch++)
                {
                    float x = half * (0.2f + 0.6f * (float)rng.NextDouble()), y = half * (0.2f + 0.6f * (float)rng.NextDouble());
                    float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
                    int steps = branch < 2 ? 420 : 160 + rng.Next(120);
                    float width = branch < 2 ? 2.6f : 1.4f;
                    for (int s = 0; s < steps; s++)
                    {
                        angle += ((float)rng.NextDouble() - 0.5f) * 0.55f;
                        x += Mathf.Cos(angle) * 1.6f;
                        y += Mathf.Sin(angle) * 1.6f;
                        if (x < 4 || y < 4 || x > half - 4 || y > half - 4) break;
                        float w = width * (1f - s / (float)steps * 0.7f);
                        Dab(px, size, ox + x, oy + y, w);
                    }
                }
            }
            return px;
        }

        private static void Dab(Color[] px, int size, float cx, float cy, float radius)
        {
            int r = Mathf.CeilToInt(radius + 2f);
            for (int y = (int)cy - r; y <= (int)cy + r; y++)
            for (int x = (int)cx - r; x <= (int)cx + r; x++)
            {
                if (x < 0 || y < 0 || x >= size || y >= size) continue;
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float a = Mathf.Clamp01(radius + 1f - d);           // the gap itself
                float halo = Mathf.Clamp01((radius + 3f - d) / 3f) * 0.35f; // crumbled edge
                Color c = px[y * size + x];
                float alpha = Mathf.Max(c.a, Mathf.Max(a, halo));
                float dark = a > 0.5f ? 0.02f : 0.12f;
                if (c.a > 0f) dark = Mathf.Min(dark, c.r); // a later stroke's halo never lightens an earlier gap
                px[y * size + x] = new Color(dark, dark * 0.95f, dark * 0.9f, alpha);
            }
        }
    }
}

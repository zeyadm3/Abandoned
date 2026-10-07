using System.IO;
using Abandoned.UI;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the horror menus' assets: original procedural textures (film grain, vignette, scratches, a
    /// water stain, a warm glow), the matte material for the title backdrop's figure, and
    /// Data/UI/Resources/MenuEffectsConfig wired to them. Rerunning keeps the config's tuned values.
    /// </summary>
    public static class MenuEffectsBuilder
    {
        private const string TextureFolder = "Assets/_Project/Art/UI";
        public const string ConfigPath = UiContentBuilder.Folder + "/Resources/MenuEffectsConfig.asset";
        private const string FigureMaterialPath = PolishAssets.Folder + "/Menu_Silhouette.mat";

        [MenuItem("Tools/Abandoned/UI/Create Menu Effects")]
        public static void Create()
        {
            Texture2D grain = Write("MenuGrain", 256, 256, true, (x, y, r) =>
            {
                float v = (float)r.NextDouble();
                return new Color(v, v, v, Mathf.Abs(v - 0.5f) * 1.5f);
            });
            Texture2D vignette = Write("MenuVignette", 512, 512, false, (x, y, r) =>
            {
                float u = x / 511f - 0.5f, w = y / 511f - 0.5f;
                float d = Mathf.Sqrt(u * u * 1.2f + w * w * 1.6f) * 2f;
                return new Color(0f, 0f, 0f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - 0.55f) / 0.75f)) * 0.95f);
            });
            Texture2D scratches = Scratches();
            Texture2D stain = Write("MenuStain", 256, 256, false, (x, y, r) =>
            {
                float u = x / 255f, v = y / 255f;
                float n = Mathf.PerlinNoise(u * 4f + 3f, v * 4f + 9f);
                float ring = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.55f)) * 2.1f);
                float alpha = Mathf.Clamp01((n * ring - 0.18f) * 2.2f);
                // A tide mark: darker at the stain's dried edge.
                float edge = Mathf.Clamp01(1f - Mathf.Abs(alpha - 0.25f) * 6f) * 0.4f;
                return new Color(0.24f, 0.18f, 0.1f, Mathf.Clamp01(alpha * 0.45f + edge));
            });
            Texture2D glow = Write("MenuGlow", 256, 64, false, (x, y, r) =>
            {
                float u = x / 255f, v = y / 63f;
                float h = Mathf.Clamp01(1f - Mathf.Abs(u - 0.32f) / 0.5f), w = Mathf.Clamp01(1f - Mathf.Abs(v - 0.5f) * 2f);
                return new Color(1f, 1f, 1f, Mathf.Pow(h, 1.6f) * Mathf.Pow(w, 1.3f));
            });
            Texture2D scanlines = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/HorrorScanlines.png");

            var figure = AssetDatabase.LoadAssetAtPath<Material>(FigureMaterialPath);
            if (figure == null)
            {
                PolishAssets.EnsureFolder(PolishAssets.Folder);
                figure = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "Menu_Silhouette" };
                AssetDatabase.CreateAsset(figure, FigureMaterialPath);
            }
            figure.SetColor("_BaseColor", new Color(0.004f, 0.004f, 0.005f));
            figure.SetTexture("_BaseMap", grain);
            GeneratedMaterialRepair.Apply(figure);

            PolishAssets.EnsureFolder(UiContentBuilder.Folder + "/Resources");
            var config = SerializedWiring.LoadOrCreateAsset<MenuEffectsConfig>(ConfigPath);
            config.EditorSetup(grain, vignette, scratches, stain, glow, scanlines, figure, LightingAssets.Dust());
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        // Long faint gouges with a few short deep ones, as if keys were dragged across the paint.
        private static Texture2D Scratches()
        {
            const int w = 512, h = 128;
            var pixels = new Color[w * h];
            var random = new System.Random(4471);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(1f, 1f, 1f, 0f);
            for (int s = 0; s < 46; s++)
            {
                bool deep = s % 7 == 0;
                float x0 = (float)random.NextDouble() * w, y0 = (float)random.NextDouble() * h;
                float angle = (float)(random.NextDouble() - 0.5) * (deep ? 1.2f : 0.5f);
                float length = deep ? 20f + (float)random.NextDouble() * 50f : 60f + (float)random.NextDouble() * 220f;
                float strength = deep ? 0.85f : 0.25f + (float)random.NextDouble() * 0.3f;
                for (float t = 0f; t < length; t += 0.5f)
                {
                    int x = Mathf.RoundToInt(x0 + Mathf.Cos(angle) * t), y = Mathf.RoundToInt(y0 + Mathf.Sin(angle) * t + Mathf.Sin(t * 0.07f) * 1.5f);
                    if (x < 0 || x >= w || y < 0 || y >= h) continue;
                    float fade = Mathf.Sin(t / length * Mathf.PI);
                    int i = y * w + x;
                    pixels[i].a = Mathf.Max(pixels[i].a, strength * fade);
                    if (deep && y + 1 < h) pixels[i + w].a = Mathf.Max(pixels[i + w].a, strength * fade * 0.6f);
                }
            }
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            return Save(texture, "MenuScratches", false);
        }

        private static Texture2D Write(string name, int w, int h, bool repeat, System.Func<int, int, System.Random, Color> pixel)
        {
            // A fixed per-texture seed (string hashes aren't stable across runtimes), so rebuilds are byte-identical.
            int seed = 17;
            foreach (char c in name) seed = seed * 31 + c;
            var random = new System.Random(seed & 0x7fffffff);
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++) texture.SetPixel(x, y, pixel(x, y, random));
            return Save(texture, name, repeat);
        }

        private static Texture2D Save(Texture2D texture, string name, bool repeat)
        {
            texture.Apply();
            string path = $"{TextureFolder}/{name}.png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}

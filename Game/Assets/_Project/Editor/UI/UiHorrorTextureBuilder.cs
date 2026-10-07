using System.IO;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Original procedural paper, edge dirt and scan lines; subtle enough to keep controls readable.</summary>
    public static class UiHorrorTextureBuilder
    {
        private const string Folder = "Assets/_Project/Art/UI";

        [MenuItem("Tools/Abandoned/UI/Create Horror Textures")]
        public static void Create()
        {
            Write("HorrorPaper", 256, false);
            Write("HorrorGrime", 256, true);
            var scan = new Texture2D(4, 8, TextureFormat.RGBA32, false);
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 4; x++) scan.SetPixel(x, y, new Color(0f, 0f, 0f, y % 4 == 0 ? 0.1f : 0f));
            Save(scan, "HorrorScanlines", repeat: true);
        }

        private static void Write(string name, int size, bool transparent)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var random = new System.Random(transparent ? 8827 : 3399);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)(size - 1), v = y / (float)(size - 1);
                    float edge = 1f - Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v)) * 2f;
                    float noise = Mathf.PerlinNoise(u * 13f + 11f, v * 13f + 7f);
                    float fibre = (float)random.NextDouble() - 0.5f;
                    float stain = Mathf.Pow(Mathf.Max(0f, noise - 0.42f), 2f) * 0.55f;
                    if (transparent)
                    {
                        float alpha = Mathf.Pow(edge, 8f) * (0.1f + stain) + Mathf.Abs(fibre) * 0.015f;
                        texture.SetPixel(x, y, new Color(0.09f, 0.075f, 0.054f, alpha));
                    }
                    else
                    {
                        float dark = stain + Mathf.Pow(edge, 12f) * 0.1f + fibre * 0.018f;
                        texture.SetPixel(x, y, new Color(0.74f - dark, 0.70f - dark, 0.60f - dark, 1f));
                    }
                }
            Save(texture, name, repeat: false);
        }

        private static void Save(Texture2D texture, string name, bool repeat)
        {
            texture.Apply();
            string path = $"{Folder}/{name}.png";
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
        }
    }
}

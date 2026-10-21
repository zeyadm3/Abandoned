using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>One downloaded ambientCG set in Art/Scanned (Tools/Art/fetch_textures.py), with its importers fixed and
    /// the metal/smoothness map packed for URP. Used by <see cref="MaterialUpgrade"/>.</summary>
    public sealed class ScannedSet
    {
        private const int DetailMapSize = 2048, SupportMapSize = 1024; // colour and normal; AO and metal/smoothness

        public Texture2D Color, Normal, Occlusion;
        public float MeanLuminance = 0.5f;
        public bool HasMetal => metal != null;
        private string folder, id, rough, metal;

        public static ScannedSet Load(string id)
        {
            string folder = $"{MaterialUpgrade.ScannedRoot}/{id}";
            string color = Find(folder, "_Color.jpg"), normal = Find(folder, "_NormalGL.jpg");
            if (color == null || normal == null) return null;
            string ao = Find(folder, "_AmbientOcclusion.jpg");
            Import(color, srgb: true, normalMap: false, DetailMapSize);
            Import(normal, srgb: false, normalMap: true, DetailMapSize);
            if (ao != null) Import(ao, srgb: false, normalMap: false, SupportMapSize);
            return new ScannedSet
            {
                Color = AssetDatabase.LoadAssetAtPath<Texture2D>(color),
                Normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normal),
                Occlusion = ao != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(ao) : null,
                MeanLuminance = MaterialUpgrade.Luminance(MeanColor(color)),
                folder = folder,
                id = id,
                rough = Find(folder, "_Roughness.jpg"),
                metal = Find(folder, "_Metalness.jpg"),
            };
        }

        /// <summary>The metal/smoothness map: with the scan's metalness, or (painted) with none.</summary>
        public Texture2D Packed(bool bare)
        {
            if (rough == null) return null;
            bool withMetal = bare && metal != null;
            string outPath = $"{folder}/{id}_{(withMetal ? "MetalSmooth" : "Smooth")}.png";
            if (!File.Exists(outPath)) Pack(outPath, rough, withMetal ? metal : null);
            Import(outPath, srgb: false, normalMap: false, SupportMapSize);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(outPath);
        }

        private static string Find(string folder, string suffix)
        {
            if (!Directory.Exists(folder)) return null;
            string hit = Directory.GetFiles(folder, "*" + suffix).FirstOrDefault();
            return hit?.Replace('\\', '/');
        }

        private static void Import(string path, bool srgb, bool normalMap, int size)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter ti) return;
            TextureImporterType type = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            // Floors are seen at grazing angles: 8x anisotropy keeps a corridor's far end sharp instead of a smear.
            if (ti.textureType == type && ti.sRGBTexture == srgb && ti.maxTextureSize == size && ti.mipmapEnabled && ti.anisoLevel == 8) return;
            ti.textureType = type;
            ti.sRGBTexture = srgb;
            ti.maxTextureSize = size;
            ti.mipmapEnabled = true;
            ti.anisoLevel = 8;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            ti.SaveAndReimport();
        }

        /// <summary>Average linear colour of an image file (PNG or JPG), read straight from disk so the asset need not
        /// be readable. Grey if it can't be read.</summary>
        public static Color MeanColor(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return new Color(0.5f, 0.5f, 0.5f);
            var tex = new Texture2D(2, 2);
            try
            {
                if (!tex.LoadImage(File.ReadAllBytes(path))) return new Color(0.5f, 0.5f, 0.5f);
                Color32[] px = tex.GetPixels32();
                double r = 0, g = 0, b = 0;
                int step = Mathf.Max(1, px.Length / 65536), n = 0;
                for (int i = 0; i < px.Length; i += step, n++)
                {
                    Color c = new Color(px[i].r / 255f, px[i].g / 255f, px[i].b / 255f).linear;
                    r += c.r;
                    g += c.g;
                    b += c.b;
                }
                n = Mathf.Max(1, n);
                return new Color((float)(r / n), (float)(g / n), (float)(b / n));
            }
            finally { Object.DestroyImmediate(tex); }
        }

        // URP Lit reads metallic from R and smoothness from A of one map.
        private static void Pack(string outPath, string roughPath, string metalPath)
        {
            var rough = new Texture2D(2, 2);
            rough.LoadImage(File.ReadAllBytes(roughPath));
            Texture2D metal = null;
            if (metalPath != null)
            {
                metal = new Texture2D(2, 2);
                metal.LoadImage(File.ReadAllBytes(metalPath));
                if (metal.width != rough.width || metal.height != rough.height) { Object.DestroyImmediate(metal); metal = null; }
            }
            Color32[] r = rough.GetPixels32();
            Color32[] mt = metal != null ? metal.GetPixels32() : null;
            var packed = new Color32[r.Length];
            for (int i = 0; i < r.Length; i++)
                packed[i] = new Color32(mt != null ? mt[i].r : (byte)0, 0, 0, (byte)(255 - r[i].r));
            var tex = new Texture2D(rough.width, rough.height, TextureFormat.RGBA32, false, true);
            tex.SetPixels32(packed);
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rough);
            if (metal != null) Object.DestroyImmediate(metal);
            AssetDatabase.ImportAsset(outPath);
        }
    }
}

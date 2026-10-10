using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Original shared wear maps; URP stores metallic in red and smoothness in alpha.</summary>
    public static class SalvageMaterials
    {
        public const string Folder = "Assets/_Project/Art/Generated/Salvage0155/Materials";
        private static readonly Dictionary<string, Material> materials = new();
        private static readonly string[] keys = { "Metal", "Rust", "Plastic", "Rubber", "Glass", "Circuit",
            "Copper", "Wood", "Fabric", "Paint", "Safety", "Paper", "Concrete", "Wrap", "Net", "Cable" };

        public static void Prepare()
        {
            PolishAssets.EnsureFolder(Folder);
            materials.Clear();
            foreach (string key in keys) materials[key] = Author(key);
        }

        public static Material Get(string key)
        {
            if (materials.TryGetValue(key, out Material material)) return material;
            material = AssetDatabase.LoadAssetAtPath<Material>($"{Folder}/Salvage_{key}.mat");
            materials[key] = material != null ? material : Author(key);
            return materials[key];
        }

        private static Material Author(string key)
        {
            PolishAssets.EnsureFolder(Folder);
            const int size = 512;
            var albedo = new Color32[size * size];
            var metallic = new Color32[albedo.Length];
            var normal = new Color32[albedo.Length];
            var ao = new Color32[albedo.Length];
            Color baseColor = BaseColor(key);
            int seed = 0; foreach (char c in key) seed = (seed * 31 + c) & 0x7fff;
            float metal = key is "Metal" or "Copper" ? .85f : key is "Rust" or "Paint" ? .18f : 0;
            float smooth = key is "Glass" or "Wrap" ? .62f : key is "Rubber" or "Fabric" ? .08f : .26f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int i = x + y * size;
                float u = x / (float)size, v = y / (float)size;
                float grain = Mathf.PerlinNoise(u * 71 + seed, v * 67 + seed);
                float dust = Mathf.SmoothStep(.38f, .76f, Mathf.PerlinNoise(u * 8 + seed, v * 8));
                float scratch = (x * 11 + y * 3 + seed) % 173 < 2 ? .22f : 0;
                float weave = key == "Fabric" ? ((x + y) % 4 < 2 ? .025f : -.025f) : 0;
                float timber = key == "Wood" ? Mathf.Sin(u * 95 + grain * 3) * .035f : 0;
                Color tint = baseColor * (.84f + grain * .26f + weave + timber);
                tint = Color.Lerp(tint, new Color(.49f, .47f, .41f), dust * .16f);
                tint += new Color(scratch, scratch, scratch, 0);
                if (key == "Net") tint.a = x % 48 < 8 || y % 48 < 8 ? 1 : 0;
                else tint.a = key == "Wrap" ? .32f : 1;
                albedo[i] = tint;
                metallic[i] = new Color32((byte)(metal * (1 - dust * .4f) * 255), 0, 0,
                    (byte)(Mathf.Clamp01(smooth * (1 - dust * .55f) - scratch * .1f) * 255));
                ao[i] = new Color32(255, (byte)((.9f + grain * .1f) * 255), 255, 255);
                float bump = grain - .5f;
                normal[i] = new Color32((byte)(128 + bump * 10), (byte)(128 + weave * 65), 255, 255);
            }
            Texture2D colorMap = SaveMap(key, "BaseColor", albedo, size, true, false);
            Texture2D metalMap = SaveMap(key, "MetallicSmoothness", metallic, size, false, false);
            Texture2D normalMap = SaveMap(key, "Normal", normal, size, false, true);
            Texture2D aoMap = SaveMap(key, "Occlusion", ao, size, false, false);
            string path = $"{Folder}/Salvage_{key}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BaseMap", colorMap);
            material.SetTexture("_MetallicGlossMap", metalMap);
            material.SetTexture("_BumpMap", normalMap);
            material.SetTexture("_OcclusionMap", aoMap);
            material.SetFloat("_Smoothness", 1);
            material.SetFloat("_BumpScale", .35f);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_OCCLUSIONMAP");
            if (key == "Wrap")
            {
                material.SetFloat("_Surface", 1); material.SetFloat("_ZWrite", 0);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetOverrideTag("RenderType", "Transparent");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.renderQueue = 3000;
            }
            if (key == "Net")
            {
                material.SetFloat("_AlphaClip", 1); material.SetFloat("_Cutoff", .5f);
                material.EnableKeyword("_ALPHATEST_ON"); material.renderQueue = 2450;
            }
            if (key is "Net" or "Fabric" or "Wrap") material.SetFloat("_Cull", 0);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture2D SaveMap(string key, string suffix, Color32[] pixels, int size, bool srgb, bool isNormal)
        {
            string path = $"{Folder}/{key}_{suffix}.png";
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels); texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb; importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat; importer.maxTextureSize = size;
            importer.alphaIsTransparency = suffix == "BaseColor" && key is "Wrap" or "Net";
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Color BaseColor(string key) => key switch
        {
            "Metal" => new(.34f, .38f, .40f), "Rust" => new(.42f, .20f, .10f),
            "Plastic" => new(.13f, .15f, .17f), "Rubber" or "Cable" => new(.075f, .08f, .075f),
            "Glass" => new(.075f, .13f, .15f), "Circuit" => new(.10f, .27f, .16f),
            "Copper" => new(.56f, .30f, .15f), "Wood" => new(.46f, .33f, .19f),
            "Fabric" => new(.38f, .35f, .29f), "Paint" => new(.48f, .40f, .13f),
            "Safety" or "Net" => new(.85f, .29f, .055f), "Paper" => new(.66f, .63f, .53f),
            "Concrete" => new(.35f, .36f, .34f), "Wrap" => new(.74f, .78f, .75f),
            _ => Color.gray
        };
    }
}

using System.IO;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public static class MeridianSkyBuilder
    {
        private const string Folder = "Assets/_Project/Art/Generated/MeridianAtmosphere";

        public static void Apply(MeridianPresentationConfig config)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = Folder + "/OvercastNight.png";
            if (!File.Exists(path)) CreateCloudTexture(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.maxTextureSize != 512 || !importer.GetPlatformTextureSettings("Standalone").overridden)
            {
                importer.maxTextureSize = 512;
                importer.mipmapEnabled = true;
                importer.sRGBTexture = true;
                importer.isReadable = false;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                var desktop = importer.GetPlatformTextureSettings("Standalone");
                desktop.overridden = true;
                desktop.maxTextureSize = 512;
                desktop.format = TextureImporterFormat.BC7;
                importer.SetPlatformTextureSettings(desktop);
                importer.SaveAndReimport();
            }
            string materialPath = Folder + "/OvercastNight.mat";
            Material sky = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (sky == null)
            {
                sky = new Material(Shader.Find("Abandoned/MeridianSky"));
                AssetDatabase.CreateAsset(sky, materialPath);
            }
            sky.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            sky.SetFloat("_Exposure", config.SkyExposure);
            EditorUtility.SetDirty(sky);
            RenderSettings.skybox = sky;
        }

        private static void CreateCloudTexture(string path)
        {
            const int size = 512;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float v = y / (float)(size - 1);
                float horizon = Mathf.Exp(-Mathf.Pow((v - .5f) * 6f, 2f));
                float cloud = Mathf.PerlinNoise(x / 83f, y / 39f) * .45f
                    + Mathf.PerlinNoise(x / 171f, y / 77f) * .55f;
                Color colour = Color.Lerp(new Color(.075f, .105f, .13f), new Color(.18f, .21f, .23f), horizon);
                pixels[y * size + x] = colour * Mathf.Lerp(.68f, 1.1f, cloud);
                pixels[y * size + x].a = 1f;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
        }
    }
}

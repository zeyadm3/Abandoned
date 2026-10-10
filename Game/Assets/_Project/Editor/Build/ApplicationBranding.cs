using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Shared light artwork for standalone players, plus macOS appearance-aware icon sources.</summary>
    public static class ApplicationBranding
    {
        public const string LightPath = "Assets/_Project/Art/Branding/AbandonedIcon-Light.png";
        public const string DarkPath = "Assets/_Project/Art/Branding/AbandonedIcon-Dark.png";
        public const string MacIconName = "Abandoned";

        public static string MacIconSource => Path.Combine(
            Directory.GetParent(Application.dataPath).Parent.FullName, "Tools", "Branding", MacIconName + ".icon");

        // Standalone settings are shared by Windows and macOS. Windows must only embed the light image;
        // Apple's additional appearances are installed separately into the finished Mac bundle.
        public static void ApplyPlayerSettings()
        {
            Texture2D light = LoadIcon(LightPath);
            int[] sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Any);
            if (sizes.Length == 0) throw new InvalidOperationException("Unity exposes no standalone application icon slots.");
            PlayerSettings.SetIcons(NamedBuildTarget.Standalone, sizes.Select(_ => light).ToArray(), IconKind.Any);
        }

        private static Texture2D LoadIcon(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Application icon artwork is missing.", path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"No texture importer for {path}.");
            if (importer.textureType != TextureImporterType.Default || importer.mipmapEnabled ||
                importer.textureCompression != TextureImporterCompression.Uncompressed || importer.maxTextureSize != 1024 ||
                !importer.alphaIsTransparency || !importer.sRGBTexture)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 1024;
                importer.alphaIsTransparency = true;
                importer.sRGBTexture = true;
                importer.SaveAndReimport();
            }
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (icon == null || icon.width != 1024 || icon.height != 1024)
                throw new InvalidOperationException($"Application icon must be a 1024 by 1024 PNG: {path}.");
            return icon;
        }

        public static void WriteMacIconSource()
        {
            LoadIcon(LightPath);
            LoadIcon(DarkPath);
            string assets = Path.Combine(MacIconSource, "Assets");
            Directory.CreateDirectory(assets);
            File.Copy(LightPath, Path.Combine(assets, Path.GetFileName(LightPath)), true);
            File.Copy(DarkPath, Path.Combine(assets, Path.GetFileName(DarkPath)), true);
            // Native visibility specializations compile into Aqua and DarkAqua icon layers in Assets.car.
            // Disable material effects so the approved artwork retains its contrast and silhouette.
            File.WriteAllText(Path.Combine(MacIconSource, "icon.json"), IconDefinition);
        }

        private const string IconDefinition = @"{
  ""fill"": ""none"",
  ""groups"": [
    {
      ""name"": ""Abandoned"",
      ""layers"": [
        {
          ""name"": ""Light artwork"",
          ""image-name"": ""AbandonedIcon-Light.png"",
          ""glass"": false,
          ""hidden-specializations"": [
            { ""value"": false },
            { ""appearance"": ""dark"", ""value"": true }
          ]
        },
        {
          ""name"": ""Dark artwork"",
          ""image-name"": ""AbandonedIcon-Dark.png"",
          ""glass"": false,
          ""hidden-specializations"": [
            { ""value"": true },
            { ""appearance"": ""dark"", ""value"": false }
          ]
        }
      ],
      ""specular"": false,
      ""shadow"": { ""kind"": ""neutral"", ""opacity"": 0 },
      ""translucency"": { ""enabled"": false, ""value"": 0 }
    }
  ],
  ""supported-platforms"": { ""squares"": [""macOS""] }
}
";
    }
}

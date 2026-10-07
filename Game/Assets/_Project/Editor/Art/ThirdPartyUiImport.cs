using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Import settings for third-party UI art (UI overhaul step 1): Kenney CC0 icon PNGs become crisp UI
    /// textures (no mipmaps, clamped, uncompressed, alpha as transparency) for UI Toolkit backgrounds.
    /// </summary>
    public class ThirdPartyUiImport : AssetPostprocessor
    {
        private static readonly string[] IconFolders = { "Kenney/GameIcons/", "Kenney/GenericItems/", "Kenney/BoardGameIcons/", "Kenney/InputPrompts/" };

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ThirdPartyModelImport.Root)) return;
            bool icon = false;
            foreach (string folder in IconFolders) icon |= assetPath.Contains(folder);
            if (!icon) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.mipmapEnabled = false;
            importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            importer.filterMode = UnityEngine.FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 256;
        }
    }
}

using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Import settings for UI art (UI overhaul): Kenney CC0 icon PNGs and our rendered loot icons become crisp UI
    /// textures (no mipmaps, clamped, uncompressed, alpha as transparency) for UI Toolkit backgrounds.
    /// </summary>
    public class ThirdPartyUiImport : AssetPostprocessor
    {
        private static readonly string[] IconFolders = { "Kenney/GameIcons/", "Kenney/GenericItems/", "Kenney/BoardGameIcons/", "Kenney/InputPrompts/" };

        private void OnPreprocessTexture()
        {
            bool icon = assetPath.StartsWith(LootIconRenderer.Folder + "/");
            if (assetPath.StartsWith(ThirdPartyModelImport.Root))
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

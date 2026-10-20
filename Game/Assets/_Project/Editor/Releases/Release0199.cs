using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.19.9 font coverage: dynamic font assets made from the Noto fonts (OFL: Latin/Greek/Cyrillic, Arabic,
    /// Simplified Chinese with Japanese kana, Korean), set as the global fallbacks of the menu panel's text
    /// settings. Barlow stays the face; any glyph it lacks (a crewmate's name, a chat line) falls through.
    /// </summary>
    public static class Release0199
    {
        private const string FontFolder = "Assets/_Project/Art/ThirdParty/Fonts/Noto";
        private const string AssetFolder = "Assets/_Project/Data/UI/Fonts";
        private const string TextSettingsPath = AssetFolder + "/PanelText.asset";
        private static readonly string[] Fonts = { "NotoSans", "NotoSansArabic", "NotoSansSC", "NotoSansKR" };

        [MenuItem(ToolMenu.UI + "Font Fallbacks (0.19.9)", priority = ToolMenu.UiP + 20)]
        public static void Author()
        {
            PolishAssets.EnsureFolder(AssetFolder);
            var fallbacks = new List<FontAsset>();
            foreach (string name in Fonts)
            {
                string path = $"{AssetFolder}/{name} SDF.asset";
                var asset = AssetDatabase.LoadAssetAtPath<FontAsset>(path);
                if (asset == null)
                {
                    var font = AssetDatabase.LoadAssetAtPath<Font>($"{FontFolder}/{name}.ttf");
                    if (font == null)
                    {
                        Debug.LogWarning($"[Fonts0199] {name}.ttf missing.");
                        continue;
                    }
                    asset = FontAsset.CreateFontAsset(font);
                    asset.name = $"{name} SDF";
                    asset.isMultiAtlasTexturesEnabled = true;
                    // Otherwise each player build rewrites the asset and the next platform's build is "dirty".
                    var so = new SerializedObject(asset);
                    SerializedProperty clear = so.FindProperty("m_ClearDynamicDataOnBuild");
                    if (clear != null)
                    {
                        clear.boolValue = false;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                    AssetDatabase.CreateAsset(asset, path);
                    if (asset.material != null)
                    {
                        asset.material.name = $"{name} SDF Material";
                        AssetDatabase.AddObjectToAsset(asset.material, asset);
                    }
                    foreach (Texture2D atlas in asset.atlasTextures)
                        if (atlas != null)
                        {
                            atlas.name = $"{name} SDF Atlas";
                            AssetDatabase.AddObjectToAsset(atlas, asset);
                        }
                }
                fallbacks.Add(asset);
            }
            var settings = SerializedWiring.LoadOrCreateAsset<PanelTextSettings>(TextSettingsPath);
            settings.fallbackFontAssets = fallbacks;
            EditorUtility.SetDirty(settings);
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(UiContentBuilder.PanelPath);
            if (panel != null)
            {
                panel.textSettings = settings;
                EditorUtility.SetDirty(panel);
            }
            AssetDatabase.SaveAssets();
            BuildScript.ApplyPlayerSettings();
            Debug.Log($"[Fonts0199] {fallbacks.Count} fallback fonts on the menu panel's text settings.");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}

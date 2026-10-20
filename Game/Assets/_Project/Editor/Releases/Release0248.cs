using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.8 language fonts. The 0.19.9 Chinese and Korean fallbacks were variable fonts, which Unity draws at
    /// their default weight (Thin): fine for a crewmate's name, unreadable for a whole interface. This swaps
    /// them for fixed Medium files, adds Japanese (its characters must not borrow Chinese shapes) and removes
    /// the two old files. Latin/Greek/Cyrillic and Arabic stay as they were. The order per language is set at
    /// run time (UI.LanguageFonts). Details: Docs/LOCALIZATION_0.24.8.md.
    /// </summary>
    public static class Release0248
    {
        private const string FontFolder = "Assets/_Project/Art/ThirdParty/Fonts/Noto";
        private const string AssetFolder = "Assets/_Project/Data/UI/Fonts";
        private const string TextSettingsPath = AssetFolder + "/PanelText.asset";
        private static readonly string[] Kept = { "NotoSans", "NotoSansArabic" };
        private static readonly string[] Added = { "NotoSansSC-Medium", "NotoSansJP-Medium", "NotoSansKR-Medium" };
        private static readonly string[] Retired = { "NotoSansSC", "NotoSansKR" };

        [MenuItem(ToolMenu.UI + "Language Fonts (0.24.8)", priority = ToolMenu.UiP + 21)]
        public static void Author()
        {
            PolishAssets.EnsureFolder(AssetFolder);
            var fallbacks = new List<FontAsset>();
            foreach (string name in Kept)
            {
                var kept = AssetDatabase.LoadAssetAtPath<FontAsset>($"{AssetFolder}/{name} SDF.asset");
                if (kept != null) fallbacks.Add(kept);
                else Debug.LogWarning($"[Fonts0248] {name} SDF.asset is missing; run Tools > Abandoned > UI > Font Fallbacks (0.19.9) first.");
            }
            foreach (string name in Added)
            {
                FontAsset added = Create(name);
                if (added == null)
                {
                    // Half a swap would leave a script with no font at all: keep the old files and say so.
                    Debug.LogError($"[Fonts0248] {FontFolder}/{name}.otf is missing; nothing was changed.");
                    return;
                }
                fallbacks.Add(added);
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
            foreach (string name in Retired)
            {
                AssetDatabase.DeleteAsset($"{AssetFolder}/{name} SDF.asset");
                AssetDatabase.DeleteAsset($"{FontFolder}/{name}.ttf");
            }
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Fonts0248] {fallbacks.Count} fallback fonts on the menu panel's text settings; the two variable-weight files are gone.");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        private static FontAsset Create(string name)
        {
            string path = $"{AssetFolder}/{name} SDF.asset";
            var asset = AssetDatabase.LoadAssetAtPath<FontAsset>(path);
            if (asset != null) return asset;
            var font = AssetDatabase.LoadAssetAtPath<Font>($"{FontFolder}/{name}.otf");
            if (font == null) return null;
            // A whole interface in these scripts needs over a thousand characters: sampled at 48 pt into 2048 px
            // pages, one page holds about that many (the default 90 pt into 1024 px holds about a hundred).
            asset = FontAsset.CreateFontAsset(font, 48, 5, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
            asset.name = $"{name} SDF";
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
            return asset;
        }
    }
}

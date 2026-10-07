using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Menu assets (M7.4): the UI Toolkit panel settings (our theme, scaled from 1920x1080) and the
    /// credits text, regenerated from Docs/ASSET_CREDITS.md so the game's credits never drift from it.
    /// </summary>
    public static class UiContentBuilder
    {
        public const string Folder = "Assets/_Project/Data/UI";
        public const string PanelPath = Folder + "/MenuPanel.asset";
        public const string CreditsPath = Folder + "/Credits.txt";
        public const string ThemePath = "Assets/_Project/Art/UI/MenuTheme.tss";
        public const string StylePath = "Assets/_Project/Art/UI/Menu.uss";
        public const string ThemeAssetsPath = Folder + "/Resources/UiThemeAssets.asset";
        // UI overhaul: Barlow for everything small (readable), Saira Stencil One only for big titles (OFL).
        public const string FontPath = ThirdPartyModelImport.Root + "Fonts/Barlow/Barlow-Medium.ttf";
        public const string TitleFontPath = ThirdPartyModelImport.Root + "Fonts/SairaStencilOne/SairaStencilOne-Regular.ttf";
        public const string IconsPath = Folder + "/Resources/UiIcons.asset";

        [MenuItem("Tools/Abandoned/UI/Build Menu Assets")]
        public static void CreateMissing()
        {
            UiHorrorTextureBuilder.Create();
            UiGearIconBuilder.Create();
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Data", "UI");
            if (!AssetDatabase.IsValidFolder(Folder + "/Resources")) AssetDatabase.CreateFolder(Folder, "Resources");
            // Re-import after image generation so the USS stores real texture references on a clean checkout.
            AssetDatabase.ImportAsset(StylePath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(ThemePath, ImportAssetOptions.ForceUpdate);
            var themeAssets = SerializedWiring.LoadOrCreateAsset<Abandoned.UI.UiThemeAssets>(ThemeAssetsPath);
            themeAssets.EditorSetup(AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath),
                AssetDatabase.LoadAssetAtPath<Font>(FontPath), AssetDatabase.LoadAssetAtPath<Font>(TitleFontPath));
            EditorUtility.SetDirty(themeAssets);
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelPath);
            }
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            if (panel.themeStyleSheet == null) Debug.LogError($"[UI] Theme missing at {ThemePath}.");
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1920, 1080);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            // Scale by height: ultrawide gets more room at the sides, 16:10 a little more height, text stays the same size.
            panel.match = 1f;
            panel.sortingOrder = 100;
            EditorUtility.SetDirty(panel);
            WriteCredits();
            BuildIcons();
            AssetDatabase.SaveAssets();
        }

        // Icon packs and their id prefixes (UiIcons): colour item art, white menu icons, white HUD symbols, mouse glyphs.
        private static readonly (string folder, string prefix)[] IconPacks =
        {
            (ThirdPartyModelImport.Root + "Kenney/GenericItems", "item"), (ThirdPartyModelImport.Root + "Kenney/GameIcons", "icon"),
            (ThirdPartyModelImport.Root + "Kenney/BoardGameIcons", "board"), (ThirdPartyModelImport.Root + "Kenney/InputPrompts", "mouse"),
            (LootIconRenderer.Folder, "loot"),
            (UiGearIconBuilder.Folder, "item"),
        };

        /// <summary>Data/UI/Resources/UiIcons: every icon PNG in the packs by "prefix/name".</summary>
        public static Abandoned.UI.UiIcons BuildIcons()
        {
            if (!AssetDatabase.IsValidFolder(Folder + "/Resources")) AssetDatabase.CreateFolder(Folder, "Resources");
            var icons = SerializedWiring.LoadOrCreateAsset<Abandoned.UI.UiIcons>(IconsPath);
            var byId = new Dictionary<string, Abandoned.UI.UiIcons.Entry>(System.StringComparer.Ordinal);
            foreach ((string folder, string prefix) in IconPacks)
            {
                string path = folder;
                if (!AssetDatabase.IsValidFolder(path)) continue;
                foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { path }))
                {
                    string file = AssetDatabase.GUIDToAssetPath(guid);
                    if (!file.EndsWith(".png")) continue;
                    string id = $"{prefix}/{Path.GetFileNameWithoutExtension(file)}";
                    byId[id] = new Abandoned.UI.UiIcons.Entry { Id = id, Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(file) };
                }
            }
            var list = byId.Values.OrderBy(e => e.Id, System.StringComparer.Ordinal).ToList();
            icons.EditorSet(list);
            EditorUtility.SetDirty(icons);
            return icons;
        }

        private static void WriteCredits()
        {
            string docs = Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName, "Docs", "ASSET_CREDITS.md");
            var text = new StringBuilder();
            text.AppendLine("# ART, SOUND AND FONTS");
            if (File.Exists(docs))
            {
                foreach (string line in File.ReadAllLines(docs))
                {
                    string[] cells = line.Split('|').Select(c => c.Trim()).ToArray();
                    // | Pack | Author | Licence | Source | What we use | In the project |
                    if (cells.Length < 7 || cells[1] == "Pack" || cells[1].StartsWith("-")) continue;
                    text.AppendLine($"{cells[1]} by {cells[2]} ({cells[3]}): {cells[5]}.");
                }
            }
            else Debug.LogWarning($"[UI] {docs} not found; credits list only the technology.");
            text.AppendLine("Everything else (synthesised sounds, ambience, greybox) was made for the game.");
            text.AppendLine("# TECHNOLOGY");
            foreach (string t in Technology) text.AppendLine(t);
            string content = text.ToString();
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, CreditsPath);
            if (File.Exists(path) && File.ReadAllText(path) == content) return;
            File.WriteAllText(path, content);
            AssetDatabase.ImportAsset(CreditsPath);
        }

        private static readonly List<string> Technology = new()
        {
            "Unity 6 with the Universal Render Pipeline, Netcode for GameObjects, Cinemachine, AI Navigation and the Input System.",
            "Facepunch.Steamworks by Facepunch Studios (MIT licence).",
            "Facepunch Transport for Netcode for GameObjects, community package (MIT licence).",
        };
    }
}

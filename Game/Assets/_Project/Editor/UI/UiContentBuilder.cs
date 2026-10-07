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
        public const string FontPath = ThirdPartyModelImport.Root + "Kenney/Fonts/Kenney Future.ttf";
        public const string TitleFontPath = ThirdPartyModelImport.Root + "Kenney/Fonts/Kenney Future Narrow.ttf";

        [MenuItem("Tools/Abandoned/UI/Build Menu Assets")]
        public static void CreateMissing()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Data", "UI");
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
            panel.match = 0.5f;
            panel.sortingOrder = 100;
            EditorUtility.SetDirty(panel);
            WriteCredits();
            AssetDatabase.SaveAssets();
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

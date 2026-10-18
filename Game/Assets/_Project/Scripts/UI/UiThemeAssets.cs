using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>The application stylesheet is explicitly attached, independently of the control theme.</summary>
    public class UiThemeAssets : ScriptableObject
    {
        public const string ResourcePath = "UiThemeAssets";
        [SerializeField] private StyleSheet styles;
        [SerializeField] private Font bodyFont;
        [SerializeField] private Font titleFont;
        public StyleSheet Styles => styles;
        public Font BodyFont => bodyFont;
        public Font TitleFont => titleFont;

        private static readonly string[] Layers =
        {
            "Revamp", "AbandonedTheme", "AbandonedMenus", "HorrorHud", "AbandonedPanels", "FieldHud"
        };

        /// <summary>One theme stack for the menu document and independent in-world displays.</summary>
        public static void Apply(VisualElement root)
        {
            if (root == null) return;
            root.AddToClassList("horror-theme");
            UiThemeAssets assets = Resources.Load<UiThemeAssets>(ResourcePath);
            if (assets != null)
            {
                if (assets.Styles != null && !root.styleSheets.Contains(assets.Styles)) root.styleSheets.Add(assets.Styles);
                if (assets.BodyFont != null) root.style.unityFontDefinition = FontDefinition.FromFont(assets.BodyFont);
            }
            foreach (string path in Layers)
            {
                StyleSheet sheet = Resources.Load<StyleSheet>(path);
                if (sheet != null && !root.styleSheets.Contains(sheet)) root.styleSheets.Add(sheet);
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(StyleSheet sheet, Font body, Font title)
        {
            styles = sheet;
            bodyFont = body;
            titleFont = title;
        }
#endif
    }
}

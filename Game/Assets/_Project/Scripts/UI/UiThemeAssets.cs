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

        /// <summary>The game's one stylesheet (Art/UI/Resources/Ui.uss): fonts and palette included.</summary>
        public const string Sheet = "Ui";

        /// <summary>One theme for the menu document and independent in-world displays.</summary>
        public static void Apply(VisualElement root)
        {
            if (root == null) return;
            root.AddToClassList("horror-theme");
            StyleSheet sheet = Resources.Load<StyleSheet>(Sheet);
            if (sheet != null && !root.styleSheets.Contains(sheet)) root.styleSheets.Add(sheet);
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

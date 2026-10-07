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

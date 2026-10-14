using System.Text;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// F1 text line (0.24.8): the language in use, how many lines its sheets gave, and how many English lines
    /// were asked for without a translation. Turning F1 on also writes those lines to the Console, so they can
    /// be added to the sheets. Developer builds only.
    /// </summary>
    public class TextDebugOverlay : MonoBehaviour
    {
        private GUIStyle style;
        private string line = "";
        private float nextRefresh;

        private void Start()
        {
            if (!DevTools.Enabled) enabled = false;
        }

        private void OnEnable() => DebugView.Changed += OnDebugChanged;

        private void OnDisable() => DebugView.Changed -= OnDebugChanged;

        private void OnDebugChanged(bool visible)
        {
            if (!visible || Loc.Missing.Count == 0) return;
            var list = new StringBuilder($"[Text] {Loc.Missing.Count} lines shown in English because '{Languages.Current.Code}' has no translation for them:\n");
            // Key lines arrive already as "key: id"; the rest are English lines to paste as "en:".
            foreach (string line in Loc.Missing)
                list.Append(line.StartsWith(Loc.KeyLine, System.StringComparison.Ordinal) ? "" : "en: ").Append(line.Replace("\n", "\\n")).Append('\n');
            Debug.Log(list.ToString());
        }

        private void Update()
        {
            if (!DebugView.Visible || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 1f;
            LanguageInfo language = Languages.Current;
            line = $"TEXT {language.Code}{(Languages.Chosen ? "" : " (automatic)")}  {TextTable.Count} lines loaded  " +
                   $"{Loc.Missing.Count} asked for without a translation (F1 off and on lists them in the Console)";
        }

        private void OnGUI()
        {
            if (!DebugView.Visible || line.Length == 0) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 13 };
            // One row above the performance line.
            GUI.Label(new Rect(10f, Screen.height - 46f, Screen.width - 20f, 22f), line, style);
        }
    }
}

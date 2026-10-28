using UnityEngine;

namespace Abandoned.Core
{
    public sealed class ArtRemasterDiagnostics : MonoBehaviour
    {
        [SerializeField] private string summary;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // F1 only, and F1 never opens in a player build: there the GUI loop doesn't run for it at all.
        private GUIStyle style;

        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.UpperRight, wordWrap = true };
            GUI.Label(new Rect(Screen.width - 690, 116, 680, 38), "ART REMASTER · " + summary, style);
        }
#endif

#if UNITY_EDITOR
        public void EditorSetup(string description) => summary = description;
#endif
    }
}

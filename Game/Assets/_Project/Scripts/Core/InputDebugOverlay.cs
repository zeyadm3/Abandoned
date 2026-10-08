using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Core
{
    /// <summary>
    /// Shows the live value of every Gameplay and UI action so bindings can be checked
    /// in Play mode without any gameplay code. Shown while <see cref="DebugView"/> is on (F1).
    /// </summary>
    public class InputDebugOverlay : MonoBehaviour
    {
        [SerializeField] private int fontSize = 14;

        private AbandonedInput input;
        private GUIStyle style;
        private readonly StringBuilder text = new StringBuilder();

        private void Awake()
        {
            // Own instance on purpose: Project-wide Actions is None, so nothing else enables these.
            input = new AbandonedInput();
        }

        private void OnEnable()
        {
            input.Gameplay.Enable();
            input.UI.Enable();
        }

        private void OnDisable() => input.Disable();

        private void OnDestroy()
        {
            input?.Dispose();
        }

        // QA P-08: a debug overlay only; in player builds it never draws, so it doesn't sit in the GUI loop either.
        private void Start()
        {
            if (!Abandoned.Core.DevTools.Enabled) enabled = false;
        }

        private void OnGUI()
        {
            if (!DebugView.Visible) return;

            style ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = fontSize,
                richText = true
            };

            text.Clear();
            text.AppendLine("<b>INPUT DEBUG</b>  (F1)");
            AppendMap(input.Gameplay.Get());
            AppendMap(input.UI.Get());

            var content = new GUIContent(text.ToString());
            Vector2 size = style.CalcSize(content);
            GUI.Box(new Rect(10, 10, size.x, size.y), content, style);
        }

        private void AppendMap(InputActionMap map)
        {
            text.AppendLine($"\n<b>{map.name}</b>");
            foreach (InputAction action in map.actions)
            {
                string value = action.expectedControlType == "Vector2"
                    ? action.ReadValue<Vector2>().ToString("F2")
                    : action.IsPressed() ? "<color=#7CFC00>PRESSED</color>" : "-";
                text.AppendLine($"{action.name,-12} {value}");
            }
        }
    }
}

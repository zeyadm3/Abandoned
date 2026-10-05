using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Core
{
    /// <summary>
    /// Shows the live value of every Gameplay and UI action so bindings can be checked
    /// in Play mode without any gameplay code. Toggle with F1 (Debug/ToggleDebug).
    /// </summary>
    public class InputDebugOverlay : MonoBehaviour
    {
        [SerializeField] private bool visibleOnStart = true;
        [SerializeField] private int fontSize = 14;

        private AbandonedInput input;
        private bool visible;
        private GUIStyle style;
        private readonly StringBuilder text = new StringBuilder();

        private void Awake()
        {
            // Own instance on purpose: Project-wide Actions is None, so nothing else enables these.
            input = new AbandonedInput();
            visible = visibleOnStart;
        }

        private void OnEnable()
        {
            input.Debug.ToggleDebug.performed += OnToggleDebug;
            input.Enable();
        }

        private void OnDisable()
        {
            input.Debug.ToggleDebug.performed -= OnToggleDebug;
            input.Disable();
        }

        private void OnDestroy()
        {
            input?.Dispose();
        }

        private void OnToggleDebug(InputAction.CallbackContext _) => visible = !visible;

        private void OnGUI()
        {
            if (!visible) return;

            style ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = fontSize,
                richText = true
            };

            text.Clear();
            text.AppendLine("<b>INPUT DEBUG</b>  (F1 to hide)");
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

using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Core
{
    /// <summary>
    /// Listens for Debug/ToggleDebug (F1) and flips <see cref="DebugView"/>. One per scene.
    /// Uses only the Debug map so F1 works whether Gameplay or UI is active.
    /// </summary>
    public class DebugViewToggle : MonoBehaviour
    {
        [SerializeField] private bool visibleOnStart;

        private AbandonedInput input;

        private void Awake()
        {
            input = new AbandonedInput();
            DebugView.SetVisible(visibleOnStart);
        }

        private void OnEnable()
        {
            input.Debug.ToggleDebug.performed += OnToggle;
            input.Debug.Enable();
        }

        private void OnDisable()
        {
            input.Debug.ToggleDebug.performed -= OnToggle;
            input.Debug.Disable();
        }

        private void OnDestroy() => input?.Dispose();

        private static void OnToggle(InputAction.CallbackContext _) => DebugView.Toggle();
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Controller menu navigation (0.23.0): while a menu or screen has the cursor and the pad is in use, the
    /// d-pad or left stick moves focus (UI Toolkit navigation events, with key repeat), A presses the focused
    /// control, and the first control takes focus when a pad starts driving a screen. Sliders and choices take
    /// left/right themselves. B closes screens through PadInput.BackPressed; Menu toggles the pause menu.
    /// The focus highlight is the revamp's (0.22.9).
    /// </summary>
    public class PadMenuNavigator : MonoBehaviour
    {
        private const float FirstRepeat = 0.4f, Repeat = 0.12f;

        private float nextMove;
        private Vector2 held;

        private void Update()
        {
            Gamepad pad = Gamepad.current;
            if (pad == null || !PadInput.UsingPad || !CursorOwner.UiActive || HudLayer.Root == null) return;
            VisualElement root = HudLayer.Root.parent;
            if (root == null || root.panel == null) return;
            Focusable focused = root.panel.focusController.focusedElement;
            if (focused == null || !Visible(focused as VisualElement))
            {
                VisualElement first = FirstFocusable(root);
                first?.Focus();
                focused = first;
            }
            Vector2 move = pad.dpad.ReadValue() + pad.leftStick.ReadValue();
            Vector2 dir = Mathf.Abs(move.x) > Mathf.Abs(move.y) ? new Vector2(Mathf.Sign(move.x), 0f) : new Vector2(0f, Mathf.Sign(move.y));
            if (move.magnitude < 0.5f) { held = Vector2.zero; }
            else if (dir != held || Time.unscaledTime >= nextMove)
            {
                nextMove = Time.unscaledTime + (dir != held ? FirstRepeat : Repeat);
                held = dir;
                Navigate(focused as VisualElement, dir);
            }
            if (pad.buttonSouth.wasPressedThisFrame && focused is VisualElement target)
            {
                using NavigationSubmitEvent submit = NavigationSubmitEvent.GetPooled();
                submit.target = target;
                target.SendEvent(submit);
            }
        }

        private static void Navigate(VisualElement from, Vector2 dir)
        {
            if (from == null) return;
            NavigationMoveEvent.Direction d = dir.x > 0 ? NavigationMoveEvent.Direction.Right : dir.x < 0 ? NavigationMoveEvent.Direction.Left
                : dir.y > 0 ? NavigationMoveEvent.Direction.Up : NavigationMoveEvent.Direction.Down;
            using NavigationMoveEvent e = NavigationMoveEvent.GetPooled(d);
            e.target = from;
            from.SendEvent(e);
        }

        private static VisualElement FirstFocusable(VisualElement root)
        {
            var found = new List<VisualElement>();
            root.Query<VisualElement>().Where(v => v.focusable && v.canGrabFocus && v.tabIndex >= 0 && Visible(v)).ToList(found);
            // Prefer a button on the topmost (last added) screen.
            for (int i = found.Count - 1; i >= 0; i--) if (found[i] is Button) return found[i];
            return found.Count > 0 ? found[^1] : null;
        }

        private static bool Visible(VisualElement v)
        {
            for (VisualElement e = v; e != null; e = e.parent)
                if (e.resolvedStyle.display == DisplayStyle.None || e.resolvedStyle.visibility == Visibility.Hidden || e.resolvedStyle.opacity <= 0.01f) return false;
            return v != null && v.layout.width > 0f && v.layout.height > 0f;
        }
    }
}

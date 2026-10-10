using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Core
{
    /// <summary>
    /// Gamepad support, first tier (0.19.8): which device the player used last (prompts follow it), and one
    /// "back" check (Esc, or the pad's B / Start) for the screens that close on Escape.
    /// </summary>
    public static class PadInput
    {
        /// <summary>The last thing pressed or moved was on a gamepad.</summary>
        public static bool UsingPad { get; private set; }

        /// <summary>Esc, or B on a pad, this frame.</summary>
        public static bool BackPressed =>
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Hook()
        {
            UsingPad = false;
            InputSystem.onAfterUpdate -= Watch;
            InputSystem.onAfterUpdate += Watch;
        }

        // A real press or a real stick tilt switches to the pad; any key, click or mouse move switches back.
        private static void Watch()
        {
            Gamepad pad = Gamepad.current;
            if (pad != null && pad.wasUpdatedThisFrame)
            {
                bool active = pad.leftStick.ReadValue().sqrMagnitude > 0.25f || pad.rightStick.ReadValue().sqrMagnitude > 0.25f;
                foreach (var control in pad.allControls)
                    if (!active && control is UnityEngine.InputSystem.Controls.ButtonControl b && b.wasPressedThisFrame) active = true;
                if (active) UsingPad = true;
            }
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) UsingPad = false;
            Mouse mouse = Mouse.current;
            if (mouse != null && (mouse.delta.ReadValue().sqrMagnitude > 4f || mouse.leftButton.wasPressedThisFrame)) UsingPad = false;
        }

        /// <summary>The pad button a binding path means, in Xbox terms ("A", "RT", "D-Pad Up").</summary>
        public static string Glyph(string path)
        {
            string control = path.Contains("/") ? path.Substring(path.IndexOf('/') + 1) : path;
            return control switch
            {
                "buttonSouth" => "A", "buttonEast" => "B", "buttonWest" => "X", "buttonNorth" => "Y",
                "leftTrigger" => "LT", "rightTrigger" => "RT", "leftShoulder" => "LB", "rightShoulder" => "RB",
                "leftStickPress" => "L3", "rightStickPress" => "R3", "start" => "Menu", "select" => "View",
                "dpad/up" => "D-Pad Up", "dpad/down" => "D-Pad Down", "dpad/left" => "D-Pad Left", "dpad/right" => "D-Pad Right",
                "leftStick" => "Left Stick", "rightStick" => "Right Stick",
                _ => control,
            };
        }
    }
}

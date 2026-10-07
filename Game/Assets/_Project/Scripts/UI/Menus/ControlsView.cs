using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Key rebinding (GDD 20): every rebindable action with its key; click one, press the new key
    /// (Esc cancels). Saved at once and applied to the running game. Warns when a key is used twice.
    /// </summary>
    public class ControlsView
    {
        private static readonly Dictionary<string, string> Names = new()
        {
            ["Move"] = "Move", ["Sprint"] = "Sprint", ["Crouch"] = "Crouch", ["Jump"] = "Jump", ["Interact"] = "Interact / pick up",
            ["Use"] = "Use / throw", ["Drop"] = "Drop / hold to set down", ["Rotate"] = "Rotate held object", ["Scan"] = "Scan", ["Flashlight"] = "Flashlight", ["HandSlot1"] = "Hand slot 1",
            ["HandSlot2"] = "Hand slot 2", ["Inventory"] = "Inventory", ["PushToTalk"] = "Push to talk", ["Radio"] = "Radio",
        };

        private readonly AbandonedInput editing = new();
        private readonly List<(InputAction action, int index, Button button)> rows = new();
        private readonly Label warning;
        private InputActionRebindingExtensions.RebindingOperation operation;

        private static int finishedFrame = -10;

        /// <summary>Waiting for a key (or the Esc that cancelled it is still this frame's): Esc isn't Back.</summary>
        public static bool Listening { get; private set; }

        public static bool Busy => Listening || UnityEngine.Time.frameCount - finishedFrame <= 1;

        public VisualElement Root { get; }

        public ControlsView(MenuUi menu)
        {
            InputBindings.Apply(editing.asset);
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Panel(Root, wide: true);
            MenuKit.Text(panel, "CONTROLS", "heading");
            MenuKit.Text(panel, "Click a key, then press the new one. Esc cancels.", "subtitle");
            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("scroll");
            panel.Add(scroll);

            foreach (string name in InputBindings.Rebindable)
            {
                InputAction action = editing.asset.FindAction($"Gameplay/{name}", true);
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding b = action.bindings[i];
                    if (b.isComposite) continue;
                    string label = b.isPartOfComposite ? $"{Names[name]} {b.name}" : Names[name];
                    VisualElement row = MenuKit.Row(scroll);
                    row.AddToClassList("setting");
                    MenuKit.Text(row, label, "setting__label");
                    int index = i;
                    Button button = MenuKit.Button(row, "", () => Listen(action, index), SoundId.UiClick, small: true);
                    button.style.minWidth = 220;
                    rows.Add((action, index, button));
                }
            }
            warning = MenuKit.Text(panel, "", "text");
            warning.AddToClassList("text--error");

            VisualElement buttons = MenuKit.Row(panel);
            MenuKit.Button(buttons, "Reset to defaults", () =>
            {
                Cancel();
                editing.asset.RemoveAllBindingOverrides();
                InputBindings.ResetAll();
                warning.text = "";
                Refresh();
            }, SoundId.UiConfirm).AddToClassList("grow");
            Button back = MenuKit.Button(buttons, "Back", () =>
            {
                Cancel();
                menu.Back();
            }, SoundId.UiBack);
            back.AddToClassList("grow");
            back.style.marginLeft = 10;
            Refresh();
        }

        private void Listen(InputAction action, int index)
        {
            Cancel();
            Listening = true;
            foreach ((InputAction a, int i, Button b) in rows)
                if (a == action && i == index) b.text = "Press a key...";
            action.Disable();
            operation = action.PerformInteractiveRebinding(index)
                .WithCancelingThrough("<Keyboard>/escape")
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithControlsExcluding("<Mouse>/scroll")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(op =>
                {
                    string path = action.bindings[index].effectivePath;
                    string clash = InputBindings.ConflictWith(editing.asset, action, index, path);
                    warning.text = clash != null ? $"That key is also bound to {clash}." : "";
                    InputBindings.Save(editing.asset);
                    GameAudio.PlayUi(SoundId.UiConfirm);
                    Finish();
                })
                .OnCancel(_ => Finish())
                .Start();
        }

        private void Finish()
        {
            operation?.Dispose();
            operation = null;
            Listening = false;
            finishedFrame = UnityEngine.Time.frameCount;
            Refresh();
        }

        private void Cancel()
        {
            if (operation == null) return;
            operation.Cancel();
            Finish();
        }

        private void Refresh()
        {
            foreach ((InputAction action, int index, Button button) in rows)
                button.text = action.GetBindingDisplayString(index, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
        }
    }
}

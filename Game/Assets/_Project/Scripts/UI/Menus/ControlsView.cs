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
            ["Move"] = Loc.Mark("Move"), ["Sprint"] = Loc.Mark("Sprint"), ["Crouch"] = Loc.Mark("Crouch"), ["Jump"] = Loc.Mark("Jump"), ["Interact"] = Loc.Mark("Interact / pick up"),
            ["Use"] = Loc.Mark("Use / throw"), ["Drop"] = Loc.Mark("Drop / hold to set down"), ["Rotate"] = Loc.Mark("Rotate held object"), ["Scan"] = Loc.Mark("Scan"), ["Flashlight"] = Loc.Mark("Flashlight"), ["HandSlot1"] = Loc.Mark("Hand slot 1"),
            ["HandSlot2"] = Loc.Mark("Hand slot 2"), ["Inventory"] = Loc.Mark("Inventory"), ["PushToTalk"] = Loc.Mark("Push to talk"), ["Radio"] = Loc.Mark("Radio"),
            ["GiveGear"] = Loc.Mark("Give gear / take a fallen crewmate's"), ["Chat"] = Loc.Mark("Text chat"), ["PhotoCamera"] = Loc.Mark("Free camera (photo)"), ["HideHud"] = Loc.Mark("Hide the HUD (clips)"),
            ["Ping"] = Loc.Mark("Ping"), ["EmotePoint"] = Loc.Mark("Emote: point"), ["EmoteWave"] = Loc.Mark("Emote: wave"), ["EmotePanic"] = Loc.Mark("Emote: panic"),
        };

        // 0.23.2: an action's second key and its pad button are their own rows.
        private static string Label(string name, InputBinding b)
        {
            string label = Names.TryGetValue(name, out string n) ? Loc.T(n) : name;
            // The parts of Move are named by the input system. @loc:direction "up", "down", "left", "right"
            if (b.isPartOfComposite) label += " " + Loc.C("direction", b.name);
            if (b.groups != null && b.groups.Contains("Gamepad")) label = Loc.T("{0}  (pad)", label);
            else if (b.name == "alt") label = Loc.T("{0}  (2nd key)", label);
            return label;
        }

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
            MenuKit.Text(panel, Loc.T("FIELD CONTROLS / PERSONAL BINDINGS"), "menu-record");
            MenuKit.Text(panel, Loc.T("CONTROLS"), "heading");
            MenuKit.Text(panel, Loc.T("Click a key, then press the new one. Esc cancels."), "subtitle");
            var scroll = MenuKit.Scroll(panel);

            foreach (string name in InputBindings.Rebindable)
            {
                InputAction action = editing.asset.FindAction($"Gameplay/{name}", true);
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding b = action.bindings[i];
                    if (b.isComposite) continue;
                    string label = Label(name, b);
                    VisualElement row = MenuKit.Row(scroll);
                    row.AddToClassList("setting");
                    MenuKit.Text(row, label, "setting__label");
                    int index = i;
                    Button button = MenuKit.Button(row, "", () => Listen(action, index), SoundId.UiClick, small: true);
                    button.AddToClassList("controls__binding");
                    rows.Add((action, index, button));
                }
            }
            warning = MenuKit.Text(panel, "", "text");
            warning.AddToClassList("text--error");

            VisualElement buttons = MenuKit.Row(panel);
            buttons.AddToClassList("controls__footer");
            buttons.style.flexWrap = Wrap.Wrap;
            MenuKit.Button(buttons, Loc.T("Reset to defaults"), () =>
            {
                Cancel();
                editing.asset.RemoveAllBindingOverrides();
                InputBindings.ResetAll();
                warning.text = "";
                Refresh();
            }, SoundId.UiConfirm).AddToClassList("grow");
            Button back = MenuKit.Button(buttons, Loc.T("Back"), () =>
            {
                Cancel();
                menu.Back();
            }, SoundId.UiBack);
            back.AddToClassList("grow");
            Refresh();
        }

        private void Listen(InputAction action, int index)
        {
            Cancel();
            Listening = true;
            foreach ((InputAction a, int i, Button b) in rows)
                if (a == action && i == index) b.text = Loc.T("Press a key...");
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
                    warning.text = clash != null ? Loc.T("That key is also bound to {0}.", clash) : "";
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
            {
                string shown = action.GetBindingDisplayString(index, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
                button.text = string.IsNullOrEmpty(shown) ? "-" : shown;
            }
        }
    }
}

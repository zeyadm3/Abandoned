using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The keys a lesson uses, as key caps on the lesson card with the player's own bindings: each lights up
    /// while it is held and stays marked once it has been used in this lesson, so the player sees the game
    /// answer their hands. The keys come from the lesson's text ({key:Sprint}) and its check (look, walk).
    /// </summary>
    public sealed class TutorialKeyHints
    {
        // The Controls screen's names for the actions (already in the text sheets), plus "Look" for the mouse.
        private static readonly Dictionary<string, string> Names = new()
        {
            ["Move"] = Loc.Mark("Move"), ["Look"] = Loc.Mark("Look"), ["Sprint"] = Loc.Mark("Sprint"), ["Crouch"] = Loc.Mark("Crouch"),
            ["Jump"] = Loc.Mark("Jump"), ["Interact"] = Loc.Mark("Interact / pick up"), ["Use"] = Loc.Mark("Use / hold to throw"),
            ["Drop"] = Loc.Mark("Set down / discard pocket"), ["Scan"] = Loc.Mark("Scan"), ["Flashlight"] = Loc.Mark("Flashlight"),
            ["HandSlot1"] = Loc.Mark("Pocket 1"), ["HandSlot2"] = Loc.Mark("Pocket 2"), ["Inventory"] = Loc.Mark("Inventory"),
            ["PushToTalk"] = Loc.Mark("Push to talk"), ["Radio"] = Loc.Mark("Radio"), ["Ping"] = Loc.Mark("Ping"),
        };

        private readonly List<string> actions = new();
        private readonly List<VisualElement> caps = new();
        private readonly List<InputAction> inputs = new();
        private readonly HashSet<int> used = new();
        private VisualElement row;
        private PlayerInputReader reader;

        public int Count => actions.Count;

        /// <summary>Lays out the keys for this lesson's (localized, unformatted) text, at <paramref name="index"/> in the card.</summary>
        public void Build(VisualElement parent, int index, TutorialStepDefinition step, string localizedText)
        {
            row?.RemoveFromHierarchy();
            actions.Clear();
            caps.Clear();
            inputs.Clear();
            used.Clear();
            reader = null;
            row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList("training-card__keys");
            parent.Insert(Mathf.Clamp(index, 0, parent.childCount), row);
            if (step == null) return;
            if (step.Check == TutorialCheck.Look) Add("Look");
            if (step.Check is TutorialCheck.Walk or TutorialCheck.Sprint or TutorialCheck.Crouch) Add("Move");
            int at = 0;
            string text = localizedText ?? "";
            while ((at = text.IndexOf("{key:", at, System.StringComparison.Ordinal)) >= 0)
            {
                int end = text.IndexOf('}', at);
                if (end < 0) break;
                Add(text.Substring(at + 5, end - at - 5));
                at = end + 1;
            }
            foreach (string action in actions)
            {
                var key = new VisualElement { pickingMode = PickingMode.Ignore };
                key.AddToClassList("training-key");
                UiKit.Key(key, action == "Look" ? Loc.T("Mouse") : InputBindings.Display(action));
                var label = new Label(Names.TryGetValue(action, out string name) ? Loc.T(name) : action) { pickingMode = PickingMode.Ignore };
                label.AddToClassList("training-key__label");
                key.Add(label);
                row.Add(key);
                caps.Add(key);
            }
            MenuKit.Show(row, actions.Count > 0);
        }

        private void Add(string action)
        {
            if (!string.IsNullOrEmpty(action) && !actions.Contains(action)) actions.Add(action);
        }

        /// <summary>Every frame: light what's held.</summary>
        public void Tick()
        {
            if (row == null || row.panel == null || actions.Count == 0) return;
            NetworkPlayer me = NetworkPlayer.Local;
            PlayerInputReader now = me != null ? me.GetComponent<PlayerInputReader>() : null;
            if (now != reader)
            {
                reader = now;
                inputs.Clear();
            }
            if (reader == null) return;
            if (inputs.Count != actions.Count)
            {
                inputs.Clear();
                foreach (string action in actions) inputs.Add(reader.Actions != null ? reader.Actions.FindAction("Gameplay/" + action) : null);
            }
            for (int i = 0; i < actions.Count; i++)
            {
                bool down = Pressed(i);
                if (down) used.Add(i);
                caps[i].EnableInClassList("training-key--down", down);
                caps[i].EnableInClassList("training-key--used", !down && used.Contains(i));
            }
        }

        private bool Pressed(int i)
        {
            if (CursorOwner.UiActive) return false;
            PlayerInputFrame frame = reader.Current;
            switch (actions[i])
            {
                case "Look": return frame.Look.sqrMagnitude > 0.25f;
                case "Move": return frame.Move.sqrMagnitude > 0.04f;
                default: return inputs[i] != null && inputs[i].enabled && inputs[i].IsPressed();
            }
        }
    }
}

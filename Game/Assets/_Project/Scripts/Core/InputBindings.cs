using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Core
{
    /// <summary>
    /// The player's key rebinding (GDD 20): binding overrides as JSON in PlayerPrefs, applied to every
    /// AbandonedInput instance (each input component owns one) and re-applied live when they change.
    /// </summary>
    public static class InputBindings
    {
        private const string Key = "input.bindings";

        /// <summary>Gameplay actions a player may rebind (Look is the mouse; Esc stays the menu key).</summary>
        public static readonly string[] Rebindable =
        {
            "Move", "Sprint", "Crouch", "Jump", "Interact", "Use", "Drop", "Scan", "Flashlight",
            "HandSlot1", "HandSlot2", "HandSlot3", "HandSlot4", "Inventory", "PushToTalk", "Radio", "Rotate", "GiveGear", "Chat", "PhotoCamera", "HideHud", "TogglePerspective", "Ping", "EmotePoint", "EmoteWave", "EmotePanic",
        };

        // Menu-level keys read outside the player (chat, free camera, clip mode, perspective): one always-on copy of the
        // actions with the player's bindings, so they follow rebinding like everything else (QA B-34).
        private static readonly string[] MenuKeys = { "Chat", "PhotoCamera", "HideHud", "TogglePerspective" };
        private static AbandonedInput menuKeys;

        /// <summary>This frame the player pressed a Gameplay action read outside the player (Chat, PhotoCamera, HideHud).</summary>
        public static bool WasPressed(string action)
        {
            if (menuKeys == null)
            {
                menuKeys = new AbandonedInput();
                Apply(menuKeys.asset);
                foreach (string name in MenuKeys) menuKeys.asset.FindAction($"Gameplay/{name}")?.Enable();
            }
            InputAction a = menuKeys.asset.FindAction($"Gameplay/{action}");
            return a != null && a.WasPressedThisFrame();
        }

        public static event Action Changed;

        private static AbandonedInput display;

        /// <summary>The key an action is on now, for HUD prompts ("E", or the player's own key).</summary>
        public static string Display(string action)
        {
            if (display == null)
            {
                display = new AbandonedInput();
                Apply(display.asset);
            }
            InputAction a = display.asset.FindAction($"Gameplay/{action}");
            if (a == null) return action;
            // 0.19.8: the prompt follows the device last used; pad buttons in Xbox terms.
            if (PadInput.UsingPad)
            {
                foreach (InputBinding b in a.bindings)
                    if (b.groups != null && b.groups.Contains("Gamepad") && !b.isComposite) return PadInput.Glyph(b.effectivePath);
            }
            // 0.23.2 gave every key an empty second slot for an alternate key: leave unset slots out, or the
            // prompt reads "E | ".
            var shown = new System.Collections.Generic.List<string>(2);
            for (int i = 0; i < a.bindings.Count; i++)
            {
                InputBinding b = a.bindings[i];
                if (b.isPartOfComposite || b.groups == null || !b.groups.Contains("Keyboard&Mouse")) continue;
                if (!b.isComposite && string.IsNullOrEmpty(b.effectivePath)) continue;
                string text = a.GetBindingDisplayString(i);
                if (!string.IsNullOrEmpty(text)) shown.Add(text);
            }
            return string.Join(" | ", shown);
        }

        public static string Json => Prefs.GetString(Key, string.Empty);

        /// <summary>Loads the saved overrides into an asset (call after creating an AbandonedInput).</summary>
        public static void Apply(InputActionAsset asset)
        {
            if (asset == null) return;
            InputActionMap map = asset.FindActionMap("Gameplay");
            if (map != null && !map.enabled)
            {
                for (int slot = 3; slot <= 4; slot++)
                    if (map.FindAction("HandSlot" + slot) == null)
                        map.AddAction("HandSlot" + slot, InputActionType.Button).AddBinding(new InputBinding
                        {
                            path = "<Keyboard>/" + slot,
                            groups = "Keyboard&Mouse",
                            // Saved overrides must identify the same binding in every input reader.
                            id = new Guid(slot == 3 ? "7f77e677-2a18-4e42-a948-865a5d3c43d0" : "6fc84962-9efe-4f38-872d-e450f9e9224a"),
                        });
                if (map.FindAction("TogglePerspective") == null)
                    map.AddAction("TogglePerspective", InputActionType.Button).AddBinding(new InputBinding
                    {
                        path = "<Keyboard>/f5",
                        groups = "Keyboard&Mouse",
                        id = new Guid("c3f15d9a-5e7b-4d7a-8f12-9c3a64b810f5"),
                    });
            }
            asset.RemoveAllBindingOverrides();
            string json = Json;
            if (string.IsNullOrEmpty(json)) return;
            // A damaged prefs file must not take every input reader down with it: fall back to the default keys.
            try { asset.LoadBindingOverridesFromJson(json); }
            catch (Exception e)
            {
                Debug.LogWarning($"[Input] Saved key bindings could not be read ({e.Message}); using the defaults.");
                asset.RemoveAllBindingOverrides();
                Prefs.Delete(Key);
                Prefs.Save();
            }
        }

        /// <summary>Saves an edited asset's overrides and re-applies them everywhere.</summary>
        public static void Save(InputActionAsset edited)
        {
            Prefs.SetString(Key, edited.SaveBindingOverridesAsJson());
            Prefs.Save();
            if (display != null) Apply(display.asset);
            if (menuKeys != null) Apply(menuKeys.asset);
            Changed?.Invoke();
        }

        public static void ResetAll()
        {
            Prefs.Delete(Key);
            Prefs.Save();
            if (display != null) Apply(display.asset);
            if (menuKeys != null) Apply(menuKeys.asset);
            Changed?.Invoke();
        }

        /// <summary>Another rebindable binding already on this control path, or null.</summary>
        public static string ConflictWith(InputActionAsset asset, InputAction action, int bindingIndex, string path)
        {
            foreach (string name in Rebindable)
            {
                InputAction other = asset.FindAction($"Gameplay/{name}");
                if (other == null) continue;
                for (int i = 0; i < other.bindings.Count; i++)
                {
                    if (other == action && i == bindingIndex) continue;
                    if (other.bindings[i].isComposite) continue;
                    if (string.Equals(other.bindings[i].effectivePath, path, StringComparison.OrdinalIgnoreCase))
                        return other.bindings[i].isPartOfComposite ? $"{name} {other.bindings[i].name}" : name;
                }
            }
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Changed = null;
            display?.Dispose();
            display = null;
            menuKeys?.Dispose();
            menuKeys = null;
        }
    }
}

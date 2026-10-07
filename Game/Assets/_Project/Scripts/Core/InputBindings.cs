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
            "HandSlot1", "HandSlot2", "Inventory", "PushToTalk", "Radio", "Rotate",
        };

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
            return a != null ? a.GetBindingDisplayString() : action;
        }

        public static string Json => Prefs.GetString(Key, string.Empty);

        /// <summary>Loads the saved overrides into an asset (call after creating an AbandonedInput).</summary>
        public static void Apply(InputActionAsset asset)
        {
            if (asset == null) return;
            asset.RemoveAllBindingOverrides();
            string json = Json;
            if (!string.IsNullOrEmpty(json)) asset.LoadBindingOverridesFromJson(json);
        }

        /// <summary>Saves an edited asset's overrides and re-applies them everywhere.</summary>
        public static void Save(InputActionAsset edited)
        {
            Prefs.SetString(Key, edited.SaveBindingOverridesAsJson());
            Prefs.Save();
            if (display != null) Apply(display.asset);
            Changed?.Invoke();
        }

        public static void ResetAll()
        {
            Prefs.Delete(Key);
            Prefs.Save();
            if (display != null) Apply(display.asset);
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
        }
    }
}

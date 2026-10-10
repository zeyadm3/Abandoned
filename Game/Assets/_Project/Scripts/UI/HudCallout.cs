using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// One-line HUD callouts and the full-screen blackout (0.22.2), on the HUD layer in the revamp's style, for
    /// the 0.17–0.21 features that first drew with IMGUI: flashlight charging, the drone and thermal readouts,
    /// the ghost's keys, the vent crawl. Each owner sets or clears its own line; the newest shows.
    /// </summary>
    public static class HudCallout
    {
        private static readonly List<(object owner, string text)> Lines = new();
        private static Label label;
        private static VisualElement blackout;
        private static Label blackoutText;
        private static object blackoutOwner;

        public static void Set(object owner, string text)
        {
            int at = Lines.FindIndex(l => l.owner == owner);
            if (at >= 0)
            {
                if (Lines[at].text == text) return;
                Lines[at] = (owner, text);
            }
            else Lines.Add((owner, text));
            Refresh();
        }

        public static void Clear(object owner)
        {
            if (Lines.RemoveAll(l => l.owner == owner) > 0) Refresh();
        }

        /// <summary>Everything goes black but a line of text (the vent crawl); null text lifts it.</summary>
        public static void Blackout(object owner, string text)
        {
            if (!Ensure()) return;
            if (text == null)
            {
                if (blackoutOwner == owner) blackout.style.display = DisplayStyle.None;
                return;
            }
            blackoutOwner = owner;
            blackout.style.display = DisplayStyle.Flex;
            blackoutText.text = text;
        }

        private static void Refresh()
        {
            if (!Ensure()) return;
            label.text = Lines.Count > 0 ? Lines[^1].text : "";
            label.style.display = Lines.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static bool Ensure()
        {
            if (HudLayer.Root == null) return false;
            if (label == null || label.panel == null)
            {
                label = HudLayer.Label("hud-callout");
                label.pickingMode = PickingMode.Ignore;
                label.style.display = DisplayStyle.None;
            }
            if (blackout == null || blackout.panel == null)
            {
                blackout = HudLayer.Add(new VisualElement(), "hud-blackout");
                blackout.pickingMode = PickingMode.Ignore;
                blackoutText = new Label { pickingMode = PickingMode.Ignore };
                blackoutText.AddToClassList("hud-blackout__text");
                blackout.Add(blackoutText);
                blackout.style.display = DisplayStyle.None;
            }
            return true;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Lines.Clear();
            label = null;
            blackout = null;
        }
    }
}

using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// "EXCLUSIVE EARLY ACCESS" in the bottom-right corner of every screen of the friends-and-family build
    /// (0.13.4b.EEA, user 2026-10-08): menus, HQ, jobs, travel and death screens alike, and it stays in F10
    /// clip mode so shared screenshots and clips carry it. Sits below the hand slots, never takes clicks.
    /// </summary>
    public class EarlyAccessWatermark
    {
        public const string Text = "EXCLUSIVE EARLY ACCESS"; // @loc

        private readonly Label label;

        /// <summary>Only builds whose version carries the EEA tag show it.</summary>
        public static bool Enabled => Application.version.Contains("EEA");

        public EarlyAccessWatermark(VisualElement root)
        {
            label = new Label(Core.Loc.T(Text)) { pickingMode = PickingMode.Ignore };
            label.AddToClassList("ea-watermark");
            label.AddToClassList("font-display-semi");
            root.Add(label);
        }

        /// <summary>
        /// Screens, toasts and panels are added to the same document later (some bring themselves to the
        /// front when they open), so the watermark climbs back on top whenever something lands over it.
        /// </summary>
        public void KeepOnTop()
        {
            VisualElement parent = label.parent;
            if (parent != null && parent[parent.childCount - 1] != label) label.BringToFront();
        }
    }
}

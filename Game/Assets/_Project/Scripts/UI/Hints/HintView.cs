using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>One tip at a time on the HUD (left side, under the top bar), fading out after a while.</summary>
    public class HintView
    {
        private const float Seconds = 9f;

        private readonly VisualElement box;
        private readonly Label text;
        private float until;

        public HintId? Showing { get; private set; }

        public HintView()
        {
            box = HudLayer.Add(new VisualElement(), "hud-panel", "hud-hint");
            if (box == null) return;
            Label title = MenuKit.Text(box, "TIP", "section");
            title.pickingMode = PickingMode.Ignore;
            text = MenuKit.Text(box, "", "text");
            text.pickingMode = PickingMode.Ignore;
            MenuKit.Show(box, false);
        }

        public bool Valid => box != null && box.panel != null;

        public void Show(HintId id, string message)
        {
            Showing = id;
            until = Time.time + Seconds;
            text.text = message;
            MenuKit.Show(box, true);
        }

        public void Tick()
        {
            if (Showing == null || Time.time < until) return;
            Showing = null;
            MenuKit.Show(box, false);
        }

        public void Hide()
        {
            Showing = null;
            if (box != null) MenuKit.Show(box, false);
        }

        public void Remove() => box?.RemoveFromHierarchy();
    }
}

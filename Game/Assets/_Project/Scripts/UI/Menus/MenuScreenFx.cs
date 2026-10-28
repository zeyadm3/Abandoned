using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>How one menu screen hands over to the next.</summary>
    public enum MenuTransition { StaticCut, FlickerIn, CrtOff }

    /// <summary>
    /// Screen changes in the menus: a short dip to black and a fade in of the new screen. The old film grain,
    /// scanlines, tracking band, flashes and shakes were full-screen tiled quads rewritten every frame (the
    /// scanline tile alone was ~100k quads at 1440p), which made every menu lag; they are gone.
    /// </summary>
    public class MenuScreenFx
    {
        private const float DipSeconds = 0.12f, FadeSeconds = 0.15f;

        private readonly VisualElement black;
        private VisualElement fading;
        private float dipStart = -1f, fadeStart = -1f;

        public MenuScreenFx(VisualElement root)
        {
            black = new VisualElement { pickingMode = PickingMode.Ignore };
            UiKit.FillScreen(black);
            black.style.backgroundColor = Color.black;
            black.style.display = DisplayStyle.None;
            root.Add(black);
        }

        public void Play(MenuTransition next)
        {
            dipStart = Time.unscaledTime;
            black.style.opacity = 0.7f;
            black.style.display = DisplayStyle.Flex;
        }

        public void FadeIn(VisualElement view)
        {
            if (fading != null && fading != view) fading.style.opacity = StyleKeyword.Null;
            fading = view;
            fadeStart = Time.unscaledTime;
            view.style.opacity = 0f;
        }

        public void Tick()
        {
            float now = Time.unscaledTime;
            if (dipStart >= 0f)
            {
                float t = (now - dipStart) / DipSeconds;
                if (t >= 1f)
                {
                    dipStart = -1f;
                    black.style.display = DisplayStyle.None;
                }
                else black.style.opacity = 0.7f * (1f - t);
            }
            if (fading != null)
            {
                float t = (now - fadeStart) / FadeSeconds;
                if (t >= 1f)
                {
                    fading.style.opacity = StyleKeyword.Null;
                    fading = null;
                }
                else fading.style.opacity = t;
            }
        }

        public void Detach() => black.RemoveFromHierarchy();
    }
}

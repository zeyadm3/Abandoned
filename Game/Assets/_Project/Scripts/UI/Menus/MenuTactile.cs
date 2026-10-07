using Abandoned.Audio;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Makes a menu button feel physical. Hover and keyboard/gamepad focus get the same reaction: the
    /// words jitter, a warm glow bleeds up under them, a marker line is drawn in and the label stutters
    /// with a little static. Pressing pushes it in with a quick flash and a glitch of the text.
    /// "Reduce menu effects" keeps the glow, marker and press but drops jitter, flicker, static and flash.
    /// Nothing is ever added inside the button: a Button is a text element, and one with children stops
    /// measuring its own text (0.12.2's small buttons collapsed to one letter per line).
    /// </summary>
    public static class MenuTactile
    {
        private const string Lit = "menu-button--lit", Pressed = "menu-button--pressed", Glitch = "menu-button--glitch";
        private static readonly Color GlowTint = new(1f, 0.62f, 0.3f, 0f);

        public static void Attach(Button button, bool small)
        {
            MenuEffectsConfig config = MenuEffectsConfig.Current;
            if (config.Glow != null)
            {
                button.style.backgroundImage = config.Glow;
                button.style.unityBackgroundImageTintColor = GlowTint;
            }

            bool hovered = false, focused = false;
            void Refresh(bool arriving)
            {
                bool lit = (hovered || focused) && button.enabledSelf;
                if (lit == button.ClassListContains(Lit)) return;
                button.EnableInClassList(Lit, lit);
                if (config.Glow != null) button.style.unityBackgroundImageTintColor = Tint(lit, config);
                if (lit && arriving) React(button, small, config);
            }
            button.RegisterCallback<PointerEnterEvent>(_ => { hovered = true; Refresh(true); });
            button.RegisterCallback<PointerLeaveEvent>(_ => { hovered = false; Release(button); Refresh(false); });
            button.RegisterCallback<FocusInEvent>(_ =>
            {
                focused = true;
                // The mouse already announced itself on enter; a keyboard or pad arrival gets the hover sound here.
                if (!hovered) GameAudio.PlayUi(SoundId.UiClick, 0.25f);
                Refresh(true);
            });
            button.RegisterCallback<FocusOutEvent>(_ => { focused = false; Refresh(false); });
            // Trickle-down: the button's own clickable captures the pointer before a bubbling handler would run.
            button.RegisterCallback<PointerDownEvent>(e => { if (e.button == 0) Press(button, config); }, TrickleDown.TrickleDown);
            button.RegisterCallback<PointerUpEvent>(_ => Release(button), TrickleDown.TrickleDown);
            button.RegisterCallback<NavigationSubmitEvent>(_ =>
            {
                Press(button, config);
                button.schedule.Execute(() => Release(button)).StartingIn(90);
            }, TrickleDown.TrickleDown);
        }

        private static Color Tint(bool lit, MenuEffectsConfig config) => new(GlowTint.r, GlowTint.g, GlowTint.b, lit ? config.HoverGlowOpacity : 0f);

        private static void React(Button button, bool small, MenuEffectsConfig config)
        {
            if (MenuEffectsConfig.Calm) return;
            // Jitter: a few random nudges around the hover offset, then the stylesheet takes over again.
            float baseX = small ? 0f : 6f;
            int steps = Mathf.Max(1, Mathf.RoundToInt(config.HoverJitterSeconds / 0.03f));
            for (int i = 0; i < steps; i++)
            {
                int step = i;
                button.schedule.Execute(() =>
                {
                    if (step == steps - 1) { button.style.translate = StyleKeyword.Null; return; }
                    float a = config.HoverJitter;
                    button.style.translate = new Translate(baseX + Random.Range(-a, a), Random.Range(-a, a));
                }).StartingIn(step * 30);
            }
            // A tube catching: two quick dips in brightness with a breath of static over the label.
            float dip = 1f - config.HoverFlickerDip;
            int[] at = { 0, 45, 80, 125 };
            float[] opacity = { dip, 1f, Mathf.Lerp(dip, 1f, 0.5f), -1f };
            for (int i = 0; i < at.Length; i++)
            {
                float o = opacity[i];
                button.schedule.Execute(() => button.style.opacity = o < 0f ? StyleKeyword.Null : new StyleFloat(o)).StartingIn(at[i]);
            }
            // The static is the button's own background showing snow for a moment, then its glow again.
            if (config.Grain == null || config.Glow == null) return;
            button.style.backgroundImage = config.Grain;
            button.style.unityBackgroundImageTintColor = new Color(0.8f, 0.78f, 0.7f, 0.3f);
            button.schedule.Execute(() =>
            {
                button.style.backgroundImage = config.Glow;
                button.style.unityBackgroundImageTintColor = Tint(button.ClassListContains(Lit), config);
            }).StartingIn(110);
        }

        private static void Press(Button button, MenuEffectsConfig config)
        {
            if (!button.enabledSelf) return;
            button.AddToClassList(Pressed);
            button.style.scale = new Scale(new Vector2(config.PressScale, config.PressScale));
            button.style.translate = new Translate(config.PressOffset, config.PressOffset * 0.5f);
            if (MenuEffectsConfig.Calm) return;
            MenuScreenFx.Current?.Flash(0.5f);
            button.AddToClassList(Glitch);
            button.schedule.Execute(() => button.RemoveFromClassList(Glitch)).StartingIn(70);
        }

        private static void Release(Button button)
        {
            if (!button.ClassListContains(Pressed)) return;
            button.RemoveFromClassList(Pressed);
            button.style.scale = StyleKeyword.Null;
            button.style.translate = StyleKeyword.Null;
        }

        /// <summary>The weight behind Host, Join and Quit: a full flash and the screen jolts.</summary>
        public static void Important(Button button)
        {
            MenuScreenFx fx = MenuScreenFx.Current;
            if (fx == null) return;
            fx.Flash();
            VisualElement screen = button.parent;
            while (screen != null && screen.parent != null && !screen.parent.ClassListContains("menu-root")) screen = screen.parent;
            fx.Shake(screen);
        }
    }
}

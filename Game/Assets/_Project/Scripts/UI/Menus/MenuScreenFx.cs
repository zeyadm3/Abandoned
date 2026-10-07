using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>How one menu screen hands over to the next.</summary>
    public enum MenuTransition { StaticCut, FlickerIn, CrtOff }

    /// <summary>
    /// The full-screen layers over every menu: animated film grain, scanlines, a vignette and a rare VHS
    /// tracking band, plus the screen transitions (a static cut, a flicker in from black, a CRT switching
    /// off), a white press flash and the small shake important buttons give. Everything is a handful of
    /// textured UI Toolkit quads moved a few times a second, so it costs next to nothing; "Reduce menu
    /// effects" turns every flash, flicker, roll and shake into a plain fade.
    /// </summary>
    public class MenuScreenFx
    {
        public static MenuScreenFx Current { get; private set; }

        private readonly MenuEffectsConfig config;
        private readonly VisualElement overlay, grain, scanlines, vignette, band;
        private readonly VisualElement transition, black, noise, line, flash;
        private float nextGrain, nextBand, bandStart = -1f;
        private float transitionStart = -1f, flashStart = -1f, flashPeak;
        private MenuTransition kind;
        private VisualElement shaking, fading;
        private float shakeUntil, shakeStart, shakeAmount, fadeStart = -1f;

        public MenuScreenFx(VisualElement root, MenuEffectsConfig effects)
        {
            config = effects;
            Current = this;

            overlay = Layer(root, "menu-fx");
            grain = Tiled(overlay, config.Grain, 256f, config.GrainOpacity);
            scanlines = Tiled(overlay, config.Scanlines, 4f, config.ScanlineOpacity, 8f);
            vignette = Layer(overlay, "menu-fx__vignette");
            if (config.Vignette != null) vignette.style.backgroundImage = config.Vignette;
            vignette.style.opacity = config.VignetteOpacity;
            band = new VisualElement { pickingMode = PickingMode.Ignore };
            band.AddToClassList("menu-fx__band");
            band.style.position = Position.Absolute;
            band.style.left = 0;
            band.style.right = 0;
            band.style.height = Length.Percent(7f);
            if (config.Grain != null) band.style.backgroundImage = config.Grain;
            band.style.display = DisplayStyle.None;
            overlay.Add(band);
            nextBand = Time.unscaledTime + MenuEffectsConfig.Range(config.TrackingBandInterval);

            flash = Layer(root, "menu-fx__flash");
            flash.style.backgroundColor = new Color(0.86f, 0.84f, 0.76f);
            flash.style.opacity = 0f;

            transition = Layer(root, "menu-fx__transition");
            black = Layer(transition, "menu-fx__black");
            black.style.backgroundColor = Color.black;
            noise = Tiled(transition, config.Grain, 128f, 0f);
            line = new VisualElement { pickingMode = PickingMode.Ignore };
            line.style.position = Position.Absolute;
            line.style.backgroundColor = new Color(0.86f, 0.86f, 0.8f);
            transition.Add(line);
            transition.style.display = DisplayStyle.None;
        }

        /// <summary>The grain, scanlines and vignette sit over a menu screen, never over the game itself.</summary>
        public void SetVisible(bool visible) => MenuKit.Show(overlay, visible);

        public void Play(MenuTransition next)
        {
            kind = next;
            transitionStart = Time.unscaledTime;
            transition.style.display = DisplayStyle.Flex;
            Tick();
        }

        /// <summary>Fade a freshly opened screen in, sputtering like a tube catching unless the player asked for calm.</summary>
        public void FadeIn(VisualElement view)
        {
            if (fading != null && fading != view) fading.style.opacity = StyleKeyword.Null;
            fading = view;
            fadeStart = Time.unscaledTime;
            view.style.opacity = 0f;
        }

        public void Flash(float strength = 1f)
        {
            if (MenuEffectsConfig.Calm) return;
            flashPeak = config.PressFlashOpacity * strength;
            flashStart = Time.unscaledTime;
        }

        /// <summary>A short decaying shake of one element (the menu screen) for important actions.</summary>
        public void Shake(VisualElement target)
        {
            if (MenuEffectsConfig.Calm || target == null) return;
            if (shaking != null && shaking != target) shaking.style.translate = StyleKeyword.Null;
            shaking = target;
            shakeStart = Time.unscaledTime;
            shakeUntil = shakeStart + config.ShakeSeconds;
            shakeAmount = config.ImportantShake;
        }

        public void Tick()
        {
            float now = Time.unscaledTime;
            bool calm = MenuEffectsConfig.Calm;
            if (overlay.resolvedStyle.display != DisplayStyle.None && now >= nextGrain)
            {
                // Grain crawls by jumping its tile offset a few times a second (still while calm).
                nextGrain = now + 1f / (calm ? 4f : config.GrainFps);
                Jump(grain, 256f);
            }
            TickBand(now, calm);
            TickTransition(now, calm);
            TickFlash(now);
            TickShake(now);
            TickFade(now, calm);
        }

        private void TickBand(float now, bool calm)
        {
            if (bandStart < 0f && now >= nextBand && !calm && overlay.resolvedStyle.display != DisplayStyle.None)
            {
                bandStart = now;
                band.style.display = DisplayStyle.Flex;
            }
            if (bandStart < 0f) return;
            float t = (now - bandStart) / 1.4f;
            if (t >= 1f || calm)
            {
                bandStart = -1f;
                band.style.display = DisplayStyle.None;
                nextBand = now + MenuEffectsConfig.Range(config.TrackingBandInterval);
                return;
            }
            band.style.top = Length.Percent(Mathf.Lerp(-8f, 104f, t));
            band.style.opacity = config.TrackingBandOpacity * Mathf.Sin(t * Mathf.PI);
            Jump(band, 256f);
        }

        private void TickTransition(float now, bool calm)
        {
            if (transitionStart < 0f) return;
            float length = calm ? 0.14f : config.TransitionSeconds * (kind == MenuTransition.CrtOff ? 1.6f : 1f);
            float t = (now - transitionStart) / Mathf.Max(0.01f, length);
            if (t >= 1f)
            {
                transitionStart = -1f;
                transition.style.display = DisplayStyle.None;
                return;
            }
            line.style.display = DisplayStyle.None;
            noise.style.opacity = 0f;
            if (calm)
            {
                black.style.opacity = 0.6f * (1f - t);
                return;
            }
            switch (kind)
            {
                case MenuTransition.StaticCut:
                    // A hard cut to snow that clears as the new screen settles.
                    black.style.opacity = t < 0.35f ? 0.85f : 0.85f * (1f - (t - 0.35f) / 0.65f);
                    noise.style.opacity = config.TransitionStaticOpacity * (1f - t);
                    Jump(noise, 128f);
                    break;
                case MenuTransition.FlickerIn:
                    // Black, a stutter of light, black again, then the screen.
                    black.style.opacity = t < 0.25f ? 1f : t < 0.4f ? 0.25f : t < 0.55f ? 0.9f : 1f - (t - 0.55f) / 0.45f;
                    noise.style.opacity = t < 0.5f ? config.TransitionStaticOpacity * 0.4f : 0f;
                    Jump(noise, 128f);
                    break;
                case MenuTransition.CrtOff:
                    // The picture collapses to a bright line, the line to a dot, then the game comes back.
                    float collapse = Mathf.Clamp01(t / 0.45f), shrink = Mathf.Clamp01((t - 0.45f) / 0.3f);
                    black.style.opacity = t < 0.75f ? Mathf.Lerp(0.2f, 1f, collapse) : 1f - (t - 0.75f) / 0.25f;
                    line.style.display = t < 0.75f ? DisplayStyle.Flex : DisplayStyle.None;
                    float height = Mathf.Lerp(100f, 0.4f, collapse), width = Mathf.Lerp(100f, 0.5f, shrink);
                    line.style.height = Length.Percent(height);
                    line.style.width = Length.Percent(width);
                    line.style.top = Length.Percent((100f - height) * 0.5f);
                    line.style.left = Length.Percent((100f - width) * 0.5f);
                    line.style.opacity = Mathf.Lerp(0.15f, 0.9f, collapse);
                    break;
            }
        }

        private void TickFlash(float now)
        {
            if (flashStart < 0f) return;
            float t = (now - flashStart) / 0.14f;
            flash.style.opacity = t >= 1f ? 0f : flashPeak * (1f - t);
            if (t >= 1f) flashStart = -1f;
        }

        private void TickShake(float now)
        {
            if (shaking == null) return;
            if (now >= shakeUntil)
            {
                shaking.style.translate = StyleKeyword.Null;
                shaking = null;
                return;
            }
            float left = (shakeUntil - now) / Mathf.Max(0.01f, shakeUntil - shakeStart);
            float a = shakeAmount * left;
            shaking.style.translate = new Translate(Random.Range(-a, a), Random.Range(-a, a) * 0.6f);
        }

        private void TickFade(float now, bool calm)
        {
            if (fading == null) return;
            float t = (now - fadeStart) / Mathf.Max(0.01f, config.PauseFadeSeconds);
            if (t >= 1f)
            {
                fading.style.opacity = StyleKeyword.Null;
                fading = null;
                return;
            }
            float opacity = t;
            if (!calm && t < 0.6f) opacity *= Mathf.PerlinNoise(now * 31f, 0.3f) > 0.45f ? 1f : 0.25f;
            fading.style.opacity = opacity;
        }

        private static void Jump(VisualElement e, float tile)
        {
            e.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Left, Random.Range(0f, tile));
            e.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Top, Random.Range(0f, tile));
        }

        private static VisualElement Layer(VisualElement parent, string cls)
        {
            var layer = new VisualElement { pickingMode = PickingMode.Ignore };
            UiKit.FillScreen(layer);
            layer.AddToClassList(cls);
            parent.Add(layer);
            return layer;
        }

        private static VisualElement Tiled(VisualElement parent, Texture2D texture, float width, float opacity, float height = -1f)
        {
            VisualElement layer = Layer(parent, "menu-fx__tiled");
            if (texture != null) layer.style.backgroundImage = texture;
            layer.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
            layer.style.backgroundSize = new BackgroundSize(new Length(width), new Length(height > 0f ? height : width));
            layer.style.opacity = opacity;
            return layer;
        }

        public void Detach()
        {
            if (Current == this) Current = null;
        }
    }
}

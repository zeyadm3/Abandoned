using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Blood on the lens for a badly hurt player (PlayerHealthEffects.Trauma), under every other HUD element.
    /// From 20 HP: one still layer of splatter in the screen corners. From 10 HP, on top of it: a thin smeared
    /// film reaching further in, heavy splatter round the whole edge that swells with the heartbeat, and drips
    /// that slowly run down. The pictures come from <see cref="BloodSplatter"/>.
    /// </summary>
    public sealed class TraumaOverlay
    {
        private readonly VisualElement root, corners, film, heavy, drips;

        private TraumaOverlay(VisualElement hud)
        {
            root = new VisualElement { pickingMode = PickingMode.Ignore };
            UiKit.FillScreen(root);
            root.style.overflow = Overflow.Hidden;
            hud.Insert(0, root);
            corners = Layer(BloodSplatter.Corners);
            film = Layer(BloodSplatter.Film);
            heavy = Layer(BloodSplatter.Heavy);
            drips = Layer(BloodSplatter.Drips);
            // The drips lengthen downward from the top of the screen.
            drips.style.transformOrigin = new TransformOrigin(Length.Percent(50f), Length.Percent(0f));
            Hide();
        }

        /// <summary>Null when there's no HUD (bare test rigs).</summary>
        public static TraumaOverlay Create() => HudLayer.Root != null ? new TraumaOverlay(HudLayer.Root) : null;

        /// <summary>False once the HUD it was built on has been torn down (the menu document rebuilt).</summary>
        public bool Valid => root.panel != null;

        private VisualElement Layer(Texture2D picture)
        {
            var layer = new VisualElement { pickingMode = PickingMode.Ignore };
            UiKit.FillScreen(layer);
            layer.style.backgroundImage = new StyleBackground(picture);
            layer.style.backgroundSize = new BackgroundSize(Length.Percent(100f), Length.Percent(100f));
            root.Add(layer);
            return layer;
        }

        /// <param name="cornerOpacity">The corner splatter (severe trauma and below).</param>
        /// <param name="heavyOpacity">The film, the heavy splatter and the drips (near death).</param>
        /// <param name="dripRun">How much further the drips have run, as a fraction of the screen's height.</param>
        /// <param name="beat">The heartbeat envelope, 0..1; the heavy layer swells with it.</param>
        public void Set(float cornerOpacity, float heavyOpacity, float dripRun, float beat)
        {
            root.style.display = DisplayStyle.Flex;
            Fade(corners, cornerOpacity);
            Fade(film, heavyOpacity * 0.8f);
            Fade(heavy, heavyOpacity);
            Fade(drips, heavyOpacity);
            drips.style.scale = new Scale(new Vector3(1f, 1f + dripRun, 1f));
            float swell = 1f + 0.012f * beat;
            heavy.style.scale = new Scale(new Vector3(swell, swell, 1f));
        }

        private static void Fade(VisualElement layer, float opacity)
        {
            bool shown = opacity > 0.004f;
            layer.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            if (shown) layer.style.opacity = Mathf.Clamp01(opacity);
        }

        public void Hide() => root.style.display = DisplayStyle.None;

        public void Remove() => root.RemoveFromHierarchy();
    }
}

using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The worn ABANDONED logo: stencil letters over faint red and cyan ghosts (a tired CRT's fringing),
    /// with scratches and a water stain laid over the paint. Now and then the signal slips: the layers
    /// tear sideways for a couple of frames and the logo dims. "Reduce menu effects" holds it still.
    /// </summary>
    public class MenuTitle
    {
        private readonly MenuEffectsConfig config;
        private readonly Label face, red, cyan;
        private readonly VisualElement wear;
        private float nextGlitch, glitchUntil = -1f;

        public VisualElement Root { get; }

        public MenuTitle(VisualElement parent, string text, Font font, MenuEffectsConfig effects)
        {
            config = effects;
            Root = new VisualElement { pickingMode = PickingMode.Ignore };
            Root.AddToClassList("menu-title");
            parent.Add(Root);
            red = Layer("menu-title__ghost", new Color(0.75f, 0.12f, 0.08f, config.ChromaticOpacity));
            cyan = Layer("menu-title__ghost", new Color(0.2f, 0.62f, 0.66f, config.ChromaticOpacity * 0.8f));
            face = Layer("menu-title__face", default);
            face.style.position = Position.Relative;
            wear = new VisualElement { pickingMode = PickingMode.Ignore };
            wear.AddToClassList("menu-title__wear");
            if (config.Scratches != null) wear.style.backgroundImage = config.Scratches;
            wear.style.opacity = config.TitleWearOpacity;
            Root.Add(wear);
            var stain = new VisualElement { pickingMode = PickingMode.Ignore };
            stain.AddToClassList("menu-title__stain");
            if (config.Stain != null) stain.style.backgroundImage = config.Stain;
            Root.Add(stain);
            Settle();
            nextGlitch = Time.unscaledTime + MenuEffectsConfig.Range(config.TitleGlitchInterval);

            Label Layer(string cls, Color tint)
            {
                var label = new Label(text) { pickingMode = PickingMode.Ignore };
                label.AddToClassList("title");
                label.AddToClassList("main__logo");
                label.AddToClassList(cls);
                if (font != null) label.style.unityFontDefinition = FontDefinition.FromFont(font);
                if (tint.a > 0f) label.style.color = tint;
                Root.Add(label);
                return label;
            }
        }

        public void Tick()
        {
            float now = Time.unscaledTime;
            if (MenuEffectsConfig.Calm)
            {
                if (glitchUntil >= 0f) { glitchUntil = -1f; Settle(); }
                return;
            }
            if (glitchUntil < 0f && now >= nextGlitch)
            {
                glitchUntil = now + Random.Range(0.06f, 0.16f);
                nextGlitch = now + MenuEffectsConfig.Range(config.TitleGlitchInterval);
            }
            if (glitchUntil < 0f)
            {
                // Between slips the logo breathes faintly, like a sign on a weak supply.
                face.style.opacity = 0.93f + Mathf.PerlinNoise(now * 1.7f, 0.4f) * 0.07f;
                return;
            }
            if (now >= glitchUntil)
            {
                glitchUntil = -1f;
                Settle();
                return;
            }
            float shift = config.TitleGlitchShift;
            red.style.translate = new Translate(-config.ChromaticOffset - Random.Range(0f, shift), Random.Range(-1f, 1f));
            cyan.style.translate = new Translate(config.ChromaticOffset + Random.Range(0f, shift), Random.Range(-1f, 1f));
            face.style.translate = new Translate(Random.Range(-shift, shift) * 0.4f, 0f);
            face.style.opacity = Random.value < 0.5f ? 0.55f : 0.9f;
        }

        private void Settle()
        {
            red.style.translate = new Translate(-config.ChromaticOffset, 0f);
            cyan.style.translate = new Translate(config.ChromaticOffset, 0f);
            face.style.translate = StyleKeyword.Null;
            face.style.opacity = StyleKeyword.Null;
        }
    }
}

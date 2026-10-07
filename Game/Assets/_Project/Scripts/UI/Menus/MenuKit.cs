using System;
using Abandoned.Audio;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>Small builders for the menus' UI Toolkit elements (styles in Art/UI/Menu.uss).</summary>
    public static class MenuKit
    {
        public static VisualElement Panel(VisualElement parent, bool wide = false)
        {
            var panel = new VisualElement();
            panel.AddToClassList("panel");
            if (wide) panel.AddToClassList("panel--wide");
            parent.Add(panel);
            return panel;
        }

        public static Label Text(VisualElement parent, string text, string cls = "text")
        {
            var label = new Label(text);
            label.AddToClassList(cls);
            parent.Add(label);
            return label;
        }

        public static Button Button(VisualElement parent, string text, Action onClick, SoundId sound = SoundId.UiClick, bool small = false)
        {
            var button = new Button(() =>
            {
                GameAudio.PlayUi(sound);
                onClick();
            }) { text = text };
            button.AddToClassList("menu-button");
            if (small) button.AddToClassList("menu-button--small");
            else
            {
                // Big buttons: an arrow that shows on hover, a tick and a little jolt as the pointer lands.
                var arrow = new Label("\u25B6") { pickingMode = PickingMode.Ignore };
                arrow.AddToClassList("menu-button__arrow");
                button.Add(arrow);
            }
            button.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (!button.enabledSelf) return;
                GameAudio.PlayUi(SoundId.UiClick, 0.25f);
                if (!small) UiKit.Jolt(button);
            });
            parent.Add(button);
            return button;
        }

        public static VisualElement Row(VisualElement parent)
        {
            var row = new VisualElement();
            row.AddToClassList("row");
            parent.Add(row);
            return row;
        }

        /// <summary>A labelled 0..100% slider; <paramref name="commit"/> runs as it moves.</summary>
        public static Slider Percent(VisualElement parent, string label, float value, Action<float> commit)
        {
            VisualElement row = Setting(parent, label, out Label shown);
            var slider = new Slider(0f, 1f) { value = value };
            slider.AddToClassList("setting__control");
            shown.text = $"{value * 100f:0}%";
            slider.RegisterValueChangedCallback(e =>
            {
                shown.text = $"{e.newValue * 100f:0}%";
                commit(e.newValue);
            });
            row.Insert(1, slider);
            return slider;
        }

        /// <summary>A labelled slider over a range, showing the value with a format.</summary>
        public static Slider Range(VisualElement parent, string label, float min, float max, float value, string format, Action<float> commit)
        {
            VisualElement row = Setting(parent, label, out Label shown);
            var slider = new Slider(min, max) { value = value };
            slider.AddToClassList("setting__control");
            shown.text = value.ToString(format);
            slider.RegisterValueChangedCallback(e =>
            {
                shown.text = e.newValue.ToString(format);
                commit(e.newValue);
            });
            row.Insert(1, slider);
            return slider;
        }

        /// <summary>A labelled on/off switch drawn as a small button pair.</summary>
        public static void Switch(VisualElement parent, string label, bool value, string on, string off, Action<bool> commit)
        {
            VisualElement row = Setting(parent, label, out Label shown);
            shown.RemoveFromHierarchy();
            Button onButton = null, offButton = null;
            void Show(bool v)
            {
                onButton.EnableInClassList("menu-button--on", v);
                offButton.EnableInClassList("menu-button--on", !v);
            }
            onButton = Button(row, on, () => { Show(true); commit(true); }, SoundId.UiClick, small: true);
            offButton = Button(row, off, () => { Show(false); commit(false); }, SoundId.UiClick, small: true);
            Show(value);
        }

        /// <summary>A labelled choice cycled with arrows (◀ value ▶), R.E.P.O.-style; <paramref name="commit"/> gets the new index.</summary>
        public static void Choice(VisualElement parent, string label, string[] options, int index, Action<int> commit)
        {
            VisualElement row = Setting(parent, label, out Label shown);
            shown.RemoveFromHierarchy();
            if (options == null || options.Length == 0) return;
            int current = Mathf.Clamp(index, 0, options.Length - 1);
            var value = new Label(options[current]);
            value.AddToClassList("choice__value");
            void Step(int by)
            {
                current = (current + by + options.Length) % options.Length;
                value.text = options[current];
                commit(current);
            }
            Button(row, "\u25C0", () => Step(-1), SoundId.UiClick, small: true).AddToClassList("choice__arrow");
            row.Add(value);
            Button(row, "\u25B6", () => Step(1), SoundId.UiClick, small: true).AddToClassList("choice__arrow");
        }

        public static void Toggle(VisualElement parent, string label, bool value, Action<bool> commit) =>
            Switch(parent, label, value, "On", "Off", commit);

        private static VisualElement Setting(VisualElement parent, string label, out Label value)
        {
            var row = new VisualElement();
            row.AddToClassList("setting");
            var name = new Label(label);
            name.AddToClassList("setting__label");
            row.Add(name);
            value = new Label();
            value.AddToClassList("setting__value");
            row.Add(value);
            parent.Add(row);
            return row;
        }

        public static void Show(VisualElement element, bool visible) =>
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}

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
            panel.style.flexDirection = FlexDirection.Column;
            panel.style.width = wide ? 960f : 660f;
            panel.style.height = Length.Percent(86f);
            panel.style.flexShrink = 1;
            panel.style.minWidth = 0;
            panel.style.minHeight = 0;
            panel.style.maxWidth = Length.Percent(92f);
            panel.style.maxHeight = Length.Percent(92f);
            panel.AddToClassList("panel");
            if (wide) panel.AddToClassList("panel--wide");
            parent.Add(panel);
            return panel;
        }

        public static Label Text(VisualElement parent, string text, string cls = "text")
        {
            var label = new Label(text);
            label.style.minWidth = 0;
            label.style.flexShrink = 0;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.AddToClassList(cls);
            parent.Add(label);
            return label;
        }

        /// <param name="important">Host, Join, Quit: the press flashes the screen and jolts it.</param>
        public static Button Button(VisualElement parent, string text, Action onClick, SoundId sound = SoundId.UiClick, bool small = false,
            bool important = false)
        {
            Button button = null;
            button = new Button(() =>
            {
                GameAudio.PlayUi(sound);
                if (important) MenuTactile.Important(button);
                onClick();
            }) { text = text };
            button.AddToClassList("menu-button");
            button.style.minHeight = small ? 36f : 46f;
            button.style.height = StyleKeyword.Auto;
            button.style.flexShrink = 0;
            button.style.minWidth = 0;
            button.style.whiteSpace = WhiteSpace.Normal;
            if (small) button.AddToClassList("menu-button--small");
            button.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (button.enabledSelf) GameAudio.PlayUi(SoundId.UiClick, 0.25f);
            });
            MenuTactile.Attach(button, small);
            parent.Add(button);
            return button;
        }

        public static VisualElement Row(VisualElement parent)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.flexShrink = 0;
            row.style.minWidth = 0;
            row.AddToClassList("row");
            parent.Add(row);
            return row;
        }

        public static ScrollView Scroll(VisualElement parent, params string[] classes)
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Auto
            };
            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = 0;
            scroll.style.minWidth = 0;
            scroll.contentContainer.style.flexDirection = FlexDirection.Column;
            scroll.contentContainer.style.flexShrink = 0;
            scroll.contentContainer.style.minWidth = 0;
            scroll.AddToClassList("scroll");
            foreach (string cls in classes) scroll.AddToClassList(cls);
            parent?.Add(scroll);
            return scroll;
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
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.minHeight = 48f;
            row.style.flexShrink = 0;
            row.style.minWidth = 0;
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

        public static void Show(VisualElement element, bool visible)
        {
            if (element != null) element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}

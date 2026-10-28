using Abandoned.Core;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// A centred, clickable panel just above the HUD layer for screens the world opens (contract board,
    /// shop, gear rack, appraisal). Contents are rebuilt only when the screen's state key changes.
    /// </summary>
    public sealed class ScreenPanel
    {
        private string builtFor;

        public VisualElement Root { get; }
        public VisualElement Panel { get; }

        private ScreenPanel(VisualElement root, VisualElement panel)
        {
            Root = root;
            Panel = panel;
            Panel.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent changed)
        {
            // Station panels have different widths from menus, so use their own measured space.
            Panel.EnableInClassList("archive-panel--compact", changed.newRect.width < 1000f);
            Panel.EnableInClassList("archive-panel--narrow", changed.newRect.width < 680f);
            Panel.EnableInClassList("archive-panel--short", changed.newRect.height < 640f);
        }

        /// <summary>Null when there's no HUD (bare test rigs).</summary>
        public static ScreenPanel Create(bool wide)
        {
            VisualElement hud = HudLayer.Root;
            if (hud == null || hud.parent == null) return null;
            // Beside the HUD layer, not in it: clip mode (F10) hides the HUD, but an open board must stay
            // visible. Still under the menus, which come later in the document.
            var root = new VisualElement();
            UiKit.FillScreen(root);
            root.style.flexDirection = FlexDirection.Column;
            root.style.alignItems = Align.Center;
            root.style.justifyContent = Justify.Center;
            root.style.display = DisplayStyle.None;
            foreach (string c in new[] { "backdrop", "backdrop--dim", "backdrop--center", "archive-backdrop" }) root.AddToClassList(c);
            hud.parent.Insert(hud.parent.IndexOf(hud) + 1, root);
            root.pickingMode = PickingMode.Position; // the panel's buttons take the clicks
            var panel = MenuKit.Panel(root, wide);
            panel.AddToClassList("archive-panel");
            var screen = new ScreenPanel(root, panel);
            return screen;
        }

        public void Show(bool visible)
        {
            MenuKit.Show(Root, visible);
            if (!visible) builtFor = null; // rebuilt fresh next time it opens
        }

        /// <summary>Rebuilds the contents when <paramref name="key"/> differs from the last build.</summary>
        public void Build(string key, Action<VisualElement> build)
        {
            // An open screen is worded when it is built, so a new language is a new build too.
            key = Languages.Version + "|" + key;
            if (key == builtFor) return;
            builtFor = key;
            Panel.Clear();
            ScrollView scroll = MenuKit.Scroll(Panel, "screen-panel__scroll");
            scroll.contentContainer.style.width = Length.Percent(100f);
            build(scroll.contentContainer);
            foreach (VisualElement sheet in scroll.Query<VisualElement>(className: "sheet").ToList()) sheet.style.rotate = new Rotate(0f);
        }

        public void Remove()
        {
            Panel.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Root.RemoveFromHierarchy();
        }
    }
}

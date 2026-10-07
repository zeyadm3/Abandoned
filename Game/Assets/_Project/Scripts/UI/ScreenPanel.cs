using System;
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
        }

        /// <summary>Null when there's no HUD (bare test rigs).</summary>
        public static ScreenPanel Create(bool wide)
        {
            VisualElement hud = HudLayer.Root;
            if (hud == null || hud.parent == null) return null;
            // Beside the HUD layer, not in it: clip mode (F10) hides the HUD, but an open board must stay
            // visible. Still under the menus, which come later in the document.
            var root = new VisualElement();
            foreach (string c in new[] { "backdrop", "backdrop--dim", "backdrop--center" }) root.AddToClassList(c);
            hud.parent.Insert(hud.parent.IndexOf(hud) + 1, root);
            root.pickingMode = PickingMode.Position; // the panel's buttons take the clicks
            return new ScreenPanel(root, MenuKit.Panel(root, wide));
        }

        public void Show(bool visible)
        {
            MenuKit.Show(Root, visible);
            if (!visible) builtFor = null; // rebuilt fresh next time it opens
        }

        /// <summary>Rebuilds the contents when <paramref name="key"/> differs from the last build.</summary>
        public void Build(string key, Action<VisualElement> build)
        {
            if (key == builtFor) return;
            builtFor = key;
            Panel.Clear();
            build(Panel);
        }

        public void Remove() => Root.RemoveFromHierarchy();
    }
}

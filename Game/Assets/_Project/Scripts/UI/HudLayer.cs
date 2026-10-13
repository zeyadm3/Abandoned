using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The in-game HUD's place on the menu document (M8.1): a full-screen layer under the menus that
    /// ignores the mouse and hides while a menu is open. HUD components add their elements here; with
    /// no menus (bare test rigs) there's no layer and they simply draw nothing.
    /// </summary>
    public static class HudLayer
    {
        private static VisualElement root;

        public static VisualElement Root => root != null && root.panel != null ? root : null;

        internal static void Attach(VisualElement layer)
        {
            root = layer;
            UiKit.FillScreen(root);
            world = null;
            root.AddToClassList("horror-hud");
        }

        /// <summary>A new element on the HUD (or null when there's no HUD).</summary>
        public static T Add<T>(T element, params string[] classes) where T : VisualElement
        {
            if (Root == null) return null;
            element.pickingMode = PickingMode.Ignore;
            foreach (string c in classes) element.AddToClassList(c);
            Root.Add(element);
            return element;
        }

        public static Label Label(params string[] classes) => Add(new Label(), classes);

        /// <summary>
        /// Empties the HUD (0.24.8, the language changed). Every piece already builds itself again when it
        /// finds its elements gone, so they all come back worded in the new language.
        /// </summary>
        internal static void Clear()
        {
            if (root == null) return;
            root.Clear();
            world = null;
        }

        /// <summary>The visible border box in an ancestor's coordinates, after layout has settled.</summary>
        public static bool TryLocalRect(VisualElement element, VisualElement ancestor, out Rect rect)
        {
            rect = default;
            if (element == null || ancestor == null || element.panel == null || element.panel != ancestor.panel) return false;
            VisualElement parent = element;
            while (parent != null)
            {
                if (parent.resolvedStyle.display == DisplayStyle.None) return false;
                if (parent == ancestor) break;
                parent = parent.parent;
            }
            if (parent != ancestor) return false;
            rect = element.ChangeCoordinatesTo(ancestor, new Rect(Vector2.zero, element.layout.size));
            return Finite(rect.x) && Finite(rect.y) && Finite(rect.width) && Finite(rect.height) &&
                   rect.width > 0f && rect.height > 0f;
        }

        public static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        /// <summary>HQ notes follow the ledger's actual height, including wrapped assignment text.</summary>
        public static void PlaceFieldNote(VisualElement note)
        {
            VisualElement hud = Root;
            if (note == null || hud == null) return;
            float top = 110f;
            if (TryLocalRect(hud.Q(className: "hq-brief"), hud, out Rect ledger))
                top = Mathf.Max(top, ledger.yMax + 12f);
            note.style.top = top;
        }

        /// <summary>Align central records to the space between the measured corner instruments.</summary>
        public static void FitCenterLane(VisualElement element, VisualElement surface, float maxWidth = 640f)
        {
            if (element == null || surface == null || !Finite(surface.layout.width) || surface.layout.width <= 0f) return;
            float width = surface.layout.width;
            float left = 28f, right = width - 28f;
            VisualElement hud = Root;
            if (hud != null && hud.panel == surface.panel)
            {
                if (TryLocalRect(hud.Q(className: "h-vitals"), hud, out Rect vitals))
                    left = Mathf.Max(left, hud.ChangeCoordinatesTo(surface, vitals).xMax + 16f);
                if (TryLocalRect(hud.Q(className: "h-inventory"), hud, out Rect inventory))
                    right = Mathf.Min(right, hud.ChangeCoordinatesTo(surface, inventory).xMin - 16f);
            }
            float available = Mathf.Max(0f, right - left);
            float inset = Mathf.Max(0f, (available - maxWidth) * 0.5f);
            element.style.left = left + inset;
            element.style.right = width - right + inset;
            element.style.minWidth = 0f;
        }

        private static VisualElement world;

        /// <summary>A layer under the rest of the HUD for world-anchored marks (value tags), so they never cover prompts.</summary>
        public static VisualElement World
        {
            get
            {
                if (Root == null) return null;
                if (world == null || world.parent != Root)
                {
                    world = new VisualElement { pickingMode = PickingMode.Ignore };
                    UiKit.FillScreen(world);
                    world.AddToClassList("hud-layer");
                    Root.Insert(0, world);
                }
                return world;
            }
        }

        /// <summary>Puts a marker over a world point; hides it when the point is behind the camera.</summary>
        public static void Place(VisualElement marker, Vector3 world, Camera camera)
        {
            if (marker == null) return;
            Vector3 view = camera != null ? camera.WorldToViewportPoint(world) : Vector3.back;
            bool visible = view.z > 0f;
            marker.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible || marker.panel == null) return;
            // From the viewport, so it holds whatever size the panel is drawn at (screen, UI scale, a capture).
            Rect area = marker.panel.visualTree.layout;
            marker.style.left = view.x * area.width;
            marker.style.top = (1f - view.y) * area.height;
        }

        public static void Remove(VisualElement element) => element?.RemoveFromHierarchy();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => root = world = null;
    }
}

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

        internal static void Attach(VisualElement layer) => root = layer;

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

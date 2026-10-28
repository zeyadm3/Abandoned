using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Which uGUI graphics must keep receiving raycasts. Shared by the triage audit (which reports the rest) and the UI
    /// fixer (which turns the rest off), so the two can never disagree about what is "interactive".
    /// </summary>
    public static class UiRaycastRules
    {
        /// <summary>Graphics another component relies on for hits: Selectable targets, ScrollRect viewports and content.</summary>
        public static HashSet<Graphic> Needed(IEnumerable<GameObject> objects)
        {
            var needed = new HashSet<Graphic>();
            foreach (GameObject go in objects)
            {
                foreach (Selectable s in go.GetComponents<Selectable>())
                    if (s.targetGraphic != null) needed.Add(s.targetGraphic);
                foreach (ScrollRect scroll in go.GetComponents<ScrollRect>())
                {
                    // A scroll view only drags where something under the pointer is a raycast target.
                    if (scroll.viewport != null && scroll.viewport.TryGetComponent(out Graphic viewport)) needed.Add(viewport);
                    if (scroll.content != null && scroll.content.TryGetComponent(out Graphic content)) needed.Add(content);
                }
            }
            return needed;
        }

        /// <summary>True when the graphic's own object takes pointer input, or another component needs it to.</summary>
        public static bool IsInteractive(Graphic graphic, HashSet<Graphic> needed)
        {
            if (needed.Contains(graphic)) return true;
            GameObject go = graphic.gameObject;
            if (go.GetComponent<Selectable>() != null || go.GetComponent<ScrollRect>() != null || go.GetComponent<EventTrigger>() != null)
                return true;
            foreach (MonoBehaviour behaviour in go.GetComponents<MonoBehaviour>())
                if (behaviour is IPointerClickHandler or IPointerDownHandler or IPointerUpHandler or IPointerEnterHandler
                    or IPointerExitHandler or IBeginDragHandler or IDragHandler or IEndDragHandler or IScrollHandler or IDropHandler)
                    return true;
            return false;
        }

        /// <summary>A graphic stretched over most of its canvas: left as a raycast target it swallows every click beneath it.</summary>
        public static bool CoversScreen(Graphic graphic)
        {
            RectTransform rect = graphic.rectTransform;
            return rect.anchorMin.x <= 0.05f && rect.anchorMin.y <= 0.05f && rect.anchorMax.x >= 0.95f && rect.anchorMax.y >= 0.95f;
        }
    }
}

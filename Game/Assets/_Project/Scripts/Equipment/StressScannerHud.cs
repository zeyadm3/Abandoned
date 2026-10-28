using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Equipment
{
    /// <summary>
    /// The stress scanner in hand (GDD 6.3 tools): the floor section the player looks at (or stands on),
    /// its health and load against capacity, colour-coded. Reads the replicated structure, so it works
    /// on every machine. Owner only.
    /// </summary>
    public class StressScannerHud : MonoBehaviour
    {
        [SerializeField] private PlayerEquipment equipment;
        [SerializeField] private Transform eye;
        [SerializeField] private float range = 8f;

        private VisualElement panel, condition;
        private VisualElement layoutHud, lower;
        private Label status, load;
        private int mask;

        public StructuralSection Target { get; private set; }

        private void Update()
        {
            Target = null;
            if (equipment == null || !equipment.IsOwner || equipment.InHand == null || equipment.InHand.Kind != EquipmentKind.StressScanner) return;
            if (mask == 0) mask = LayerMask.GetMask(Core.GameLayers.Structure);
            Vector3 from = eye != null ? eye.position : transform.position + Vector3.up * 1.6f;
            Vector3 dir = eye != null ? eye.forward : transform.forward;
            if (Physics.Raycast(from, dir, out RaycastHit hit, range, mask, QueryTriggerInteraction.Ignore) ||
                Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, 2f, mask, QueryTriggerInteraction.Ignore))
                Target = hit.collider.GetComponentInParent<StructuralSection>();
        }

        private void LateUpdate()
        {
            if (Target == null)
            {
                if (panel != null) UI.MenuKit.Show(panel, false);
                ReleaseLayout();
                return;
            }
            if (panel == null || panel.panel == null)
            {
                UI.HudLayer.Remove(panel);
                panel = UI.HudLayer.Add(new VisualElement(), "hud-panel", "hud-scanner", "instrument-hud", "instrument-hud--structure", "instrument-hud--stress");
                if (panel == null) return;
                UI.MenuKit.Text(panel, Loc.T("STRUCTURE SCAN"), "instrument-hud__title");
                UI.MenuKit.Text(panel, Loc.T("SURFACE / LOAD DIAGNOSTIC"), "instrument-hud__meta");
                status = UI.MenuKit.Text(panel, "", "instrument-hud__readout");
                condition = UI.UiKit.Segments(panel, 12, "instrument-hud__signal");
                load = UI.MenuKit.Text(panel, "", "instrument-hud__count");
            }
            float health = Target.HealthFraction;
            // Colourblind-safe: blue / orange / vermilion (Okabe-Ito) instead of green / yellow / red.
            string color = Core.GameSettings.ColorblindScanner
                ? health > 0.6f ? "#56b4e9" : health > 0.3f ? "#e69f00" : "#d55e00"
                : health > 0.6f ? "#7dff7d" : health > 0.3f ? "#ffd24d" : "#ff5544";
            string weight = Target.Capacity > 0f ? Loc.T("{0:0} / {1:0} KG", Target.Load, Target.Capacity) : "-";
            // The stage as the enum names it. @loc:stage "Stable", "Stressed", "Cracking", "Failing", "Collapsed"
            string text = $"{Loc.C("stage", Target.Stage.ToString()).ToUpperInvariant()} / {health:P0}";
            UI.MenuKit.Show(panel, true);
            if (status.text != text) status.text = text;
            load.text = Loc.T("LOAD / {0}", weight);
            if (ColorUtility.TryParseHtmlString(color, out Color tint))
            {
                status.style.color = tint;
                UI.UiKit.SetSegments(condition, health, tint);
            }
            Position();
        }

        private void Position()
        {
            VisualElement hud = UI.HudLayer.Root;
            if (Target == null || panel == null || !UI.HudLayer.TryLocalRect(hud, hud, out Rect area) || area.height <= 36f) return;
            VisualElement anchor = hud.Q(className: "h-inventory");
            bool found = UI.HudLayer.TryLocalRect(anchor, hud, out Rect bounds);
            VisualElement inspection = hud.Q(className: "h-inspection");
            if (UI.HudLayer.TryLocalRect(inspection, hud, out Rect inspected) && (!found || inspected.yMin < bounds.yMin))
            {
                anchor = inspection;
                bounds = inspected;
                found = true;
            }
            BindLayout(hud, anchor);
            panel.style.left = StyleKeyword.Auto;
            panel.style.right = found ? Mathf.Max(0f, area.width - bounds.xMax) : 28f;
            panel.style.top = found ? new StyleLength(StyleKeyword.Auto) : new StyleLength(Length.Percent(48f));
            panel.style.bottom = found ? new StyleLength(Mathf.Clamp(area.height - bounds.yMin + 12f, 24f, area.height - 12f)) : new StyleLength(StyleKeyword.Auto);
        }

        private void BindLayout(VisualElement hud, VisualElement anchor)
        {
            if (layoutHud == hud && lower == anchor) return;
            ReleaseLayout();
            layoutHud = hud;
            lower = anchor;
            layoutHud?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            lower?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent _) => Position();

        private void ReleaseLayout()
        {
            layoutHud?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            lower?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            layoutHud = lower = null;
        }

        private void OnDisable()
        {
            if (panel != null) UI.MenuKit.Show(panel, false);
            ReleaseLayout();
        }

        private void OnDestroy()
        {
            ReleaseLayout();
            UI.HudLayer.Remove(panel);
        }
    }
}

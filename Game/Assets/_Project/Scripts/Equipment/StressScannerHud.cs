using Abandoned.Structure;
using UnityEngine;

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

        private GUIStyle style;
        private readonly RaycastHit[] hits = new RaycastHit[8];

        public StructuralSection Target { get; private set; }

        private void Update()
        {
            Target = null;
            if (equipment == null || !equipment.IsOwner || equipment.InHand == null || equipment.InHand.Kind != EquipmentKind.StressScanner) return;
            int mask = LayerMask.GetMask(Core.GameLayers.Structure);
            Vector3 from = eye != null ? eye.position : transform.position + Vector3.up * 1.6f;
            Vector3 dir = eye != null ? eye.forward : transform.forward;
            if (Physics.Raycast(from, dir, out RaycastHit hit, range, mask, QueryTriggerInteraction.Ignore) ||
                Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, 2f, mask, QueryTriggerInteraction.Ignore))
                Target = hit.collider.GetComponentInParent<StructuralSection>();
        }

        private void OnGUI()
        {
            if (Target == null) return;
            style ??= new GUIStyle(GUI.skin.box) { fontSize = 16, richText = true, alignment = TextAnchor.MiddleCenter };
            float health = Target.HealthFraction;
            // Colourblind-safe: blue / orange / vermilion (Okabe-Ito) instead of green / yellow / red.
            string color = Core.GameSettings.ColorblindScanner
                ? health > 0.6f ? "#56b4e9" : health > 0.3f ? "#e69f00" : "#d55e00"
                : health > 0.6f ? "#7dff7d" : health > 0.3f ? "#ffd24d" : "#ff5544";
            string load = Target.Capacity > 0f ? $"{Target.Load:0} / {Target.Capacity:0} kg" : "-";
            GUI.Box(new Rect(Screen.width / 2f + 40f, Screen.height / 2f - 30f, 300f, 60f),
                $"<color={color}>{Target.Stage}  {health:P0}</color>\nload {load}", style);
        }
    }
}

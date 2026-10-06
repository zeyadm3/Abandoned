using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// F1: over every nearby shared item, its crew (carrying n/required, lifted or dragged), the crew's
    /// speed cap and whether this machine simulates it; a marker per carry point (free / held, with the
    /// pull toward its carrier's target). Gizmos (SharedCarryable) draw the same in the Scene view.
    /// </summary>
    public class SharedCarryDebugView : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float range = 15f;

        private const float RefreshInterval = 0.5f;

        private readonly List<SharedCarryable> items = new();
        private float refreshTimer;
        private GUIStyle style;

        private void Update()
        {
            if (!DebugView.Visible) return;
            refreshTimer -= Time.deltaTime;
            if (refreshTimer > 0f) return;
            refreshTimer = RefreshInterval;
            items.Clear();
            items.AddRange(FindObjectsByType<SharedCarryable>(FindObjectsSortMode.None));
        }

        private void OnGUI()
        {
            Camera camera = Camera.main;
            if (!DebugView.Visible || camera == null) return;
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11, richText = true };

            foreach (SharedCarryable item in items)
            {
                if (item == null || item.Grabbable.IsPocketed) continue;
                Bounds bounds = item.Grabbable.GetBounds();
                if (Vector3.Distance(camera.transform.position, bounds.center) > range) continue;
                string sim = item.Grabbable.HasPhysicsAuthority ? "sim here" : "follows";
                string cap = item.IsLifted ? $"  cap {item.GroupMaxSpeed:0.0} m/s" : "";
                string accel = item.CarrierCount > 0 && item.Grabbable.HasPhysicsAuthority ? $"  a {item.LastAcceleration.magnitude:0.0}" : "";
                Label(camera, bounds.center + Vector3.up * (bounds.extents.y + 0.55f),
                    $"<color=#FFD060>{SharedCarryText.Of(item)}</color>  {sim}{cap}{accel}", 360f);
                for (int i = 0; i < item.PointCount; i++)
                {
                    PlayerCarrier carrier = item.CarrierAt(i);
                    string text = carrier == null ? "<color=#8F8>o</color>"
                        : $"<color=#FF8>#{i} {carrier.name} pull {item.LastPull[i].magnitude:0.00}</color>";
                    Label(camera, item.PointWorld(i), text, 200f);
                }
            }
        }

        private void Label(Camera camera, Vector3 world, string text, float width)
        {
            Vector3 screen = camera.WorldToScreenPoint(world);
            if (screen.z <= 0f) return;
            GUI.Label(new Rect(screen.x - width / 2f, Screen.height - screen.y - 9f, width, 18f), text, style);
        }
    }
}

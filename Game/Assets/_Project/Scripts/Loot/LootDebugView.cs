using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>F1: value, condition and weight labels over nearby loot, and its resting load state.</summary>
    public class LootDebugView : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float range = 15f;

        private const float RefreshInterval = 0.5f;

        private readonly List<LootItem> items = new();
        private float refreshTimer;
        private GUIStyle style;

        private void Update()
        {
            if (!DebugView.Visible) return;
            refreshTimer -= Time.deltaTime;
            if (refreshTimer > 0f) return;
            refreshTimer = RefreshInterval;
            items.Clear();
            items.AddRange(FindObjectsByType<LootItem>(FindObjectsSortMode.None));
        }

        private void OnGUI()
        {
            Camera camera = Camera.main;
            if (!DebugView.Visible || camera == null) return;
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12, richText = true };

            foreach (LootItem item in items)
            {
                if (item == null) continue;
                Vector3 world = item.transform.position + Vector3.up * (item.Definition.Size.y * 0.5f + 0.25f);
                if (Vector3.Distance(camera.transform.position, world) > range) continue;
                Vector3 screen = camera.WorldToScreenPoint(world);
                if (screen.z <= 0f) continue;
                string colour = item.CurrentValue < item.FullValue ? "#FF9060" : "#B0FFB0";
                string load = item.LoadWeight > 0f ? $"{item.LoadWeight:0} kg on floor" : "not resting";
                string text = $"<color={colour}>${item.CurrentValue:N0}</color>/${item.FullValue:N0}  {item.Definition.Fragility}\n{load}";
                GUI.Label(new Rect(screen.x - 90f, Screen.height - screen.y - 18f, 180f, 36f), text, style);
            }
        }
    }
}

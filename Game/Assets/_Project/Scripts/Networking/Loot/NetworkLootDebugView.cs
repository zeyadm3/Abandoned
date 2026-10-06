using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// F1: who owns each nearby networked loot item's physics, who holds it (host state), whether
    /// this machine simulates it, the last clamped release speed, the speed a follower copy is moving
    /// at (what hits players here) and the last strike by a client-carried item; gizmos tint owned
    /// items and draw followers' motion.
    /// Sits under the value labels of <see cref="Abandoned.Loot.LootDebugView"/>.
    /// </summary>
    public class NetworkLootDebugView : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float range = 15f;

        private const float RefreshInterval = 0.5f;

        private readonly List<NetworkLoot> items = new();
        private float refreshTimer;
        private GUIStyle style;

        private void Update()
        {
            if (!DebugView.Visible) return;
            refreshTimer -= Time.deltaTime;
            if (refreshTimer > 0f) return;
            refreshTimer = RefreshInterval;
            items.Clear();
            foreach (NetworkLoot loot in FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None))
                if (loot.IsSpawned) items.Add(loot);
        }

        private void OnGUI()
        {
            Camera camera = Camera.main;
            if (!DebugView.Visible || camera == null) return;
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11, richText = true };

            foreach (NetworkLoot loot in items)
            {
                if (loot == null || !loot.IsSpawned || loot.Grabbable.IsPocketed) continue;
                Vector3 world = loot.transform.position + Vector3.up * (loot.Item.Definition.Size.y * 0.5f + 0.25f);
                if (Vector3.Distance(camera.transform.position, world) > range) continue;
                Vector3 screen = camera.WorldToScreenPoint(world);
                if (screen.z <= 0f) continue;
                string sim = loot.Grabbable.HasPhysicsAuthority ? "<color=#8F8>sim here</color>" : "<color=#AAA>follows</color>";
                string release = loot.LastReleaseSpeed >= 0f ? $"  rel {loot.LastReleaseSpeed:0.0} m/s" : "";
                float followed = loot.Grabbable.HasPhysicsAuthority ? 0f : loot.Grabbable.Velocity.magnitude;
                string moving = followed > 0.1f ? $"  v {followed:0.0} m/s" : "";
                string struck = loot.LastStruckSpeed >= 0f ? $"  struck {loot.LastStruckSpeed:0.0} m/s" : "";
                string text = $"owner c{loot.OwnerClientId}  {loot.Hold}  {sim}{moving}{release}{struck}";
                GUI.Label(new Rect(screen.x - 170f, Screen.height - screen.y + 16f, 340f, 18f), text, style);
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !DebugView.Visible) return;
            foreach (NetworkLoot loot in items)
            {
                if (loot == null || !loot.IsSpawned) continue;
                Gizmos.color = loot.IsOwner ? Color.green : Color.gray;
                Bounds b = loot.Grabbable.GetBounds();
                Gizmos.DrawWireCube(b.center, b.size + Vector3.one * 0.05f);
                if (!loot.Grabbable.HasPhysicsAuthority)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(b.center, loot.Grabbable.Velocity * 0.25f);
                }
            }
        }
    }
}

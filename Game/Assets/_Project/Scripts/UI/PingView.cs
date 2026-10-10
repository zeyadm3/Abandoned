using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Ping markers on this machine (0.19.0): the local player's ping key asks the host to mark what they look
    /// at; every mark shows as a coloured diamond with what it is and how far, following its target if it moves.
    /// </summary>
    public class PingView : MonoBehaviour
    {
        private static readonly Color[] SeatColours =
        {
            new(1f, 0.55f, 0.1f), new(0.2f, 0.9f, 0.4f), new(0.3f, 0.7f, 1f), new(1f, 0.3f, 0.8f),
        };

        private GUIStyle style;

        private void Update()
        {
            NetworkPlayer me = NetworkPlayer.Local;
            RunPings pings = RunPings.Current;
            if (me == null || pings == null || me.IsDead || CursorOwner.UiActive) return;
            var reader = me.GetComponent<PlayerInputReader>();
            Camera cam = Camera.main;
            if (reader != null && reader.PingPressed && cam != null)
            {
                pings.RequestPing(cam.transform.position, cam.transform.forward);
                AchievementWatcher.Ping();
            }
        }

        private void OnGUI()
        {
            RunPings pings = RunPings.Current;
            Camera cam = Camera.main;
            if (pings == null || cam == null || !pings.IsSpawned || pings.Marks.Count == 0 || HudLayer.Root == null) return;
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            style.fontSize = Mathf.RoundToInt(Screen.height * 0.018f);
            NetworkManager net = NetworkManager.Singleton;
            foreach (PingMark m in pings.Marks)
            {
                Vector3 at = m.Position;
                if (m.Target != 0 && net != null && net.SpawnManager != null && net.SpawnManager.SpawnedObjects.TryGetValue(m.Target, out NetworkObject target) && target != null)
                    at = target.transform.position + Vector3.up * 0.6f;
                Vector3 screen = cam.WorldToScreenPoint(at);
                if (screen.z <= 0f) continue;
                float y = Screen.height - screen.y;
                float left = (float)(m.Until - pings.Now);
                Color c = SeatColours[m.Seat % SeatColours.Length];
                c.a = Mathf.Clamp01(left / 2f);
                GUI.color = c;
                string what = m.Kind switch { PingKind.Loot => "LOOT", PingKind.Monster => "MONSTER!", PingKind.WeakFloor => "WEAK FLOOR", _ => "HERE" };
                float metres = Vector3.Distance(cam.transform.position, at);
                GUI.Label(new Rect(screen.x - 120f, y - 34f, 240f, 24f), "◆", style);
                GUI.Label(new Rect(screen.x - 120f, y - 14f, 240f, 24f), $"{what}  {metres:0} m", style);
            }
            GUI.color = Color.white;
        }
    }
}

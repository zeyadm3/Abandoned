using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

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

        private readonly System.Collections.Generic.List<(VisualElement root, Label text)> markers = new();

        // 0.22.3: UI Toolkit world markers in the revamp's style (were IMGUI): a diamond and a label per ping.
        private void LateUpdate()
        {
            RunPings pings = RunPings.Current;
            Camera cam = Camera.main;
            int count = pings != null && pings.IsSpawned && cam != null && HudLayer.World != null ? pings.Marks.Count : 0;
            // A HUD built again (a new language) left the old markers behind.
            if (count > 0 && markers.Count > 0 && markers[0].root.panel == null) markers.Clear();
            while (markers.Count < count)
            {
                var root = new VisualElement { pickingMode = PickingMode.Ignore };
                root.AddToClassList("ping-mark");
                var diamond = new VisualElement { pickingMode = PickingMode.Ignore };
                diamond.AddToClassList("ping-mark__diamond");
                root.Add(diamond);
                var text = new Label { pickingMode = PickingMode.Ignore };
                text.AddToClassList("ping-mark__text");
                root.Add(text);
                HudLayer.World.Add(root);
                markers.Add((root, text));
            }
            for (int i = 0; i < markers.Count; i++)
            {
                if (i >= count || markers[i].root.panel == null)
                {
                    markers[i].root.style.display = DisplayStyle.None;
                    continue;
                }
                PingMark m = pings.Marks[i];
                Vector3 at = m.Position;
                NetworkManager net = NetworkManager.Singleton;
                if (m.Target != 0 && net != null && net.SpawnManager != null && net.SpawnManager.SpawnedObjects.TryGetValue(m.Target, out NetworkObject target) && target != null)
                    at = target.transform.position + Vector3.up * 0.6f;
                HudLayer.Place(markers[i].root, at, cam);
                Color c = SeatColours[m.Seat % SeatColours.Length];
                float left = (float)(m.Until - pings.Now);
                markers[i].root.style.opacity = Mathf.Clamp01(left / 2f);
                markers[i].root.Q(className: "ping-mark__diamond").style.backgroundColor = c;
                markers[i].text.style.color = c;
                string what = m.Kind switch { PingKind.Loot => Loc.T("LOOT"), PingKind.Monster => Loc.T("MONSTER!"), PingKind.WeakFloor => Loc.T("WEAK FLOOR"), _ => Loc.T("HERE") };
                markers[i].text.text = $"{what}  {Vector3.Distance(cam.transform.position, at):0} m";
            }
        }
    }
}

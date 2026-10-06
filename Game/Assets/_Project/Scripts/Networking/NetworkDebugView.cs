using System.Text;
using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// F1 view of the session: connection state, transport, client ids with round-trip times, and
    /// every spawned player's ownership flags, plus a label over each player in the world.
    /// </summary>
    public class NetworkDebugView : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap bootstrap;
        [SerializeField] private int fontSize = 13;

        private readonly StringBuilder text = new();
        private GUIStyle style, label;

        private void OnGUI()
        {
            if (!DebugView.Visible || bootstrap == null) return;
            style ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = fontSize, richText = true };
            label ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = fontSize, richText = true };

            var content = new GUIContent(Describe());
            Vector2 size = style.CalcSize(content);
            GUI.Box(new Rect(10f, Screen.height - size.y - 40f, size.x, size.y), content, style);
            DrawPlayerLabels();
        }

        private string Describe()
        {
            NetworkManager nm = bootstrap.Manager;
            text.Clear().AppendLine("<b>NETWORK</b>");
            text.AppendLine($"State      {bootstrap.Status}");
            text.AppendLine($"Transport  {bootstrap.Transport}   Authority {(GameAuthority.IsHost ? "HOST" : "client")}");
            if (nm != null && nm.IsListening)
            {
                text.AppendLine($"Local id   {nm.LocalClientId}  host={nm.IsHost} server={nm.IsServer} client={nm.IsClient}");
                NetworkTransport transport = nm.NetworkConfig.NetworkTransport;
                if (nm.IsServer)
                {
                    foreach (ulong id in nm.ConnectedClientsIds)
                        text.AppendLine($"  client {id}  rtt {(id == nm.LocalClientId ? 0 : transport.GetCurrentRtt(id))} ms" +
                                        (bootstrap.Slots.TryGetSlot(id, out int slot) ? $"  spawn {slot}" : ""));
                }
                else if (nm.IsConnectedClient)
                {
                    text.AppendLine($"  rtt to host {transport.GetCurrentRtt(NetworkManager.ServerClientId)} ms");
                }
            }
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                PlayerNetState s = p.State;
                text.AppendLine($"Player {p.OwnerClientId}  owner={p.IsOwner} local={p.IsLocalPlayer}  " +
                                $"{(s.Ragdolled ? "RAGDOLL " : "")}{(s.Grounded ? "ground" : "air")}{(s.Sprinting ? " sprint" : "")}{(s.Crouching ? " crouch" : "")}");
            }
            return text.ToString().TrimEnd();
        }

        private void DrawPlayerLabels()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p.IsOwner) continue;
                Vector3 screen = cam.WorldToScreenPoint(p.transform.position + Vector3.up * 2.2f);
                if (screen.z <= 0f) continue;
                GUI.Label(new Rect(screen.x - 60f, Screen.height - screen.y - 10f, 120f, 20f),
                    $"<color=#7CF>P{p.OwnerClientId}</color>", label);
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !DebugView.Visible) return;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                Gizmos.color = p.IsOwner ? Color.green : Color.cyan;
                Gizmos.DrawWireCube(p.transform.position + Vector3.up * 2.3f, Vector3.one * 0.2f);
            }
        }
    }
}

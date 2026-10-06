using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Placeholder OnGUI session menu until the real lobby screen (M7): transport choice, Host (also
    /// solo play), Join by address, Disconnect, status and errors; over Steam also the lobby's member
    /// list and an Invite button (Steam overlay). Shown while offline or while the cursor is free (Esc),
    /// so it never covers the game during play.
    /// </summary>
    public class NetworkPanel : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap bootstrap;
        [OptionalReference, SerializeField] private SteamLobby lobby;
        [SerializeField] private int fontSize = 14;

        private const float Width = 330f;

        private string address;
        private GUIStyle box, error;

        private void Start() => address = $"{bootstrap.Config.DefaultJoinAddress}:{bootstrap.Config.Port}";

        private void OnGUI()
        {
            if (bootstrap == null || bootstrap.Config == null) return;
            if (bootstrap.IsRunning && Cursor.lockState == CursorLockMode.Locked) return;

            box ??= new GUIStyle(GUI.skin.box) { fontSize = fontSize, alignment = TextAnchor.UpperLeft };
            error ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize, wordWrap = true, normal = { textColor = new Color(1f, 0.45f, 0.4f) } };

            GUILayout.BeginArea(new Rect(10f, 10f, Width, 360f), box);
            GUILayout.Label("<b>NETWORK</b> (placeholder menu)", new GUIStyle(GUI.skin.label) { richText = true, fontSize = fontSize });
            GUILayout.Label(bootstrap.Status);
            if (!string.IsNullOrEmpty(bootstrap.LastError)) GUILayout.Label(bootstrap.LastError, error);
            SteamLobbyFlow flow = lobby != null ? lobby.Flow : null;
            if (flow != null && !string.IsNullOrEmpty(flow.LastError)) GUILayout.Label(flow.LastError, error);

            if (bootstrap.IsRunning)
            {
                if (flow != null && flow.InLobby) DrawLobby(flow);
                if (GUILayout.Button("Disconnect")) bootstrap.Disconnect();
            }
            else
            {
                DrawOfflineControls();
            }
            GUILayout.EndArea();
        }

        private void DrawLobby(SteamLobbyFlow flow)
        {
            GUILayout.Label($"Steam lobby ({flow.Members.Count}/{bootstrap.Config.MaxPlayers}, friends only):");
            foreach (LobbyMember member in flow.Members) GUILayout.Label($"  {member.Name}");
            if (bootstrap.Manager.IsHost && GUILayout.Button("Invite friends (Steam overlay)")) flow.OpenInviteOverlay();
        }

        private void DrawOfflineControls()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Transport", GUILayout.Width(80f));
            TransportToggle(TransportMode.UnityTransport, "Direct IP");
            TransportToggle(TransportMode.Steam, "Steam");
            GUILayout.EndHorizontal();

            bool steam = bootstrap.Transport == TransportMode.Steam;
            if (GUILayout.Button(steam ? "Host (friends-only Steam lobby)" : "Host (or play solo)")) bootstrap.StartHost();
            if (steam) GUILayout.Label("Friends join through your Steam invite, or by your SteamID64:");

            GUILayout.Label(steam ? "Host's SteamID64:" : "Address (ip:port):");
            address = GUILayout.TextField(address ?? string.Empty);
            if (GUILayout.Button("Join")) bootstrap.StartClient(address);
        }

        private void TransportToggle(TransportMode mode, string label)
        {
            bool on = bootstrap.Transport == mode;
            if (GUILayout.Toggle(on, label, GUI.skin.button) && !on) bootstrap.SelectTransport(mode);
        }
    }
}

using System.Text;
using Abandoned.Audio;
using Abandoned.Networking;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Esc during a game: resume, settings, the crew (the lobby screen: who's here, invites, the address
    /// friends join), leave the game or quit. The world keeps running behind it.
    /// </summary>
    public class PauseMenuView
    {
        private readonly MenuUi menu;
        private readonly Label crew, joinHint;
        private readonly Button invite, leave;

        public VisualElement Root { get; }

        public PauseMenuView(MenuUi menu)
        {
            this.menu = menu;
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Panel(Root);
            MenuKit.Text(panel, "MENU", "heading");
            MenuKit.Text(panel, "The game keeps running.", "subtitle");

            MenuKit.Button(panel, "Resume", menu.Resume, SoundId.UiConfirm);
            MenuKit.Button(panel, "Settings", () => menu.Push(MenuScreen.Settings));
            MenuKit.Button(panel, "How to play", () => menu.Push(MenuScreen.HowToPlay));
            MenuKit.Button(panel, "Achievements", () => menu.Push(MenuScreen.Achievements));

            MenuKit.Text(panel, "CREW", "section");
            crew = MenuKit.Text(panel, "", "text");
            joinHint = MenuKit.Text(panel, "", "text");
            joinHint.AddToClassList("text--small");
            invite = MenuKit.Button(panel, "Invite friends (Steam overlay)", () => menu.Lobby?.Flow?.OpenInviteOverlay());

            leave = MenuKit.Button(panel, "Leave game", () => menu.Bootstrap.Disconnect(), SoundId.UiBack);
            MenuKit.Button(panel, "Quit to desktop", menu.Quit, SoundId.UiBack);
        }

        public void Refresh()
        {
            NetworkBootstrap b = menu.Bootstrap;
            bool isHost = b.Manager != null && b.Manager.IsHost;
            var lines = new StringBuilder();
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned) continue;
                lines.Append($"Player {p.OwnerClientId + 1}");
                if (p.OwnerClientId == NetworkManagerHostId) lines.Append(" (host)");
                if (p.IsOwner) lines.Append(" - you");
                if (p.IsDead) lines.Append(" - ghost");
                lines.AppendLine();
            }
            SteamLobbyFlow flow = menu.Lobby != null ? menu.Lobby.Flow : null;
            if (flow != null && flow.InLobby)
            {
                lines.Append($"Steam lobby {flow.Members.Count}/{b.Config.MaxPlayers}:");
                foreach (LobbyMember m in flow.Members) lines.Append($"  {m.Name}");
            }
            crew.text = lines.ToString().TrimEnd();

            MenuKit.Show(invite, isHost && flow != null && flow.InLobby);
            joinHint.text = isHost && b.HostPort != 0 ? $"Friends join at your LAN address:{b.HostPort} (players join at the HQ only)." : "";
            leave.text = isHost ? "End game for everyone" : "Leave game";
        }

        private const ulong NetworkManagerHostId = Unity.Netcode.NetworkManager.ServerClientId;
    }
}

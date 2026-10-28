using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The crew board in the HQ dispatch office (2026 remake; what the pause menu's right-hand side used to be): who is
    /// here and how to hear them, Steam invites, how friends join, and the host's rules for the company's money and
    /// vehicles.
    /// </summary>
    public class CrewBoardView
    {
        private readonly MenuUi menu;
        private readonly CrewRoster roster;
        private readonly Label crewTitle, joinHint;
        private readonly Button invite;
        private readonly VisualElement hostRules;
        private Company.CompanyService rulesFor;

        public VisualElement Root { get; }

        public CrewBoardView(MenuUi menu)
        {
            this.menu = menu;
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            VisualElement panel = MenuKit.Page(Root);
            panel.AddToClassList("crew-board");
            crewTitle = MenuKit.Text(panel, Loc.T("CREW"), "heading");
            ScrollView body = MenuKit.Scroll(panel, "pause__crew-body");
            body.style.flexGrow = 1;
            body.style.flexShrink = 1;
            body.style.minHeight = 0;
            invite = MenuKit.Button(body, Loc.T("Invite friends"), () => menu.Lobby?.Flow?.OpenInviteOverlay(), SoundId.UiConfirm, important: true);
            joinHint = MenuKit.Text(body, "", "text");
            joinHint.AddToClassList("text--small");
            Label note = MenuKit.Text(body, Loc.T("Voice volume and mute only change what you hear. They are in Settings > Voice too."), "text");
            note.AddToClassList("text--small");
            roster = new CrewRoster(body, kick: true);
            hostRules = new VisualElement();
            hostRules.style.flexShrink = 0;
            hostRules.style.marginTop = 12f;
            body.Add(hostRules);
            MenuKit.Button(panel, Loc.T("Done"), menu.Back, SoundId.UiBack);
        }

        // QA B-08: the host decides what the rest of the crew may do with the boss's money and vehicles.
        private void BuildHostRules(bool isHost)
        {
            Company.CompanyService company = Company.CompanyService.Current;
            bool show = isHost && company != null && company.IsSpawned;
            MenuKit.Show(hostRules, show);
            if (!show || rulesFor == company) return;
            rulesFor = company;
            hostRules.Clear();
            MenuKit.Text(hostRules, Loc.T("HOST RULES: THE CREW MAY"), "section");
            MenuKit.Toggle(hostRules, Loc.T("Buy gear on the company account"), company.CrewMay(Company.CrewRule.Spend), v => company.SetCrewRule(Company.CrewRule.Spend, v));
            MenuKit.Toggle(hostRules, Loc.T("Start the van"), company.CrewMay(Company.CrewRule.Drive), v => company.SetCrewRule(Company.CrewRule.Drive, v));
            MenuKit.Toggle(hostRules, Loc.T("Pull the truck lever"), company.CrewMay(Company.CrewRule.Lever), v => company.SetCrewRule(Company.CrewRule.Lever, v));
        }

        public void Refresh()
        {
            NetworkBootstrap b = menu.Bootstrap;
            bool isHost = b.Manager != null && b.Manager.IsHost;
            SteamLobbyFlow flow = menu.Lobby != null ? menu.Lobby.Flow : null;
            int count = roster.Refresh(b);
            BuildHostRules(isHost);
            int max = b.Config != null ? b.Config.MaxPlayers : 4;
            crewTitle.text = Loc.T("CREW  {0}/{1}", count, max);
            MenuKit.Show(invite, isHost && flow != null && flow.InLobby);
            joinHint.text = flow != null && flow.InLobby ? Loc.T("Steam lobby: {0}/{1} - friends join from the overlay or your profile.", flow.Members.Count, max)
                : isHost && b.HostPort != 0 ? (GameSettings.StreamerMode ? Loc.T("Friends join at your LAN address (hidden: streamer mode). Players join at the HQ only.")
                    : Loc.T("Friends join at your LAN address, port {0}. Players join at the HQ only.", b.HostPort)) : "";
        }
    }
}

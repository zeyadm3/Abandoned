using System.Collections.Generic;
using System.Text;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Voice;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Esc during a game (UI step 4): the game keeps running behind a dimmed screen. Left, the big buttons
    /// (resume, settings, how to play, achievements, invite, leave, quit); right, the crew: everyone in their
    /// coverall colour, host/you/ghost tags, ping where this machine knows it, a talking light, and for
    /// everyone else a voice volume slider and a mute button (this machine only, R.E.P.O.-style).
    /// </summary>
    public class PauseMenuView
    {
        private readonly MenuUi menu;
        private readonly VisualElement crewList;
        private readonly Label crewTitle, joinHint;
        private readonly Button invite, leave;
        private readonly Dictionary<ulong, Row> rows = new();
        private readonly VisualElement hostRules;
        private readonly Tutorial.TutorialPauseControls training;
        private Company.CompanyService rulesFor;
        private string shownCrew;

        private sealed class Row
        {
            public VisualElement Root, Swatch, Talking;
            public Label Name, Tags, Ping;
            public Button Mute;
        }

        public VisualElement Root { get; }

        public PauseMenuView(MenuUi menu)
        {
            this.menu = menu;
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            Root.AddToClassList("pause");
            ScrollView content = MenuKit.Scroll(Root, "pause__scroll");
            content.contentContainer.AddToClassList("pause__content");
            content.style.width = Length.Percent(100f);
            content.style.height = Length.Percent(100f);
            var columns = new VisualElement();
            columns.AddToClassList("pause__columns");
            content.Add(columns);

            VisualElement left = new();
            left.AddToClassList("pause__left");
            columns.Add(left);
            MenuKit.Text(left, Loc.T("RECOVERY IN PROGRESS"), "pause__eyebrow");
            MenuKit.Text(left, Loc.T("PAUSED"), "title");
            MenuKit.Text(left, Loc.T("The game keeps running. The building does not wait."), "subtitle").AddToClassList("pause__warning");
            MenuKit.Button(left, Loc.T("Resume"), menu.Resume, SoundId.UiConfirm).AddToClassList("pause__resume");
            training = new Tutorial.TutorialPauseControls(left, menu);
            MenuKit.Button(left, Loc.T("Settings"), () => menu.Push(MenuScreen.Settings));
            MenuKit.Button(left, Loc.T("How to play"), () => menu.Push(MenuScreen.HowToPlay));
            MenuKit.Button(left, Loc.T("Achievements"), () => menu.Push(MenuScreen.Achievements));
            invite = MenuKit.Button(left, Loc.T("Invite friends"), () => menu.Lobby?.Flow?.OpenInviteOverlay());
            leave = MenuKit.Button(left, Loc.T("Leave game"), () =>
            {
                Company.CompanyService.Current?.MarkLeavingMidJob();
                menu.Bootstrap.Disconnect();
            }, SoundId.UiBack, important: true);
            MenuKit.Button(left, Loc.T("Quit to desktop"), () =>
            {
                Company.CompanyService.Current?.MarkLeavingMidJob();
                menu.Quit();
            }, SoundId.UiBack, important: true);
            MenuKit.Button(left, Loc.T("Report a problem"), () =>
            {
                Launch.OpenLogFolder();
                Launch.OpenFeedback();
            }, small: true).tooltip = Loc.T("Opens your log folder (attach Player.log) and the feedback page.");

            // One scrolling body under the title: the crew rows get the room they need and the host's rules sit
            // below them inside the panel, never spilling out of it.
            VisualElement right = MenuKit.Panel(columns);
            right.style.width = StyleKeyword.Null;
            right.style.maxWidth = StyleKeyword.Null;
            right.style.height = 600f;
            right.style.minHeight = 300f;
            right.style.maxHeight = 640f;
            right.style.flexDirection = FlexDirection.Column;
            right.AddToClassList("pause__crew");
            crewTitle = MenuKit.Text(right, Loc.T("CREW"), "heading");
            crewTitle.style.flexShrink = 0;
            ScrollView body = MenuKit.Scroll(right, "pause__crew-body");
            body.style.flexGrow = 1;
            body.style.flexShrink = 1;
            body.style.minHeight = 0;
            Label note = MenuKit.Text(body, Loc.T("Voice volume and mute only change what you hear."), "text");
            note.AddToClassList("text--small");
            crewList = new VisualElement();
            crewList.AddToClassList("pause__crew-list");
            crewList.style.flexShrink = 0;
            crewList.style.marginTop = 8f;
            crewList.style.marginBottom = 8f;
            body.Add(crewList);
            joinHint = MenuKit.Text(body, "", "text");
            joinHint.AddToClassList("text--small");
            joinHint.style.flexShrink = 0;
            hostRules = new VisualElement();
            hostRules.style.flexShrink = 0;
            hostRules.style.marginTop = 12f;
            body.Add(hostRules);
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

            // Rows are rebuilt when the crew changes; their live bits (ping, talking) update every frame.
            var key = new StringBuilder();
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.IsSpawned) key.Append(p.OwnerClientId).Append(',');
            if (key.ToString() != shownCrew)
            {
                shownCrew = key.ToString();
                crewList.Clear();
                rows.Clear();
                foreach (NetworkPlayer p in NetworkPlayer.All)
                    if (p != null && p.IsSpawned) rows[p.OwnerClientId] = Build(p, isHost);
            }
            BuildHostRules(isHost);
            training.Refresh();
            int count = 0;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || !rows.TryGetValue(p.OwnerClientId, out Row row)) continue;
                count++;
                Live(p, row, b);
            }
            int max = b.Config != null ? b.Config.MaxPlayers : 4;
            crewTitle.text = Loc.T("CREW  {0}/{1}", count, max);
            MenuKit.Show(invite, isHost && flow != null && flow.InLobby);
            joinHint.text = flow != null && flow.InLobby ? Loc.T("Steam lobby: {0}/{1} - friends join from the overlay or your profile.", flow.Members.Count, max)
                : isHost && b.HostPort != 0 ? (GameSettings.StreamerMode ? Loc.T("Friends join at your LAN address (hidden: streamer mode). Players join at the HQ only.")
                    : Loc.T("Friends join at your LAN address, port {0}. Players join at the HQ only.", b.HostPort)) : "";
            bool midJob = isHost && Company.CompanyService.Current != null && Company.CompanyService.Current.JobInProgress;
            leave.text = !isHost ? Loc.T("Leave game") : midJob ? Loc.T("End game (job counts as failed)") : Loc.T("End game for everyone");
        }

        private Row Build(NetworkPlayer p, bool isHost)
        {
            var row = new Row { Root = new VisualElement() };
            row.Root.AddToClassList("crew-row");
            VisualElement identity = MenuKit.Row(row.Root);
            identity.AddToClassList("crew-row__identity");
            row.Swatch = new VisualElement();
            row.Swatch.AddToClassList("crew-row__swatch");
            PlayerCosmetics look = p.GetComponent<PlayerCosmetics>();
            CosmeticDefinition suit = look != null && look.Catalog != null ? look.Catalog.Coverall(look.Choice.Coverall) : null;
            if (suit != null) row.Swatch.style.backgroundColor = suit.Color;
            identity.Add(row.Swatch);

            var who = new VisualElement();
            who.AddToClassList("crew-row__who");
            identity.Add(who);
            row.Name = MenuKit.Text(who, p.DisplayName, "crew-row__name");
            row.Tags = MenuKit.Text(who, "", "crew-row__tags");
            row.Talking = new VisualElement();
            row.Talking.AddToClassList("crew-row__talking");
            identity.Add(row.Talking);
            row.Ping = MenuKit.Text(identity, "", "crew-row__ping");

            if (!p.IsOwner)
            {
                VisualElement controls = MenuKit.Row(row.Root);
                controls.AddToClassList("crew-row__controls");
                controls.style.flexWrap = Wrap.Wrap;
                ulong id = p.OwnerClientId;
                var slider = new Slider(0f, 2f) { value = VoiceSettings.PlayerVolume(id) };
                slider.AddToClassList("crew-row__volume");
                slider.RegisterValueChangedCallback(e => VoiceSettings.SetPlayerVolume(id, e.newValue));
                controls.Add(slider);
                row.Mute = MenuKit.Button(controls, VoiceSettings.PlayerMuted(id) ? Loc.T("Muted") : Loc.T("Mute"), () =>
                {
                    VoiceSettings.SetPlayerMuted(id, !VoiceSettings.PlayerMuted(id));
                    row.Mute.text = VoiceSettings.PlayerMuted(id) ? Loc.T("Muted") : Loc.T("Mute");
                    row.Mute.EnableInClassList("menu-button--on", VoiceSettings.PlayerMuted(id));
                }, SoundId.UiClick, small: true);
                row.Mute.EnableInClassList("menu-button--on", VoiceSettings.PlayerMuted(id));
                if (isHost)
                {
                    // Two presses, so a stray click doesn't throw a friend out (QA B-24).
                    Button kick = null;
                    float armedUntil = -1f;
                    kick = MenuKit.Button(controls, Loc.T("Kick"), () =>
                    {
                        if (Time.unscaledTime > armedUntil)
                        {
                            armedUntil = Time.unscaledTime + 3f;
                            kick.text = Loc.T("Sure?");
                            return;
                        }
                        Company.CompanyService.Current?.Kick(id);
                    }, SoundId.UiBack, small: true);
                }
            }
            crewList.Add(row.Root);
            return row;
        }

        private static void Live(NetworkPlayer p, Row row, NetworkBootstrap b)
        {
            var tags = new List<string>();
            if (p.OwnerClientId == Unity.Netcode.NetworkManager.ServerClientId) tags.Add(Loc.T("HOST"));
            if (p.IsOwner) tags.Add(Loc.T("YOU"));
            if (p.IsDead) tags.Add(Loc.T("GHOST"));
            string t = string.Join("  -  ", tags);
            if (row.Tags.text != t) row.Tags.text = t;
            string ping = Ping(p, b);
            if (row.Ping.text != ping) row.Ping.text = ping;
            NetworkVoice voice = p.GetComponent<NetworkVoice>();
            bool talking = voice != null && (p.IsOwner ? false : voice.IsSpeaking);
            row.Talking.EnableInClassList("crew-row__talking--on", talking);
        }

        // Round trip in ms where this machine knows it: the host knows everyone's, a client only its own.
        private static string Ping(NetworkPlayer p, NetworkBootstrap b)
        {
            var nm = b.Manager;
            if (nm == null || !nm.IsListening || nm.NetworkConfig.NetworkTransport == null) return "";
            if (nm.IsServer)
                return p.OwnerClientId == nm.LocalClientId ? "" : Loc.T("{0} ms", nm.NetworkConfig.NetworkTransport.GetCurrentRtt(p.OwnerClientId));
            return p.IsOwner ? Loc.T("{0} ms", nm.NetworkConfig.NetworkTransport.GetCurrentRtt(Unity.Netcode.NetworkManager.ServerClientId)) : "";
        }
    }
}

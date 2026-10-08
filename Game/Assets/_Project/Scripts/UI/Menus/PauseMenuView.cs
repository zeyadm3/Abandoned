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
        private Company.CompanyService rulesFor;
        private VisualElement companyTools;
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
            content.style.width = Length.Percent(100f);
            content.style.height = Length.Percent(100f);
            var columns = new VisualElement();
            columns.AddToClassList("pause__columns");
            content.Add(columns);

            VisualElement left = new();
            left.AddToClassList("pause__left");
            columns.Add(left);
            MenuKit.Text(left, "PAUSED", "title");
            MenuKit.Text(left, "The game keeps running.", "subtitle");
            MenuKit.Button(left, "Resume", menu.Resume, SoundId.UiConfirm);
            MenuKit.Button(left, "Settings", () => menu.Push(MenuScreen.Settings));
            MenuKit.Button(left, "How to play", () => menu.Push(MenuScreen.HowToPlay));
            MenuKit.Button(left, "Achievements", () => menu.Push(MenuScreen.Achievements));
            invite = MenuKit.Button(left, "Invite friends", () => menu.Lobby?.Flow?.OpenInviteOverlay());
            leave = MenuKit.Button(left, "Leave game", () =>
            {
                Company.CompanyService.Current?.MarkLeavingMidJob();
                menu.Bootstrap.Disconnect();
            }, SoundId.UiBack, important: true);
            MenuKit.Button(left, "Quit to desktop", () =>
            {
                Company.CompanyService.Current?.MarkLeavingMidJob();
                menu.Quit();
            }, SoundId.UiBack, important: true);
            MenuKit.Button(left, "Report a problem", () =>
            {
                Launch.OpenLogFolder();
                Launch.OpenFeedback();
            }, small: true).tooltip = "Opens your log folder (attach Player.log) and the feedback page.";

            VisualElement right = MenuKit.Panel(columns);
            right.style.height = 540f;
            right.style.minHeight = 300f;
            right.style.maxHeight = 620f;
            right.AddToClassList("pause__crew");
            crewTitle = MenuKit.Text(right, "CREW", "heading");
            MenuKit.Text(right, "Voice volume and mute only change what you hear.", "text").AddToClassList("text--small");
            crewList = MenuKit.Scroll(right, "pause__crew-list");
            joinHint = MenuKit.Text(right, "", "text");
            joinHint.AddToClassList("text--small");
            hostRules = new VisualElement();
            right.Add(hostRules);
        }

        // QA B-08: the host decides what the rest of the crew may do with the company.
        private void BuildHostRules(bool isHost)
        {
            Company.CompanyService company = Company.CompanyService.Current;
            bool show = isHost && company != null && company.IsSpawned;
            MenuKit.Show(hostRules, show);
            if (show && companyTools != null) MenuKit.Show(companyTools, !company.JobInProgress);
            if (!show || rulesFor == company) return;
            rulesFor = company;
            hostRules.Clear();
            MenuKit.Text(hostRules, "CREW MAY", "section");
            MenuKit.Toggle(hostRules, "Spend company money", company.CrewMay(Company.CrewRule.Spend), v => company.SetCrewRule(Company.CrewRule.Spend, v));
            MenuKit.Toggle(hostRules, "Start the van", company.CrewMay(Company.CrewRule.Drive), v => company.SetCrewRule(Company.CrewRule.Drive, v));
            MenuKit.Toggle(hostRules, "Pull the truck lever", company.CrewMay(Company.CrewRule.Lever), v => company.SetCrewRule(Company.CrewRule.Lever, v));
            // QA B-24: the company itself, between jobs only.
            companyTools = new VisualElement();
            hostRules.Add(companyTools);
            MenuKit.Text(companyTools, "COMPANY", "section");
            var rename = new TextField { value = company.State.Name.ToString(), maxLength = NetworkPlayer.MaxNameLength };
            rename.AddToClassList("field");
            rename.RegisterCallback<FocusOutEvent>(_ => company.RenameCompany(rename.value));
            companyTools.Add(rename);
            Button fresh = null;
            float armedUntil = -1f;
            fresh = MenuKit.Button(companyTools, "Start a new company", () =>
            {
                if (Time.unscaledTime > armedUntil)
                {
                    armedUntil = Time.unscaledTime + 3f;
                    fresh.text = "Lose everything? Press again";
                    return;
                }
                fresh.text = "Start a new company";
                company.StartNewCompany();
                rename.value = company.State.Name.ToString();
            }, SoundId.UiBack, small: true);
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
            int count = 0;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || !rows.TryGetValue(p.OwnerClientId, out Row row)) continue;
                count++;
                Live(p, row, b);
            }
            int max = b.Config != null ? b.Config.MaxPlayers : 4;
            crewTitle.text = $"CREW  {count}/{max}";
            MenuKit.Show(invite, isHost && flow != null && flow.InLobby);
            joinHint.text = flow != null && flow.InLobby ? $"Steam lobby: {flow.Members.Count}/{max} - friends join from the overlay or your profile."
                : isHost && b.HostPort != 0 ? $"Friends join at your LAN address, port {b.HostPort}. Players join at the HQ only." : "";
            bool midJob = isHost && Company.CompanyService.Current != null && Company.CompanyService.Current.JobInProgress;
            leave.text = !isHost ? "Leave game" : midJob ? "End game (job counts as failed)" : "End game for everyone";
        }

        private Row Build(NetworkPlayer p, bool isHost)
        {
            var row = new Row { Root = new VisualElement() };
            row.Root.AddToClassList("crew-row");
            row.Swatch = new VisualElement();
            row.Swatch.AddToClassList("crew-row__swatch");
            PlayerCosmetics look = p.GetComponent<PlayerCosmetics>();
            CosmeticDefinition suit = look != null && look.Catalog != null ? look.Catalog.Coverall(look.Choice.Coverall) : null;
            if (suit != null) row.Swatch.style.backgroundColor = suit.Color;
            row.Root.Add(row.Swatch);

            var who = new VisualElement();
            who.AddToClassList("crew-row__who");
            row.Root.Add(who);
            row.Name = MenuKit.Text(who, p.DisplayName, "crew-row__name");
            row.Tags = MenuKit.Text(who, "", "crew-row__tags");
            row.Talking = new VisualElement();
            row.Talking.AddToClassList("crew-row__talking");
            row.Root.Add(row.Talking);
            row.Ping = MenuKit.Text(row.Root, "", "crew-row__ping");

            if (!p.IsOwner)
            {
                ulong id = p.OwnerClientId;
                var slider = new Slider(0f, 2f) { value = VoiceSettings.PlayerVolume(id) };
                slider.AddToClassList("crew-row__volume");
                slider.RegisterValueChangedCallback(e => VoiceSettings.SetPlayerVolume(id, e.newValue));
                row.Root.Add(slider);
                row.Mute = MenuKit.Button(row.Root, VoiceSettings.PlayerMuted(id) ? "Muted" : "Mute", () =>
                {
                    VoiceSettings.SetPlayerMuted(id, !VoiceSettings.PlayerMuted(id));
                    row.Mute.text = VoiceSettings.PlayerMuted(id) ? "Muted" : "Mute";
                    row.Mute.EnableInClassList("menu-button--on", VoiceSettings.PlayerMuted(id));
                }, SoundId.UiClick, small: true);
                row.Mute.EnableInClassList("menu-button--on", VoiceSettings.PlayerMuted(id));
                if (isHost)
                {
                    // Two presses, so a stray click doesn't throw a friend out (QA B-24).
                    Button kick = null;
                    float armedUntil = -1f;
                    kick = MenuKit.Button(row.Root, "Kick", () =>
                    {
                        if (Time.unscaledTime > armedUntil)
                        {
                            armedUntil = Time.unscaledTime + 3f;
                            kick.text = "Sure?";
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
            if (p.OwnerClientId == Unity.Netcode.NetworkManager.ServerClientId) tags.Add("HOST");
            if (p.IsOwner) tags.Add("YOU");
            if (p.IsDead) tags.Add("GHOST");
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
                return p.OwnerClientId == nm.LocalClientId ? "" : $"{nm.NetworkConfig.NetworkTransport.GetCurrentRtt(p.OwnerClientId)} ms";
            return p.IsOwner ? $"{nm.NetworkConfig.NetworkTransport.GetCurrentRtt(Unity.Netcode.NetworkManager.ServerClientId)} ms" : "";
        }
    }
}

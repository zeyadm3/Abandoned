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
    /// The crew as a list (moved out of the pause menu in the 2026 remake): everyone in their colour, host/you/ghost
    /// tags, ping where this machine knows it, a talking light, and for everyone else a voice volume slider and a mute
    /// button (this machine only); the host may also kick. Used by the HQ crew board and Settings > Voice.
    /// </summary>
    public sealed class CrewRoster
    {
        private readonly VisualElement list;
        private readonly bool allowKick;
        private readonly Dictionary<ulong, Row> rows = new();
        private string shownCrew;

        private sealed class Row
        {
            public VisualElement Root, Swatch, Talking;
            public Label Name, Tags, Ping;
            public Button Mute;
        }

        public VisualElement Root => list;

        public CrewRoster(VisualElement parent, bool kick)
        {
            allowKick = kick;
            list = new VisualElement();
            list.AddToClassList("pause__crew-list");
            list.style.flexShrink = 0;
            list.style.marginTop = 8f;
            list.style.marginBottom = 8f;
            parent.Add(list);
        }

        /// <summary>Every frame while shown. Returns how many are in the crew.</summary>
        public int Refresh(NetworkBootstrap b)
        {
            bool isHost = b != null && b.Manager != null && b.Manager.IsHost;
            var key = new StringBuilder();
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.IsSpawned) key.Append(p.OwnerClientId).Append(',');
            if (key.ToString() != shownCrew)
            {
                shownCrew = key.ToString();
                list.Clear();
                rows.Clear();
                foreach (NetworkPlayer p in NetworkPlayer.All)
                    if (p != null && p.IsSpawned) rows[p.OwnerClientId] = Build(p, isHost);
            }
            int count = 0;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || !rows.TryGetValue(p.OwnerClientId, out Row row)) continue;
                count++;
                Live(p, row, b);
            }
            return count;
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
                if (isHost && allowKick)
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
            list.Add(row.Root);
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
            bool talking = voice != null && !p.IsOwner && voice.IsSpeaking;
            row.Talking.EnableInClassList("crew-row__talking--on", talking);
        }

        // Round trip in ms where this machine knows it: the host knows everyone's, a client only its own.
        private static string Ping(NetworkPlayer p, NetworkBootstrap b)
        {
            var nm = b != null ? b.Manager : null;
            if (nm == null || !nm.IsListening || nm.NetworkConfig.NetworkTransport == null) return "";
            if (nm.IsServer)
                return p.OwnerClientId == nm.LocalClientId ? "" : Loc.T("{0} ms", nm.NetworkConfig.NetworkTransport.GetCurrentRtt(p.OwnerClientId));
            return p.IsOwner ? Loc.T("{0} ms", nm.NetworkConfig.NetworkTransport.GetCurrentRtt(Unity.Netcode.NetworkManager.ServerClientId)) : "";
        }
    }
}

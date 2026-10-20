using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Play (UI step 6): how to play over (Steam or direct IP), then three cards: Solo (just you; friends can
    /// still join at the HQ), Host (run a crew: how friends get in) and Join (a friend's SteamID64 or address,
    /// or accept their Steam invite). Status and errors from the network bootstrap underneath.
    /// </summary>
    public class PlayView
    {
        private readonly MenuUi menu;
        private readonly Button steam, directIp, hostButton;
        private readonly Label hostHow, joinHow, status, error;
        private readonly TextField address;

        public VisualElement Root { get; }

        public PlayView(MenuUi menu)
        {
            this.menu = menu;
            NetworkBootstrap b = menu.Bootstrap;
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            Root.AddToClassList("backdrop--center");
            VisualElement panel = MenuKit.Panel(Root, wide: true);
            panel.AddToClassList("panel--xwide");
            panel.AddToClassList("play");
            panel.style.width = 1240f;
            panel.style.maxWidth = Length.Percent(96f);
            MenuKit.Text(panel, Loc.T("RECOVERY ASSIGNMENT / CREW ACCESS"), "menu-record");
            MenuKit.Text(panel, Loc.T("PLAY"), "heading");
            ScrollView body = MenuKit.Scroll(panel, "play__body");

            VisualElement over = MenuKit.Row(body);
            over.AddToClassList("play__transport");
            over.style.flexWrap = Wrap.Wrap;
            MenuKit.Text(over, Loc.T("Play over"), "setting__label");
            steam = MenuKit.Button(over, "Steam", () => b.SelectTransport(TransportMode.Steam), SoundId.UiClick, small: true);
            directIp = MenuKit.Button(over, Loc.T("Direct IP"), () => b.SelectTransport(TransportMode.UnityTransport), SoundId.UiClick, small: true);

            var cards = new VisualElement();
            cards.style.flexDirection = FlexDirection.Row;
            cards.style.flexWrap = Wrap.Wrap;
            cards.style.flexShrink = 0;
            cards.style.minWidth = 0;
            cards.AddToClassList("hud-cards");
            cards.AddToClassList("play__cards");
            // QA B-24: up to three saves on this machine; hosting (or solo) plays the chosen one.
            var slots = new string[Company.SaveStore.Slots];
            for (int i = 0; i < slots.Length; i++) slots[i] = $"{i + 1}: {Company.SaveStore.Describe(i + 1)}";
            MenuKit.Choice(body, Loc.T("Save"), slots, Company.SaveStore.Slot - 1, i => Company.SaveStore.Slot = i + 1);
            body.Add(cards);

            VisualElement solo = Card(cards, Loc.T("SOLO"), "icon/singleplayer", Loc.T("One worker. An unstable building. Use the trolley for heavy salvage. Friends can still join you at the HQ."));
            MenuKit.Button(Actions(solo), Loc.T("Start"), () => b.StartHost(), SoundId.UiConfirm, important: true);

            // The practice house: hosts like Solo, then the crew is driven straight there (friends may still join at the HQ first).
            if (Company.CompanyService.TrainingAvailable)
            {
                VisualElement training = Card(cards, Loc.T("TRAINING"), "icon/home", Loc.T("A practice job in an empty house: carrying, floors, the dark, something listening, the truck. No pay, no risk."));
                MenuKit.Button(Actions(training), Loc.T("Start training"), () =>
                {
                    Tutorial.TutorialLaunch.Request();
                    b.StartHost();
                }, SoundId.UiConfirm, important: true);
            }

            VisualElement host = Card(cards, Loc.T("HOST A CREW"), "icon/multiplayer", Loc.T("Lead a crew of up to four for the company. Friends join you at the HQ between jobs."));
            hostHow = MenuKit.Text(host, "", "text");
            hostHow.AddToClassList("text--small");
            hostButton = MenuKit.Button(Actions(host), Loc.T("Host"), () => b.StartHost(), SoundId.UiConfirm, important: true);

            VisualElement join = Card(cards, Loc.T("JOIN"), "icon/exitRight", Loc.T("Join a friend's crew."));
            joinHow = MenuKit.Text(join, "", "text");
            joinHow.AddToClassList("text--small");
            address = new TextField { value = b.Config != null ? $"{b.Config.DefaultJoinAddress}:{b.Config.Port}" : "" };
            address.isPasswordField = Core.GameSettings.StreamerMode; // 0.19.7: never show an address on stream
            address.AddToClassList("field");
            join.Add(address);
            VisualElement joinActions = Actions(join);
            MenuKit.Button(joinActions, Loc.T("Join"), () => b.StartClient(address.value), SoundId.UiConfirm, important: true);
            // 0.23.5: back to the crew you were last with (they must be at the HQ).
            string last = Core.Prefs.GetString(NetworkPlayer.LastJoinKey, "");
            if (!string.IsNullOrEmpty(last))
                MenuKit.Button(joinActions, Loc.T("Rejoin your last crew"), () => b.StartClient(last), SoundId.UiConfirm, small: true);

            status = MenuKit.Text(body, "", "text");
            status.AddToClassList("text--small");
            error = MenuKit.Text(body, "", "text");
            error.AddToClassList("text--error");
            MenuKit.Button(panel, Loc.T("Back"), menu.Back, SoundId.UiBack);
        }

        private static VisualElement Actions(VisualElement card)
        {
            var actions = new VisualElement();
            actions.AddToClassList("play__actions");
            card.Add(actions);
            return actions;
        }

        private static VisualElement Card(VisualElement parent, string title, string icon, string text)
        {
            var card = new VisualElement();
            card.style.flexDirection = FlexDirection.Column;
            card.style.flexGrow = 1;
            card.style.flexBasis = 260f;
            card.style.minWidth = 0;
            card.style.flexShrink = 0;
            card.AddToClassList("hud-card");
            card.AddToClassList("play__card");
            parent.Add(card);
            VisualElement head = MenuKit.Row(card);
            UiKit.Icon(head, icon).AddToClassList("play__icon");
            MenuKit.Text(head, title, "section").AddToClassList("play__title");
            MenuKit.Text(card, text, "text").AddToClassList("play__blurb");
            return card;
        }

        public void Refresh()
        {
            NetworkBootstrap b = menu.Bootstrap;
            address.isPasswordField = Core.GameSettings.StreamerMode;
            bool overSteam = b.Transport == TransportMode.Steam;
            steam.EnableInClassList("menu-button--on", overSteam);
            directIp.EnableInClassList("menu-button--on", !overSteam);
            hostButton.text = overSteam ? Loc.T("Host a Steam lobby") : Loc.T("Host");
            hostHow.text = overSteam ? Loc.T("Friends-only Steam lobby: invite them from the pause menu (Steam overlay), or they join from your profile.")
                : Loc.T("Friends on your network join your LAN address, port {0}.", (b.Config != null ? b.Config.Port : 0));
            joinHow.text = overSteam ? Loc.T("Accept their invite in Steam, or paste their SteamID64 here:") : Loc.T("Their address and port (address:port):");
            if (status.text != b.Status) status.text = b.Status;
            SteamLobbyFlow flow = menu.Lobby != null ? menu.Lobby.Flow : null;
            // Errors are kept (and arrive from the host) as English with their values; worded here.
            string e = Loc.FromWire(!string.IsNullOrEmpty(b.LastError) ? b.LastError : flow != null ? flow.LastError : "");
            if (error.text != e) error.text = e;
        }
    }
}

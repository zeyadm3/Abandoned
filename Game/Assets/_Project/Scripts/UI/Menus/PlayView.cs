using Abandoned.Audio;
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
            MenuKit.Text(panel, "PLAY", "heading");
            ScrollView body = MenuKit.Scroll(panel, "play__body");

            VisualElement over = MenuKit.Row(body);
            MenuKit.Text(over, "Play over", "setting__label");
            steam = MenuKit.Button(over, "Steam", () => b.SelectTransport(TransportMode.Steam), SoundId.UiClick, small: true);
            directIp = MenuKit.Button(over, "Direct IP", () => b.SelectTransport(TransportMode.UnityTransport), SoundId.UiClick, small: true);

            var cards = new VisualElement();
            cards.style.flexDirection = FlexDirection.Row;
            cards.style.flexWrap = Wrap.Wrap;
            cards.style.flexShrink = 0;
            cards.style.minWidth = 0;
            cards.AddToClassList("hud-cards");
            cards.AddToClassList("play__cards");
            body.Add(cards);

            VisualElement solo = Card(cards, "SOLO", "icon/singleplayer", "Just you and the building. Hard, not impossible: the trolley helps. Friends can still join you at the HQ.");
            MenuKit.Button(solo, "Start", () => b.StartHost(), SoundId.UiConfirm, important: true);

            VisualElement host = Card(cards, "HOST A CREW", "icon/multiplayer", "Run the company: up to four, friends join you at the HQ between jobs.");
            hostHow = MenuKit.Text(host, "", "text");
            hostHow.AddToClassList("text--small");
            hostButton = MenuKit.Button(host, "Host", () => b.StartHost(), SoundId.UiConfirm, important: true);

            VisualElement join = Card(cards, "JOIN", "icon/exitRight", "Work for a friend's company.");
            joinHow = MenuKit.Text(join, "", "text");
            joinHow.AddToClassList("text--small");
            address = new TextField { value = b.Config != null ? $"{b.Config.DefaultJoinAddress}:{b.Config.Port}" : "" };
            address.AddToClassList("field");
            join.Add(address);
            MenuKit.Button(join, "Join", () => b.StartClient(address.value), SoundId.UiConfirm, important: true);

            status = MenuKit.Text(body, "", "text");
            status.AddToClassList("text--small");
            error = MenuKit.Text(body, "", "text");
            error.AddToClassList("text--error");
            MenuKit.Button(panel, "Back", menu.Back, SoundId.UiBack);
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
            bool overSteam = b.Transport == TransportMode.Steam;
            steam.EnableInClassList("menu-button--on", overSteam);
            directIp.EnableInClassList("menu-button--on", !overSteam);
            hostButton.text = overSteam ? "Host a Steam lobby" : "Host";
            hostHow.text = overSteam ? "Friends-only Steam lobby: invite them from the pause menu (Steam overlay), or they join from your profile."
                : $"Friends on your network join your LAN address, port {(b.Config != null ? b.Config.Port : 0)}.";
            joinHow.text = overSteam ? "Accept their invite in Steam, or paste their SteamID64 here:" : "Their address and port (address:port):";
            if (status.text != b.Status) status.text = b.Status;
            SteamLobbyFlow flow = menu.Lobby != null ? menu.Lobby.Flow : null;
            string e = !string.IsNullOrEmpty(b.LastError) ? b.LastError : flow != null ? flow.LastError : "";
            if (error.text != e) error.text = e;
        }
    }
}

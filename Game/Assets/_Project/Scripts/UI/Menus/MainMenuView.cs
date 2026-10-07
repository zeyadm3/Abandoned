using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The title screen over the HQ while offline: play solo or host (Direct IP or a friends-only Steam
    /// lobby), join by address or SteamID64, settings, credits, quit. Says why the last game ended.
    /// </summary>
    public class MainMenuView
    {
        private readonly MenuUi menu;
        private readonly Label status, error, notice, addressLabel;
        private readonly VisualElement noticeBox;
        private readonly Button host, directIp, steam;
        private readonly TextField address;

        public VisualElement Root { get; }

        public MainMenuView(MenuUi menu)
        {
            this.menu = menu;
            NetworkBootstrap b = menu.Bootstrap;
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            VisualElement panel = MenuKit.Panel(Root);

            Label title = MenuKit.Text(panel, "ABANDONED", "title");
            if (menu.TitleFont != null) title.style.unityFontDefinition = FontDefinition.FromFont(menu.TitleFont);
            MenuKit.Text(panel, Demo.IsDemo ? $"DEMO - {Demo.MaxJobs} jobs at the abandoned mall. Salvage crew wanted." : "Salvage crew wanted. Buildings unstable.", "subtitle");

            noticeBox = new VisualElement();
            panel.Add(noticeBox);
            notice = MenuKit.Text(noticeBox, "", "text");
            notice.AddToClassList("text--error");
            MenuKit.Button(noticeBox, "OK", SessionEndNotice.Clear, SoundId.UiConfirm, small: true);

            host = MenuKit.Button(panel, "Play", () => b.StartHost(), SoundId.UiConfirm);

            VisualElement transport = MenuKit.Row(panel);
            MenuKit.Text(transport, "Play over", "text").AddToClassList("grow");
            directIp = MenuKit.Button(transport, "Direct IP", () => b.SelectTransport(TransportMode.UnityTransport), small: true);
            steam = MenuKit.Button(transport, "Steam", () => b.SelectTransport(TransportMode.Steam), small: true);

            addressLabel = MenuKit.Text(panel, "", "section");
            VisualElement join = MenuKit.Row(panel);
            address = new TextField { value = b.Config != null ? $"{b.Config.DefaultJoinAddress}:{b.Config.Port}" : "" };
            address.AddToClassList("field");
            address.AddToClassList("grow");
            join.Add(address);
            MenuKit.Button(join, "Join", () => b.StartClient(address.value), SoundId.UiConfirm, small: true);

            MenuKit.Button(panel, "Settings", () => menu.Push(MenuScreen.Settings));
            MenuKit.Button(panel, "Achievements", () => menu.Push(MenuScreen.Achievements));
            MenuKit.Button(panel, "Credits", () => menu.Push(MenuScreen.Credits));
            MenuKit.Button(panel, "Quit", menu.Quit, SoundId.UiBack);

            status = MenuKit.Text(panel, "", "text");
            status.AddToClassList("text--small");
            error = MenuKit.Text(panel, "", "text");
            error.AddToClassList("text--error");
            MenuKit.Text(Root, $"v{VersionInfo.Display}", "footer");
        }

        /// <summary>Every frame while showing: status, errors and the transport's wording.</summary>
        public void Refresh()
        {
            NetworkBootstrap b = menu.Bootstrap;
            bool overSteam = b.Transport == TransportMode.Steam;
            host.text = overSteam ? "Host a Steam lobby" : "Play (solo or host)";
            directIp.EnableInClassList("menu-button--on", !overSteam);
            steam.EnableInClassList("menu-button--on", overSteam);
            addressLabel.text = overSteam ? "Join a friend by their SteamID64 (or accept their Steam invite)" : "Join a friend at address:port";
            status.text = b.Status;

            string message = SessionEndNotice.Message;
            MenuKit.Show(noticeBox, message.Length > 0);
            notice.text = message;
            SteamLobbyFlow flow = menu.Lobby != null ? menu.Lobby.Flow : null;
            error.text = !string.IsNullOrEmpty(b.LastError) ? b.LastError : flow != null ? flow.LastError : "";
        }
    }
}

using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The end of the demo (M8.3): thanks, what the crew achieved, and the wishlist button (the Steam
    /// overlay's store page, or the browser). The host can start a new company to play again.
    /// </summary>
    public class DemoEndView
    {
        private readonly MenuUi menu;
        private readonly Label stats;
        private readonly Button restart;
        private readonly Label waiting;

        public VisualElement Root { get; }

        /// <summary>Tests: how the last wishlist press was handled ("overlay" / "browser").</summary>
        public static string LastWishlistRoute { get; private set; }

        public DemoEndView(MenuUi menu)
        {
            this.menu = menu;
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            Root.AddToClassList("backdrop--center");
            VisualElement panel = MenuKit.Panel(Root, wide: true);
            Label title = MenuKit.Text(panel, "THANKS FOR PLAYING", "title");
            if (menu.TitleFont != null) title.style.unityFontDefinition = FontDefinition.FromFont(menu.TitleFont);
            title.style.fontSize = 56;
            MenuKit.Text(panel, "That's the end of the ABANDONED demo.", "subtitle");
            stats = MenuKit.Text(panel, "");
            MenuKit.Text(panel, "The full game has more buildings, more threats, more loot and a lot more floors to fall through. " +
                                "Wishlisting on Steam is the best way to help a solo developer.");
            MenuKit.Button(panel, "Wishlist on Steam", Wishlist, SoundId.UiConfirm);
            restart = MenuKit.Button(panel, "Start a new company", () =>
            {
                CompanyService.Current?.RestartDemo();
                menu.Back();
            });
            waiting = MenuKit.Text(panel, "The host can start a new company to play again.");
            waiting.AddToClassList("text--small");
            MenuKit.Button(panel, "Back to the HQ", menu.Back, SoundId.UiBack);
            MenuKit.Button(panel, "Quit", menu.Quit, SoundId.UiBack);
        }

        public void Refresh()
        {
            CompanyService company = CompanyService.Current;
            bool host = company != null && company.IsServer;
            MenuKit.Show(restart, host);
            MenuKit.Show(waiting, !host);
            if (company == null) return;
            CompanyNetState s = company.State;
            stats.text = $"JOBS {s.Runs}   BEST HAUL ${s.BestHaul:N0}   COMPANY {(s.Money < 0 ? $"DEBT ${-s.Money:N0}" : $"${s.Money:N0}")}   LEVEL {s.Level}";
        }

        private static void Wishlist()
        {
            DemoConfig config = Demo.Config;
            uint appId = config != null ? config.StoreAppId : 0;
            if (SteamBootstrap.Instance != null && SteamBootstrap.Instance.OpenStorePage(appId))
            {
                LastWishlistRoute = "overlay";
                return;
            }
            LastWishlistRoute = "browser";
            string url = config != null && !string.IsNullOrEmpty(config.StoreUrl) ? config.StoreUrl : "https://store.steampowered.com/";
            if (appId != 0) url = $"https://store.steampowered.com/app/{appId}/";
            if (!Application.isBatchMode) Application.OpenURL(url);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => LastWishlistRoute = null;
    }
}

using System;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The title screen (2026-10-28 UI redo): the name, one line under it, and the ways on in a column down the left
    /// with the world visible behind. Links and the version sit in one line along the bottom. Everything else
    /// (friends, training, achievements) happens at the HQ.
    /// </summary>
    public class MainMenuView
    {
        private readonly Label notice;
        private readonly VisualElement noticeBox, rail, links;
        private readonly Button continueItem;
        private readonly EarlyAccessNotice earlyAccess;

        public VisualElement Root { get; }

        public MainMenuView(MenuUi menu)
        {
            Root = new VisualElement();
            Root.AddToClassList("main");
            Root.AddToClassList("mm-main");
            var shade = new VisualElement { pickingMode = PickingMode.Ignore };
            shade.AddToClassList("mm-main__shade");
            Root.Add(shade);

            rail = MenuKit.Scroll(Root, "mm-rail");
            Label logo = MenuKit.Text(rail, "ABANDONED", "main__logo");
            logo.AddToClassList("title");
            logo.pickingMode = PickingMode.Ignore;
            MenuKit.Text(rail, Demo.IsDemo ? Loc.T("DEMO / {0} SALVAGE JOBS", Demo.MaxJobs) : Loc.T("TAKE WHAT'S LEFT. LEAVE WHILE YOU CAN."), "subtitle");

            noticeBox = new VisualElement();
            noticeBox.AddToClassList("main__notice");
            rail.Add(noticeBox);
            notice = MenuKit.Text(noticeBox, "", "text");
            notice.style.flexGrow = 1;
            notice.style.flexShrink = 1;
            notice.AddToClassList("text--error");
            MenuKit.Button(noticeBox, Loc.T("OK"), SessionEndNotice.Clear, SoundId.UiConfirm, small: true);
            MenuKit.Show(noticeBox, false);

            var nav = new VisualElement();
            nav.AddToClassList("mm-nav");
            rail.Add(nav);
            continueItem = Item(nav, Loc.T("Continue"), () => menu.Bootstrap.StartHost(), SoundId.UiConfirm);
            var slots = new string[Company.SaveStore.Slots];
            for (int i = 0; i < slots.Length; i++) slots[i] = $"{i + 1}: {Company.SaveStore.Describe(i + 1)}";
            MenuKit.Choice(nav, Loc.T("Save"), slots, Company.SaveStore.Slot - 1, i => Company.SaveStore.Slot = i + 1);
            Item(nav, Loc.T("Settings"), () => menu.Push(MenuScreen.Settings));
            Item(nav, Loc.T("Credits"), () => menu.Push(MenuScreen.Credits));
            // Development builds keep the direct join (LAN, nettests, Multiplayer Play Mode).
            if (DevTools.Enabled) Item(nav, Loc.T("Join by address (dev)"), () => menu.Push(MenuScreen.Play));
            Item(nav, Loc.T("Quit"), menu.Quit, SoundId.UiBack);

            var status = new VisualElement();
            status.AddToClassList("mm-status");
            Root.Add(status);
            links = MenuKit.Row(status);
            links.AddToClassList("mm-status__links");
            if (Launch.HasDiscord) MenuKit.Button(links, "Discord", Launch.OpenDiscord, small: true);
            if (Launch.HasFeedback) MenuKit.Button(links, Loc.T("Feedback"), Launch.OpenFeedback, small: true);
            MenuKit.Button(links, Loc.T("Open logs"), Launch.OpenLogFolder, small: true);
            MenuKit.Text(status, $"v{VersionInfo.Display}", "mm-status__version").pickingMode = PickingMode.Ignore;

            // Last, so it sits over everything until Continue.
            earlyAccess = new EarlyAccessNotice(Root, menu.TitleFont, () =>
            {
                rail.SetEnabled(true);
                links.SetEnabled(true);
                continueItem.Focus();
            });
            rail.SetEnabled(!earlyAccess.Visible);
            links.SetEnabled(!earlyAccess.Visible);
        }

        private static Button Item(VisualElement nav, string text, Action click, SoundId sound = SoundId.UiClick)
        {
            Button button = MenuKit.Button(nav, text, click, sound);
            button.AddToClassList("mm-nav__item");
            return button;
        }

        /// <summary>Every frame while showing: only the last game's notice can change.</summary>
        public void Refresh()
        {
            // Kept (and sent by the host) as English with its values; worded here.
            string message = Loc.FromWire(SessionEndNotice.Message);
            if (notice.text == message) return;
            notice.text = message;
            MenuKit.Show(noticeBox, message.Length > 0);
        }
    }
}

using System;
using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The entry screen (laid out again in 0.24.12): a rail down the left with the stencil title and the five ways
    /// on, a card on the right that says what the highlighted one does (and carries the rotating recovery memo),
    /// and a status bar along the bottom with the links, the keys and the version. The world stays visible behind.
    /// </summary>
    public class MainMenuView
    {
        private static readonly string[] Memos =
        {
            "RECOVERY NOTICE / The building is abandoned. You are not alone.", // @loc
            "STRUCTURAL WARNING / Listen to the floor. Weight has consequences.", // @loc
            "EXTRACTION NOTICE / The truck will leave. Make it back before it does.", // @loc
            "SALVAGE RECORD / Take what's left. Leave while you can.", // @loc
        };

        private readonly MenuUi menu;
        private readonly Label notice, memo, cardIndex, cardTitle, cardText;
        private readonly VisualElement noticeBox, rail, links, nav;
        private readonly List<Button> items = new();
        private readonly MenuTitle title;
        private readonly EarlyAccessNotice earlyAccess;
        private int memoShown = -1;
        private Button continueItem;

        // The chosen save, or a new company for a new hire; friends join you at the HQ.
        private static string ContinueText()
        {
            int slot = Company.SaveStore.Slot;
            string saved = Company.SaveStore.Describe(slot);
            return Loc.T("Save {0}: {1}. To the HQ; invite friends from the crew board there.", slot, saved);
        }

        public VisualElement Root { get; }

        public MainMenuView(MenuUi menu)
        {
            this.menu = menu;
            Root = new VisualElement();
            Root.AddToClassList("main");
            Root.AddToClassList("mm-main");
            var shade = new VisualElement { pickingMode = PickingMode.Ignore };
            shade.AddToClassList("mm-main__shade");
            Root.Add(shade);

            rail = MenuKit.Scroll(Root, "mm-rail");
            MenuKit.Text(rail, Loc.T("ABANDONED SALVAGING / RECOVERY DIVISION"), "mm-eyebrow");
            title = new MenuTitle(rail, "ABANDONED", menu.TitleFont, MenuEffectsConfig.Current);
            MenuKit.Text(rail, Demo.IsDemo ? Loc.T("DEMO / {0} SALVAGE JOBS", Demo.MaxJobs) : Loc.T("TAKE WHAT'S LEFT. LEAVE WHILE YOU CAN."), "subtitle");
            var rule = new VisualElement { pickingMode = PickingMode.Ignore };
            rule.AddToClassList("mm-rule");
            rail.Add(rule);

            noticeBox = new VisualElement();
            noticeBox.AddToClassList("main__notice");
            rail.Add(noticeBox);
            notice = MenuKit.Text(noticeBox, "", "text");
            notice.style.flexGrow = 1;
            notice.style.flexShrink = 1;
            notice.AddToClassList("text--error");
            MenuKit.Button(noticeBox, Loc.T("OK"), SessionEndNotice.Clear, SoundId.UiConfirm, small: true);

            nav = new VisualElement();
            nav.AddToClassList("mm-nav");
            rail.Add(nav);

            var card = new VisualElement { pickingMode = PickingMode.Ignore };
            card.AddToClassList("mm-card");
            Root.Add(card);
            cardIndex = MenuKit.Text(card, "", "mm-card__index");
            cardTitle = MenuKit.Text(card, "", "mm-card__title");
            cardText = MenuKit.Text(card, "", "mm-card__text");
            memo = MenuKit.Text(card, "", "mm-card__memo");

            // 2026 remake: a short title screen. Everything else happens at the HQ: friends are invited from the crew
            // board, training is the clipboard by the dispatch telephone, achievements are in the trophy cabinet.
            continueItem = Item(Loc.T("Continue"), "", () => menu.Bootstrap.StartHost(), SoundId.UiConfirm);
            var slots = new string[Company.SaveStore.Slots];
            for (int i = 0; i < slots.Length; i++) slots[i] = $"{i + 1}: {Company.SaveStore.Describe(i + 1)}";
            MenuKit.Choice(nav, Loc.T("Save"), slots, Company.SaveStore.Slot - 1, i =>
            {
                Company.SaveStore.Slot = i + 1;
                Describe(0);
            });
            Item(Loc.T("Settings"), Loc.T("Controls, picture, sound, voice and accessibility."), () => menu.Push(MenuScreen.Settings));
            Item(Loc.T("Credits"), Loc.T("The people and the free assets behind the game."), () => menu.Push(MenuScreen.Credits));
            // Development builds keep the direct join (LAN, nettests, Multiplayer Play Mode).
            if (DevTools.Enabled)
                Item(Loc.T("Join by address (dev)"), Loc.T("Host or join over the network directly. Development builds only."), () => menu.Push(MenuScreen.Play));
            Item(Loc.T("Quit"), Loc.T("Leave the building. The salvage will still be there."), menu.Quit, SoundId.UiBack, important: true);

            var status = new VisualElement();
            status.AddToClassList("mm-status");
            Root.Add(status);
            links = MenuKit.Row(status);
            links.AddToClassList("mm-status__links");
            if (Launch.HasDiscord) MenuKit.Button(links, "Discord", Launch.OpenDiscord, small: true);
            if (Launch.HasFeedback) MenuKit.Button(links, Loc.T("Feedback"), Launch.OpenFeedback, small: true);
            MenuKit.Button(links, Loc.T("Open logs"), Launch.OpenLogFolder, small: true);
            MenuKit.Text(status, $"Enter   {Loc.T("Select")}        Esc   {Loc.T("Back")}", "mm-status__keys").pickingMode = PickingMode.Ignore;
            MenuKit.Text(status, $"v{VersionInfo.Display}  -  {Loc.T(EarlyAccessNotice.Tag)}", "mm-status__version").pickingMode = PickingMode.Ignore;

            // Last, so it sits over everything until Continue (friends-and-family build, 2026-10-08).
            earlyAccess = new EarlyAccessNotice(Root, menu.TitleFont, () =>
            {
                rail.SetEnabled(true);
                links.SetEnabled(true);
                items[0].Focus();
            });
            rail.SetEnabled(!earlyAccess.Visible);
            links.SetEnabled(!earlyAccess.Visible);
            Describe(0);
        }

        private Button Item(string text, string description, Action click, SoundId sound = SoundId.UiClick, bool important = false)
        {
            int index = items.Count;
            Button button = MenuKit.Button(nav, text, click, sound, important: important);
            button.AddToClassList("mm-nav__item");
            button.userData = description;
            button.RegisterCallback<PointerEnterEvent>(_ => Describe(index));
            button.RegisterCallback<FocusInEvent>(_ => Describe(index));
            items.Add(button);
            return button;
        }

        // The card follows the pointer and the focus; the item it describes stays marked.
        private void Describe(int index)
        {
            if (index < 0 || index >= items.Count) return;
            for (int i = 0; i < items.Count; i++) items[i].EnableInClassList("mm-nav__item--active", i == index);
            cardIndex.text = $"{index + 1:00} / {items.Count:00}";
            cardTitle.text = items[index].text;
            cardText.text = items[index] == continueItem ? ContinueText() : items[index].userData as string ?? "";
        }

        /// <summary>Every frame while showing: the last game's notice, the memo, the logo's signal.</summary>
        public void Refresh()
        {
            // Kept (and sent by the host) as English with its values, like the Play screen's errors; worded here.
            string message = Loc.FromWire(SessionEndNotice.Message);
            rail.SetEnabled(!earlyAccess.Visible);
            links.SetEnabled(!earlyAccess.Visible);
            MenuKit.Show(noticeBox, message.Length > 0);
            if (notice.text != message) notice.text = message;
            int m = (int)(Time.unscaledTime / 9f) % Memos.Length;
            if (m != memoShown)
            {
                memoShown = m;
                memo.text = Loc.T(Memos[m]);
            }
            title.Tick();
        }
    }
}

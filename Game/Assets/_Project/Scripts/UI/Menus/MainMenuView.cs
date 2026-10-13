using System;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The abandoned entry screen: fractured structural framing, worn stencil lettering and an empty
    /// world behind the recovery record. The actual navigation remains readable and easy to focus.
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
        private readonly Label notice, memo;
        private readonly VisualElement noticeBox, column, links;
        private readonly MenuTitle title;
        private readonly EarlyAccessNotice earlyAccess;
        private int memoShown = -1;

        public VisualElement Root { get; }

        public MainMenuView(MenuUi menu)
        {
            this.menu = menu;
            Root = new VisualElement();
            Root.AddToClassList("main");
            var shade = new VisualElement { pickingMode = PickingMode.Ignore };
            shade.AddToClassList("main__shade");
            Root.Add(shade);
            BuildRuin();
            MenuKit.Text(Root, Loc.T("ABANDONED SALVAGING / RECOVERY DIVISION"), "main__eyebrow");
            column = MenuKit.Scroll(Root, "main__column");

            title = new MenuTitle(column, "ABANDONED", menu.TitleFont, MenuEffectsConfig.Current);
            MenuKit.Text(column, Demo.IsDemo ? Loc.T("DEMO / {0} JOBS AT THE ABANDONED MALL", Demo.MaxJobs) : Loc.T("TAKE WHAT'S LEFT. LEAVE WHILE YOU CAN."), "subtitle");
            var fracture = new VisualElement { pickingMode = PickingMode.Ignore };
            fracture.AddToClassList("main__fracture");
            column.Add(fracture);

            noticeBox = new VisualElement();
            noticeBox.AddToClassList("main__notice");
            column.Add(noticeBox);
            notice = MenuKit.Text(noticeBox, "", "text");
            notice.style.flexGrow = 1;
            notice.style.flexShrink = 1;
            notice.AddToClassList("text--error");
            MenuKit.Button(noticeBox, Loc.T("OK"), SessionEndNotice.Clear, SoundId.UiConfirm, small: true);

            var actions = new VisualElement();
            actions.AddToClassList("main__actions");
            column.Add(actions);
            Navigation(actions, "01", Loc.T("Play"), () => menu.Push(MenuScreen.Play), SoundId.UiConfirm);
            Navigation(actions, "02", Loc.T("Settings"), () => menu.Push(MenuScreen.Settings));
            Navigation(actions, "03", Loc.T("Achievements"), () => menu.Push(MenuScreen.Achievements));
            Navigation(actions, "04", Loc.T("Credits"), () => menu.Push(MenuScreen.Credits));
            Navigation(actions, "05", Loc.T("Quit"), menu.Quit, SoundId.UiBack, important: true);

            var footer = new VisualElement();
            footer.AddToClassList("main__footer");
            Root.Add(footer);
            links = MenuKit.Row(footer);
            links.style.flexWrap = Wrap.Wrap;
            links.AddToClassList("main__links");
            if (Launch.HasDiscord) MenuKit.Button(links, "Discord", Launch.OpenDiscord, small: true);
            if (Launch.HasFeedback) MenuKit.Button(links, Loc.T("Feedback"), Launch.OpenFeedback, small: true);
            MenuKit.Button(links, Loc.T("Open logs"), Launch.OpenLogFolder, small: true);

            VisualElement record = MenuKit.Row(footer);
            record.AddToClassList("main__record");
            record.style.flexWrap = Wrap.Wrap;
            memo = MenuKit.Text(record, "", "main__memo");
            memo.style.flexShrink = 1;
            Label version = MenuKit.Text(record, $"v{VersionInfo.Display}  -  {Loc.T(EarlyAccessNotice.Tag)}", "footer");
            version.style.flexShrink = 1;
            // Last, so it sits over everything until Continue (friends-and-family build, 2026-10-08).
            earlyAccess = new EarlyAccessNotice(Root, menu.TitleFont, () =>
            {
                column.SetEnabled(true);
                links.SetEnabled(true);
                actions.Q<Button>()?.Focus();
            });
            column.SetEnabled(!earlyAccess.Visible);
            links.SetEnabled(!earlyAccess.Visible);
        }

        private void Navigation(VisualElement parent, string number, string text, Action click,
            SoundId sound = SoundId.UiClick, bool important = false)
        {
            VisualElement row = MenuKit.Row(parent);
            row.AddToClassList("main__action");
            Label index = MenuKit.Text(row, number, "main__index");
            index.pickingMode = PickingMode.Ignore;
            Button button = MenuKit.Button(row, text, click, sound, important: important);
            button.AddToClassList("main__button");
            bool hovered = false, focused = false;
            void Mark() => row.EnableInClassList("main__action--active", hovered || focused);
            button.RegisterCallback<PointerEnterEvent>(_ => { hovered = true; Mark(); });
            button.RegisterCallback<PointerLeaveEvent>(_ => { hovered = false; Mark(); });
            button.RegisterCallback<FocusInEvent>(_ => { focused = true; Mark(); });
            button.RegisterCallback<FocusOutEvent>(_ => { focused = false; Mark(); });
        }

        private void BuildRuin()
        {
            // A broken structural A echoes the title's stencil cuts without covering the world backdrop.
            var ruin = new VisualElement { pickingMode = PickingMode.Ignore };
            ruin.AddToClassList("main__ruin");
            Root.Add(ruin);
            Part("main__ruin-leg", "main__ruin-leg--left");
            Part("main__ruin-leg", "main__ruin-leg--right");
            Part("main__ruin-beam", "main__ruin-beam--left");
            Part("main__ruin-beam", "main__ruin-beam--right");
            Part("main__ruin-fault");
            MenuKit.Text(ruin, Loc.T("STRUCTURE\nCOMPROMISED"), "main__ruin-caption").pickingMode = PickingMode.Ignore;

            void Part(params string[] classes)
            {
                var part = new VisualElement { pickingMode = PickingMode.Ignore };
                foreach (string cls in classes) part.AddToClassList(cls);
                ruin.Add(part);
            }
        }

        /// <summary>Every frame while showing: the last game's notice, the memo, the logo's signal.</summary>
        public void Refresh()
        {
            string message = SessionEndNotice.Message;
            column.SetEnabled(!earlyAccess.Visible);
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

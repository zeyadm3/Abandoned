using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The title screen over the HQ garage while offline (UI step 6, R.E.P.O.-style): the stencilled logo and
    /// a short list of big words (Play, Settings, Achievements, Credits, Quit) on a shaded left side, the
    /// garage drifting slowly behind, a company memo and the version at the bottom. Says why the last game
    /// ended. Play opens <see cref="PlayView"/> (solo, host, join).
    /// </summary>
    public class MainMenuView
    {
        private static readonly string[] Memos =
        {
            "COMPANY MEMO: Early Access. Report problems from the pause menu; the boss reads them. Eventually.",
            "COMPANY MEMO: Hard hats are mandatory. They do nothing against a floor, but they're mandatory.",
            "COMPANY MEMO: The truck leaves when it leaves. Be in it.",
            "COMPANY MEMO: If it's worth money, it's ours. If it's on fire, it's yours.",
            "COMPANY MEMO: Press the scan key in the building. Prices go up when you know them.",
        };

        private readonly MenuUi menu;
        private readonly Label notice, memo;
        private readonly VisualElement noticeBox;
        private Camera driftCamera;
        private Quaternion driftBase;
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
            var column = new VisualElement();
            column.style.flexDirection = FlexDirection.Column;
            column.style.minWidth = 0;
            column.AddToClassList("main__column");
            Root.Add(column);

            Label title = MenuKit.Text(column, "ABANDONED", "title");
            title.AddToClassList("main__logo");
            if (menu.TitleFont != null) title.style.unityFontDefinition = FontDefinition.FromFont(menu.TitleFont);
            MenuKit.Text(column, Demo.IsDemo ? $"DEMO - {Demo.MaxJobs} jobs at the abandoned mall" : "SALVAGE CREW WANTED. BUILDINGS UNSTABLE.", "subtitle");
            UiKit.Hazard(column).AddToClassList("main__tape");

            noticeBox = new VisualElement();
            noticeBox.AddToClassList("main__notice");
            column.Add(noticeBox);
            notice = MenuKit.Text(noticeBox, "", "text");
            notice.style.flexGrow = 1;
            notice.style.flexShrink = 1;
            notice.AddToClassList("text--error");
            MenuKit.Button(noticeBox, "OK", SessionEndNotice.Clear, SoundId.UiConfirm, small: true);

            MenuKit.Button(column, "Play", () => menu.Push(MenuScreen.Play), SoundId.UiConfirm).AddToClassList("main__button");
            MenuKit.Button(column, "Settings", () => menu.Push(MenuScreen.Settings)).AddToClassList("main__button");
            MenuKit.Button(column, "Achievements", () => menu.Push(MenuScreen.Achievements)).AddToClassList("main__button");
            MenuKit.Button(column, "Credits", () => menu.Push(MenuScreen.Credits)).AddToClassList("main__button");
            MenuKit.Button(column, "Quit", menu.Quit, SoundId.UiBack).AddToClassList("main__button");

            VisualElement links = MenuKit.Row(Root);
            links.style.flexWrap = Wrap.Wrap;
            links.AddToClassList("main__links");
            if (Launch.HasDiscord) MenuKit.Button(links, "Discord", Launch.OpenDiscord, small: true);
            if (Launch.HasFeedback) MenuKit.Button(links, "Feedback", Launch.OpenFeedback, small: true);
            MenuKit.Button(links, "Open logs", Launch.OpenLogFolder, small: true);

            memo = MenuKit.Text(Root, "", "main__memo");
            MenuKit.Text(Root, $"v{VersionInfo.Display}", "footer");
        }

        /// <summary>Every frame while showing: the last game's notice, the memo, the drifting garage.</summary>
        public void Refresh()
        {
            string message = SessionEndNotice.Message;
            MenuKit.Show(noticeBox, message.Length > 0);
            if (notice.text != message) notice.text = message;
            int m = (int)(Time.unscaledTime / 9f) % Memos.Length;
            if (m != memoShown)
            {
                memoShown = m;
                memo.text = Memos[m];
            }
            Drift();
        }

        // A slow look around the garage behind the menu (the HQ's scene camera; nobody is playing yet).
        private void Drift()
        {
            Camera cam = Camera.main;
            if (cam == null || menu.Bootstrap.IsRunning) { driftCamera = null; return; }
            if (cam != driftCamera)
            {
                driftCamera = cam;
                driftBase = cam.transform.rotation;
            }
            float t = Time.unscaledTime;
            cam.transform.rotation = driftBase * Quaternion.Euler(Mathf.Sin(t * 0.11f) * 1.5f, Mathf.Sin(t * 0.07f) * 4f, 0f);
        }
    }
}

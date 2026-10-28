using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Esc during a game (cut down in the 2026 remake: everything else lives in the HQ): resume, settings, leave and
    /// quit, plus the training job's own buttons in the practice house. The game keeps running behind a dimmed screen.
    /// The crew, invites and the host's rules are on the HQ crew board; voice levels are in Settings > Voice too;
    /// achievements are in the trophy cabinet and how to play is on the notice board.
    /// </summary>
    public class PauseMenuView
    {
        private readonly MenuUi menu;
        private readonly Button leave;
        private int leaveState = -1;
        private readonly Tutorial.TutorialPauseControls training;

        public VisualElement Root { get; }

        public PauseMenuView(MenuUi menu)
        {
            this.menu = menu;
            Root = new VisualElement();
            Root.AddToClassList("backdrop");
            Root.AddToClassList("backdrop--dim");
            Root.AddToClassList("pause");
            ScrollView content = MenuKit.Scroll(Root, "pause__scroll");
            content.contentContainer.AddToClassList("pause__content");
            content.style.width = Length.Percent(100f);
            content.style.height = Length.Percent(100f);
            var columns = new VisualElement();
            columns.AddToClassList("pause__columns");
            content.Add(columns);

            VisualElement left = new();
            left.AddToClassList("pause__left");
            columns.Add(left);
            MenuKit.Text(left, Loc.T("PAUSED"), "title");
            MenuKit.Text(left, Loc.T("The game keeps running. The building does not wait."), "subtitle").AddToClassList("pause__warning");
            MenuKit.Button(left, Loc.T("Resume"), menu.Resume, SoundId.UiConfirm).AddToClassList("pause__resume");
            training = new Tutorial.TutorialPauseControls(left, menu);
            MenuKit.Button(left, Loc.T("Settings"), () => menu.Push(MenuScreen.Settings));
            leave = MenuKit.Button(left, Loc.T("Leave game"), () =>
            {
                Company.CompanyService.Current?.MarkLeavingMidJob();
                menu.Bootstrap.Disconnect();
            }, SoundId.UiBack, important: true);
            MenuKit.Button(left, Loc.T("Quit to desktop"), () =>
            {
                Company.CompanyService.Current?.MarkLeavingMidJob();
                menu.Quit();
            }, SoundId.UiBack, important: true);
        }

        public void Refresh()
        {
            NetworkBootstrap b = menu.Bootstrap;
            bool isHost = b.Manager != null && b.Manager.IsHost;
            training.Refresh();
            bool midJob = isHost && Company.CompanyService.Current != null && Company.CompanyService.Current.JobInProgress;
            int state = !isHost ? 0 : midJob ? 1 : 2;
            if (state == leaveState) return;
            leaveState = state;
            leave.text = state == 0 ? Loc.T("Leave game") : state == 1 ? Loc.T("End game (job counts as failed)") : Loc.T("End game for everyone");
        }
    }
}

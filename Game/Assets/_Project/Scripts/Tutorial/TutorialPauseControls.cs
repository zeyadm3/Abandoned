using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.UI;
using UnityEngine.UIElements;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The pause menu's training buttons, shown only in the practice house: the host may skip the current lesson,
    /// put the crew back at the start of the chapter (its salvage back in place) or take everyone home to the HQ.
    /// Everyone else is told the host has them. Every player can switch the trainer's voice and the guide arrows.
    /// </summary>
    public sealed class TutorialPauseControls
    {
        private readonly VisualElement root;
        private readonly Button skip, restart, leave;
        private readonly Label note;

        public TutorialPauseControls(VisualElement parent, MenuUi menu)
        {
            root = new VisualElement();
            root.AddToClassList("pause__training");
            parent.Add(root);
            MenuKit.Text(root, Loc.T("TRAINING"), "section");
            skip = MenuKit.Button(root, Loc.T("Skip this lesson"), () =>
            {
                RunTraining.Current?.RequestSkip();
                menu.Resume();
            }, SoundId.UiConfirm);
            restart = MenuKit.Button(root, Loc.T("Restart this chapter"), () =>
            {
                RunTraining.Current?.RequestResetChapter();
                menu.Resume();
            }, SoundId.UiConfirm);
            leave = MenuKit.Button(root, Loc.T("Leave training (back to the HQ)"), () =>
            {
                CompanyService.Current?.ReturnFromTraining();
                menu.Resume();
            }, SoundId.UiBack);
            note = MenuKit.Text(root, "", "text");
            note.AddToClassList("text--small");
            // Each player's own: how much hand-holding they want.
            MenuKit.Toggle(root, Loc.T("Trainer's voice"), TutorialSettings.Voice, on => TutorialSettings.Voice = on);
            MenuKit.Toggle(root, Loc.T("Guide arrows"), TutorialSettings.Arrows, on => TutorialSettings.Arrows = on);
        }

        public void Refresh()
        {
            RunTraining training = RunTraining.Current;
            bool show = TutorialMode.Active && training != null && training.IsSpawned;
            MenuKit.Show(root, show);
            if (!show) return;
            bool host = training.IsServer;
            CompanyService company = CompanyService.Current;
            MenuKit.Show(skip, host && training.CanSkip);
            MenuKit.Show(restart, host && !training.Finished);
            MenuKit.Show(leave, host && company != null && company.IsSpawned);
            string text = host ? Loc.T("Training never pays and nothing here is saved.") : Loc.T("The host can skip lessons, restart a chapter or end the training.");
            if (note.text != text) note.text = text;
        }
    }
}

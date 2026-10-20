using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The training job's guidance on this machine. The lesson card at the top (what to do, its keys lighting up
    /// as they're pressed, the lesson's own progress bar); the chapter checklist on the left; a marker over the
    /// target with an edge arrow when it's off screen; chevrons on the floor along the way there; frames around
    /// the HUD pieces a lesson talks about; and the voices (the dispatcher's radio line, then the trainer reading
    /// the lesson). A finished lesson stays on the card, stamped DONE, for a moment before the next slides in.
    /// It also measures this player for the personal lessons (<see cref="TutorialWatch"/>) and records the
    /// training as done. Local and cosmetic, on the session object; it rebuilds itself when the HUD is emptied.
    /// </summary>
    public class TutorialHud : MonoBehaviour
    {
        private const float DoneHold = 0.9f;

        private readonly TutorialWatch watch = new();
        private readonly TutorialRadioVoice voice = new();
        private readonly TutorialObjectiveCard card = new();
        private readonly TutorialChecklist checklist = new();
        private readonly TutorialWaypoint waypoint = new();
        private readonly TutorialHighlights highlights = new();
        private TutorialFloorPath floor;
        private int shown = -1, quarter;
        private float holdUntil, nextMeter;
        private bool recorded;

        private void Awake()
        {
            // Never in batch runs (tests, nettests, captures): it would sit over the screens they check.
            if (Application.isBatchMode) enabled = false;
        }

        private void Update()
        {
            RunTraining training = RunTraining.Current;
            bool live = TutorialMode.Active && training != null && training.IsSpawned;
            if (!live)
            {
                if (shown >= 0) voice.Stop();
                shown = -1;
                recorded = false;
                HideAll();
                if (floor != null) Destroy(floor.gameObject);
                return;
            }
            watch.Tick(training);
            voice.Tick();
            if (training.Finished && !recorded)
            {
                recorded = true;
                TutorialProgress.MarkDone();
                ToastFeed.Show(Loc.T("TRAINING COMPLETE"), Loc.T("That's the job. The real ones pay, and the buildings fight back."));
            }
            Follow(training);
            Draw(training);
        }

        // The card shows the crew's lesson; a finished one is held, stamped DONE, before the next comes in.
        private void Follow(RunTraining training)
        {
            int step = training.Step;
            if (step == shown || Time.unscaledTime < holdUntil) return;
            bool finishedOne = shown >= 0 && step > shown && holdUntil <= 0f;
            if (finishedOne)
            {
                holdUntil = Time.unscaledTime + DoneHold;
                card.Progress(1f, null, true);
                GameAudio.PlayUi(SoundId.UiConfirm, 0.4f);
                return;
            }
            bool first = shown < 0;
            shown = step;
            holdUntil = nextMeter = 0f;
            quarter = 0;
            TutorialStepDefinition s = training.CurrentStep;
            if (s == null) return;
            // The radio and the trainer go with the new lesson, even under a menu.
            if (!string.IsNullOrEmpty(s.RadioLine))
            {
                GameAudio.PlayUi(SoundId.RadioStatic, 0.3f);
                RunHorrorDirector.Say(s.RadioLine, TutorialConfig.Current.RadioSeconds);
            }
            voice.Speak(gameObject, s.RadioLine, s.VoiceLine, first ? 1.2f : 0.3f);
            card.Lesson(s, step, training.Catalog.Count, CompanyService.CrewSize, slideIn: !first);
        }

        private void Draw(RunTraining training)
        {
            NetworkPlayer me = NetworkPlayer.Local;
            RunState run = training.Run;
            TutorialStepDefinition s = training.Catalog.At(shown);
            bool departed = run != null && run.IsSpawned && run.State.Phase == RunPhase.Departed;
            bool visible = s != null && !training.Finished && !departed && me != null && me.IsSpawned && HudLayer.Root != null
                           && (MenuUi.Current == null || !MenuUi.Current.ClipMode);
            card.Show(visible);
            checklist.Show(visible);
            bool holding = Time.unscaledTime < holdUntil;
            bool arrows = visible && !holding && TutorialSettings.Arrows && shown == training.Step;
            TutorialGuide g = training.Guide;
            waypoint.Tick(g, arrows);
            if (floor == null) floor = TutorialFloorPath.Create();
            floor.Draw(g, arrows);
            highlights.Set(visible ? s.Highlight : "");
            highlights.Tick(visible && !holding);
            if (!visible) return;
            // A rebuilt card (a language change) is filled again.
            card.Lesson(s, shown, training.Catalog.Count, CompanyService.CrewSize, slideIn: false);
            checklist.Refresh(training, shown);
            card.Tick();
            // The bar eases itself (TrainingHud.uss), so ten updates a second are plenty.
            if (holding || Time.unscaledTime < nextMeter) return;
            nextMeter = Time.unscaledTime + 0.1f;
            float fraction;
            TutorialReadout readout;
            float value, goal;
            if (s.Personal) fraction = watch.Progress(s, out readout, out value, out goal);
            else
            {
                fraction = g.Fraction;
                readout = g.Readout;
                value = g.Value;
                goal = g.Goal;
            }
            card.Progress(fraction, TutorialGuideText.Readout(readout, value, goal), false);
            Tick(fraction, readout);
        }

        // A soft tick each quarter of the way, for lessons with something to count.
        private void Tick(float fraction, TutorialReadout readout)
        {
            int now = Mathf.Clamp(Mathf.FloorToInt(fraction * 4f), 0, 4);
            if (now > quarter && readout != TutorialReadout.None && now < 4) GameAudio.PlayUi(SoundId.ChargeTick, 0.18f);
            quarter = Mathf.Max(quarter, now);
        }

        private void HideAll()
        {
            card.Show(false);
            checklist.Show(false);
            waypoint.Hide();
            highlights.Tick(false);
            highlights.Clear();
        }

        private void OnDisable()
        {
            voice.Stop();
            HideAll();
            if (floor != null) Destroy(floor.gameObject);
        }

        private void OnDestroy()
        {
            card.Remove();
            checklist.Remove();
            waypoint.Remove();
            highlights.Clear();
        }
    }
}

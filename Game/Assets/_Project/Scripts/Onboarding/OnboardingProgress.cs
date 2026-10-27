using System;
using Abandoned.Core;
using Abandoned.Player;
using Abandoned.Tutorial;

namespace Abandoned.Onboarding
{
    /// <summary>
    /// This player's hiring, on their own machine (Prefs, like the wardrobe): never in the company save. A machine
    /// that played before the hiring existed (a finished training, a run, a company save) counts as hired, but still
    /// sees the character creator once. Quitting part way resumes at the same stage on the next launch.
    /// </summary>
    public static class OnboardingProgress
    {
        private const string StageKey = "onboarding.stage", CreatorKey = "onboarding.creator", TrainingStartKey = "onboarding.trainingStart",
            TrainingMinutesKey = "onboarding.trainingMinutes";

        public static OnboardingStage Stage
        {
            get
            {
                if (!Prefs.HasKey(StageKey)) Prefs.SetInt(StageKey, (int)(PlayedBefore ? OnboardingStage.Hired : OnboardingStage.New));
                return (OnboardingStage)Prefs.GetInt(StageKey, 0);
            }
            set
            {
                Prefs.SetInt(StageKey, (int)value);
                Prefs.Save();
            }
        }

        /// <summary>The worker and their kit have been picked (the creator after "you're hired").</summary>
        public static bool CreatorDone
        {
            get => Prefs.GetInt(CreatorKey, 0) == 1;
            set
            {
                Prefs.SetInt(CreatorKey, value ? 1 : 0);
                Prefs.Save();
            }
        }

        /// <summary>Anything left before this player reaches the HQ as one of the crew.</summary>
        public static bool Pending => Stage != OnboardingStage.Hired || !CreatorDone;

        /// <summary>In the training job as part of being hired: it can't be skipped or left.</summary>
        public static bool InHiringTraining => Stage == OnboardingStage.Interviewed && TutorialMode.Active;

        /// <summary>How long the hiring's training took, in minutes (0 when unknown); HR mentions it.</summary>
        public static float TrainingMinutes => Prefs.GetFloat(TrainingMinutesKey, 0f);

        public static void TrainingStarted()
        {
            if (Prefs.GetString(TrainingStartKey, "").Length > 0) return;
            Prefs.SetString(TrainingStartKey, DateTime.UtcNow.Ticks.ToString());
            Prefs.Save();
        }

        public static void TrainingFinished()
        {
            if (long.TryParse(Prefs.GetString(TrainingStartKey, ""), out long ticks))
                Prefs.SetFloat(TrainingMinutesKey, (float)(DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalMinutes);
            Stage = OnboardingStage.Trained;
        }

        private static bool PlayedBefore => TutorialProgress.Done || PlayerProfile.Runs > 0 || Company.SaveStore.AnyExists;

        /// <summary>Tests and the dev menu: start again as a new hire.</summary>
        public static void Reset()
        {
            foreach (string k in new[] { StageKey, CreatorKey, TrainingStartKey, TrainingMinutesKey }) Prefs.Delete(k);
            Prefs.SetInt(StageKey, (int)OnboardingStage.New);
            Prefs.Save();
        }
    }
}

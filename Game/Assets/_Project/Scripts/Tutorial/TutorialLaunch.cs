using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The ways into training, on the session object. The main menu's Training card hosts a game and asks for it
    /// (<see cref="Request"/>): once the HQ is up, the host's crew is driven to the house. And a player who has never
    /// finished training is told about the clipboard by the dispatch telephone, once per session at the HQ.
    /// </summary>
    public class TutorialLaunch : MonoBehaviour
    {
        // A request older than this was for a host that never started (the menu was left, the port was busy).
        private const float RequestLifetime = 45f, SettleSeconds = 1.5f;

        private static float requestedAt = -1f;
        private float homeSince = -1f;
        private bool reminded;

        /// <summary>The main menu asked for training: go there as soon as this machine hosts at the HQ.</summary>
        public static void Request() => requestedAt = Time.unscaledTime;

        public static bool Pending => requestedAt >= 0f && Time.unscaledTime - requestedAt < RequestLifetime;

        private void Update()
        {
            CompanyService company = CompanyService.Current;
            SessionTravel travel = SessionTravel.Current;
            bool home = company != null && company.IsSpawned && travel != null && travel.IsSpawned && CompanyService.AtHome
                        && NetworkPlayer.Local != null && NetworkPlayer.Local.IsSpawned;
            if (!home)
            {
                homeSince = -1f;
                if (requestedAt >= 0f && !Pending) requestedAt = -1f;
                return;
            }
            if (homeSince < 0f) homeSince = Time.unscaledTime;
            if (Time.unscaledTime - homeSince < SettleSeconds) return;
            if (Pending)
            {
                requestedAt = -1f;
                if (company.IsServer && CompanyService.TrainingAvailable) company.StartTraining();
                return;
            }
            Remind();
        }

        private void Remind()
        {
            if (reminded || Application.isBatchMode || TutorialProgress.Done || !TutorialConfig.Current.RemindAtHq || !CompanyService.TrainingAvailable) return;
            reminded = true;
            ToastFeed.Show(Loc.T("NEW TO THE JOB?"), Loc.T("The training clipboard by the dispatch telephone takes the crew to a practice house. No pay, no risk."));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => requestedAt = -1f;
    }
}

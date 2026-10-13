using Abandoned.Core;
using System;
using Abandoned.Company;
using Abandoned.Contracts;
using Abandoned.Extraction;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// The daily job's leaderboard (0.19.12): when a daily job's truck leaves with a living crew, every player
    /// submits the crew's haul to that day's Steam leaderboard ("daily_20261010", best score kept) and is told
    /// the crew's rank. Needs the game's own App ID and Steam running; the best haul is also kept locally.
    /// </summary>
    public class DailyLeaderboard : MonoBehaviour
    {
        private RunState hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode) return;
            var go = new GameObject("DailyLeaderboard");
            DontDestroyOnLoad(go);
            go.AddComponent<DailyLeaderboard>();
        }

        private void Update()
        {
            RunState run = RunState.Current;
            if (run == hooked) return;
            if (hooked != null) hooked.Departed -= OnDeparted;
            hooked = run;
            if (hooked != null) hooked.Departed += OnDeparted;
        }

        private static void OnDeparted(RunResults results)
        {
            CompanyService company = CompanyService.Current;
            if (company == null || results == null || results.Wiped) return;
            // The day the job was rolled for, not today's: a job that runs past midnight UTC still carries
            // yesterday's seed and belongs on yesterday's board.
            string day = company.ActiveDailyDay;
            if (day == null) return;
            string key = $"daily.best.{day}";
            if (results.Haul > Core.Prefs.GetInt(key, 0)) Core.Prefs.SetInt(key, results.Haul);
            Submit(day, results.Haul);
        }

        private static async void Submit(string day, int haul)
        {
            SteamBootstrap steam = SteamBootstrap.Instance;
            if (steam == null || !steam.IsAvailable) return;
            try
            {
                Steamworks.Data.Leaderboard? board = await Steamworks.SteamUserStats.FindOrCreateLeaderboardAsync(
                    $"daily_{day}", Steamworks.Data.LeaderboardSort.Descending, Steamworks.Data.LeaderboardDisplay.Numeric);
                if (board == null) return;
                Steamworks.Data.LeaderboardUpdate? update = await board.Value.SubmitScoreAsync(haul);
                if (update != null)
                    ToastFeed.Show(Loc.T("DAILY JOB"), Loc.T("Your crew's ${0:N0} ranks #{1:N0} today.", haul, update.Value.NewGlobalRank), null, ToastFeed.Kind.Good);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Steam] Daily leaderboard: {e.Message}");
            }
        }
    }
}

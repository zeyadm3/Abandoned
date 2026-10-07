using System;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// This player's achievements (M9.5): play stats kept with Prefs (PlayerPrefs; memory in batch mode),
    /// achievements unlocked when a stat reaches its threshold, announced locally and handed to Steam
    /// through <see cref="Backend"/> when it runs. Every stat is counted on the player's own machine.
    /// </summary>
    public static class Achievements
    {
        public const string StatRuns = "runs", StatEscapes = "escapes", StatHaul = "haul", StatQuotaMet = "quota_met",
            StatJackpots = "jackpots", StatCollapses = "collapses", StatDeaths = "deaths", StatRevives = "revives",
            StatHunterFalls = "hunter_falls", StatDarkEscapes = "dark_escapes";

        private const string StatPrefix = "ach.stat.", UnlockPrefix = "ach.unlocked.";
        private static AchievementCatalog catalog;

        /// <summary>Steam (or a test fake): told about every unlock. Null = local only.</summary>
        public static Action<AchievementDefinition> Backend { get; set; }

        public static event Action<AchievementDefinition> Unlocked;

        public static AchievementCatalog Catalog => catalog != null ? catalog : catalog = Resources.Load<AchievementCatalog>(AchievementCatalog.ResourcePath);

        public static long Stat(string stat) => long.TryParse(Prefs.GetString(StatPrefix + stat, "0"), out long v) ? v : 0L;

        public static bool IsUnlocked(AchievementDefinition a) => a != null && Prefs.GetInt(UnlockPrefix + a.Id, 0) == 1;

        public static void Increment(string stat, long by = 1)
        {
            if (by <= 0) return;
            Prefs.SetString(StatPrefix + stat, (Stat(stat) + by).ToString());
            Check(stat);
            Prefs.Save();
        }

        /// <summary>A run ended for this player (the appraisal).</summary>
        public static void RecordRun(bool escaped, long crewHaul, bool quotaMet, int jackpotsOut, bool dark)
        {
            Increment(StatRuns);
            if (escaped) Increment(StatEscapes);
            Increment(StatHaul, Math.Max(0L, crewHaul));
            if (quotaMet) Increment(StatQuotaMet);
            if (escaped && jackpotsOut > 0) Increment(StatJackpots, jackpotsOut);
            if (escaped && dark) Increment(StatDarkEscapes);
        }

        private static void Check(string stat)
        {
            if (Catalog == null) return;
            long value = Stat(stat);
            foreach (AchievementDefinition a in Catalog.Items)
            {
                if (a == null || a.Stat != stat || value < a.Threshold || IsUnlocked(a)) continue;
                Prefs.SetInt(UnlockPrefix + a.Id, 1);
                Debug.Log($"[Achievements] Unlocked {a.Id}.");
                Unlocked?.Invoke(a);
                Backend?.Invoke(a);
            }
        }

        /// <summary>Hands every unlocked achievement to the backend again (Steam just started).</summary>
        public static void ResendUnlocked()
        {
            if (Backend == null || Catalog == null) return;
            foreach (AchievementDefinition a in Catalog.Items)
                if (IsUnlocked(a)) Backend(a);
        }

        /// <summary>Tests: forget every stat and unlock.</summary>
        public static void ResetAll()
        {
            if (Catalog != null)
                foreach (AchievementDefinition a in Catalog.Items)
                    if (a != null) { Prefs.Delete(UnlockPrefix + a.Id); Prefs.Delete(StatPrefix + a.Stat); }
            Prefs.Save();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            catalog = null;
            Backend = null;
            Unlocked = null;
        }
    }
}

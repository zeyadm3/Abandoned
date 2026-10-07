using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// This player's own progress and picks, on their own machine (cosmetics are personal; the company
    /// save is the host's): runs played, runs survived, lifetime crew haul, and the outfit chosen.
    /// </summary>
    public static class PlayerProfile
    {
        private const string RunsKey = "profile.runs", EscapesKey = "profile.escapes", HaulKey = "profile.haul",
            CoverallKey = "profile.coverall", HatKey = "profile.hat", AccessoryKey = "profile.accessory";

        public static int Runs => Prefs.GetInt(RunsKey, 0);
        public static int Escapes => Prefs.GetInt(EscapesKey, 0);
        public static long Haul => long.TryParse(Prefs.GetString(HaulKey, "0"), out long h) ? h : 0L;
        public static string Coverall => Prefs.GetString(CoverallKey, "");
        public static string Hat => Prefs.GetString(HatKey, "");
        public static string Accessory => Prefs.GetString(AccessoryKey, "");

        public static bool Unlocked(CosmeticDefinition d) => d != null && d.IsUnlocked(Runs, Escapes, Haul);

        /// <summary>A run ended (the appraisal): count it, whether we got out, and the crew's haul.</summary>
        public static void RecordRun(bool madeItOut, long crewHaul)
        {
            Prefs.SetInt(RunsKey, Runs + 1);
            if (madeItOut) Prefs.SetInt(EscapesKey, Escapes + 1);
            Prefs.SetString(HaulKey, (Haul + System.Math.Max(0L, crewHaul)).ToString());
            Prefs.Save();
        }

        public static void Wear(string coverallId, string hatId, string accessoryId = null)
        {
            Prefs.SetString(CoverallKey, coverallId ?? "");
            Prefs.SetString(HatKey, hatId ?? "");
            Prefs.SetString(AccessoryKey, accessoryId ?? "");
            Prefs.Save();
        }

        /// <summary>Tests only: a clean profile (and they must restore what they changed).</summary>
        public static void ResetAll()
        {
            foreach (string k in new[] { RunsKey, EscapesKey, HaulKey, CoverallKey, HatKey, AccessoryKey }) Prefs.Delete(k);
            Prefs.Save();
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;

namespace Abandoned.Player
{
    /// <summary>
    /// This player's own progress and picks, on their own machine (cosmetics are personal; the company
    /// save is the host's): runs played, runs survived, lifetime crew haul, what they've bought at the
    /// wardrobe, and the outfit chosen.
    /// </summary>
    public static class PlayerProfile
    {
        private const string RunsKey = "profile.runs", EscapesKey = "profile.escapes", HaulKey = "profile.haul",
            CoverallKey = "profile.coverall", HatKey = "profile.hat", AccessoryKey = "profile.accessory", OwnedKey = "profile.owned";

        private static HashSet<string> owned;

        public static int Runs => Prefs.GetInt(RunsKey, 0);
        public static int Escapes => Prefs.GetInt(EscapesKey, 0);
        public static long Haul => long.TryParse(Prefs.GetString(HaulKey, "0"), out long h) ? h : 0L;
        public static string Coverall => Prefs.GetString(CoverallKey, "");
        public static string Hat => Prefs.GetString(HatKey, "");
        public static string Accessory => Prefs.GetString(AccessoryKey, "");

        /// <summary>Free, bought here, or awarded by an achievement this player has.</summary>
        public static bool Unlocked(CosmeticDefinition d)
        {
            if (d == null) return false;
            return d.Unlock switch
            {
                CosmeticUnlock.Free => true,
                CosmeticUnlock.Buy => Owns(d),
                _ => Achievements.IsUnlocked(Achievement(d.RewardAchievement)),
            };
        }

        public static bool Owns(CosmeticDefinition d) => d != null && Owned().Contains(d.Key);

        /// <summary>The host took the company's money for it: it's this player's from now on.</summary>
        public static void Grant(CosmeticDefinition d)
        {
            if (d == null || !Owned().Add(d.Key)) return;
            Prefs.SetString(OwnedKey, string.Join(",", owned));
            Prefs.Save();
        }

        public static AchievementDefinition Achievement(string id)
        {
            if (string.IsNullOrEmpty(id) || Achievements.Catalog == null) return null;
            foreach (AchievementDefinition a in Achievements.Catalog.Items)
                if (a != null && a.Id == id) return a;
            return null;
        }

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
            foreach (string k in new[] { RunsKey, EscapesKey, HaulKey, CoverallKey, HatKey, AccessoryKey, OwnedKey }) Prefs.Delete(k);
            owned = null;
            Prefs.Save();
        }

        private static HashSet<string> Owned()
        {
            if (owned != null) return owned;
            owned = new HashSet<string>();
            foreach (string key in Prefs.GetString(OwnedKey, "").Split(','))
                if (key.Length > 0) owned.Add(key);
            return owned;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => owned = null;
    }
}

using System;
using System.Collections.Generic;

namespace Abandoned.Company
{
    /// <summary>
    /// The company, as saved on the host's machine (JSON): money (negative = debt), experience and level,
    /// the missed-quota streak (3 = bankruptcy, GDD 13), owned equipment and what's unlocked.
    /// Plain data so JsonUtility can write it and tests can build it.
    /// </summary>
    [Serializable]
    public class CompanySave
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string companyName = "Zeyad Salvage Co.";
        public int money;
        public int xp;
        public int level = 1;
        public int missedQuotas;
        public int runs;
        public int bankruptcies;
        public int bestHaul;
        public List<OwnedItem> equipment = new();
        public List<string> unlocks = new();
        // A job taken but not yet settled (quota > 0): the host quit or crashed mid-run.
        public int pendingQuota;
        public float pendingBonus;

        [Serializable]
        public struct OwnedItem
        {
            public string id;
            public int count;
        }

        public int CountOf(string id)
        {
            foreach (OwnedItem o in equipment) if (o.id == id) return o.count;
            return 0;
        }

        public void Add(string id, int count = 1)
        {
            for (int i = 0; i < equipment.Count; i++)
            {
                if (equipment[i].id != id) continue;
                equipment[i] = new OwnedItem { id = id, count = equipment[i].count + count };
                return;
            }
            equipment.Add(new OwnedItem { id = id, count = count });
        }

        public bool Remove(string id, int count = 1)
        {
            for (int i = 0; i < equipment.Count; i++)
            {
                if (equipment[i].id != id || equipment[i].count < count) continue;
                int left = equipment[i].count - count;
                if (left == 0) equipment.RemoveAt(i);
                else equipment[i] = new OwnedItem { id = id, count = left };
                return true;
            }
            return false;
        }

        /// <summary>A new company (also after bankruptcy): the GDD 12 basic kit, nothing else.</summary>
        public static CompanySave New(int bankruptciesSoFar = 0)
        {
            var save = new CompanySave { bankruptcies = bankruptciesSoFar };
            foreach ((string id, int count) in StarterKit) save.Add(id, count);
            return save;
        }

        /// <summary>
        /// GDD 12 "Basic (starting)": a flashlight and radio for each of up to 4 players, one medkit, and a
        /// hand trolley so a solo player can move Heavy items from the first run.
        /// </summary>
        public static readonly (string id, int count)[] StarterKit = { ("flashlight", 4), ("radio", 4), ("medkit", 1), ("hand_trolley", 1) };
    }
}

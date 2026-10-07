using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The company's rules (GDD 5, 13), pure: a run's haul (times the contract bonus when the quota is
    /// met) is paid out; a missed quota also costs a share of the shortfall (debt is negative money, not
    /// a lost save); experience from the haul plus a quota bonus raises the level; three misses in a row
    /// and the company goes bankrupt and starts over with the starter kit.
    /// </summary>
    public static class CompanyLedger
    {
        public static RunOutcome Apply(CompanySave save, CompanyConfig c, int haul, int quota, float payoutBonus)
        {
            bool met = haul >= quota;
            int payout = met ? Mathf.RoundToInt(haul * (1f + payoutBonus)) : haul;
            int penalty = met ? 0 : Mathf.RoundToInt((quota - haul) * c.ShortfallPenalty);
            int xp = Mathf.FloorToInt(haul / 1000f * c.XpPerThousand) + (met ? c.QuotaXp : 0);
            int oldLevel = save.level;

            save.runs++;
            save.money += payout - penalty;
            save.xp += xp;
            save.level = Mathf.Max(save.level, c.LevelFor(save.xp));
            save.missedQuotas = met ? 0 : save.missedQuotas + 1;
            if (haul > save.bestHaul) save.bestHaul = haul;

            bool bankrupt = save.missedQuotas >= c.MissesToBankruptcy;
            var outcome = new RunOutcome(haul, quota, payout, penalty, xp, save.level, save.level > oldLevel, save.missedQuotas, bankrupt);
            if (bankrupt) GoBankrupt(save);
            return outcome;
        }

        /// <summary>GDD 13: start a new company; only the count of past failures (and cosmetics, later) survive.</summary>
        public static void GoBankrupt(CompanySave save)
        {
            CompanySave fresh = CompanySave.New(save.bankruptcies + 1);
            fresh.companyName = save.companyName;
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(fresh), save);
        }

        public static bool TryBuy(CompanySave save, string itemId, int price)
        {
            if (price < 0 || save.money < price) return false;
            save.money -= price;
            save.Add(itemId);
            return true;
        }
    }
}

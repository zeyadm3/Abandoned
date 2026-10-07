using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abandoned.Contracts
{
    /// <summary>
    /// Rolls the contract board (GDD 14), pure and seeded: same company level + board seed = same three
    /// offers on every machine. Early Access has one location, the mall (GDD 8), so variety comes from
    /// stability, power, window, quota and one modifier each (no modifier repeats on a board).
    /// </summary>
    public static class ContractGenerator
    {
        public const string MallLocation = "Abandoned Mall", MallScene = "Mall";

        /// <param name="lootEstimate">What a full mall run tends to hold, for the board's estimate.</param>
        public static List<Contract> Roll(ContractConfig c, int companyLevel, int boardSeed, int lootEstimate)
        {
            var random = new System.Random(boardSeed);
            var pool = c.Modifiers.Where(m => m != null && m.MinLevel <= companyLevel).ToList();
            var offers = new List<Contract>();
            for (int i = 0; i < c.Offers; i++)
            {
                ContractModifier m = null;
                if (pool.Count > 0)
                {
                    m = pool[random.Next(pool.Count)];
                    pool.Remove(m);
                }
                offers.Add(Roll(c, m, companyLevel, random, lootEstimate));
            }
            return offers;
        }

        private static Contract Roll(ContractConfig c, ContractModifier m, int level, System.Random random, int lootEstimate)
        {
            float stability = Mathf.Clamp(Lerp(c.StabilityMin, c.StabilityMax, random) + (m != null ? m.StabilityDelta : 0f), 0.2f, 1f);
            float window = Mathf.Round(Lerp(c.WindowMin, c.WindowMax, random) * (m != null ? m.WindowMultiplier : 1f) / 30f) * 30f;
            float spread = 1f + (float)(random.NextDouble() * 2 - 1) * c.QuotaSpread;
            int quota = Mathf.RoundToInt(c.BaseQuota * (1f + c.QuotaGrowth * (level - 1)) * spread / 1000f) * 1000;
            float loot = m != null ? m.LootMultiplier : 1f;
            bool powerOff = (m != null && m.PowerOff) || random.NextDouble() < c.PowerOffChance;
            float bonus = Lerp(c.BonusMin, c.BonusMax, random) + (m != null ? m.BonusDelta : 0f);
            int seed = random.Next(1, int.MaxValue);
            float risk = (1f - stability) * 2f + (powerOff ? 0.4f : 0f) + (window < 720f ? 0.3f : 0f);
            return new Contract
            {
                Location = MallLocation,
                Scene = MallScene,
                ModifierId = m != null ? m.Id : "",
                ModifierName = m != null ? m.DisplayName : "None",
                Seed = seed,
                Quota = quota,
                LootMin = Mathf.RoundToInt(lootEstimate * 0.6f * loot / 1000f) * 1000,
                LootMax = Mathf.RoundToInt(lootEstimate * 1.1f * loot / 1000f) * 1000,
                Stability = Mathf.Round(stability * 100f) / 100f,
                WindowSeconds = window,
                PayoutBonus = Mathf.Round(bonus * 100f) / 100f,
                PowerOff = powerOff,
                ThreatLevel = (byte)(risk < 0.6f ? 0 : risk < 1f ? 1 : 2),
            };
        }

        private static float Lerp(float a, float b, System.Random r) => a + (b - a) * (float)r.NextDouble();
    }
}

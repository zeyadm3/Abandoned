using Abandoned.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Abandoned.Contracts
{
    /// <summary>
    /// Rolls the contract board (GDD 14), pure and seeded: same company level + board seed = same three
    /// offers on every machine. Locations are ordinary content data: a board does not repeat a destination
    /// while it has other eligible ones, then variety continues through stability, power, window, quota and a
    /// modifier (no modifier repeats on a board; one offer is always plain).
    /// </summary>
    public static class ContractGenerator
    {
        public const string MallId = "mall", MallLocation = "Abandoned Mall", MallScene = "Mall";

        /// <param name="lootEstimate">What a full mall run tends to hold, for the board's estimate.</param>
        public static List<Contract> Roll(ContractConfig c, int companyLevel, int boardSeed, int lootEstimate)
        {
            var random = new System.Random(boardSeed);
            var pool = c.Modifiers.Where(m => m != null && m.MinLevel <= companyLevel).ToList();
            var offers = new List<Contract>();
            var locations = EligibleLocations(c, companyLevel);
            var locationPool = new List<LocationDefinition>(locations);
            // QA D-06: one offer is always a plain job, so new crews learn the core salvage loop first.
            int plainSlot = random.Next(Mathf.Max(1, c.Offers));
            for (int i = 0; i < c.Offers; i++)
            {
                ContractModifier m = null;
                if (pool.Count > 0 && i != plainSlot)
                {
                    m = pool[random.Next(pool.Count)];
                    pool.Remove(m);
                }
                offers.Add(Roll(c, m, companyLevel, random, lootEstimate, DrawLocation(locationPool, locations, random)));
            }
            offers.Add(Daily(c, lootEstimate, DateTime.UtcNow)); // 0.19.12
            return offers;
        }

        /// <summary>0.19.12: today's id ("20261010"); the daily job and its leaderboard are named after it.</summary>
        // Invariant: a Thai or Hijri calendar would otherwise name another year and roll another daily job.
        public static string DayId(DateTime utc) => utc.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        /// <summary>
        /// 0.19.12: the daily job. The same for everyone that UTC day: rolled from the date at company level 1
        /// with no modifier, so the run seed (the location's loot, route changes, alarms) matches too.
        /// </summary>
        public static Contract Daily(ContractConfig c, int lootEstimate, DateTime utc)
        {
            int day = int.Parse(DayId(utc), CultureInfo.InvariantCulture);
            var random = new System.Random(day * 7919 + 17);
            var locations = EligibleLocations(c, 1);
            LocationDefinition location = DrawLocation(new List<LocationDefinition>(locations), locations, random);
            Contract daily = Roll(c, null, 1, random, lootEstimate, location);
            daily.ModifierId = DailyId;
            daily.ModifierName = Loc.Mark("Daily job");
            // Sent as English; boards and the van word it. The complete names live in the jobs text sheet.
            daily.Location = Loc.Mark(location.DisplayName + " (today's daily job)");
            return daily;
        }

        public const string DailyId = "daily";

        /// <summary>
        /// The UTC day whose daily job this contract is (null = not a daily job, or not a recent one). The
        /// board is rolled when the crew reaches the HQ, so a job taken or finished after midnight still
        /// belongs to the day it was rolled for; the day before and after cover clocks that disagree.
        /// </summary>
        public static string DailyDay(ContractConfig c, Contract contract, DateTime utcNow)
        {
            if (c == null || contract.ModifierId != DailyId) return null;
            for (int back = -1; back <= 3; back++)
            {
                DateTime day = utcNow.AddDays(-back);
                // The run seed comes from the date alone (the loot estimate only fills the board's text).
                if (Daily(c, 0, day).Seed == contract.Seed) return DayId(day);
            }
            return null;
        }

        private static Contract Roll(ContractConfig c, ContractModifier m, int level, System.Random random, int lootEstimate,
            LocationDefinition location)
        {
            location ??= LocationCatalog.LegacyMall;
            float stability = Mathf.Clamp(Lerp(c.StabilityMin, c.StabilityMax, random) + location.StabilityDelta +
                (m != null ? m.StabilityDelta : 0f), 0.2f, 1f);
            float window = Mathf.Round(Lerp(c.WindowMin, c.WindowMax, random) * location.WindowMultiplier *
                (m != null ? m.WindowMultiplier : 1f) / 30f) * 30f;
            float spread = 1f + (float)(random.NextDouble() * 2 - 1) * c.QuotaSpread;
            int quota = Mathf.RoundToInt(c.BaseQuota * location.QuotaMultiplier * (1f + c.QuotaGrowth * (level - 1)) * spread / 1000f) * 1000;
            float loot = m != null ? m.LootMultiplier : 1f;
            bool powerOff = (m != null && m.PowerOff) || random.NextDouble() < Mathf.Clamp01(c.PowerOffChance + location.PowerOffOffset);
            float bonus = Lerp(c.BonusMin, c.BonusMax, random) + (m != null ? m.BonusDelta : 0f);
            int seed = random.Next(1, int.MaxValue);
            float risk = (1f - stability) * 2f + (powerOff ? 0.4f : 0f) + (window < 720f ? 0.3f : 0f) + (m != null ? m.ExtraThreats * 0.35f : 0f);
            return new Contract
            {
                LocationId = location.Id,
                Location = Loc.Mark(location.DisplayName),
                Scene = location.Scene,
                ModifierId = m != null ? m.Id : "",
                ModifierName = m != null ? m.DisplayName : Loc.Mark("None"),
                Seed = seed,
                Quota = quota,
                LootMin = Mathf.RoundToInt(lootEstimate * location.LootEstimateMultiplier * 0.6f * loot / 1000f) * 1000,
                LootMax = Mathf.RoundToInt(lootEstimate * location.LootEstimateMultiplier * 1.1f * loot / 1000f) * 1000,
                Stability = Mathf.Round(stability * 100f) / 100f,
                WindowSeconds = window,
                PayoutBonus = Mathf.Round(bonus * 100f) / 100f,
                PowerOff = powerOff,
                ThreatLevel = (byte)(risk < 0.6f ? 0 : risk < 1f ? 1 : 2),
            };
        }

        private static List<LocationDefinition> EligibleLocations(ContractConfig config, int companyLevel)
        {
            var eligible = new List<LocationDefinition>();
            if (config != null)
                foreach (LocationDefinition location in config.AvailableLocations(companyLevel))
                    if (location != null && location.BoardWeight > 0 && !string.IsNullOrWhiteSpace(location.Id) &&
                        !string.IsNullOrWhiteSpace(location.Scene)) eligible.Add(location);
            if (eligible.Count == 0) eligible.Add(LocationCatalog.LegacyMall);
            return eligible;
        }

        // Draw without replacement until the board has shown every eligible destination. Weight remains useful
        // after the pool refills, but cannot make a three-card board show the same place three times.
        private static LocationDefinition DrawLocation(List<LocationDefinition> pool, IReadOnlyList<LocationDefinition> all,
            System.Random random)
        {
            if (pool.Count == 0) pool.AddRange(all);
            int total = 0;
            foreach (LocationDefinition location in pool) total += Mathf.Max(1, location.BoardWeight);
            int pick = random.Next(Mathf.Max(1, total));
            int chosen = pool.Count - 1;
            for (int i = 0; i < pool.Count; i++)
            {
                pick -= Mathf.Max(1, pool[i].BoardWeight);
                if (pick >= 0) continue;
                chosen = i;
                break;
            }
            LocationDefinition result = pool[chosen];
            pool.RemoveAt(chosen);
            return result;
        }

        private static float Lerp(float a, float b, System.Random r) => a + (b - a) * (float)r.NextDouble();
    }
}

using System;
using System.Collections.Generic;

namespace Abandoned.Loot
{
    /// <summary>
    /// Decides a run's loot from its seed: which jackpot points get a jackpot (1-2 of them), which
    /// ordinary points are filled, and with what (weighted by rarity among the items that fit the
    /// point's store kind and size). Pure and deterministic: same points + catalog + seed = same run.
    /// </summary>
    public static class LootSpawnPlanner
    {
        public readonly struct Placement
        {
            public readonly int Point;
            public readonly LootDefinition Definition;
            public readonly int ValueSeed;

            public Placement(int point, LootDefinition definition, int valueSeed)
            {
                Point = point;
                Definition = definition;
                ValueSeed = valueSeed;
            }
        }

        /// <param name="fillMultiplier">Contract: share of ordinary points filled ("picked over" &lt; 1).</param>
        /// <param name="fragileRarity">Contract: weight for High/Extreme fragility items ("fragile collection").</param>
        public static List<Placement> Plan(IReadOnlyList<LootSpawnPoint> points, IReadOnlyList<LootDefinition> definitions,
            LootSpawnConfig config, int seed, float fillMultiplier = 1f, float fragileRarity = 1f, int extraJackpots = 0)
        {
            var random = new Random(seed);
            var plan = new List<Placement>();

            var jackpotPoints = new List<int>();
            for (int i = 0; i < points.Count; i++) if (points[i].IsJackpot) jackpotPoints.Add(i);
            Shuffle(jackpotPoints, random);
            int jackpots = Math.Min(jackpotPoints.Count, random.Next(config.JackpotsMin, config.JackpotsMax + 1) + Math.Max(0, extraJackpots));

            for (int i = 0; i < points.Count; i++)
            {
                LootSpawnPoint p = points[i];
                // Draw for every point (used or not), so one point's outcome never shifts the others'.
                double fill = random.NextDouble();
                double pick = random.NextDouble();
                int valueSeed = random.Next(1, int.MaxValue);
                bool use = p.IsJackpot ? jackpotPoints.IndexOf(i) < jackpots : fill < config.FillChance * fillMultiplier;
                if (!use) continue;
                LootDefinition d = Pick(p, definitions, pick, fragileRarity);
                if (d != null) plan.Add(new Placement(i, d, valueSeed));
            }
            return plan;
        }

        private static float Weight(LootDefinition d, float fragileRarity) =>
            d.Rarity * (d.Fragility >= Fragility.High ? fragileRarity : 1f);

        private static LootDefinition Pick(LootSpawnPoint point, IReadOnlyList<LootDefinition> definitions, double roll, float fragileRarity)
        {
            float total = 0f;
            foreach (LootDefinition d in definitions) if (point.Fits(d)) total += Weight(d, fragileRarity);
            if (total <= 0f) return null;
            double target = roll * total;
            LootDefinition last = null;
            foreach (LootDefinition d in definitions)
            {
                if (!point.Fits(d)) continue;
                last = d;
                target -= Weight(d, fragileRarity);
                if (target <= 0) return d;
            }
            return last; // rounding left a sliver at the end

        }

        private static void Shuffle(List<int> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Abandoned.Loot
{
    /// <summary>One real jackpot is guaranteed before an optional featured Heavy prize uses a remaining site.</summary>
    public static class FeaturedLootSelection
    {
        public static HashSet<int> Select(IReadOnlyList<LootSpawnPoint> points, IReadOnlyList<LootDefinition> definitions,
            LootSpawnConfig config, Random random, int extraJackpots)
        {
            var candidates = new List<int>();
            var jackpots = new List<int>();
            for (int i = 0; i < points.Count; i++)
            {
                LootSpawnPoint p = points[i];
                if ((!p.IsJackpot && !p.IsFeatured) || !HasEligibleItem(p, definitions)) continue;
                candidates.Add(i);
                if (p.IsJackpot) jackpots.Add(i);
            }
            var selected = new HashSet<int>();
            // A sofa alone cannot consume the run's only prize roll and silently remove every jackpot.
            if (jackpots.Count == 0) return selected;
            int count = Math.Min(candidates.Count, Math.Max(1,
                random.Next(config.JackpotsMin, config.JackpotsMax + 1) + Math.Max(0, extraJackpots)));
            int first = jackpots[random.Next(jackpots.Count)];
            selected.Add(first);
            candidates.Remove(first);
            while (selected.Count < count && candidates.Count > 0)
            {
                int pick = random.Next(candidates.Count);
                selected.Add(candidates[pick]);
                candidates.RemoveAt(pick);
            }
            return selected;
        }

        private static bool HasEligibleItem(LootSpawnPoint point, IReadOnlyList<LootDefinition> definitions)
        {
            foreach (LootDefinition d in definitions)
                if (point.Fits(d) && d.Rarity > 0f) return true;
            return false;
        }
    }
}

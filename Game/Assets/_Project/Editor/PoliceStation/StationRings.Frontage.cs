using System.Collections.Generic;
using UnityEngine;
using Site = Abandoned.EditorTools.StationSite;
using Street = Abandoned.EditorTools.StationStreet;

namespace Abandoned.EditorTools
{
    /// <summary>A row of stock buildings along any street front, for a map's own blocks inside its old ground.</summary>
    public static partial class StationRings
    {
        /// <summary>
        /// Buildings from <paramref name="pool"/> side by side from <paramref name="start"/>, <paramref name="length"/>
        /// metres along <paramref name="along"/>, fronts facing <paramref name="yaw"/>, none deeper than
        /// <paramref name="maxDepth"/>; now and then an alley, and with chance <paramref name="vacancy"/> a vacant lot
        /// (a dumpster, litter, a tree gone wild, or nothing).
        /// </summary>
        public static void Frontage(Transform blocks, Transform street, StationArt a, StationRingStock stock, IList<int> pool, Vector3 start,
            Vector3 along, float length, float yaw, float maxDepth, float vacancy, int seed, bool winter = false)
        {
            along.y = 0f;
            along.Normalize();
            var fits = new List<int>();
            foreach (int design in pool)
                if (stock.Depth(design) <= maxDepth) fits.Add(design);
            if (fits.Count == 0) return;
            var rng = new System.Random(seed);
            (GameObject prefab, float height)[] trees = Trees(winter);
            Vector3 back = Quaternion.Euler(0f, yaw, 0f) * Vector3.back;
            float[] alleys = { 0f, 0f, 1f, 2f, 3.5f };
            float cursor = .5f;
            while (length - cursor > 6f)
            {
                if (rng.NextDouble() < vacancy)
                {
                    float gap = Mathf.Min(8f + (float)rng.NextDouble() * 12f, length - cursor);
                    Vector3 lot = start + along * (cursor + gap * .5f);
                    lot.y = Site.GroundTop;
                    switch (rng.Next(4))
                    {
                        case 0: Street.Dumpster(street, a, lot + back * 5f, yaw, rng.Next(2) == 0); break;
                        case 1: Street.Litter(street, a, lot + back * 3f, rng.Next(1000)); break;
                        case 2: Tree(street, trees, lot + back * 6f, rng); break;
                    }
                    cursor += gap;
                    continue;
                }
                int pick = -1;
                for (int tries = 0; tries < 8 && pick < 0; tries++)
                {
                    int i = fits[rng.Next(fits.Count)];
                    if (cursor + stock.Width(i) <= length - .5f) pick = i;
                }
                if (pick < 0) break;
                float w = stock.Width(pick);
                stock.Place(blocks, pick, start + along * (cursor + w * .5f), yaw);
                cursor += w + alleys[rng.Next(alleys.Length)];
            }
        }
    }
}

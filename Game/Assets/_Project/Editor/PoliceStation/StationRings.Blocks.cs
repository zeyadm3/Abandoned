using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Plan = Abandoned.EditorTools.StationTownPlan;
using Street = Abandoned.EditorTools.StationStreet;
using Way = Abandoned.EditorTools.StationRingSpec.Way;

namespace Abandoned.EditorTools
{
    /// <summary>The rings' rows of buildings, their vacant lots, and the street furniture down the ring roads.</summary>
    public static partial class StationRings
    {
        private static void Rows(Transform blocks, Transform street, StationArt a, StationRingSpec spec, StationRingStock stock, Layout l, int side,
            System.Random rng, (GameObject prefab, float height)[] trees)
        {
            List<Way> ways = spec.Ways(side);
            float inward = Layout.Inward(side);
            bool x = Layout.AlongX(side);
            // The block's row facing the first ring road, its row facing the second (the west and east ones stop short
            // of the corners, where the south and north rows already stand), and the edge's row facing in.
            Vector2 front = l.Span(side, l.A1, l.A1);
            Vector2 back = x ? l.Span(side, l.B0, l.B0) : new Vector2(l.In.yMin - l.B0 + RowDepth, l.In.yMax + l.B0 - RowDepth);
            Vector2 edge = l.Span(side, l.B1, l.B1);
            Row(blocks, street, a, spec, stock, l, side, Cut(front, ways, Walk + .5f), l.A1 + .3f, inward, 1f, spec.Front, RowDepth, rng, trees);
            Row(blocks, street, a, spec, stock, l, side, Cut(back, ways, Walk + .5f), l.B0 - .3f, inward + 180f, -1f, spec.Back, RowDepth, rng, trees);
            Row(blocks, street, a, spec, stock, l, side, Cut(edge, ways, Walk + .5f), l.B1 + .3f, inward, 1f, spec.Edge, EdgeDepth, rng, trees);
        }

        // Buildings side by side down a stretch of street front, fronts at o, now and then an alley or a vacant lot.
        private static void Row(Transform blocks, Transform street, StationArt a, StationRingSpec spec, StationRingStock stock, Layout l, int side,
            List<Vector2> stretches, float o, float yaw, float deeper, List<int> pool, float maxDepth, System.Random rng,
            (GameObject prefab, float height)[] trees)
        {
            List<int> fits = pool.FindAll(design => stock.Depth(design) <= maxDepth);
            if (fits.Count == 0) return;
            float[] alleys = { 0f, 0f, 1f, 2f, 3.5f };
            foreach (Vector2 stretch in stretches)
            {
                float cursor = stretch.x + .5f;
                while (stretch.y - cursor > 6f)
                {
                    if (rng.NextDouble() < spec.Vacancy)
                    {
                        float gap = Mathf.Min(8f + (float)rng.NextDouble() * 14f, stretch.y - cursor);
                        Vacant(street, a, spec, stock, l, side, (cursor + cursor + gap) * .5f, o, deeper, yaw, rng, trees);
                        cursor += gap;
                        continue;
                    }
                    int pick = -1;
                    for (int tries = 0; tries < 8 && pick < 0; tries++)
                    {
                        int i = fits[rng.Next(fits.Count)];
                        if (cursor + stock.Width(i) <= stretch.y - .5f) pick = i;
                    }
                    if (pick < 0) break;
                    float w = stock.Width(pick);
                    stock.Place(blocks, pick, l.P(side, cursor + w * .5f, o), yaw);
                    cursor += w + alleys[rng.Next(alleys.Length)];
                }
            }
        }

        // An empty lot: a dumpster, a heap of litter, a tree gone wild, a car left on it, or nothing but the ground.
        private static void Vacant(Transform s, StationArt a, StationRingSpec spec, StationRingStock stock, Layout l, int side, float u, float o,
            float deeper, float yaw, System.Random rng, (GameObject prefab, float height)[] trees)
        {
            switch (rng.Next(5))
            {
                case 0: Street.Dumpster(s, a, l.P(side, u, o + deeper * 5f), yaw, rng.Next(2) == 0); break;
                case 1: Street.Litter(s, a, l.P(side, u, o + deeper * 3f), rng.Next(1000)); break;
                case 2: Tree(s, trees, l.P(side, u, o + deeper * 6f), rng); break;
                case 3:
                    if (spec.Cars.Count > 0) stock.Place(s, spec.Cars[rng.Next(spec.Cars.Count)], l.P(side, u, o + deeper * 6f), yaw + 180f + (float)rng.NextDouble() * 30f - 15f);
                    break;
            }
        }

        private static void Dressing(Transform s, StationArt a, StationRingSpec spec, StationRingStock stock, Layout l, int side, System.Random rng,
            (GameObject prefab, float height)[] trees)
        {
            List<Way> ways = spec.Ways(side);
            float inward = Layout.Inward(side);
            // The first ring road's lamps along its outer sidewalk, heads over the road, still lit.
            Vector2 walkA = l.Span(side, l.Ar1, l.A1);
            for (float u = walkA.x + 14f; u < walkA.y - 6f; u += 36f)
                if (Clear(u, ways, 3f)) Street.Lamp(s, a, l.P(side, u, l.Ar1 + .5f), l.P(side, u, l.Ar0 + 2f), spec.LampIntensity);
            // Hydrants, bins and meters on the same sidewalk, by the buildings.
            foreach (Vector2 p in Cut(walkA, ways, 0f))
                if (p.y - p.x > 20f) Plan.Kerbside(s, a, l.P(side, p.x, l.A1 - .8f), Layout.Along(side), p.y - p.x, inward, spec.Seed + side * 7 + Mathf.RoundToInt(p.x));
            // The second ring is past where the power still reaches: its lamps are dark, a power line runs down its inner
            // sidewalk, and the trees on its outer sidewalk have grown out.
            Vector2 walkB = l.Span(side, l.Br1, l.B1);
            for (float u = walkB.x + 20f; u < walkB.y - 6f; u += 44f)
                if (Clear(u, ways, 3f)) DarkLamp(s, l.P(side, u, l.Br1 + .5f), inward);
            for (float u = walkB.x + 9f; u < walkB.y - 6f; u += 22f)
                if (Clear(u, ways, 4f) && rng.NextDouble() < .7) Tree(s, trees, l.P(side, u + (float)rng.NextDouble() * 3f, l.Br1 + 1.5f), rng);
            var feet = new List<Vector3>();
            Vector2 walkIn = l.Span(side, l.B0, l.Br0);
            for (float u = walkIn.x + 8f; u < walkIn.y - 4f; u += 32f)
                if (Clear(u, ways, 2f)) feet.Add(l.P(side, u, l.Br0 - .6f));
            if (feet.Count > 1) Street.PowerLine(s, a, feet);
            // Cars left along the outer kerb of both ring roads, thinner on the second.
            Cars(s, spec, stock, l, side, l.Ar1 - 1.2f, 9f, 30f, rng);
            Cars(s, spec, stock, l, side, l.Br1 - 1.2f, 14f, 48f, rng);
        }

        private static void Cars(Transform s, StationRingSpec spec, StationRingStock stock, Layout l, int side, float o, float minGap, float maxGap, System.Random rng)
        {
            if (spec.Cars.Count == 0) return;
            List<Way> ways = spec.Ways(side);
            Vector2 span = l.Span(side, o, o);
            // Noses the way the traffic on that side of the road ran.
            float yaw = (Layout.AlongX(side) ? 90f : 0f) + (side == StationRingSpec.South || side == StationRingSpec.East ? 180f : 0f);
            for (float u = span.x + 16f + (float)rng.NextDouble() * 6f; u < span.y - 16f; u += minGap + (float)rng.NextDouble() * (maxGap - minGap))
            {
                if (!Clear(u, ways, 8f)) continue;
                stock.Place(s, spec.Cars[rng.Next(spec.Cars.Count)], l.P(side, u, o), yaw + (float)rng.NextDouble() * 4f - 2f);
            }
        }

        // A street lamp nobody has power for: the scan alone, no light.
        private static void DarkLamp(Transform s, Vector3 foot, float yaw) =>
            StationScans.Put(s, "street_lamp_01", foot, yaw, new Vector3(2.5f, 9f, 2.5f), solid: true);

        // The scanned trees the maps' sites use, tall ones only (the saplings would look planted yesterday).
        private static (GameObject prefab, float height)[] Trees()
        {
            var tall = new List<(GameObject, float)>();
            foreach ((GameObject prefab, float height) in MapTrees.Forest(false))
                if (prefab != null && height >= 4f) tall.Add((prefab, height));
            return tall.ToArray();
        }

        private static void Tree(Transform s, (GameObject prefab, float height)[] trees, Vector3 at, System.Random rng)
        {
            if (trees.Length == 0) return;
            (GameObject prefab, float height) = trees[rng.Next(trees.Length)];
            var tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab, s);
            if (tree == null) return;
            tree.name = "StreetTree";
            tree.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f));
            tree.transform.localScale = Vector3.one * ((7f + (float)rng.NextDouble() * 4f) / height);
        }
    }
}

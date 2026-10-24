using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The water the drawing puts round and through the city (Docs/CITY_PLAN.md step 2): the sea down the west side,
    /// the river across it, the banks that stop you seeing under the ground where either cuts in, and a pier pair
    /// wherever a named road meets the river, for step 3 to lay its bridge decks on.
    /// </summary>
    public static class CityWater
    {
        /// <summary>How far below the water the banks carry on, so no gap shows at a low camera angle.</summary>
        private const float BankSkirt = 4f;
        private const float PierHalf = 3f, PierSpacing = 22f;

        /// <summary>Builds the water under <paramref name="parent"/>. Returns how many river crossings it found.</summary>
        public static int Build(Transform parent, Material sea, Material river, Material bank, Material pier)
        {
            Transform group = GreyboxFactory.Group("Water", parent);
            Sea(group, sea);
            River(group, river, bank);
            Coast(group, bank);
            return Piers(group, pier);
        }

        // One plane under the whole draft. The ground floats above it, so the sea only shows where the coast and
        // the river have cut the ground away: one quad instead of a shaped body of water.
        private static void Sea(Transform group, Material material)
        {
            Rect b = CityPlan.Bounds;
            float pad = CityGround.Outskirts + CityPlan.Cell;
            var piece = new CityMesh.Piece();
            piece.Flat(b.xMin - pad, b.yMin - pad, b.xMax + pad, b.yMax + pad, CityPlan.SeaY);
            CityMesh.Render("Sea", group, piece.Save("Water_Sea"), material);
        }

        // The river as a strip down its centreline, with a wall up each side to the paving.
        private static void River(Transform group, Material water, Material banks)
        {
            var surface = new CityMesh.Piece();
            var walls = new CityMesh.Piece();
            Vector2[] line = CityPlan.River;
            float half = CityPlan.RiverWidth * .5f;
            Vector2 lastL = Vector2.zero, lastR = Vector2.zero;
            for (int i = 0; i < line.Length; i++)
            {
                Vector2 n = Normal(line, i);
                Vector2 l = line[i] + n * half, r = line[i] - n * half;
                if (i > 0)
                {
                    surface.Quad(At(lastL, CityPlan.RiverY), At(l, CityPlan.RiverY),
                                 At(r, CityPlan.RiverY), At(lastR, CityPlan.RiverY));
                    Both(walls, lastL, l);
                    Both(walls, lastR, r);
                }
                lastL = l;
                lastR = r;
            }
            CityMesh.Render("River", group, surface.Save("Water_River"), water);
            CityMesh.Render("RiverBanks", group, walls.Save("Water_RiverBanks"), banks);
        }

        // The shoreline, walled the same way so the paving never ends in mid-air over the sea.
        private static void Coast(Transform group, Material banks)
        {
            var walls = new CityMesh.Piece();
            Vector2[] line = CityPlan.Coast;
            for (int i = 1; i < line.Length; i++) Both(walls, line[i - 1], line[i], CityPlan.SeaY);
            CityMesh.Render("CoastBanks", group, walls.Save("Water_CoastBanks"), banks);
        }

        // Both windings, so a bank reads from the water and from the street without a two-sided shader.
        private static void Both(CityMesh.Piece piece, Vector2 from, Vector2 to, float water = float.NaN)
        {
            float bottom = (float.IsNaN(water) ? CityPlan.RiverY : water) - BankSkirt;
            piece.Wall(from, to, CityPlan.GroundY, bottom);
            piece.Wall(to, from, CityPlan.GroundY, bottom);
        }

        // A pair of piers wherever a named road or a Circus arm crosses the river, ready for step 3's decks.
        private static int Piers(Transform group, Material material)
        {
            Transform piers = GreyboxFactory.Group("Piers", group);
            int found = 0;
            foreach (Vector2[] road in Roads())
                foreach (Vector2 crossing in Crossings(road))
                {
                    Vector2 along = Along(road, crossing);
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 at = crossing + along * (PierSpacing * side);
                        GreyboxFactory.Box($"Pier_{found}_{(side < 0 ? "W" : "E")}", piers,
                            new Vector3(at.x, (CityPlan.GroundY + CityPlan.RiverY - BankSkirt) * .5f, at.y),
                            new Vector3(PierHalf * 2f, CityPlan.GroundY - CityPlan.RiverY + BankSkirt, PierHalf * 2f),
                            material);
                    }
                    found++;
                }
            return found;
        }

        // Every road that could reach the river: the named ones and the arms leaving the Circus.
        private static IEnumerable<Vector2[]> Roads()
        {
            foreach (CityPlan.Arterial a in CityPlan.Named) yield return a.Points;
            foreach (float bearing in CityPlan.CircusArms)
            {
                float r = (90f - bearing) * Mathf.Deg2Rad;
                var on = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
                yield return new[] { CityPlan.CircusCentre + on * CityPlan.CircusRing, CityPlan.CircusCentre + on * 2600f };
            }
        }

        // Where a road's line passes over the water, walked finely enough not to step across the river.
        private static IEnumerable<Vector2> Crossings(Vector2[] road)
        {
            float half = CityPlan.RiverWidth * .5f;
            bool wasOver = false;
            Vector2 entered = Vector2.zero;
            for (int i = 0; i < road.Length - 1; i++)
            {
                float len = Vector2.Distance(road[i], road[i + 1]);
                for (float t = 0f; t <= len; t += 8f)
                {
                    Vector2 p = Vector2.Lerp(road[i], road[i + 1], t / Mathf.Max(len, .001f));
                    bool over = CityPlan.FromRiver(p) < half;
                    if (over && !wasOver) entered = p;
                    else if (!over && wasOver) yield return (entered + p) * .5f;
                    wasOver = over;
                }
            }
        }

        private static Vector2 Along(Vector2[] road, Vector2 near)
        {
            Vector2 best = Vector2.right;
            float closest = float.MaxValue;
            for (int i = 0; i < road.Length - 1; i++)
            {
                float d = Vector2.Distance((road[i] + road[i + 1]) * .5f, near);
                if (d >= closest) continue;
                closest = d;
                best = (road[i + 1] - road[i]).normalized;
            }
            return best;
        }

        // Sideways from the line at point i, averaged across the bend so the banks do not pinch.
        private static Vector2 Normal(Vector2[] line, int i)
        {
            Vector2 a = line[Mathf.Max(0, i - 1)], b = line[Mathf.Min(line.Length - 1, i + 1)];
            Vector2 d = (b - a).normalized;
            return new Vector2(-d.y, d.x);
        }

        private static Vector3 At(Vector2 p, float y) => new(p.x, y, p.y);
    }
}

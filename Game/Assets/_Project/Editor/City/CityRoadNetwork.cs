using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Every road in the city as a centreline (Docs/CITY_PLAN.md step 3): each district's own grid at its own
    /// angle, the roads the drawing names, and the Circus with the five arms leaving it. Only lines and widths —
    /// what the surface is made of is <see cref="CityRoads"/>'s problem.
    /// </summary>
    public static class CityRoadNetwork
    {
        /// <summary>A road's centreline and how wide it runs.</summary>
        public readonly struct Line
        {
            public readonly string Name;
            public readonly Vector2[] Points;
            public readonly float HalfWidth;
            public readonly bool Arterial;

            public Line(string name, Vector2[] points, float halfWidth, bool arterial)
            {
                Name = name; Points = points; HalfWidth = halfWidth; Arterial = arterial;
            }
        }

        /// <summary>How far an arm runs out of the Circus before the district grid carries on from it.</summary>
        private const float ArmReach = 900f;

        public static List<Line> All()
        {
            var lines = new List<Line>();
            foreach (CityPlan.Arterial a in CityPlan.Named)
                lines.Add(new Line(a.Name, a.Points, a.HalfWidth, true));
            Circus(lines);
            foreach (CityPlan.District d in CityPlan.Districts) Grid(lines, d);
            // A spur off each site's approach side. It runs into the district grid rather than to a named junction,
            // which is enough: with a street every 120 m it always meets one, and the raster makes the join.
            foreach (CityPlan.Site site in CityPlan.Sites)
            {
                if (site.Scene == "Prison") continue;   // reached by boat from the quay, not by road
                (Vector2 from, Vector2 to) = CitySites.Approach(site);
                lines.Add(new Line($"{site.Scene}_Spur", new[] { from, to }, CityPlan.ArterialHalfWidth, true));
            }
            return lines;
        }

        // The ring road round the Circus park, and the arms that leave it on the drawing's bearings.
        private static void Circus(List<Line> lines)
        {
            var ring = new List<Vector2>();
            for (int i = 0; i <= 48; i++)
            {
                float a = i / 48f * Mathf.PI * 2f;
                ring.Add(CityPlan.CircusCentre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * CityPlan.CircusRing);
            }
            lines.Add(new Line("Circus", ring.ToArray(), CityPlan.ArterialHalfWidth, true));
            foreach (float bearing in CityPlan.CircusArms)
            {
                Vector2 on = Bearing(bearing);
                lines.Add(new Line($"CircusArm_{bearing:0}", new[]
                {
                    CityPlan.CircusCentre + on * CityPlan.CircusRing,
                    CityPlan.CircusCentre + on * ArmReach,
                }, CityPlan.ArterialHalfWidth, true));
            }
        }

        // A district's streets, both ways across it, at the angle its blocks are turned to. Every line is walked
        // and kept only where it is actually inside the district, so a quarter never paves its neighbour.
        private static void Grid(List<Line> lines, CityPlan.District d)
        {
            Rect b = d.Bounds();
            float reach = Mathf.Max(b.width, b.height) * .75f;
            Vector2 centre = b.center;
            float rad = d.GridAngle * Mathf.Deg2Rad;
            Vector2 along = new(Mathf.Cos(rad), Mathf.Sin(rad)), across = new(-along.y, along.x);
            Runs(lines, d, centre, along, across, reach);
            Runs(lines, d, centre, across, along, reach);
        }

        private static void Runs(List<Line> lines, CityPlan.District d, Vector2 centre,
                                 Vector2 along, Vector2 across, float reach)
        {
            for (float o = -reach; o <= reach; o += CityPlan.Block)
            {
                float fromMiddle = Mathf.Abs(Mathf.Repeat(o + CityPlan.ArterialSpacing * .5f,
                                                          CityPlan.ArterialSpacing) - CityPlan.ArterialSpacing * .5f);
                bool arterial = fromMiddle < CityPlan.Block * .5f;
                Vector2 from = centre + across * o - along * reach, to = centre + across * o + along * reach;
                foreach (Vector2[] run in Inside(d, from, to))
                    lines.Add(new Line($"{d.Name}_{o:0}", run,
                        arterial ? CityPlan.ArterialHalfWidth : CityPlan.StreetHalfWidth, arterial));
            }
        }

        // The stretches of a line that lie in the district, walked at a step fine enough for a 120 m block.
        private static IEnumerable<Vector2[]> Inside(CityPlan.District d, Vector2 from, Vector2 to)
        {
            const float step = 10f;
            float len = Vector2.Distance(from, to);
            Vector2 start = Vector2.zero, last = Vector2.zero;
            bool running = false;
            for (float t = 0f; t <= len; t += step)
            {
                Vector2 p = Vector2.Lerp(from, to, t / Mathf.Max(len, .001f));
                bool here = d.Contains(p);
                if (here && !running) { start = p; running = true; }
                else if (!here && running)
                {
                    if (Vector2.Distance(start, last) > CityPlan.Block * .5f) yield return new[] { start, last };
                    running = false;
                }
                last = p;
            }
            if (running && Vector2.Distance(start, last) > CityPlan.Block * .5f) yield return new[] { start, last };
        }

        /// <summary>A compass bearing as a direction (0 is north, turning clockwise).</summary>
        public static Vector2 Bearing(float degrees)
        {
            float r = (90f - degrees) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
        }
    }
}

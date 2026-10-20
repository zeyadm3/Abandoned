using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Plan arithmetic for the training house's builders: a rectangle with others cut out of it, as a few
    /// rectangles that cover what is left (ground around the house and its basement, land around the lot).
    /// </summary>
    public static class TutorialRects
    {
        /// <summary>
        /// <paramref name="outer"/> minus every hole, as non-overlapping rects: the plan is cut on every hole's edges
        /// into a grid, the uncovered cells kept and joined along each row, then rows of the same span merged.
        /// </summary>
        public static List<Rect> Subtract(Rect outer, IList<Rect> holes)
        {
            var xs = new SortedSet<float> { outer.xMin, outer.xMax };
            var zs = new SortedSet<float> { outer.yMin, outer.yMax };
            foreach (Rect h in holes)
            {
                foreach (float x in new[] { h.xMin, h.xMax }) if (x > outer.xMin && x < outer.xMax) xs.Add(x);
                foreach (float z in new[] { h.yMin, h.yMax }) if (z > outer.yMin && z < outer.yMax) zs.Add(z);
            }
            var xl = new List<float>(xs);
            var zl = new List<float>(zs);
            var rows = new List<Rect>();
            for (int j = 0; j + 1 < zl.Count; j++)
            {
                float z0 = zl[j], z1 = zl[j + 1];
                float start = float.NaN;
                for (int i = 0; i + 1 < xl.Count; i++)
                {
                    var cell = Rect.MinMaxRect(xl[i], z0, xl[i + 1], z1);
                    bool covered = Covered(cell, holes);
                    if (!covered && float.IsNaN(start)) start = xl[i];
                    if (covered && !float.IsNaN(start))
                    {
                        rows.Add(Rect.MinMaxRect(start, z0, xl[i], z1));
                        start = float.NaN;
                    }
                }
                if (!float.IsNaN(start)) rows.Add(Rect.MinMaxRect(start, z0, xl[xl.Count - 1], z1));
            }
            // Stack rows that share the same x span and touch.
            var merged = new List<Rect>();
            foreach (Rect r in rows)
            {
                int found = merged.FindIndex(m => Mathf.Approximately(m.xMin, r.xMin) && Mathf.Approximately(m.xMax, r.xMax) && Mathf.Approximately(m.yMax, r.yMin));
                if (found >= 0) merged[found] = Rect.MinMaxRect(r.xMin, merged[found].yMin, r.xMax, r.yMax);
                else merged.Add(r);
            }
            return merged;
        }

        private static bool Covered(Rect cell, IList<Rect> holes)
        {
            Vector2 c = cell.center;
            foreach (Rect h in holes) if (h.Contains(c)) return true;
            return false;
        }

        public static bool Overlaps(Rect a, Rect b, float margin = 0f) =>
            a.xMin < b.xMax + margin && a.xMax > b.xMin - margin && a.yMin < b.yMax + margin && a.yMax > b.yMin - margin;
    }
}

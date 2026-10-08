using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Remembers which sections hold up each load source between steps (QA P-01: ~86 resting items, the
    /// players and the monsters cast ~450 rays 50 times a second). A source keeps its supports while its
    /// load points stay put, for a few steps at most, and every entry is dropped the moment the building
    /// changes (a collapse, a re-roll). Counted in steps, not seconds, so tests that step by hand agree.
    /// </summary>
    public sealed class SupportCache
    {
        private const int ValidSteps = 25; // half a second at 50 Hz: a missed change never lasts longer
        private const int PurgeEverySteps = 500;
        private const float MoveTolerance = 0.02f;

        private sealed class Entry
        {
            public readonly List<Vector3> Points = new();
            public readonly List<StructuralSection> Supports = new();
            public int Version, Step;
        }

        private readonly Dictionary<ILoadSource, Entry> entries = new();
        private readonly List<ILoadSource> gone = new();
        private int version, step;

        /// <summary>The building changed: nothing cached is trusted any more.</summary>
        public void Invalidate() => version++;

        public void BeginStep()
        {
            step++;
            if (step % PurgeEverySteps == 0) Purge();
        }

        /// <summary>The cached supports, if the source hasn't moved and nothing changed since.</summary>
        public bool TryGet(ILoadSource source, List<LoadPoint> points, List<StructuralSection> supports)
        {
            if (!entries.TryGetValue(source, out Entry e) || e.Version != version || step - e.Step > ValidSteps || e.Points.Count != points.Count)
                return false;
            for (int i = 0; i < points.Count; i++)
                if ((points[i].Position - e.Points[i]).sqrMagnitude > MoveTolerance * MoveTolerance) return false;
            foreach (StructuralSection s in e.Supports)
                if (s == null || s.IsCollapsed) return false;
            supports.Clear();
            supports.AddRange(e.Supports);
            return true;
        }

        public void Store(ILoadSource source, List<LoadPoint> points, List<StructuralSection> supports)
        {
            if (!entries.TryGetValue(source, out Entry e)) entries[source] = e = new Entry();
            e.Points.Clear();
            foreach (LoadPoint p in points) e.Points.Add(p.Position);
            e.Supports.Clear();
            e.Supports.AddRange(supports);
            e.Version = version;
            e.Step = step;
        }

        // Sources that unregistered or were destroyed don't linger.
        private void Purge()
        {
            gone.Clear();
            var live = new HashSet<ILoadSource>(LoadSources.All);
            foreach (ILoadSource source in entries.Keys)
                if (!live.Contains(source) || (source is Object o && o == null)) gone.Add(source);
            foreach (ILoadSource source in gone) entries.Remove(source);
        }
    }
}

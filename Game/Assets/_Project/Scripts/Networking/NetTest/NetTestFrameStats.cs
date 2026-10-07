using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>Frame times over a stretch of play: average, 95th percentile and worst (ms).</summary>
    public sealed class NetTestFrameStats
    {
        private readonly List<float> frames = new();
        private float started = -1f, worstAt;

        public int Count => frames.Count;

        public void Sample()
        {
            // The first frame's delta covers the wait before measuring began: not part of the run.
            if (started < 0f) { started = Time.realtimeSinceStartup; return; }
            float ms = Time.unscaledDeltaTime * 1000f;
            if (frames.Count == 0 || ms > Worst) worstAt = Time.realtimeSinceStartup - started;
            frames.Add(ms);
        }

        public float Average => frames.Count > 0 ? frames.Average() : 0f;
        public float Worst => frames.Count > 0 ? frames.Max() : 0f;

        public float Percentile(float p)
        {
            if (frames.Count == 0) return 0f;
            List<float> sorted = frames.OrderBy(f => f).ToList();
            return sorted[Mathf.Clamp(Mathf.CeilToInt(p * sorted.Count) - 1, 0, sorted.Count - 1)];
        }

        public string Summary => $"{Count} frames, avg {Average:0.00} ms, p95 {Percentile(0.95f):0.00} ms, worst {Worst:0.0} ms at {worstAt:0.0} s";
    }
}

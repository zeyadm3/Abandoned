using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Global noise bus. Host-side emitters (footsteps, impacts, creaks, collapses) raise events;
    /// threats subscribe in M5. Keeps a short history for the F1 debug view and tests.
    /// </summary>
    public static class NoiseSystem
    {
        /// <summary>Hearing radius in metres for a loudness of 1.</summary>
        public const float MaxRadius = 40f;
        public const float HistorySeconds = 2f;

        private static readonly List<NoiseEvent> Recent = new();

        public static event Action<NoiseEvent> Emitted;

        public static IReadOnlyList<NoiseEvent> RecentEvents => Recent;
        public static int TotalEmitted { get; private set; }

        public static float RadiusOf(NoiseEvent e) => e.Loudness * MaxRadius;

        public static void Emit(Vector3 position, float loudness, NoiseSource source)
        {
            if (loudness <= 0f) return;
            var e = new NoiseEvent(position, loudness, source, Time.time);
            Recent.RemoveAll(old => e.Time - old.Time > HistorySeconds);
            Recent.Add(e);
            TotalEmitted++;
            Emitted?.Invoke(e);
        }

        /// <summary>Loudest recent event of a source, or null; handy for tests and debugging.</summary>
        public static NoiseEvent? LastOf(NoiseSource source)
        {
            for (int i = Recent.Count - 1; i >= 0; i--)
                if (Recent[i].Source == source) return Recent[i];
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Recent.Clear();
            Emitted = null;
            TotalEmitted = 0;
        }
    }
}

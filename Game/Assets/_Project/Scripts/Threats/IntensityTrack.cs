using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>One player's hidden stress (0-100). Plain C# so the rules can be tested without a scene.</summary>
    public sealed class IntensityTrack
    {
        public const float Max = 100f;

        public float Value { get; private set; }
        private float lastRise = float.NegativeInfinity;

        public void Add(float amount, float now)
        {
            if (!(amount > 0f)) return;
            Value = Mathf.Min(Max, Value + amount);
            lastRise = now;
        }

        /// <summary>Booth's rule: stress drains, but never while something is still on you.</summary>
        public void Tick(float dt, bool chased, float now, CrewDirectorConfig config)
        {
            if (chased) { Add(config.ChasedPerSecond * dt, now); return; }
            if (now - lastRise < config.DrainDelay) return;
            Value = Mathf.Max(0f, Value - config.DrainPerSecond * dt);
        }

        public void Reset()
        {
            Value = 0f;
            lastRise = float.NegativeInfinity;
        }
    }
}

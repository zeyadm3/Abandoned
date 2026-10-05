using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Pure structural rules. Same inputs always give the same outputs, which is what makes
    /// collapses learnable (GDD 6.4) and lets the host's results be checked in tests.
    /// </summary>
    public static class StructureMath
    {
        /// <summary>Health lost this step from being over capacity (0 when at or under).</summary>
        public static float OverloadDrain(float load, float capacity, float drainPerSecond, float decayScale, float dt)
        {
            if (capacity <= 0f || load <= capacity) return 0f;
            float overRatio = load / capacity - 1f;
            return overRatio * drainPerSecond * decayScale * dt;
        }

        /// <summary>Health lost from one impact of the given momentum (gameplay kg × m/s).</summary>
        public static float ImpactDamage(float momentum, float threshold, float damagePerMomentum) =>
            momentum <= threshold ? 0f : (momentum - threshold) * damagePerMomentum;

        /// <summary>Stage from health and load, before Failing/Collapsed timing is applied.</summary>
        public static StructuralStage StageFor(float healthFraction, float loadRatio, StructureConfig config)
        {
            if (healthFraction <= 0f) return StructuralStage.Failing;
            if (healthFraction < config.CrackingBelow) return StructuralStage.Cracking;
            if (healthFraction < config.StressedBelow || loadRatio >= config.StressedLoadRatio) return StructuralStage.Stressed;
            return StructuralStage.Stable;
        }

        /// <summary>
        /// Seeded pre-damage: fraction of max health to remove per section (0 = untouched).
        /// Two random numbers are drawn per section whether or not it's eligible, so one section's
        /// result never depends on another's flags — the same seed always damages the same pieces.
        /// </summary>
        public static float[] PreDamagePlan(bool[] eligible, float stability, int seed, StructureConfig config)
        {
            var random = new System.Random(seed);
            float fraction = config.PreDamagedFraction(stability);
            var plan = new float[eligible.Length];
            for (int i = 0; i < eligible.Length; i++)
            {
                double pick = random.NextDouble();
                double amount = random.NextDouble();
                if (eligible[i] && pick < fraction)
                    plan[i] = Mathf.Lerp(config.PreDamageMin, config.PreDamageMax, (float)amount);
            }
            return plan;
        }

        /// <summary>Downward speed after falling a height (no air resistance).</summary>
        public static float FallSpeed(float height) => Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * Mathf.Max(0f, height));
    }
}

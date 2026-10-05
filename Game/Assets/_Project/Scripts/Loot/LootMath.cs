using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// Pure value and damage rules, kept free of Unity objects so the host's results are
    /// reproducible from a seed and an impact speed (and easy to test).
    /// </summary>
    public static class LootMath
    {
        public readonly struct Roll
        {
            public readonly int BaseValue;
            public readonly float Condition;
            public int Value => Mathf.RoundToInt(BaseValue * Condition);

            public Roll(int baseValue, float condition)
            {
                BaseValue = baseValue;
                Condition = condition;
            }
        }

        public static Roll RollValue(int valueMin, int valueMax, float conditionMin, float conditionMax, int seed)
        {
            var random = new System.Random(seed);
            int baseValue = valueMin + (int)(random.NextDouble() * (valueMax - valueMin + 1));
            baseValue = Mathf.Clamp(baseValue, valueMin, valueMax);
            float condition = conditionMin + (float)random.NextDouble() * (conditionMax - conditionMin);
            return new Roll(baseValue, condition);
        }

        /// <summary>Value lost from one impact, given the item's full (starting) value.</summary>
        public static int ImpactLoss(FragilityProfile profile, int fullValue, float impactSpeed, out bool shatters)
        {
            shatters = profile.ShatterSpeed > 0f && impactSpeed >= profile.ShatterSpeed;
            if (shatters) return fullValue;
            if (impactSpeed <= profile.ImpactThreshold) return 0;
            float fraction = Mathf.Clamp01((impactSpeed - profile.ImpactThreshold) * profile.LossPerSpeed);
            return Mathf.RoundToInt(fullValue * fraction);
        }
    }
}

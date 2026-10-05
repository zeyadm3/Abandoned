using System;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>Damage rules for one fragility level.</summary>
    [Serializable]
    public struct FragilityProfile
    {
        public Fragility Fragility;

        [Tooltip("Impact speed (m/s, along the contact normal) below which nothing is lost.")]
        [Min(0f)] public float ImpactThreshold;

        [Tooltip("Fraction of the item's full value lost per m/s above the threshold.")]
        [Min(0f)] public float LossPerSpeed;

        [Tooltip("Impact speed at which the item shatters to $0. 0 = never shatters.")]
        [Min(0f)] public float ShatterSpeed;

        public FragilityProfile(Fragility fragility, float threshold, float lossPerSpeed, float shatterSpeed)
        {
            Fragility = fragility;
            ImpactThreshold = threshold;
            LossPerSpeed = lossPerSpeed;
            ShatterSpeed = shatterSpeed;
        }
    }
}

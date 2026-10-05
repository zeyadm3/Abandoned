using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Abandoned.Loot
{
    /// <summary>Damage rules for one fragility level.</summary>
    [Serializable]
    public struct FragilityProfile
    {
        [SerializeField, FormerlySerializedAs("Fragility")] private Fragility fragility;
        [Tooltip("Impact speed (m/s, along the contact normal) below which nothing is lost.")]
        [SerializeField, Min(0f), FormerlySerializedAs("ImpactThreshold")] private float impactThreshold;
        [Tooltip("Fraction of the item's full value lost per m/s above the threshold.")]
        [SerializeField, Min(0f), FormerlySerializedAs("LossPerSpeed")] private float lossPerSpeed;
        [Tooltip("Impact speed at which the item shatters to $0. 0 = never shatters.")]
        [SerializeField, Min(0f), FormerlySerializedAs("ShatterSpeed")] private float shatterSpeed;

        public Fragility Fragility => fragility;
        public float ImpactThreshold => impactThreshold;
        public float LossPerSpeed => lossPerSpeed;
        public float ShatterSpeed => shatterSpeed;

        public FragilityProfile(Fragility fragility, float threshold, float lossPerSpeed, float shatterSpeed)
        {
            this.fragility = fragility;
            impactThreshold = threshold;
            this.lossPerSpeed = lossPerSpeed;
            this.shatterSpeed = shatterSpeed;
        }
    }
}

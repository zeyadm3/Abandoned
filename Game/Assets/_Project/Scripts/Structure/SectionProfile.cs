using System;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Base numbers for one section type at 100% stability.</summary>
    [Serializable]
    public struct SectionProfile
    {
        [SerializeField] private SectionType type;
        [Tooltip("Gameplay kg it holds indefinitely.")]
        [SerializeField, Min(1f)] private float capacity;
        [SerializeField, Min(1f)] private float maxHealth;
        [Tooltip("Mass (kg) of the section itself, used when it falls onto what's below.")]
        [SerializeField, Min(0f)] private float selfMass;

        public SectionType Type => type;
        public float Capacity => capacity;
        public float MaxHealth => maxHealth;
        public float SelfMass => selfMass;

        public SectionProfile(SectionType type, float capacity, float maxHealth, float selfMass)
        {
            this.type = type;
            this.capacity = capacity;
            this.maxHealth = maxHealth;
            this.selfMass = selfMass;
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>How impacts turn into lost value, per fragility level. One shared asset.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Loot/Loot Damage Config", fileName = "LootDamageConfig")]
    public class LootDamageConfig : ScriptableObject, IValidatable
    {
        // A hand-height drop (~1.4 m) hits at ~5.2 m/s: shatters Extreme, dents High, scuffs Medium.
        [SerializeField] private FragilityProfile[] profiles =
        {
            new(Fragility.None, 999f, 0f, 0f),
            new(Fragility.Low, 6f, 0.02f, 0f),
            new(Fragility.Medium, 4f, 0.05f, 0f),
            new(Fragility.High, 2.5f, 0.12f, 10f),
            new(Fragility.Extreme, 2f, 1f, 2f),
        };

        [field: Tooltip("Ignore further impacts on an item for this long, so one landing isn't counted per contact.")]
        [field: SerializeField, Min(0f)] public float ImpactCooldown { get; private set; } = 0.15f;
        [field: Tooltip("Impacts below this speed make no sound.")]
        [field: SerializeField, Min(0f)] public float MinSoundSpeed { get; private set; } = 0.6f;
        [field: Tooltip("Impact speed that plays at full volume.")]
        [field: SerializeField, Min(0.1f)] public float FullVolumeSpeed { get; private set; } = 8f;

        public FragilityProfile Profile(Fragility fragility)
        {
            foreach (FragilityProfile p in profiles)
                if (p.Fragility == fragility) return p;
            return new FragilityProfile(fragility, 999f, 0f, 0f);
        }

        public void Validate(List<string> errors)
        {
            foreach (Fragility f in System.Enum.GetValues(typeof(Fragility)))
            {
                int count = 0;
                foreach (FragilityProfile p in profiles) if (p.Fragility == f) count++;
                if (count != 1) errors.Add($"{name}: needs exactly one profile for {f} (has {count}).");
            }
            foreach (FragilityProfile p in profiles)
                if (p.ShatterSpeed > 0f && p.ShatterSpeed < p.ImpactThreshold)
                    errors.Add($"{name}: {p.Fragility} shatters below its damage threshold.");
        }
    }
}

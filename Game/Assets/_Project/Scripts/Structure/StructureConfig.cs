using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Every structural tunable: capacities, how overload and impacts drain health, stage
    /// thresholds, failing time, stability scaling, pre-damage and cascades.
    /// Defaults: a 2-ton statue just holds on a healthy balcony; add people or lose stability and it doesn't.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Structure/Structure Config", fileName = "StructureConfig")]
    public class StructureConfig : ScriptableObject, IValidatable
    {
        [SerializeField] private SectionProfile[] profiles =
        {
            new(SectionType.Floor, 3000f, 100f, 1500f),
            new(SectionType.Balcony, 2400f, 100f, 1200f),
            new(SectionType.Stair, 1200f, 100f, 900f),
            new(SectionType.Walkway, 1500f, 100f, 800f),
            new(SectionType.Ceiling, 600f, 60f, 1000f),
        };

        [field: Header("Stages (fraction of max health)")]
        [field: Tooltip("Below this health fraction the section is Stressed (creaks, dust).")]
        [field: SerializeField, Range(0f, 1f)] public float StressedBelow { get; private set; } = 0.85f;
        [field: Tooltip("Below this health fraction it is Cracking (visible cracks, groans).")]
        [field: SerializeField, Range(0f, 1f)] public float CrackingBelow { get; private set; } = 0.5f;
        [field: Tooltip("Load above this fraction of capacity shows at least Stressed, even at full health.")]
        [field: SerializeField, Range(0f, 2f)] public float StressedLoadRatio { get; private set; } = 0.9f;
        [field: Tooltip("Seconds between reaching 0 health (Failing: sag, snapping) and collapsing.")]
        [field: SerializeField, Min(0f)] public float FailingDuration { get; private set; } = 2f;

        [field: Header("Overload")]
        [field: Tooltip("Health lost per second per 100% over capacity (2x capacity = this much per second).")]
        [field: SerializeField, Min(0f)] public float OverloadDrainPerSecond { get; private set; } = 20f;

        [field: Header("Impacts")]
        [field: Tooltip("Impacts below this momentum (gameplay kg × m/s) do nothing.")]
        [field: SerializeField, Min(0f)] public float ImpactThreshold { get; private set; } = 300f;
        [field: Tooltip("Health lost per kg·m/s above the threshold.")]
        [field: SerializeField, Min(0f)] public float ImpactDamagePerMomentum { get; private set; } = 0.02f;
        [field: Tooltip("A single hit can't take a section that's still Stable/Stressed below this health fraction, so there's always a Cracking warning before Failing (GDD 6.4).")]
        [field: SerializeField, Range(0f, 0.5f)] public float WarningFloor { get; private set; } = 0.05f;
        [field: Tooltip("Debris and anything slower than this along the normal is ignored.")]
        [field: SerializeField, Min(0f)] public float MinImpactSpeed { get; private set; } = 1f;

        [field: Header("Load sampling")]
        [field: Tooltip("How far below a load point to look for the section carrying it (load points can be at the top of tall items).")]
        [field: SerializeField, Min(0.1f)] public float MaxSupportDistance { get; private set; } = 5f;

        [field: Header("Stability (contract %) scaling")]
        [field: Tooltip("Capacity multiplier at 0% stability (100% stability = 1).")]
        [field: SerializeField, Range(0.1f, 1f)] public float CapacityAtZeroStability { get; private set; } = 0.5f;
        [field: Tooltip("Overload drain multiplier at 0% stability.")]
        [field: SerializeField, Min(1f)] public float DecayAtZeroStability { get; private set; } = 2.5f;
        [field: Tooltip("Fraction of sections pre-damaged at 0% stability (scales down to 0 at 100%).")]
        [field: SerializeField, Range(0f, 1f)] public float PreDamagedFractionAtZero { get; private set; } = 0.6f;
        [field: SerializeField, Range(0f, 1f)] public float PreDamageMin { get; private set; } = 0.2f;
        [field: SerializeField, Range(0f, 0.95f)] public float PreDamageMax { get; private set; } = 0.7f;
        [field: Tooltip("Pre-damage never leaves a section below this health fraction (authored weaker sections stay as authored), so nothing starts Failing.")]
        [field: SerializeField, Range(0.01f, 0.5f)] public float MinStartHealth { get; private set; } = 0.15f;

        [field: Header("Cascades")]
        [field: Tooltip("How far below a collapsing section to look for sections it lands on.")]
        [field: SerializeField, Min(0f)] public float CascadeSearchDistance { get; private set; } = 8f;
        [field: Tooltip("Scales the falling section's momentum before it hits what's below.")]
        [field: SerializeField, Min(0f)] public float CascadeFactor { get; private set; } = 0.5f;

        [field: Header("Noise and debris")]
        [field: SerializeField, Range(0f, 1f)] public float CreakLoudness { get; private set; } = 0.35f;
        [field: SerializeField, Range(0f, 1f)] public float CrackLoudness { get; private set; } = 0.55f;
        [field: SerializeField, Range(0f, 1f)] public float CollapseLoudness { get; private set; } = 1f;
        [field: Tooltip("Seconds between creak noises while Stressed; halves at Cracking, quarters at Failing.")]
        [field: SerializeField, Min(0.1f)] public float CreakInterval { get; private set; } = 3f;
        [field: SerializeField, Min(0.5f)] public float DebrisLifetime { get; private set; } = 5f;

        public SectionProfile Profile(SectionType type)
        {
            foreach (SectionProfile p in profiles)
                if (p.Type == type) return p;
            return new SectionProfile(type, 1000f, 100f, 1000f);
        }

        public float CapacityScale(float stability) => Mathf.Lerp(CapacityAtZeroStability, 1f, Mathf.Clamp01(stability));

        public float DecayScale(float stability) => Mathf.Lerp(DecayAtZeroStability, 1f, Mathf.Clamp01(stability));

        public float PreDamagedFraction(float stability) => PreDamagedFractionAtZero * (1f - Mathf.Clamp01(stability));

        public void Validate(List<string> errors)
        {
            if (CrackingBelow >= StressedBelow) errors.Add($"{name}: CrackingBelow must be below StressedBelow.");
            if (PreDamageMax < PreDamageMin) errors.Add($"{name}: PreDamageMax is below PreDamageMin.");
            foreach (SectionType t in System.Enum.GetValues(typeof(SectionType)))
            {
                int count = 0;
                foreach (SectionProfile p in profiles) if (p.Type == t) count++;
                if (count != 1) errors.Add($"{name}: needs exactly one profile for {t} (has {count}).");
            }
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Contracts
{
    /// <summary>How contracts roll (GDD 14). One asset in Data/Contracts, listing the modifiers in play.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Contracts/Contract Config", fileName = "ContractConfig")]
    public class ContractConfig : ScriptableObject, IValidatable
    {
        [field: SerializeField] public ContractModifier[] Modifiers { get; private set; } = System.Array.Empty<ContractModifier>();
        [field: SerializeField, Range(1, 6)] public int Offers { get; private set; } = 3;
        [Tooltip("Rolled structural stability range (before modifiers).")]
        [field: SerializeField, Range(0.3f, 1f)] public float StabilityMin { get; private set; } = 0.6f;
        [field: SerializeField, Range(0.3f, 1f)] public float StabilityMax { get; private set; } = 0.9f;
        [Tooltip("Quota at company level 1; grows by QuotaGrowth per level.")]
        [field: SerializeField, Min(0)] public int BaseQuota { get; private set; } = 30000;
        [field: SerializeField, Range(0f, 1f)] public float QuotaGrowth { get; private set; } = 0.15f;
        [Tooltip("Quota share for a crew of 1, 2, 3, 4 (M10.9): solo is playable, co-op is the focus (GDD 3).")]
        [field: SerializeField] public float[] CrewQuotaScale { get; private set; } = { 0.55f, 0.75f, 0.9f, 1f };
        [Tooltip("Random spread of the quota (+/-).")]
        [field: SerializeField, Range(0f, 0.5f)] public float QuotaSpread { get; private set; } = 0.15f;
        [Tooltip("Extraction window range (s).")]
        [field: SerializeField, Min(60f)] public float WindowMin { get; private set; } = 600f;
        [field: SerializeField, Min(60f)] public float WindowMax { get; private set; } = 1080f;
        [Tooltip("Base payout bonus range when the quota is met.")]
        [field: SerializeField, Range(0f, 1f)] public float BonusMin { get; private set; } = 0f;
        [field: SerializeField, Range(0f, 1f)] public float BonusMax { get; private set; } = 0.2f;
        [Tooltip("Chance a contract has no power (lights off) before modifiers.")]
        [field: SerializeField, Range(0f, 1f)] public float PowerOffChance { get; private set; } = 0.2f;

        public float CrewScale(int crew) => CrewQuotaScale == null || CrewQuotaScale.Length == 0 ? 1f
            : CrewQuotaScale[Mathf.Clamp(crew, 1, CrewQuotaScale.Length) - 1];

        public void Validate(List<string> errors)
        {
            if (StabilityMax < StabilityMin) errors.Add($"{name}: StabilityMax below StabilityMin.");
            if (WindowMax < WindowMin) errors.Add($"{name}: WindowMax below WindowMin.");
            if (Modifiers.Length == 0) errors.Add($"{name}: no modifiers.");
            foreach (ContractModifier m in Modifiers) if (m == null) errors.Add($"{name}: a modifier slot is empty.");
        }

#if UNITY_EDITOR
        public void EditorSetModifiers(ContractModifier[] modifiers) => Modifiers = modifiers;
#endif
    }
}

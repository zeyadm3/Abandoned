using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    [CreateAssetMenu(menuName = "Abandoned/Player/Health Config", fileName = "PlayerHealthConfig")]
    public class PlayerHealthConfig : ScriptableObject, IValidatable
    {
        [field: SerializeField, Min(1f)] public float MaxHealth { get; private set; } = 100f;
        [field: SerializeField, Min(1f)] public float MedkitHeal { get; private set; } = 65f;
        [field: SerializeField, Min(1f)] public float ReviveHealth { get; private set; } = 50f;
        [field: Header("Fall damage")]
        [field: SerializeField, Min(0f)] public float SafeFallHeight { get; private set; } = 4f;
        [field: SerializeField, Min(1f)] public float LethalFallHeight { get; private set; } = 12f;
        [field: Tooltip("50 kg carried increases fall damage by 50%. Dragged weight is excluded.")]
        [field: SerializeField, Min(1f)] public float FullWeightPenaltyKg { get; private set; } = 100f;
        [field: SerializeField, Min(1f)] public float MaxWeightMultiplier { get; private set; } = 2.5f;
        [field: SerializeField, Min(0.05f)] public float LandingReportInterval { get; private set; } = 0.35f;
        [field: SerializeField, Min(0.1f)] public float LandingGroundTolerance { get; private set; } = 1.6f;
        [field: Tooltip("Allows an owner landing pose to lead the host's interpolated movement by a few metres.")]
        [field: SerializeField, Min(1f)] public float LandingPoseTolerance { get; private set; } = 6f;
        [field: Header("Local presentation")]
        [field: SerializeField, Range(0.1f, 0.7f)] public float LowHealthThreshold { get; private set; } = 0.35f;
        [field: SerializeField, Min(0.1f)] public float DamageFadeSeconds { get; private set; } = 1.25f;
        [field: SerializeField, Range(0f, 1f)] public float DamageVignette { get; private set; } = 0.7f;
        [field: SerializeField, Range(0f, 1f)] public float HeartbeatVolume { get; private set; } = 0.32f;
        [field: SerializeField, Min(1f)] public float ThreatHeartbeatRange { get; private set; } = 18f;

        private static PlayerHealthConfig fallback;
        public static PlayerHealthConfig Default => fallback != null ? fallback : fallback = CreateInstance<PlayerHealthConfig>();

        public float FallDamage(float height, float carriedKg)
        {
            if (height <= SafeFallHeight) return 0f;
            float fraction = Mathf.InverseLerp(SafeFallHeight, LethalFallHeight, height);
            float weight = Mathf.Clamp(1f + Mathf.Max(0f, carriedKg) / FullWeightPenaltyKg, 1f, MaxWeightMultiplier);
            return Mathf.Min(MaxHealth, fraction * fraction * MaxHealth * weight);
        }

        public void Validate(List<string> errors)
        {
            if (LethalFallHeight <= SafeFallHeight) errors.Add($"{name}: lethal fall must exceed safe fall height.");
            if (MaxHealth <= 0f) errors.Add($"{name}: maximum health must be positive.");
        }
    }
}

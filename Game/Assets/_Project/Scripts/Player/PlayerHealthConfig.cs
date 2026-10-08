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
        [field: Tooltip("Drops up to this height (m) are free. Below one 4 m storey on purpose (2026-10-09): falling a floor must hurt.")]
        [field: SerializeField, Min(0f)] public float SafeFallHeight { get; private set; } = 2.5f;
        [field: SerializeField, Min(1f)] public float LethalFallHeight { get; private set; } = 12f;
        [field: Tooltip("Shape of the damage between the safe and the lethal height: 1 = even all the way, 2 = gentle at first (the old curve).")]
        [field: SerializeField, Range(0.5f, 3f)] public float FallDamageCurve { get; private set; } = 1f;
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
        [Tooltip("Seconds between heartbeats, calm (x) to most afraid (y); and between breaths, calm to afraid.")]
        [field: SerializeField] public Vector2 HeartbeatInterval { get; private set; } = new(1.05f, 0.48f);
        [field: SerializeField] public Vector2 BreathInterval { get; private set; } = new(4.2f, 2f);
        [Tooltip("Fear from darkness alone: flashlight off (x) and on (y). Sprinting counts as SprintFear.")]
        [field: SerializeField] public Vector2 DarknessFear { get; private set; } = new(0.28f, 0.05f);
        [field: SerializeField, Range(0f, 1f)] public float SprintFear { get; private set; } = 0.4f;

        [field: Header("Vitals bar colour by health (user spec, 2026-10-09; thresholds raised the same day)")]
        [field: Tooltip("Full health down to WarningHp: solid electric blue.")]
        [field: SerializeField] public Color SafeColor { get; private set; } = new(0.04f, 0.52f, 1f);
        [field: Tooltip("From BlendStartHp down to WarningHp the blue leans toward this red...")]
        [field: SerializeField] public Color BlendTowardColor { get; private set; } = new(1f, 0.12f, 0.1f);
        [field: Tooltip("...by this much just above WarningHp. Keep it small: the spec asks for a subtle lean.")]
        [field: SerializeField, Range(0f, 1f)] public float SafeBlend { get; private set; } = 0.4f;
        [field: Tooltip("WarningHp down to CriticalHp: the bar moves into this orange-red.")]
        [field: SerializeField] public Color WarningColor { get; private set; } = new(1f, 0.27f, 0.03f);
        [field: Tooltip("CriticalHp down to SevereHp: solid crimson.")]
        [field: SerializeField] public Color CriticalColor { get; private set; } = new(0.86f, 0.08f, 0.16f);
        [field: Tooltip("SevereHp and below: dark blood red, pulsing up to LethalPulseColor on each heartbeat.")]
        [field: SerializeField] public Color LethalColor { get; private set; } = new(0.36f, 0.01f, 0.02f);
        [field: SerializeField] public Color LethalPulseColor { get; private set; } = new(0.7f, 0.03f, 0.05f);
        // The spec's first numbers (50 / 40 / 30 / 20 / 10) came too late in play: the user wants urgency sooner,
        // so one ordinary hit already brings the heartbeat and the blood.
        [field: Tooltip("Whole HP, as the HUD shows it. The bar starts leaning toward red here.")]
        [field: SerializeField, Min(0f)] public float BlendStartHp { get; private set; } = 80f;
        [field: Tooltip("The bar turns toward orange-red from here.")]
        [field: SerializeField, Min(0f)] public float WarningHp { get; private set; } = 60f;
        [field: Tooltip("Critical: crimson bar, and the steady heartbeat starts.")]
        [field: SerializeField, Min(0f)] public float CriticalHp { get; private set; } = 50f;
        [field: Tooltip("Severe: pulsing blood-red bar, faster heartbeat, muffled world, vignette, blood in the corners.")]
        [field: SerializeField, Min(0f)] public float SevereHp { get; private set; } = 35f;
        [field: Tooltip("Near death: fastest heartbeat, closing vignette, heavy dripping blood.")]
        [field: SerializeField, Min(0f)] public float NearDeathHp { get; private set; } = 20f;

        [field: Header("Low-health heartbeat: critical (x), severe (y), near death (z)")]
        [field: SerializeField] public Vector3 HeartbeatBpm { get; private set; } = new(75f, 105f, 145f);
        [field: SerializeField] public Vector3 TraumaHeartbeatVolume { get; private set; } = new(0.42f, 0.72f, 1f);
        [field: SerializeField] public Vector3 TraumaHeartbeatPitch { get; private set; } = new(0.88f, 1f, 1.12f);
        [field: Tooltip("Low-pass (Hz) on the world's sound from severe down. The player's own pulse and breath, UI and crew voices stay clear.")]
        [field: SerializeField, Range(500f, 22000f)] public float MuffleCutoff { get; private set; } = 2600f;

        [field: Header("Low-health vision")]
        [field: SerializeField] public Color TraumaVignetteColor { get; private set; } = new(0.36f, 0f, 0.012f);
        [field: Tooltip("Severe: vignette at SevereHp (x) and just above NearDeathHp (y); it creeps inward as health falls.")]
        [field: SerializeField] public Vector2 SevereVignette { get; private set; } = new(0.3f, 0.43f);
        [field: Tooltip("Near death: vignette at NearDeathHp (x) and at 0 HP (y).")]
        [field: SerializeField] public Vector2 NearDeathVignette { get; private set; } = new(0.54f, 0.68f);
        [field: Tooltip("How soft the vignette's inner edge is: severe (x), near death (y).")]
        [field: SerializeField] public Vector2 TraumaVignetteSmoothness { get; private set; } = new(0.82f, 1f);
        [field: Tooltip("Near death: how much each heartbeat swells the vignette, at NearDeathHp (x) and at 0 HP (y).")]
        [field: SerializeField] public Vector2 VignettePulse { get; private set; } = new(0.05f, 0.12f);
        [field: Tooltip("Chromatic aberration: severe (x), near death (y).")]
        [field: SerializeField] public Vector2 TraumaChromaticAberration { get; private set; } = new(0.4f, 0.8f);
        [field: Tooltip("Near death: a slight blur and lens warp over the whole view.")]
        [field: SerializeField, Range(0f, 2f)] public float NearDeathBlur { get; private set; } = 0.55f;
        [field: SerializeField, Range(-0.5f, 0.5f)] public float NearDeathDistortion { get; private set; } = -0.14f;
        [field: Tooltip("Blood on the lens: the corner splatter from severe (x), the heavy layers near death (y).")]
        [field: SerializeField] public Vector2 BloodOpacity { get; private set; } = new(0.85f, 0.92f);
        [field: Tooltip("Near death the drips run this much further down the screen (fraction of its height) over DripSeconds.")]
        [field: SerializeField, Range(0f, 0.5f)] public float DripStretch { get; private set; } = 0.14f;
        [field: SerializeField, Min(1f)] public float DripSeconds { get; private set; } = 24f;
        [field: Tooltip("Seconds the vision and blood take to fade in or out when a threshold is crossed.")]
        [field: SerializeField, Min(0.05f)] public float TraumaFadeSeconds { get; private set; } = 0.9f;

        private static PlayerHealthConfig fallback;
        public static PlayerHealthConfig Default => fallback != null ? fallback : fallback = CreateInstance<PlayerHealthConfig>();

        public float FallDamage(float height, float carriedKg)
        {
            if (height <= SafeFallHeight) return 0f;
            float fraction = Mathf.InverseLerp(SafeFallHeight, LethalFallHeight, height);
            float weight = Mathf.Clamp(1f + Mathf.Max(0f, carriedKg) / FullWeightPenaltyKg, 1f, MaxWeightMultiplier);
            return Mathf.Min(MaxHealth, Mathf.Pow(fraction, FallDamageCurve) * MaxHealth * weight);
        }

        /// <summary>0 = fine, 1 = critical, 2 = severe trauma, 3 = near death. Judged on whole HP, as the HUD shows it.</summary>
        public int TraumaTier(float health)
        {
            int hp = Mathf.CeilToInt(health);
            return hp <= NearDeathHp ? 3 : hp <= SevereHp ? 2 : hp <= CriticalHp ? 1 : 0;
        }

        /// <summary>The per-threshold value of a critical / severe / near-death triple.</summary>
        public static float Pick(Vector3 perTier, int tier) => tier >= 3 ? perTier.z : tier == 2 ? perTier.y : perTier.x;

        /// <summary>
        /// The vitals bar's colour: blue, leaning toward red from BlendStartHp, into orange-red from WarningHp, crimson
        /// from CriticalHp, then from SevereHp dark blood red that swells with each heartbeat (<paramref name="beat"/> 0..1).
        /// </summary>
        public Color HealthColor(float health, float beat)
        {
            int hp = Mathf.CeilToInt(health);
            Color leaned = Color.Lerp(SafeColor, BlendTowardColor, SafeBlend);
            if (hp > WarningHp) return Color.Lerp(SafeColor, leaned, Mathf.InverseLerp(BlendStartHp, WarningHp + 1f, health));
            if (hp > CriticalHp) return Color.Lerp(leaned, WarningColor, Mathf.InverseLerp(WarningHp, CriticalHp + 1f, health));
            if (hp > SevereHp) return CriticalColor;
            return Color.Lerp(LethalColor, LethalPulseColor, Mathf.Clamp01(beat));
        }

        public void Validate(List<string> errors)
        {
            if (!(BlendStartHp > WarningHp && WarningHp > CriticalHp && CriticalHp > SevereHp && SevereHp > NearDeathHp))
                errors.Add($"{name}: the health thresholds must fall in order (blend start > warning > critical > severe > near death).");
            if (LethalFallHeight <= SafeFallHeight) errors.Add($"{name}: lethal fall must exceed safe fall height.");
            if (MaxHealth <= 0f) errors.Add($"{name}: maximum health must be positive.");
        }
    }
}

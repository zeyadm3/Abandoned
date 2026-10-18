using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// How often monsters arrive and how they find the crew in a big building (2026-10-08: half an hour in
    /// the mall could pass without meeting anything). Data/Threats/Resources/ThreatPacingConfig; code defaults
    /// apply when the asset is missing.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Threats/Threat Pacing Config", fileName = "ThreatPacingConfig")]
    public class ThreatPacingConfig : ScriptableObject, IValidatable
    {
        public const string ResourcePath = "ThreatPacingConfig";

        [Header("Arrivals")]
        [Tooltip("Seconds into the run before the first monster appears.")]
        [field: SerializeField, Min(1f)] public float OpeningDelay { get; private set; } = 30f;
        [field: SerializeField, Min(1)] public int OpeningCount { get; private set; } = 1;
        [field: SerializeField, Min(1f)] public float SpawnRetry { get; private set; } = 3f;
        [field: SerializeField, Min(1f)] public float OpeningStagger { get; private set; } = 12f;
        [Tooltip("Seconds between later arrivals; each gap is a seeded pick in this range. Runs the whole job, not just while danger rises.")]
        [field: SerializeField] public Vector2 ArrivalInterval { get; private set; } = new(60f, 95f);
        [Tooltip("Monsters alive at once: this many, plus PerPlayer for every crew member, up to MaxAlive (the final hunter isn't counted).")]
        [field: SerializeField, Min(1)] public int BaseAlive { get; private set; } = 2;
        [field: SerializeField, Min(0)] public int PerPlayer { get; private set; } = 1;
        [field: SerializeField, Range(1, 12)] public int MaxAlive { get; private set; } = 6;

        [Header("Encounter and escape windows")]
        [Tooltip("Ordinary monsters leave after this much continuous engagement, even if sight/noise keeps refreshing. The Last Hunter is exempt.")]
        [field: SerializeField, Min(5f)] public float MaxEncounterSeconds { get; private set; } = 25f;
        [Tooltip("Sight-based monsters search their last known position for at most this long after losing sight.")]
        [field: SerializeField, Min(1f)] public float LostSightSearchSeconds { get; private set; } = 6f;
        [Tooltip("After an encounter, the monster walks away and ignores new targets/noise for this long. Pacing cannot redirect or relocate it.")]
        [field: SerializeField, Min(5f)] public float RecoverySeconds { get; private set; } = 25f;
        [Tooltip("Radius used to find connected retreat destinations away from the crew.")]
        [field: SerializeField, Min(5f)] public float WithdrawalDistance { get; private set; } = 22f;

        [Header("Drift toward the crew")]
        [Tooltip("A monster with nobody within DriftRange for this long wanders toward a random living player's area instead.")]
        [field: SerializeField, Min(5f)] public float DriftAfter { get; private set; } = 40f;
        [field: SerializeField, Min(5f)] public float DriftRange { get; private set; } = 24f;
        [Tooltip("How far from that player the drift point may land (it still has to hear or see them).")]
        [field: SerializeField, Min(2f)] public float DriftScatter { get; private set; } = 18f;

        [Header("Relocation")]
        [Tooltip("A monster further than this from every living player for RelocateAfter seconds is moved to a spawn point near the crew.")]
        [field: SerializeField, Min(10f)] public float RelocateDistance { get; private set; } = 60f;
        [field: SerializeField, Min(10f)] public float RelocateAfter { get; private set; } = 120f;

        [Header("Contract threat level: LOW / MEDIUM / HIGH on the board (2026-10-09)")]
        [Tooltip("Multiplies the seconds between arrivals, per level.")]
        [field: SerializeField] public float[] LevelGapScale { get; private set; } = { 1.15f, 1f, 0.75f };
        [Tooltip("Multiplies the opening delay, per level.")]
        [field: SerializeField] public float[] LevelOpeningScale { get; private set; } = { 1.2f, 1f, 0.75f };
        [Tooltip("Added to the alive cap, per level.")]
        [field: SerializeField] public int[] LevelAliveBonus { get; private set; } = { -1, 0, 1 };
        [Tooltip("Extra monsters that arrive with the first one, per level.")]
        [field: SerializeField] public int[] LevelOpeningExtras { get; private set; } = { 0, 0, 1 };
        [Tooltip("Multiplies non-lethal monster damage, per level.")]
        [field: SerializeField] public float[] LevelDamageScale { get; private set; } = { 0.9f, 1.1f, 1.25f };
        [Tooltip("On LOW the first monster is never a one-touch killer; it can still arrive later.")]
        [field: SerializeField] public bool LowSparesLethalOpener { get; private set; } = true;
        [Tooltip("A company's very first job: the first monster waits at least this long (s).")]
        [field: SerializeField, Min(0f)] public float FirstJobOpeningDelay { get; private set; } = 45f;

        private static ThreatPacingConfig current;

        /// <summary>The project's asset, or code defaults when it hasn't been created yet.</summary>
        public static ThreatPacingConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<ThreatPacingConfig>(ResourcePath);
                if (current == null) current = CreateInstance<ThreatPacingConfig>();
                return current;
            }
        }

        public int AliveCap(int crew) => Mathf.Clamp(BaseAlive + PerPlayer * Mathf.Max(1, crew), 1, MaxAlive);

        public int AliveCap(int crew, int level) =>
            Mathf.Clamp(BaseAlive + PerPlayer * Mathf.Max(1, crew) + At(LevelAliveBonus, level, 0), 1, MaxAlive);

        public float GapScale(int level) => Mathf.Max(0.1f, At(LevelGapScale, level, 1f));
        public float DamageScale(int level) => Mathf.Max(0.1f, At(LevelDamageScale, level, 1f));
        public int OpeningExtras(int level) => Mathf.Max(0, At(LevelOpeningExtras, level, 0));

        public float OpeningDelayFor(int level, bool firstJob)
        {
            float delay = OpeningDelay * Mathf.Max(0.1f, At(LevelOpeningScale, level, 1f));
            return firstJob ? Mathf.Max(delay, FirstJobOpeningDelay) : delay;
        }

        // An asset saved before these lists existed, or one trimmed by hand, falls back to "no change".
        private static T At<T>(T[] values, int level, T fallback) =>
            values != null && level >= 0 && level < values.Length ? values[level] : fallback;

        public float NextGap(System.Random random)
        {
            float min = Mathf.Max(5f, Mathf.Min(ArrivalInterval.x, ArrivalInterval.y));
            float max = Mathf.Max(min, Mathf.Max(ArrivalInterval.x, ArrivalInterval.y));
            return min + (float)random.NextDouble() * (max - min);
        }

        public void Validate(List<string> errors)
        {
            if (ArrivalInterval.x <= 0f || ArrivalInterval.y <= 0f) errors.Add($"{name}: ArrivalInterval must be positive.");
            if (BaseAlive > MaxAlive) errors.Add($"{name}: BaseAlive is above MaxAlive.");
            if (MaxEncounterSeconds <= 0f || LostSightSearchSeconds <= 0f || RecoverySeconds <= 0f || WithdrawalDistance <= 0f)
                errors.Add($"{name}: encounter, search, recovery and withdrawal settings must be positive.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

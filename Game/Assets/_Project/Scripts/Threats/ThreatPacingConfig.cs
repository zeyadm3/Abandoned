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
        [field: SerializeField, Min(1f)] public float OpeningDelay { get; private set; } = 25f;
        [Tooltip("Seconds between later arrivals; each gap is a seeded pick in this range. Runs the whole job, not just while danger rises.")]
        [field: SerializeField] public Vector2 ArrivalInterval { get; private set; } = new(90f, 150f);
        [Tooltip("Monsters alive at once: this many, plus PerPlayer for every crew member, up to MaxAlive (the final hunter isn't counted).")]
        [field: SerializeField, Min(1)] public int BaseAlive { get; private set; } = 3;
        [field: SerializeField, Min(0)] public int PerPlayer { get; private set; } = 1;
        [field: SerializeField, Range(1, 12)] public int MaxAlive { get; private set; } = 8;

        [Header("Drift toward the crew")]
        [Tooltip("A monster with nobody within DriftRange for this long wanders toward a random living player's area instead.")]
        [field: SerializeField, Min(5f)] public float DriftAfter { get; private set; } = 60f;
        [field: SerializeField, Min(5f)] public float DriftRange { get; private set; } = 30f;
        [Tooltip("How far from that player the drift point may land (it still has to hear or see them).")]
        [field: SerializeField, Min(2f)] public float DriftScatter { get; private set; } = 15f;

        [Header("Relocation")]
        [Tooltip("A monster further than this from every living player for RelocateAfter seconds is moved to a spawn point near the crew.")]
        [field: SerializeField, Min(10f)] public float RelocateDistance { get; private set; } = 45f;
        [field: SerializeField, Min(10f)] public float RelocateAfter { get; private set; } = 120f;

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
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

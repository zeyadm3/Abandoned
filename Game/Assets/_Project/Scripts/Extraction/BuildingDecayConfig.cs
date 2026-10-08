using System;
using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The longer the crew stays, the faster the building comes apart (user, 2026-10-08): nothing for the
    /// first minutes, then a section starts failing on a shrinking interval, and past the last minute the
    /// whole building comes down. Times are minutes on the run clock (from the truck's arrival).
    /// Data/Extraction/Resources/BuildingDecayConfig; code defaults apply when the asset is missing.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Building Decay Config", fileName = "BuildingDecayConfig")]
    public class BuildingDecayConfig : ScriptableObject, IValidatable
    {
        public const string ResourcePath = "BuildingDecayConfig";

        [Serializable]
        public struct Stage
        {
            [Tooltip("From this minute of the run...")]
            public float FromMinute;
            [Tooltip("...a section starts failing every this many seconds.")]
            public float Interval;

            public Stage(float fromMinute, float interval)
            {
                FromMinute = fromMinute;
                Interval = interval;
            }
        }

        [Tooltip("In increasing minute order. Before the first stage nothing collapses on its own.")]
        [SerializeField] private Stage[] stages =
        {
            new(3f, 50f),
            new(10f, 30f),
            new(17f, 20f),
            new(25f, 10f),
        };

        [Header("Each failure")]
        [Tooltip("Seconds a picked section cracks and groans before it gives way (then the usual Failing sag).")]
        [field: SerializeField, Min(0.5f)] public float WarningSeconds { get; private set; } = 5f;
        [Tooltip("Health left while it cracks (below the Cracking threshold, so it shows).")]
        [field: SerializeField, Range(0.01f, 0.3f)] public float CrackedHealth { get; private set; } = 0.06f;
        [Tooltip("Chance a pick comes from near the crew, so they see and hear the building go.")]
        [field: SerializeField, Range(0f, 1f)] public float NearCrewChance { get; private set; } = 0.7f;
        [field: SerializeField, Min(4f)] public float NearCrewRadius { get; private set; } = 30f;

        [Header("Total collapse")]
        [Tooltip("At this minute the whole building starts coming down.")]
        [field: SerializeField, Min(1f)] public float TotalCollapseMinute { get; private set; } = 40f;
        [Tooltip("Seconds from the alarm until it is down; anyone still inside then dies and the job is lost.")]
        [field: SerializeField, Min(10f)] public float TotalCollapseSeconds { get; private set; } = 60f;
        [Tooltip("Share of the countdown spent bringing sections down (top floor first); the rest is the last dash.")]
        [field: SerializeField, Range(0.3f, 1f)] public float TotalCollapseSpread { get; private set; } = 0.9f;
        [field: SerializeField] public string CrushedLine { get; private set; } = "The mall came down on you.";

        public IReadOnlyList<Stage> Stages => stages;

        private static BuildingDecayConfig current;

        /// <summary>The project's asset, or code defaults when it hasn't been created yet.</summary>
        public static BuildingDecayConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<BuildingDecayConfig>(ResourcePath);
                if (current == null) current = CreateInstance<BuildingDecayConfig>();
                return current;
            }
        }

        /// <summary>Seconds between failures at this point of the run; 0 = none yet.</summary>
        public float IntervalAt(float elapsedSeconds)
        {
            float minutes = elapsedSeconds / 60f, interval = 0f;
            foreach (Stage s in stages)
                if (minutes >= s.FromMinute) interval = Mathf.Max(1f, s.Interval);
            return interval;
        }

        public float TotalCollapseAt => TotalCollapseMinute * 60f;

        public void Validate(List<string> errors)
        {
            for (int i = 1; i < stages.Length; i++)
                if (stages[i].FromMinute < stages[i - 1].FromMinute) errors.Add($"{name}: stages must be in increasing minute order.");
            if (stages.Length > 0 && stages[^1].FromMinute >= TotalCollapseMinute) errors.Add($"{name}: the last stage starts after the total collapse.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

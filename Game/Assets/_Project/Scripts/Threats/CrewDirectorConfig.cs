using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// Tunables for the Crew Director (0.24.10): how fast each player's hidden stress rises and drains, where the
    /// peak is, and how long the quiet after it lasts. Changes when monsters come, never how hard they hit.
    /// Data/Threats/Resources/CrewDirectorConfig; code defaults apply when the asset is missing.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Threats/Crew Director Config", fileName = "CrewDirectorConfig")]
    public class CrewDirectorConfig : ScriptableObject, IValidatable
    {
        public const string ResourcePath = "CrewDirectorConfig";

        [Tooltip("Off: monsters arrive on ThreatPacingConfig's clock alone, as before 0.24.10.")]
        [field: SerializeField] public bool Enabled { get; private set; } = true;

        [Header("Stress (0-100, one hidden number per player, host only)")]
        [field: SerializeField, Min(0f)] public float PerHealthLost { get; private set; } = 1.2f;
        [field: SerializeField, Min(0f)] public float KnockedDown { get; private set; } = 20f;
        [Tooltip("Per second while a chasing or attacking monster is within ChaseRange.")]
        [field: SerializeField, Min(0f)] public float ChasedPerSecond { get; private set; } = 6f;
        [field: SerializeField, Min(1f)] public float ChaseRange { get; private set; } = 14f;
        [Tooltip("A floor section within StructureRange cracking further.")]
        [field: SerializeField, Min(0f)] public float SectionCracked { get; private set; } = 8f;
        [field: SerializeField, Min(0f)] public float SectionCollapsed { get; private set; } = 25f;
        [field: SerializeField, Min(1f)] public float StructureRange { get; private set; } = 8f;
        [field: SerializeField, Min(0f)] public float CrewmateDied { get; private set; } = 35f;
        [field: SerializeField, Min(1f)] public float CrewmateRange { get; private set; } = 18f;
        [Tooltip("Stress drains this much per second, but never while chased and not until DrainDelay after the last rise.")]
        [field: SerializeField, Min(0f)] public float DrainPerSecond { get; private set; } = 5f;
        [field: SerializeField, Min(0f)] public float DrainDelay { get; private set; } = 2f;

        [Header("The loop")]
        [Tooltip("When the most stressed living player reaches this, the build-up has peaked.")]
        [field: SerializeField, Range(10f, 100f)] public float Peak { get; private set; } = 70f;
        [Tooltip("Build-up lasts at least this long, so stress left over from the last peak can't skip it.")]
        [field: SerializeField, Min(0f)] public float MinBuildUp { get; private set; } = 20f;
        [field: SerializeField] public Vector2 SustainSeconds { get; private set; } = new(3f, 5f);
        [Tooltip("The fade ends when the highest stress drops below this, or after MaxFade seconds.")]
        [field: SerializeField, Min(0f)] public float FadeEndsBelow { get; private set; } = 35f;
        [field: SerializeField, Min(1f)] public float MaxFade { get; private set; } = 15f;
        [field: SerializeField] public Vector2 RelaxSeconds { get; private set; } = new(30f, 45f);

        [Header("Quiet")]
        [Tooltip("During fade and relax, idle monsters wander to a spawn point at least this far from every living player.")]
        [field: SerializeField, Min(0f)] public float RetreatDistance { get; private set; } = 28f;

        private static CrewDirectorConfig current;

        public static CrewDirectorConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<CrewDirectorConfig>(ResourcePath);
                if (current == null) current = CreateInstance<CrewDirectorConfig>();
                return current;
            }
        }

        public static float Roll(Vector2 range, System.Random random)
        {
            float min = Mathf.Max(0f, Mathf.Min(range.x, range.y)), max = Mathf.Max(range.x, range.y);
            return min + (float)random.NextDouble() * (max - min);
        }

        public void Validate(List<string> errors)
        {
            if (FadeEndsBelow >= Peak) errors.Add($"{name}: FadeEndsBelow must be under Peak.");
            if (RelaxSeconds.x <= 0f || RelaxSeconds.y <= 0f) errors.Add($"{name}: RelaxSeconds must be positive.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

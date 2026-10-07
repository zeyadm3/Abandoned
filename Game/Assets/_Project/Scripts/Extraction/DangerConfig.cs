using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>How a run gets worse (GDD 9 threat escalation, PLAYBOOK 5.6). One asset in Data/Extraction.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Danger Config", fileName = "DangerConfig")]
    public class DangerConfig : ScriptableObject, IValidatable
    {
        [Tooltip("Seconds per danger level during the extraction window.")]
        [field: SerializeField, Min(10f)] public float LevelInterval { get; private set; } = 180f;
        [Tooltip("Levels added at once when the window closes; after that it climbs twice as fast.")]
        [field: SerializeField, Range(0, 5)] public int WindowClosedJump { get; private set; } = 2;
        [field: SerializeField, Range(1, 10)] public int MaxLevel { get; private set; } = 6;

        [Header("Per level")]
        [Tooltip("Overloaded sections fail this much faster per level (0.25 = +25 %).")]
        [field: SerializeField, Range(0f, 1f)] public float DecayPerLevel { get; private set; } = 0.25f;
        [Tooltip("Monsters hear this much further per level.")]
        [field: SerializeField, Range(0f, 0.5f)] public float HearingPerLevel { get; private set; } = 0.15f;
        [Tooltip("Monsters move this much faster per level.")]
        [field: SerializeField, Range(0f, 0.3f)] public float SpeedPerLevel { get; private set; } = 0.08f;
        [Tooltip("At this level a second Blind One appears (GDD 9: more threats spawn).")]
        [field: SerializeField, Range(1, 10)] public int ExtraThreatLevel { get; private set; } = 3;

        [Header("The building ages")]
        [Tooltip("Seconds between ageing ticks; each tick weakens `level` random upper sections.")]
        [field: SerializeField, Range(1f, 60f)] public float AgingInterval { get; private set; } = 12f;
        [field: SerializeField, Range(0f, 0.5f)] public float AgingDamage { get; private set; } = 0.08f;
        [Tooltip("Ageing alone never takes a section below this health (weight finishes the job).")]
        [field: SerializeField, Range(0f, 1f)] public float AgingFloor { get; private set; } = 0.3f;

        public void Validate(List<string> errors)
        {
            if (MaxLevel < 1) errors.Add($"{name}: MaxLevel must be at least 1.");
        }

        /// <summary>The level at a moment of the run: steady during the window, a jump and double pace after.</summary>
        public int LevelAt(float elapsed, float window)
        {
            if (elapsed < window) return Mathf.Min(MaxLevel, Mathf.FloorToInt(elapsed / LevelInterval));
            int atClose = Mathf.FloorToInt(window / LevelInterval) + WindowClosedJump;
            return Mathf.Min(MaxLevel, atClose + Mathf.FloorToInt((elapsed - window) / (LevelInterval / 2f)));
        }
    }
}

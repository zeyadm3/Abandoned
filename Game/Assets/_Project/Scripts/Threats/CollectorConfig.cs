using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>The Collector (GDD 9): harmless, steals unattended loot and hides it in the building.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Threats/Collector Config", fileName = "CollectorConfig")]
    public class CollectorConfig : ScriptableObject, IValidatable
    {
        [Tooltip("Loot counts as unattended with no player within this (m).")]
        [field: SerializeField, Range(3f, 40f)] public float GuardRadius { get; private set; } = 10f;
        [Tooltip("A player this close (m) makes it drop what it carries and flee.")]
        [field: SerializeField, Range(1f, 10f)] public float ScareRadius { get; private set; } = 4f;
        [field: SerializeField, Range(0.5f, 6f)] public float Speed { get; private set; } = 2.8f;
        [field: SerializeField, Range(1f, 10f)] public float FleeSpeed { get; private set; } = 5f;
        [Tooltip("It hides loot at least this far (m) from where it took it.")]
        [field: SerializeField, Range(5f, 60f)] public float HideDistance { get; private set; } = 18f;
        [Tooltip("Seconds between thefts.")]
        [field: SerializeField, Range(0f, 120f)] public float Cooldown { get; private set; } = 12f;
        [Tooltip("Seconds it keeps walking to one item before it gives up on it for the run (unreachable).")]
        [field: SerializeField, Range(5f, 120f)] public float SeekTimeout { get; private set; } = 25f;

        public void Validate(List<string> errors)
        {
            if (FleeSpeed < Speed) errors.Add($"{name}: FleeSpeed should be at least Speed.");
        }
    }
}

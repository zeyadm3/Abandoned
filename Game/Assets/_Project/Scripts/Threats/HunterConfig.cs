using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>The Hunter (GDD 9): patrols, chases on sight, kills on contact; heavy enough to break weak floors.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Threats/Hunter Config", fileName = "HunterConfig")]
    public class HunterConfig : ScriptableObject, IValidatable
    {
        [field: SerializeField, Range(0.5f, 5f)] public float PatrolSpeed { get; private set; } = 2.2f;
        [Tooltip("Between walking (4.5) and sprinting (7): walkers get caught, sprinters escape while their stamina lasts.")]
        [field: SerializeField, Range(2f, 10f)] public float ChaseSpeed { get; private set; } = 5.2f;
        [field: SerializeField, Range(5f, 60f)] public float SightRange { get; private set; } = 22f;
        [Tooltip("Full cone (degrees) it sees in front of it.")]
        [field: SerializeField, Range(30f, 360f)] public float SightAngle { get; private set; } = 110f;
        [Tooltip("A crouching player is only seen within this (m): hiding works.")]
        [field: SerializeField, Range(1f, 30f)] public float CrouchSightRange { get; private set; } = 9f;
        [Tooltip("It senses anyone this close, whatever way it faces.")]
        [field: SerializeField, Range(0f, 6f)] public float CloseSense { get; private set; } = 3f;
        [Tooltip("Seconds without sight before a chase becomes a search.")]
        [field: SerializeField, Range(0.5f, 10f)] public float LoseAfter { get; private set; } = 3f;
        [Tooltip("Seconds it searches where it last saw you before patrolling again.")]
        [field: SerializeField, Range(1f, 30f)] public float SearchSeconds { get; private set; } = 7f;
        [field: SerializeField, Range(0.5f, 3f)] public float AttackRange { get; private set; } = 1.3f;
        [Tooltip("Gameplay kg on the floor under it (GDD 9: lure it onto weak floors).")]
        [field: SerializeField, Range(0f, 2000f)] public float Weight { get; private set; } = 450f;
        [Tooltip("Seconds it lies stunned after falling through a floor.")]
        [field: SerializeField, Range(0f, 20f)] public float StunSeconds { get; private set; } = 5f;
        [Tooltip("Patrol picks points within this distance of where it is.")]
        [field: SerializeField, Range(5f, 60f)] public float PatrolRadius { get; private set; } = 24f;

        public void Validate(List<string> errors)
        {
            if (ChaseSpeed <= PatrolSpeed) errors.Add($"{name}: ChaseSpeed should be above PatrolSpeed.");
            if (CrouchSightRange > SightRange) errors.Add($"{name}: CrouchSightRange can't exceed SightRange.");
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>The Stalker (GDD 9): follows at a distance, stares, attacks isolated players who look away too long.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Threats/Stalker Config", fileName = "StalkerConfig")]
    public class StalkerConfig : ScriptableObject, IValidatable
    {
        [Tooltip("It keeps about this far (m) from its target.")]
        [field: SerializeField, Range(3f, 30f)] public float FollowDistance { get; private set; } = 11f;
        [field: SerializeField, Range(0.5f, 5f)] public float FollowSpeed { get; private set; } = 2.4f;
        [field: SerializeField, Range(2f, 12f)] public float RushSpeed { get; private set; } = 6.5f;
        [Tooltip("A target with no teammate within this (m) is isolated.")]
        [field: SerializeField, Range(2f, 30f)] public float IsolationRadius { get; private set; } = 9f;
        [Tooltip("Seconds an isolated target can look away before it rushes.")]
        [field: SerializeField, Range(1f, 30f)] public float RushAfter { get; private set; } = 7f;
        [Tooltip("Looking within this angle (degrees) of it, with a clear line, counts as watching it.")]
        [field: SerializeField, Range(10f, 90f)] public float WatchAngle { get; private set; } = 40f;
        [field: SerializeField, Range(5f, 60f)] public float WatchRange { get; private set; } = 28f;
        [field: SerializeField, Range(0.5f, 3f)] public float AttackRange { get; private set; } = 1.2f;
        [Tooltip("How often (s) it reconsiders who is most alone.")]
        [field: SerializeField, Range(1f, 30f)] public float RetargetInterval { get; private set; } = 5f;

        public void Validate(List<string> errors)
        {
            if (RushSpeed < FollowSpeed) errors.Add($"{name}: RushSpeed should be at least FollowSpeed.");
        }
    }
}

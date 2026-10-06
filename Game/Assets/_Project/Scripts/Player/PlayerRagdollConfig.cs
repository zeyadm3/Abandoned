using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>When the player goes ragdoll and how they get back up.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Player/Ragdoll Config", fileName = "PlayerRagdollConfig")]
    public class PlayerRagdollConfig : ScriptableObject, IValidatable
    {
        [field: Tooltip("Falls from higher than this (peak to landing, m) knock the player down.")]
        [field: SerializeField, Min(0.5f)] public float FallHeight { get; private set; } = 4f;

        [field: Tooltip("Momentum (gameplay kg × closing speed m/s) of a hit that knocks the player down.")]
        [field: SerializeField, Min(0f)] public float HitMomentum { get; private set; } = 150f;
        [field: Tooltip("Hits slower than this never count, so leaning on a resting safe is fine.")]
        [field: SerializeField, Min(0f)] public float MinHitSpeed { get; private set; } = 1.5f;
        [field: Tooltip("How much of the hitting object's velocity the ragdoll takes on.")]
        [field: SerializeField, Range(0f, 1f)] public float HitVelocityTransfer { get; private set; } = 0.6f;

        [field: Header("Collapses")]
        [field: Tooltip("Extra margin around a collapsing section's surface for who goes down with it. 0 = feet must be over it.")]
        [field: SerializeField, Min(0f)] public float CollapseMargin { get; private set; } = 0f;
        [field: Tooltip("Height band above the surface (m) that still counts as standing on it.")]
        [field: SerializeField, Min(0f)] public float CollapseStandingBand { get; private set; } = 0.6f;
        [field: SerializeField, Min(0f)] public float CollapseDropSpeed { get; private set; } = 2f;

        [field: Header("Recovery")]
        [field: SerializeField, Min(0.2f)] public float MinRagdollTime { get; private set; } = 1.5f;
        [field: Tooltip("Get up once the body has been this still (pelvis speed m/s)...")]
        [field: SerializeField, Min(0f)] public float SettledSpeed { get; private set; } = 0.4f;
        [field: Tooltip("...or after this long regardless.")]
        [field: SerializeField, Min(0.5f)] public float MaxRagdollTime { get; private set; } = 6f;
        [field: Tooltip("Below this pelvis speed (m/s) a ragdolled body counts as lying on the floor (structural load).")]
        [field: SerializeField, Min(0f)] public float RestingSpeed { get; private set; } = 1.5f;
        [field: SerializeField, Min(10f)] public float TotalMass { get; private set; } = 80f;

        public void Validate(List<string> errors)
        {
            if (MaxRagdollTime < MinRagdollTime) errors.Add($"{name}: MaxRagdollTime is below MinRagdollTime.");
        }
    }
}

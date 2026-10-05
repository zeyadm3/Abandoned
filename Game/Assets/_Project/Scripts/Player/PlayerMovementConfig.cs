using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// All first-person movement tunables. Defaults aim for snappy, not floaty: high acceleration,
    /// stronger-than-real gravity, and a little coyote time and jump buffering.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Player/Movement Config", fileName = "PlayerMovementConfig")]
    public class PlayerMovementConfig : ScriptableObject, IValidatable
    {
        [field: Header("Speeds (m/s)")]
        [field: SerializeField, Min(0f)] public float WalkSpeed { get; private set; } = 4.5f;
        [field: SerializeField, Min(0f)] public float SprintSpeed { get; private set; } = 7f;
        [field: SerializeField, Min(0f)] public float CrouchSpeed { get; private set; } = 2.2f;

        [field: Header("Acceleration (m/s²)")]
        [field: SerializeField, Min(0f)] public float GroundAcceleration { get; private set; } = 60f;
        [field: SerializeField, Min(0f)] public float GroundDeceleration { get; private set; } = 80f;
        [field: SerializeField, Range(0f, 1f)] public float AirControl { get; private set; } = 0.3f;

        [field: Header("Jump and gravity")]
        [field: SerializeField, Min(0f)] public float JumpHeight { get; private set; } = 1.1f;
        [field: SerializeField, Min(0f)] public float Gravity { get; private set; } = 25f;
        [field: SerializeField, Min(1f)] public float FallGravityMultiplier { get; private set; } = 1.6f;
        [field: SerializeField, Min(0f)] public float MaxFallSpeed { get; private set; } = 40f;
        [field: Tooltip("Downward speed while grounded, so the controller hugs stairs and ramps instead of skipping off them.")]
        [field: SerializeField, Min(0f)] public float GroundStickSpeed { get; private set; } = 8f;
        [field: SerializeField, Min(0f)] public float CoyoteTime { get; private set; } = 0.1f;
        [field: SerializeField, Min(0f)] public float JumpBufferTime { get; private set; } = 0.1f;

        [field: Header("Stamina")]
        [field: SerializeField, Min(1f)] public float MaxStamina { get; private set; } = 100f;
        [field: SerializeField, Min(0f)] public float SprintDrainPerSecond { get; private set; } = 20f;
        [field: SerializeField, Min(0f)] public float JumpStaminaCost { get; private set; } = 10f;
        [field: SerializeField, Min(0f)] public float RegenPerSecond { get; private set; } = 15f;
        [field: SerializeField, Min(0f)] public float RegenDelay { get; private set; } = 1f;
        [field: Tooltip("After running dry, sprint is locked until stamina refills to this, so it doesn't flicker on and off.")]
        [field: SerializeField, Min(0f)] public float SprintResumeThreshold { get; private set; } = 20f;

        [field: Header("Crouch")]
        [field: SerializeField, Min(0.5f)] public float StandingHeight { get; private set; } = 1.8f;
        [field: SerializeField, Min(0.5f)] public float CrouchHeight { get; private set; } = 1.1f;
        [field: SerializeField, Min(0f)] public float StandingEyeHeight { get; private set; } = 1.65f;
        [field: SerializeField, Min(0f)] public float CrouchEyeHeight { get; private set; } = 0.95f;
        [field: Tooltip("Height change speed in m/s.")]
        [field: SerializeField, Min(0.1f)] public float CrouchTransitionSpeed { get; private set; } = 10f;
        [field: SerializeField] public bool CrouchIsToggle { get; private set; }

        [field: Header("Look")]
        [field: Tooltip("Degrees per mouse count. Moves to the settings menu later.")]
        [field: SerializeField, Min(0f)] public float MouseSensitivity { get; private set; } = 0.1f;
        [field: SerializeField, Range(0f, 89f)] public float MaxPitch { get; private set; } = 85f;

        [field: Header("Noise (what threats hear; independent of audio settings)")]
        [field: SerializeField, Range(0f, 1f)] public float WalkNoise { get; private set; } = 0.3f;
        [field: SerializeField, Range(0f, 1f)] public float SprintNoise { get; private set; } = 0.55f;
        [field: SerializeField, Range(0f, 1f)] public float CrouchNoise { get; private set; } = 0.08f;

        [field: Header("Body")]
        [field: Tooltip("Gameplay weight of the player themselves (kg), for structural load.")]
        [field: SerializeField, Min(1f)] public float BodyWeight { get; private set; } = 80f;

        [field: Header("Character controller")]
        [field: SerializeField, Min(0.1f)] public float Radius { get; private set; } = 0.35f;
        [field: SerializeField, Range(0f, 89f)] public float SlopeLimit { get; private set; } = 50f;
        [field: SerializeField, Min(0f)] public float StepOffset { get; private set; } = 0.35f;

        public void Validate(List<string> errors)
        {
            if (SprintSpeed < WalkSpeed) errors.Add($"{name}: SprintSpeed is below WalkSpeed.");
            if (CrouchSpeed > WalkSpeed) errors.Add($"{name}: CrouchSpeed is above WalkSpeed.");
            if (CrouchHeight >= StandingHeight) errors.Add($"{name}: CrouchHeight must be below StandingHeight.");
            if (StandingEyeHeight > StandingHeight) errors.Add($"{name}: StandingEyeHeight is above the head.");
            if (CrouchEyeHeight > CrouchHeight) errors.Add($"{name}: CrouchEyeHeight is above the crouched head.");
            if (Radius * 2f > CrouchHeight) errors.Add($"{name}: Radius is too large for CrouchHeight.");
            if (StepOffset > CrouchHeight - Radius) errors.Add($"{name}: StepOffset too high for the crouched capsule.");
            if (SprintResumeThreshold > MaxStamina) errors.Add($"{name}: SprintResumeThreshold exceeds MaxStamina.");
            if (Gravity <= 0f) errors.Add($"{name}: Gravity must be positive.");
        }
    }
}

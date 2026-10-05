using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>Tunables for picking up, holding, throwing and pocketing objects.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Interaction/Carry Config", fileName = "CarryConfig")]
    public class CarryConfig : ScriptableObject, IValidatable
    {
        [field: Header("Reach")]
        [field: SerializeField, Min(0.5f)] public float Reach { get; private set; } = 2.5f;
        [field: Tooltip("Extra distance the host allows when validating a request, for latency.")]
        [field: SerializeField, Min(0f)] public float ReachTolerance { get; private set; } = 0.75f;

        [field: Header("Hold point (relative to the camera)")]
        [field: SerializeField, Min(0.3f)] public float OneHandHoldDistance { get; private set; } = 1.1f;
        [field: SerializeField, Min(0.3f)] public float TwoHandHoldDistance { get; private set; } = 0.9f;
        [field: SerializeField] public float OneHandHoldDrop { get; private set; } = 0.25f;
        [field: SerializeField] public float TwoHandHoldDrop { get; private set; } = 0.45f;

        [field: Header("Hold spring (acceleration-based, so it ignores Rigidbody mass)")]
        [field: SerializeField, Min(0f)] public float Spring { get; private set; } = 350f;
        [field: SerializeField, Min(0f)] public float Damping { get; private set; } = 32f;
        [field: SerializeField, Min(0f)] public float MaxHoldAcceleration { get; private set; } = 250f;
        [field: SerializeField, Min(0f)] public float RotationSpring { get; private set; } = 18f;
        [field: Tooltip("At MaxSoloWeight the spring is scaled by this, so heavy things lag and wobble.")]
        [field: SerializeField, Range(0.1f, 1f)] public float HeavySpringScale { get; private set; } = 0.45f;
        [field: Tooltip("If the object ends up this far from the hold point (stuck on a door frame), it's dropped.")]
        [field: SerializeField, Min(0.5f)] public float BreakDistance { get; private set; } = 1.75f;

        [field: Header("Weight effects")]
        [field: Tooltip("Heaviest weight one player can hold; used to scale slowdown and wobble.")]
        [field: SerializeField, Min(1f)] public float MaxSoloWeight { get; private set; } = 40f;
        [field: SerializeField, Range(0.1f, 1f)] public float MinSpeedMultiplier { get; private set; } = 0.55f;
        [field: SerializeField, Min(0f)] public float ExtraStaminaDrainAtMaxWeight { get; private set; } = 1.5f;
        [field: SerializeField] public CarryClass HeaviestSoloClass { get; private set; } = CarryClass.TwoHand;

        [field: Header("Throw")]
        [field: SerializeField, Min(0f)] public float ThrowMinSpeed { get; private set; } = 3f;
        [field: SerializeField, Min(0f)] public float ThrowMaxSpeed { get; private set; } = 13f;
        [field: SerializeField, Min(0.05f)] public float ThrowChargeTime { get; private set; } = 0.8f;
        [field: Tooltip("Objects heavier than this are thrown proportionally slower (by sqrt of the ratio).")]
        [field: SerializeField, Min(0.1f)] public float ThrowReferenceWeight { get; private set; } = 3f;

        [field: Header("Inventory")]
        [field: SerializeField, Range(1, 8)] public int PocketSlots { get; private set; } = 4;

        public void Validate(List<string> errors)
        {
            if (ThrowMaxSpeed < ThrowMinSpeed) errors.Add($"{name}: ThrowMaxSpeed is below ThrowMinSpeed.");
            if (BreakDistance <= OneHandHoldDistance * 0.5f) errors.Add($"{name}: BreakDistance is too small.");
            if (HeaviestSoloClass >= CarryClass.Heavy)
                errors.Add($"{name}: HeaviestSoloClass must be below Heavy (Heavy/Huge need 2+ players or tools).");
        }

        /// <summary>0 at no weight, 1 at MaxSoloWeight.</summary>
        public float WeightFraction(float weight) => Mathf.Clamp01(weight / MaxSoloWeight);

        public float SpeedMultiplierFor(float weight) => Mathf.Lerp(1f, MinSpeedMultiplier, WeightFraction(weight));

        public float StaminaDrainMultiplierFor(float weight) => 1f + ExtraStaminaDrainAtMaxWeight * WeightFraction(weight);

        public float ThrowSpeedFor(float charge01, float weight)
        {
            float speed = Mathf.Lerp(ThrowMinSpeed, ThrowMaxSpeed, Mathf.Clamp01(charge01));
            return weight <= ThrowReferenceWeight ? speed : speed * Mathf.Sqrt(ThrowReferenceWeight / weight);
        }
    }
}

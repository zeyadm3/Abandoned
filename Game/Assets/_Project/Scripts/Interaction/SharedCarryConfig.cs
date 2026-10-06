using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// Tunables for carrying Heavy/Huge items together (GDD 15): how many people each class needs, where
    /// the carry points go, how the item follows its carriers, and how carriers are tied to it.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Interaction/Shared Carry Config", fileName = "SharedCarryConfig")]
    public class SharedCarryConfig : ScriptableObject, IValidatable
    {
        [field: Header("Crew (per carry class; a LootDefinition can override RequiredCarriers)")]
        [field: SerializeField, Range(1, SharedCarryable.MaxPoints)] public int RequiredHeavy { get; private set; } = 2;
        [field: Tooltip("GDD: 3-4 people for Huge items.")]
        [field: SerializeField, Range(1, SharedCarryable.MaxPoints)] public int RequiredHuge { get; private set; } = 3;
        [field: SerializeField, Range(1, SharedCarryable.MaxPoints)] public int PointsHeavy { get; private set; } = 2;
        [field: SerializeField, Range(1, SharedCarryable.MaxPoints)] public int PointsHuge { get; private set; } = 4;

        [field: Header("Grip")]
        [field: Tooltip("Carriers hold their point this far away horizontally (clamped at grab), so they don't stand inside the item.")]
        [field: SerializeField, Min(0.1f)] public float MinGripDistance { get; private set; } = 0.45f;
        [field: SerializeField, Min(0.2f)] public float MaxGripDistance { get; private set; } = 1f;
        [field: Tooltip("How far the item's bottom rises off the floor once enough people hold it.")]
        [field: SerializeField, Min(0.05f)] public float LiftClearance { get; private set; } = 0.3f;

        [field: Header("Item motion when lifted (host physics, acceleration-based)")]
        [field: SerializeField, Min(0f)] public float Spring { get; private set; } = 40f;
        [field: Tooltip("Below critical damping on purpose: the load sways a little after a stop.")]
        [field: SerializeField, Min(0f)] public float Damping { get; private set; } = 9f;
        [field: SerializeField, Min(1f)] public float MaxAcceleration { get; private set; } = 30f;
        [field: Tooltip("How strongly disagreeing carriers turn the item (yaw from walking around, tilt from height).")]
        [field: SerializeField, Min(0f)] public float TurnGain { get; private set; } = 3f;
        [field: Tooltip("Pull back to upright so it tilts and sways but doesn't roll over.")]
        [field: SerializeField, Min(0f)] public float UprightGain { get; private set; } = 4f;
        [field: Tooltip("Rate (1/s) the angular velocity approaches what the carriers ask for; lower = more lag and sway.")]
        [field: SerializeField, Min(0.1f)] public float AngularResponse { get; private set; } = 8f;
        [field: Tooltip("Rocking (rad/s per metre of disagreement) when carriers pull in different directions.")]
        [field: SerializeField, Min(0f)] public float WobbleGain { get; private set; } = 2.5f;
        [field: SerializeField, Min(0.1f)] public float WobbleFrequency { get; private set; } = 1.6f;
        [field: Tooltip("The item may outrun the slowest carrier's speed by this factor while springs catch up.")]
        [field: SerializeField, Range(1f, 2f)] public float SpeedSlack { get; private set; } = 1.15f;
        [field: Tooltip("Sprinting with a sofa is not a thing: carriers move at walk speed at most.")]
        [field: SerializeField] public bool AllowSprint { get; private set; }

        [field: Header("Under-crewed (fewer carriers than required): dragged along the floor")]
        [field: SerializeField, Min(0f)] public float DragSpring { get; private set; } = 90f;
        [field: SerializeField, Min(0f)] public float DragDamping { get; private set; } = 20f;
        [field: SerializeField, Min(1f)] public float DragMaxAcceleration { get; private set; } = 60f;
        [field: Tooltip("Items nobody may drag alone (Huge) only creep this fast (m/s) until the crew is complete.")]
        [field: SerializeField, Min(0f)] public float CreepSpeed { get; private set; } = 0.35f;
        [field: Tooltip("An under-crewed item nobody may drag alone moves at most this far (m) from where it was first held: a nudge, not a way to move it (GDD: Huge needs a team or trolley).")]
        [field: SerializeField, Min(0f)] public float NudgeRadius { get; private set; } = 0.75f;
        [field: Tooltip("Found this far (m) beyond the nudge budget when grabbed again (lifted away, fell), it gets a fresh budget there.")]
        [field: SerializeField, Min(0.25f)] public float NudgeResetDistance { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float DragUprightGain { get; private set; } = 12f;
        [field: Tooltip("Like AngularResponse, but stiff: a dragged item is kept upright, not swayed.")]
        [field: SerializeField, Min(0.1f)] public float DragAngularResponse { get; private set; } = 30f;

        [field: Header("Carrier tether (the player follows the item)")]
        [field: Tooltip("Carriers may stray this far (m) from where their grip puts them before walking away is blocked.")]
        [field: SerializeField, Min(0.1f)] public float TetherSlack { get; private set; } = 0.9f;
        [field: Tooltip("Beyond this distance the item pulls its carrier along.")]
        [field: SerializeField, Min(0.1f)] public float TetherPullStart { get; private set; } = 1.4f;
        [field: SerializeField, Min(0f)] public float TetherPullSpeed { get; private set; } = 3f;
        [field: Tooltip("Host lets go for a carrier this far (m) from their grip (stuck, fell, teleported).")]
        [field: SerializeField, Min(0.5f)] public float BreakDistance { get; private set; } = 2.5f;

        [field: Header("Network input (carrier -> host)")]
        [field: Tooltip("A carrier's sent hold target older than this (s) is ignored; the host uses its own view of them.")]
        [field: SerializeField, Min(0.05f)] public float TargetTimeout { get; private set; } = 0.5f;
        [field: Tooltip("Sent targets farther than this (m) from where the host sees the carrier's grip are clamped.")]
        [field: SerializeField, Min(0.1f)] public float MaxTargetDeviation { get; private set; } = 1.5f;

        public int RequiredFor(CarryClass carryClass, int definitionOverride)
        {
            if (definitionOverride > 0) return Mathf.Min(definitionOverride, SharedCarryable.MaxPoints);
            return carryClass == CarryClass.Huge ? RequiredHuge : RequiredHeavy;
        }

        /// <summary>At least one point per required carrier, so a full crew always fits.</summary>
        public int PointCountFor(CarryClass carryClass, int required) =>
            Mathf.Clamp(Mathf.Max(required, carryClass == CarryClass.Huge ? PointsHuge : PointsHeavy), 1, SharedCarryable.MaxPoints);

        public void Validate(List<string> errors)
        {
            if (PointsHeavy < RequiredHeavy) errors.Add($"{name}: PointsHeavy is below RequiredHeavy.");
            if (PointsHuge < RequiredHuge) errors.Add($"{name}: PointsHuge is below RequiredHuge.");
            if (RequiredHuge < RequiredHeavy) errors.Add($"{name}: Huge items should need at least as many carriers as Heavy.");
            if (MaxGripDistance < MinGripDistance) errors.Add($"{name}: MaxGripDistance is below MinGripDistance.");
            if (TetherPullStart < TetherSlack) errors.Add($"{name}: TetherPullStart is below TetherSlack.");
            if (BreakDistance <= TetherPullStart) errors.Add($"{name}: BreakDistance must be beyond TetherPullStart.");
        }
    }
}

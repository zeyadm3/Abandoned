using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// How far the host trusts clients about loot: release velocity and position, and impacts a
    /// carrier reports for the item its machine simulates. One asset in Data/Networking.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Networking/Loot Net Config", fileName = "LootNetConfig")]
    public class LootNetConfig : ScriptableObject, IValidatable
    {
        [field: Header("Releases (drop, throw, take out of pocket)")]
        [field: Tooltip("Velocity a remote player's drop may inherit from their movement (m/s), on top of a full-charge throw.")]
        [field: SerializeField, Min(0f)] public float MaxInheritedSpeed { get; private set; } = 8f;

        [field: Tooltip("A client's reported release position is accepted only this close to the host's view of their eyes (m).")]
        [field: SerializeField, Min(0.5f)] public float MaxReleaseDistance { get; private set; } = 3.5f;

        [field: Tooltip("Ignore repeats of the same request from this machine for this long (s): a snagged item asks every physics step.")]
        [field: SerializeField, Min(0f)] public float RequestRepeatGuard { get; private set; } = 0.4f;

        [field: Header("Carrier-reported impacts")]
        [field: Tooltip("Reported impact speeds are clamped to this (m/s).")]
        [field: SerializeField, Min(1f)] public float MaxReportedImpactSpeed { get; private set; } = 25f;

        [field: Tooltip("A reported contact point must be this close to the host's view of the item (m).")]
        [field: SerializeField, Min(0.1f)] public float MaxImpactPointDistance { get; private set; } = 2.5f;

        [field: Tooltip("At most one report per item per this interval (s); the client also sends no faster.")]
        [field: SerializeField, Min(0f)] public float ImpactReportInterval { get; private set; } = 0.05f;

        [field: Tooltip("Reports from the previous carrier still count this long after a drop (s): they were in flight.")]
        [field: SerializeField, Min(0f)] public float ReportGraceTime { get; private set; } = 0.75f;

        [field: Tooltip("How much of a carried item's hit speed pushes the loot it strikes (before the mass ratio).")]
        [field: SerializeField, Range(0f, 2f)] public float StruckPushTransfer { get; private set; } = 1f;

        [field: Header("Presentation")]
        [field: Tooltip("At most one relayed impact sound per item per this interval (s).")]
        [field: SerializeField, Min(0f)] public float ImpactSoundInterval { get; private set; } = 0.05f;

        public void Validate(List<string> errors)
        {
            if (MaxReleaseDistance < 1f) errors.Add($"{name}: MaxReleaseDistance under 1 m would refuse normal drops.");
            if (ReportGraceTime < ImpactReportInterval) errors.Add($"{name}: ReportGraceTime should be at least ImpactReportInterval.");
        }
    }
}

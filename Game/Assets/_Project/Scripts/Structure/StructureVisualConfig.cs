using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>How each stress stage looks and sounds, and how collapse debris behaves. Presentation only.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Structure/Structure Visual Config", fileName = "StructureVisualConfig")]
    public class StructureVisualConfig : ScriptableObject, IValidatable
    {
        [field: Header("Tint per stage (multiplies the section's colour)")]
        [field: SerializeField] public Color StressedTint { get; private set; } = new(0.95f, 0.92f, 0.86f);
        [field: SerializeField] public Color CrackingTint { get; private set; } = new(0.82f, 0.72f, 0.62f);
        [field: SerializeField] public Color FailingTint { get; private set; } = new(0.8f, 0.5f, 0.42f);

        [field: Header("Cracks")]
        [field: SerializeField, Min(0)] public int CracksWhenCracking { get; private set; } = 5;
        [field: SerializeField, Min(0)] public int CracksWhenFailing { get; private set; } = 10;
        [field: SerializeField] public Color CrackColor { get; private set; } = new(0.08f, 0.07f, 0.06f);
        [field: SerializeField, Min(0.01f)] public float CrackWidth { get; private set; } = 0.05f;
        [field: SerializeField] public Vector2 CrackLength { get; private set; } = new(0.6f, 1.8f);

        [field: Header("Dust (particles per second)")]
        [field: SerializeField, Min(0f)] public float StressedDust { get; private set; } = 3f;
        [field: SerializeField, Min(0f)] public float CrackingDust { get; private set; } = 12f;
        [field: SerializeField, Min(0f)] public float FailingDust { get; private set; } = 40f;
        [field: SerializeField, Min(0)] public int CollapseDustBurst { get; private set; } = 120;
        [field: SerializeField] public Material DustMaterial { get; private set; }

        [field: Header("Failing")]
        [field: SerializeField, Min(0f)] public float SagDepth { get; private set; } = 0.25f;
        [field: SerializeField, Min(0f)] public float FailingJitter { get; private set; } = 0.015f;

        [field: Header("Sound")]
        [field: SerializeField] public Vector2 CreakGapStressed { get; private set; } = new(2.5f, 4.5f);
        [field: SerializeField] public Vector2 CreakGapCracking { get; private set; } = new(1.2f, 2.2f);
        [field: SerializeField] public Vector2 CreakGapFailing { get; private set; } = new(0.2f, 0.45f);
        [field: SerializeField, Range(0f, 1f)] public float CreakVolume { get; private set; } = 0.45f;
        [field: SerializeField, Range(0f, 1f)] public float GroanVolume { get; private set; } = 0.65f;
        [field: SerializeField, Range(0f, 1f)] public float SnapVolume { get; private set; } = 0.9f;

        [field: Header("Collapse debris (cosmetic, local only)")]
        [field: SerializeField, Min(0.1f)] public float ChunkMass { get; private set; } = 8f;
        [field: SerializeField, Min(0f)] public float BurstSpeed { get; private set; } = 1.5f;
        [field: SerializeField, Min(0f)] public float BurstSpin { get; private set; } = 3f;
        [field: Tooltip("Upper limit on debris chunks alive at once; extra collapses spawn fewer chunks.")]
        [field: SerializeField, Min(1)] public int MaxLiveChunks { get; private set; } = 400;
        [field: Tooltip("Settled chunks kept as rubble for the rest of the job (2026-10-08). Past this, newer breaks clear their chunks once they land; the solid mound always stays.")]
        [field: SerializeField, Min(0)] public int MaxRestingChunks { get; private set; } = 2500;

        [field: Header("The break (2026-10-22): sag and release, broken edges, exposed bars")]
        [field: Tooltip("Seconds from the first chunk dropping (the middle) to the last (the edges).")]
        [field: SerializeField, Min(0f)] public float BreakSpread { get; private set; } = 0.22f;
        [field: Tooltip("How far a chunk sags (m) before it lets go.")]
        [field: SerializeField, Min(0f)] public float BreakSag { get; private set; } = 0.08f;
        [field: Tooltip("Chunks this close (m) to an edge still held by floor or wall may stay, hanging, as the broken lip.")]
        [field: SerializeField, Min(0f)] public float RemnantBand { get; private set; } = 0.45f;
        [field: SerializeField, Range(0f, 1f)] public float RemnantChance { get; private set; } = 0.6f;
        [field: Tooltip("How far a broken lip hinges down (degrees, min-max per edge).")]
        [field: SerializeField] public Vector2 RemnantTilt { get; private set; } = new(9f, 24f);
        [field: Tooltip("Seconds the lip and the bars take to drop to their final angle.")]
        [field: SerializeField, Min(0.05f)] public float RemnantDrop { get; private set; } = 0.4f;
        [field: Tooltip("Spacing (m) of the reinforcing bars left sticking out of a held edge.")]
        [field: SerializeField, Min(0.05f)] public float RebarSpacing { get; private set; } = 0.26f;
        [field: Tooltip("Length (m) of a bar's two bent pieces, min-max.")]
        [field: SerializeField] public Vector2 RebarLength { get; private set; } = new(0.2f, 0.65f);
        [field: SerializeField, Min(0.002f)] public float RebarRadius { get; private set; } = 0.009f;
        [field: Tooltip("Rusted steel for the bars; empty uses a plain dark rust.")]
        [field: OptionalReference, SerializeField] public Material RebarMaterial { get; private set; }

        [field: Tooltip("Camera shake momentum emitted by a collapse.")]
        [field: SerializeField, Min(0f)] public float CollapseShake { get; private set; } = 3000f;

        public void Validate(List<string> errors)
        {
            if (DustMaterial == null) errors.Add($"{name}: DustMaterial is not assigned.");
            if (CrackLength.y < CrackLength.x) errors.Add($"{name}: CrackLength max is below min.");
        }
    }
}

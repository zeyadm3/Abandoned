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
        [field: Tooltip("Camera shake momentum emitted by a collapse.")]
        [field: SerializeField, Min(0f)] public float CollapseShake { get; private set; } = 3000f;

        public void Validate(List<string> errors)
        {
            if (DustMaterial == null) errors.Add($"{name}: DustMaterial is not assigned.");
            if (CrackLength.y < CrackLength.x) errors.Add($"{name}: CrackLength max is below min.");
        }
    }
}

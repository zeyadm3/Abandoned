using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>Run and truck tuning (one asset in Data/Extraction). Contracts (M6) will override quota/window.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Extraction Config", fileName = "ExtractionConfig")]
    public class ExtractionConfig : ScriptableObject, IValidatable
    {
        [Tooltip("Haul the run must reach (GDD 10). A contract sets it from M6.")]
        [field: SerializeField, Min(0)] public int Quota { get; private set; } = 40000;
        [Tooltip("Extraction window (s): a soft limit; after it danger rises sharply (GDD 10).")]
        [field: SerializeField, Min(30f)] public float WindowSeconds { get; private set; } = 900f;
        [Tooltip("The truck honks this long (s) before it leaves with whoever is inside.")]
        [field: SerializeField, Range(1f, 30f)] public float HonkSeconds { get; private set; } = 10f;
        [Tooltip("Cargo space (m3 of item bounding boxes). Upgradeable later (GDD 13).")]
        [field: SerializeField, Min(1f)] public float CargoCapacity { get; private set; } = 14f;
        [Tooltip("How close (m) a player must be to the ignition to start the truck (host check).")]
        [field: SerializeField, Range(1f, 6f)] public float IgnitionRange { get; private set; } = 3.5f;
        [Tooltip("Seconds between horn blasts while the truck waits.")]
        [field: SerializeField, Range(0.5f, 5f)] public float HornInterval { get; private set; } = 1.5f;
        [Tooltip("Everyone died: the company tows the truck home and this share of the bay's value survives the trip (QA B-20). No bonus.")]
        [field: SerializeField, Range(0f, 1f)] public float WipeRecovery { get; private set; } = 0.5f;

        public void Validate(List<string> errors)
        {
            if (HonkSeconds <= 0f) errors.Add($"{name}: HonkSeconds must be positive.");
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Contracts
{
    /// <summary>
    /// One salvage destination.  A contract copies its stable id, English display name, and scene name from
    /// this asset, so the host can travel the crew without relying on an inspector reference over the wire.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Contracts/Location Definition", fileName = "Location_New")]
    public sealed class LocationDefinition : ScriptableObject, IValidatable
    {
        [field: SerializeField] public string Id { get; private set; } = "new_location";
        [field: SerializeField] public string DisplayName { get; private set; } = "New Location";
        [field: SerializeField, TextArea] public string Description { get; private set; } = "";
        [Tooltip("The enabled scene name used by SessionTravel. It must match the scene file name, not its path.")]
        [field: SerializeField] public string Scene { get; private set; } = "";
        [Tooltip("The company clearance required before this location can appear on the board.")]
        [field: SerializeField, Min(1)] public int MinCompanyLevel { get; private set; } = 1;
        [Tooltip("Relative chance to appear on the board. The generator avoids repeats while it can.")]
        [field: SerializeField, Min(0)] public int BoardWeight { get; private set; } = 1;

        [Header("Contract character")]
        [Tooltip("Scales the board's estimated haul. The scene's actual loot still decides what can be recovered.")]
        [field: SerializeField, Min(0.1f)] public float LootEstimateMultiplier { get; private set; } = 1f;
        [Tooltip("Scales the quota before crew-size and difficulty adjustments.")]
        [field: SerializeField, Min(0.1f)] public float QuotaMultiplier { get; private set; } = 1f;
        [Tooltip("Added to the rolled structural stability before modifiers.")]
        [field: SerializeField, Range(-0.5f, 0.5f)] public float StabilityDelta { get; private set; }
        [Tooltip("Multiplies the rolled extraction window before modifiers.")]
        [field: SerializeField, Range(0.3f, 2f)] public float WindowMultiplier { get; private set; } = 1f;
        [Tooltip("Added to the normal no-power chance. This never overrides a Power Off modifier.")]
        [field: SerializeField, Range(-1f, 1f)] public float PowerOffOffset { get; private set; }

        public string LocalName => Loc.T(DisplayName);
        public string LocalDescription => Loc.T(Description);

        public void Validate(List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(Id)) errors.Add($"{name}: Id is empty.");
            if (string.IsNullOrWhiteSpace(DisplayName)) errors.Add($"{name}: DisplayName is empty.");
            if (string.IsNullOrWhiteSpace(Scene)) errors.Add($"{name}: Scene is empty.");
            if (MinCompanyLevel < 1) errors.Add($"{name}: MinCompanyLevel must be at least 1.");
            if (BoardWeight < 1) errors.Add($"{name}: BoardWeight must be at least 1.");
            if (LootEstimateMultiplier <= 0f) errors.Add($"{name}: LootEstimateMultiplier must be positive.");
            if (QuotaMultiplier <= 0f) errors.Add($"{name}: QuotaMultiplier must be positive.");
            if (WindowMultiplier <= 0f) errors.Add($"{name}: WindowMultiplier must be positive.");
        }

        // Kept for old test fixtures and a damaged ContractConfig. The released config always has its catalog.
        internal static LocationDefinition LegacyMall()
        {
            var legacy = CreateInstance<LocationDefinition>();
            legacy.hideFlags = HideFlags.HideAndDontSave;
            legacy.Id = ContractGenerator.MallId;
            legacy.DisplayName = ContractGenerator.MallLocation;
            legacy.Scene = ContractGenerator.MallScene;
            legacy.Description = "Three floors of picked-over shops around a collapsing atrium.";
            return legacy;
        }

#if UNITY_EDITOR
        public void EditorSetup(string id, string displayName, string description, string scene, int minCompanyLevel = 1,
            int boardWeight = 1, float lootEstimateMultiplier = 1f, float quotaMultiplier = 1f,
            float stabilityDelta = 0f, float windowMultiplier = 1f, float powerOffOffset = 0f)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Scene = scene;
            MinCompanyLevel = minCompanyLevel;
            BoardWeight = boardWeight;
            LootEstimateMultiplier = lootEstimateMultiplier;
            QuotaMultiplier = quotaMultiplier;
            StabilityDelta = stabilityDelta;
            WindowMultiplier = windowMultiplier;
            PowerOffOffset = powerOffOffset;
        }
#endif
    }
}

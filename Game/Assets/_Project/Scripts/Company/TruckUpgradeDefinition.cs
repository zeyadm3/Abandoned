using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// One tier of a truck upgrade (GDD 13, M10.1), bought once with company money at the HQ shop. A new
    /// tier is a new asset in the catalog; tier N needs tier N-1 of the same kind first.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Company/Truck Upgrade", fileName = "TruckUpgrade_New")]
    public class TruckUpgradeDefinition : ScriptableObject, IValidatable
    {
        [field: SerializeField] public string Id { get; private set; } = "truck_new";
        [field: SerializeField] public string DisplayName { get; private set; } = "New Upgrade";
        [field: SerializeField, TextArea] public string Description { get; private set; } = "";
        [field: SerializeField] public TruckUpgradeKind Kind { get; private set; }
        [Tooltip("1 = the first of its kind; a higher tier needs the one below it.")]
        [field: SerializeField, Range(1, 5)] public int Tier { get; private set; } = 1;
        [field: SerializeField, Min(0)] public int Price { get; private set; } = 10000;
        [Tooltip("Company level that puts it in the shop (GDD 13).")]
        [field: SerializeField, Range(1, 30)] public int UnlockLevel { get; private set; } = 1;
        [Tooltip("Cargo: capacity multiplier. Engine: honk seconds before the truck leaves. Others: unused.")]
        [field: SerializeField, Min(0f)] public float Amount { get; private set; } = 1f;

        public void Validate(List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(Id)) errors.Add($"{name}: Id is empty.");
            if (Kind == TruckUpgradeKind.Cargo && Amount < 1f) errors.Add($"{name}: a cargo upgrade must not shrink the bay.");
            if (Kind == TruckUpgradeKind.Engine && Amount < 1f) errors.Add($"{name}: the engine's honk must be at least 1 s.");
        }

#if UNITY_EDITOR
        public void EditorSetup(string id, string displayName, string description, TruckUpgradeKind kind, int tier, int price, int unlockLevel, float amount)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Kind = kind;
            Tier = tier;
            Price = price;
            UnlockLevel = unlockLevel;
            Amount = amount;
        }
#endif
    }
}

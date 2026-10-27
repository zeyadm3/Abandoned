using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// One truck cosmetic (2026 remake): a paint, a livery, a bumper, a roof fitting, rims, mud flaps, an ornament or a
    /// horn. Bought once with company money at the HQ's truck garage and fitted to the company's truck in every job.
    /// No gameplay effect. Adding one is a new asset in the catalog, no code.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Company/Truck Cosmetic", fileName = "TruckCosmetic_New")]
    public class TruckCosmeticDefinition : ScriptableObject, IValidatable
    {
        [field: SerializeField] public string Id { get; private set; } = "truckcos_new";
        [field: SerializeField] public string DisplayName { get; private set; } = "New Fitting";
        [field: SerializeField, TextArea] public string Description { get; private set; } = "";
        public string LocalName => Loc.T(DisplayName);
        public string LocalDescription => Loc.T(Description);
        [field: SerializeField] public TruckCosmeticSlot Slot { get; private set; }
        [field: Tooltip("What every company starts with in this slot (free, fitted when nothing else is).")]
        [field: SerializeField] public bool Stock { get; private set; }
        [field: SerializeField, Min(0)] public int Price { get; private set; } = 1000;
        [field: SerializeField, Range(1, 30)] public int UnlockLevel { get; private set; } = 1;

        [field: Header("Look")]
        [field: Tooltip("Paint: the body colour.")]
        [field: SerializeField] public Color Color { get; private set; } = Color.white;
        [field: Tooltip("Paint: 0 matte, 1 mirror gloss.")]
        [field: SerializeField, Range(0f, 1f)] public float Smoothness { get; private set; } = .45f;
        [field: Tooltip("Every other slot: the part, modelled in truck space (dropped at the truck root).")]
        [field: SerializeField] public GameObject Part { get; private set; }
        [field: Tooltip("Horn: the sound the truck makes when it's about to leave.")]
        [field: SerializeField] public SoundId Horn { get; private set; } = SoundId.Horn;

        /// <summary>The company save's key: unique across slots.</summary>
        public string Key => "truckcos:" + Id;

        public void Validate(List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(Id)) errors.Add($"{name}: Id is empty.");
            if (Slot != TruckCosmeticSlot.Paint && Slot != TruckCosmeticSlot.Horn && Part == null && !Stock)
                errors.Add($"{name}: a {Slot} needs a part model.");
        }

#if UNITY_EDITOR
        public void EditorSetup(string id, string displayName, string description, TruckCosmeticSlot slot, bool stock, int price,
            int unlockLevel, Color color, float smoothness, GameObject part, SoundId horn)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Slot = slot;
            Stock = stock;
            Price = price;
            UnlockLevel = unlockLevel;
            Color = color;
            Smoothness = smoothness;
            Part = part;
            Horn = horn;
        }
#endif
    }
}

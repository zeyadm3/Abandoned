using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>One piece of gear for the shop and the hand slots (GDD 12). A new item of an existing kind is just a new asset.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Equipment/Equipment Definition", fileName = "Equipment_New")]
    public class EquipmentDefinition : ScriptableObject, IValidatable
    {
        [field: SerializeField] public string Id { get; private set; } = "new_gear";
        [field: SerializeField] public string DisplayName { get; private set; } = "New Gear";
        [field: SerializeField, TextArea] public string Description { get; private set; } = "";
        [field: SerializeField] public EquipmentKind Kind { get; private set; }
        [field: SerializeField, Min(0)] public int Price { get; private set; } = 100;
        [Tooltip("Company level that puts it in the shop (GDD 13).")]
        [field: SerializeField, Range(1, 30)] public int UnlockLevel { get; private set; } = 1;
        [Tooltip("Used up when used (planks, noise maker, medkit).")]
        [field: SerializeField] public bool Consumable { get; private set; }
        [field: SerializeField] public Color Color { get; private set; } = Color.gray;

        public void Validate(List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(Id)) errors.Add($"{name}: Id is empty.");
        }

#if UNITY_EDITOR
        public void EditorSetup(string id, string displayName, string description, EquipmentKind kind, int price, int unlockLevel, bool consumable, Color color)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Kind = kind;
            Price = price;
            UnlockLevel = unlockLevel;
            Consumable = consumable;
            Color = color;
        }
#endif
    }
}

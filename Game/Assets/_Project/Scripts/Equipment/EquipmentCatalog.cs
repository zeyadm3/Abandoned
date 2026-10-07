using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>Every piece of gear the shop sells, in a fixed order (an item's index is how the network names it).</summary>
    [CreateAssetMenu(menuName = "Abandoned/Equipment/Equipment Catalog", fileName = "EquipmentCatalog")]
    public class EquipmentCatalog : ScriptableObject, IValidatable
    {
        [SerializeField] private List<EquipmentDefinition> items = new();

        public IReadOnlyList<EquipmentDefinition> Items => items;

        public EquipmentDefinition At(int index) => index >= 0 && index < items.Count ? items[index] : null;

        public int IndexOf(string id)
        {
            for (int i = 0; i < items.Count; i++) if (items[i] != null && items[i].Id == id) return i;
            return -1;
        }

        public void Validate(List<string> errors)
        {
            if (items.Count == 0) errors.Add($"{name}: empty.");
            var ids = new HashSet<string>();
            foreach (EquipmentDefinition d in items)
                if (d == null) errors.Add($"{name}: an empty slot.");
                else if (!ids.Add(d.Id)) errors.Add($"{name}: duplicate id {d.Id}.");
        }

#if UNITY_EDITOR
        public void EditorSet(List<EquipmentDefinition> list) => items = list;
#endif
    }
}

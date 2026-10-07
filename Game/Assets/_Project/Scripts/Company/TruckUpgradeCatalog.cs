using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// Every truck upgrade, in a fixed order: an upgrade's index is its bit in the replicated mask
    /// (<see cref="CompanyNetState.TruckUpgrades"/>), so append only and keep it under 32.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Company/Truck Upgrade Catalog", fileName = "TruckUpgrades")]
    public class TruckUpgradeCatalog : ScriptableObject, IValidatable
    {
        public const int MaxUpgrades = 32;

        [SerializeField] private List<TruckUpgradeDefinition> items = new();

        public IReadOnlyList<TruckUpgradeDefinition> Items => items;

        public TruckUpgradeDefinition At(int index) => index >= 0 && index < items.Count ? items[index] : null;

        /// <summary>The upgrade one tier below (null for a first tier).</summary>
        public TruckUpgradeDefinition Previous(TruckUpgradeDefinition d)
        {
            if (d == null || d.Tier <= 1) return null;
            foreach (TruckUpgradeDefinition o in items)
                if (o != null && o.Kind == d.Kind && o.Tier == d.Tier - 1) return o;
            return null;
        }

        public int IndexOf(TruckUpgradeDefinition d) => items.IndexOf(d);

        public void Validate(List<string> errors)
        {
            if (items.Count > MaxUpgrades) errors.Add($"{name}: more than {MaxUpgrades} upgrades (they travel as a 32-bit mask).");
            var ids = new HashSet<string>();
            foreach (TruckUpgradeDefinition d in items)
                if (d == null) errors.Add($"{name}: an empty slot.");
                else if (!ids.Add(d.Id)) errors.Add($"{name}: duplicate id {d.Id}.");
                else if (d.Tier > 1 && Previous(d) == null) errors.Add($"{name}: {d.Id} has no tier {d.Tier - 1} below it.");
        }

#if UNITY_EDITOR
        public void EditorSet(List<TruckUpgradeDefinition> list) => items = list;
#endif
    }
}

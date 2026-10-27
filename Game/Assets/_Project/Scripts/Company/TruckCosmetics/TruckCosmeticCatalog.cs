using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// Every truck cosmetic in a fixed order (2026 remake): an entry's index is its bit in the replicated ownership mask
    /// and its byte in the replicated look (<see cref="CompanyNetState"/>), so append only and keep it under 64.
    /// Data/Company/Resources/TruckCosmetics.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Company/Truck Cosmetic Catalog", fileName = "TruckCosmetics")]
    public class TruckCosmeticCatalog : ScriptableObject, IValidatable
    {
        public const int Max = 64;
        public const int Slots = 8;

        [SerializeField] private List<TruckCosmeticDefinition> items = new();

        public IReadOnlyList<TruckCosmeticDefinition> Items => items;

        public TruckCosmeticDefinition At(int index) => index >= 0 && index < items.Count ? items[index] : null;

        public int IndexOf(TruckCosmeticDefinition d) => items.IndexOf(d);

        /// <summary>The free stock fitting of a slot (-1 when the slot is empty by default).</summary>
        public int StockOf(TruckCosmeticSlot slot)
        {
            for (int i = 0; i < items.Count; i++)
                if (items[i] != null && items[i].Slot == slot && items[i].Stock) return i;
            return -1;
        }

        private static TruckCosmeticCatalog loaded;
        private static bool tried;

        public static TruckCosmeticCatalog Load()
        {
            if (!tried)
            {
                tried = true;
                loaded = Resources.Load<TruckCosmeticCatalog>("TruckCosmetics");
            }
            return loaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            loaded = null;
            tried = false;
        }

        public void Validate(List<string> errors)
        {
            if (items.Count > Max) errors.Add($"{name}: more than {Max} truck cosmetics (they travel as a 64-bit mask).");
            var ids = new HashSet<string>();
            foreach (TruckCosmeticDefinition d in items)
                if (d == null) errors.Add($"{name}: an empty slot.");
                else if (!ids.Add(d.Id)) errors.Add($"{name}: duplicate id {d.Id}.");
        }

#if UNITY_EDITOR
        public void EditorSet(List<TruckCosmeticDefinition> list) => items = list;
#endif
    }
}

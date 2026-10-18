using System;
using System.Collections.Generic;
using Abandoned.Equipment;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>Four stable pockets shared by tools and small loot, including the item in hand.</summary>
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private CarryConfig config;

        private readonly List<Grabbable> items = new();
        private readonly Dictionary<Grabbable, int> slots = new();
        private PlayerEquipment equipment;
        private PlayerEquipment Gear => equipment != null ? equipment : equipment = GetComponent<PlayerEquipment>();
        // Which pocket the drop key empties (QA B-16); -1 = the newest one.
        private int selected = -1;

        public IReadOnlyList<Grabbable> Items
        {
            get
            {
                // Items destroyed while pocketed (scene unload, a shatter in M3) must not linger.
                items.RemoveAll(i => i == null);
                return items;
            }
        }
        public int Count => Items.Count;
        /// <summary>Extra pockets from gear (a backpack, M9.4); set on every machine from the replicated slots.</summary>
        public int ExtraSlots { get; set; }
        public int Capacity => EquipState.SlotCount;
        public int OccupiedCount => Count + (Gear != null ? Gear.OccupiedCount : 0);
        public bool HasSpace => FirstFreeSlot >= 0;
        public int FirstFreeSlot
        {
            get
            {
                for (int slot = 0; slot < Capacity; slot++)
                    if (AtSlot(slot) == null && (Gear == null || Gear.State[slot] < 0)) return slot;
                return -1;
            }
        }

        public float TotalWeight
        {
            get
            {
                float total = 0f;
                foreach (Grabbable item in Items) if (item.Holder == null) total += item.Weight;
                return total;
            }
        }

        /// <summary>The pocket the drop key empties: the one picked with the wheel, else the newest.</summary>
        public int SelectedIndex
        {
            get
            {
                return Selected != null ? items.IndexOf(Selected) : -1;
            }
        }

        public int SelectedSlot => Gear != null && Gear.IsSpawned ? Gear.State.Active : selected >= 0 ? selected : Count > 0 ? SlotOf(items[^1]) : 0;
        public Grabbable Selected => AtSlot(SelectedSlot);
        public Grabbable AtSlot(int slot)
        {
            foreach (Grabbable item in Items) if (SlotOf(item) == slot) return item;
            return null;
        }
        public int SlotOf(Grabbable item) => item != null && slots.TryGetValue(item, out int slot) ? slot : -1;
        public bool Contains(Grabbable item) => item != null && items.Contains(item);
        public void Select(int slot)
        {
            selected = Mathf.Clamp(slot, 0, Capacity - 1);
            Changed?.Invoke();
        }

        /// <summary>Move the pocket selection (mouse wheel while the inventory key is held).</summary>
        public void Cycle(int step)
        {
            if (step != 0) Select((SelectedSlot + step % Capacity + Capacity) % Capacity);
        }

        public event Action Changed;

        internal void Add(Grabbable item, int slot = -1)
        {
            if (item == null) return;
            if (Contains(item)) { if (slot >= 0) slots[item] = slot; return; }
            if (slot < 0) slot = FirstFreeSlot;
            if (slot < 0 || slot >= Capacity) return;
            items.Add(item);
            slots[item] = slot;
            Changed?.Invoke();
        }

        internal bool Remove(Grabbable item)
        {
            int index = items.IndexOf(item);
            if (index < 0) return false;
            items.RemoveAt(index);
            slots.Remove(item);
            Changed?.Invoke();
            return true;
        }

        internal Grabbable RemoveLast()
        {
            items.RemoveAll(i => i == null);
            if (items.Count == 0) return null;
            Grabbable item = items[^1];
            Remove(item);
            return item;
        }
    }
}

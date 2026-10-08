using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>Pocket slots for Pocket-class items (GDD 12: 4 pocket slots).</summary>
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private CarryConfig config;

        private readonly List<Grabbable> items = new();
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
        public int Capacity => config.PocketSlots + ExtraSlots;
        public bool HasSpace => items.Count < Capacity;

        public float TotalWeight
        {
            get
            {
                float total = 0f;
                foreach (Grabbable item in Items) total += item.Weight;
                return total;
            }
        }

        /// <summary>The pocket the drop key empties: the one picked with the wheel, else the newest.</summary>
        public int SelectedIndex
        {
            get
            {
                int count = Count;
                return selected >= 0 && selected < count ? selected : count - 1;
            }
        }

        public Grabbable Selected => Count > 0 ? Items[SelectedIndex] : null;

        /// <summary>Move the pocket selection (mouse wheel while the inventory key is held).</summary>
        public void Cycle(int step)
        {
            int count = Count;
            if (count == 0 || step == 0) return;
            selected = ((SelectedIndex + step) % count + count) % count;
            Changed?.Invoke();
        }

        public event Action Changed;

        internal void Add(Grabbable item)
        {
            items.Add(item);
            Changed?.Invoke();
        }

        internal bool Remove(Grabbable item)
        {
            int index = items.IndexOf(item);
            if (index < 0) return false;
            items.RemoveAt(index);
            if (index == selected) selected = -1;
            else if (index < selected) selected--;
            Changed?.Invoke();
            return true;
        }

        internal Grabbable RemoveLast()
        {
            items.RemoveAll(i => i == null);
            if (items.Count == 0) return null;
            Grabbable item = items[^1];
            items.RemoveAt(items.Count - 1);
            Changed?.Invoke();
            return item;
        }
    }
}

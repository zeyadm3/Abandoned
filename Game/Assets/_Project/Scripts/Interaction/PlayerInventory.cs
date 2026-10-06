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
        public int Capacity => config.PocketSlots;
        public bool HasSpace => items.Count < config.PocketSlots;

        public float TotalWeight
        {
            get
            {
                float total = 0f;
                foreach (Grabbable item in Items) total += item.Weight;
                return total;
            }
        }

        public event Action Changed;

        internal void Add(Grabbable item)
        {
            items.Add(item);
            Changed?.Invoke();
        }

        internal bool Remove(Grabbable item)
        {
            if (!items.Remove(item)) return false;
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

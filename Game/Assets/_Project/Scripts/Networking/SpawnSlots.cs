using System.Collections.Generic;

namespace Abandoned.Networking
{
    /// <summary>
    /// Host-side seat allocation: each connected client gets the lowest free slot (0 = host's spawn
    /// point), kept until they leave, so two players never spawn on top of each other.
    /// </summary>
    public sealed class SpawnSlots
    {
        private readonly Dictionary<ulong, int> slotByClient = new();
        private readonly int capacity;

        public SpawnSlots(int capacity) => this.capacity = capacity;

        public int Count => slotByClient.Count;
        public int Capacity => capacity;

        public bool TryAssign(ulong clientId, out int slot)
        {
            if (slotByClient.TryGetValue(clientId, out slot)) return true;
            for (slot = 0; slot < capacity; slot++)
            {
                if (slotByClient.ContainsValue(slot)) continue;
                slotByClient[clientId] = slot;
                return true;
            }
            slot = -1;
            return false;
        }

        public bool TryGetSlot(ulong clientId, out int slot) => slotByClient.TryGetValue(clientId, out slot);

        public void Release(ulong clientId) => slotByClient.Remove(clientId);

        public void Clear() => slotByClient.Clear();
    }
}

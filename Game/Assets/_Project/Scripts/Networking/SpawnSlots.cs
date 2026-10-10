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

        // 0.23.5: a player who drops keeps their seat for when they come back to the HQ.
        private readonly Dictionary<string, int> remembered = new();

        /// <summary>Before releasing a leaving player's seat: remember it under who they are.</summary>
        public void Remember(string identity, ulong clientId)
        {
            if (!string.IsNullOrEmpty(identity) && slotByClient.TryGetValue(clientId, out int slot)) remembered[identity] = slot;
        }

        /// <summary>A returning player gets their old seat back if nobody else has it now.</summary>
        public bool TryReclaim(ulong clientId, string identity, out int slot)
        {
            slot = -1;
            if (string.IsNullOrEmpty(identity) || !remembered.TryGetValue(identity, out int old)) return false;
            foreach (KeyValuePair<ulong, int> kv in slotByClient)
                if (kv.Value == old && kv.Key != clientId) return false;
            slotByClient[clientId] = old;
            remembered.Remove(identity);
            slot = old;
            return true;
        }

        public void Clear()
        {
            slotByClient.Clear();
            remembered.Clear();
        }
    }
}

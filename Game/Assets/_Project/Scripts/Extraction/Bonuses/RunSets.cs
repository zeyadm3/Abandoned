using System.Collections.Generic;
using System.Linq;
using Abandoned.Loot;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Loot sets (0.18.3), on the run state's object: at departure the host pays each set with one of every
    /// member aboard (whole, not shattered). The scan HUD shows the nearest set in the bay (every machine
    /// counts the bay itself: loot is on every machine).
    /// </summary>
    public class RunSets : NetworkBehaviour
    {
        private float nextLook;
        private string shown;

        public override void OnNetworkSpawn()
        {
            RunGoals.Add(Goal);
            if (IsServer) RunState.Settling += Settle;
        }

        public override void OnNetworkDespawn()
        {
            RunGoals.Remove(Goal);
            RunState.Settling -= Settle;
        }

        private static int Have(LootSet set, IEnumerable<LootItem> items)
        {
            var ids = new HashSet<string>(items.Where(i => i != null && i.Definition != null && !i.IsShattered).Select(i => i.Definition.Id));
            return set.Members.Count(m => m != null && ids.Contains(m.Id));
        }

        // The set closest to done in the truck right now (refreshed twice a second).
        private string Goal()
        {
            if (Time.time < nextLook) return shown;
            nextLook = Time.time + 0.5f;
            shown = null;
            TruckCargo truck = TruckCargo.Current;
            if (truck == null) return null;
            IReadOnlyCollection<LootItem> bay = truck.ItemsInside();
            float best = 0f;
            foreach (LootSet set in LootSetCatalog.Current.Sets)
            {
                if (set == null || set.Members.Length == 0) continue;
                int have = Have(set, bay);
                float done = (float)have / set.Members.Length;
                if (have == 0 || done <= best) continue;
                best = done;
                shown = have == set.Members.Length ? $"SET COMPLETE: {set.DisplayName} (+${set.Bonus:N0})" : $"SET: {set.DisplayName} {have}/{set.Members.Length}";
            }
            return shown;
        }

        private void Settle(IReadOnlyCollection<LootItem> aboard, List<(int pay, string line)> lines)
        {
            foreach (LootSet set in LootSetCatalog.Current.Sets)
                if (set != null && set.Members.Length > 0 && Have(set, aboard) == set.Members.Length)
                    lines.Add((set.Bonus, $"SET COMPLETE: {set.DisplayName}. +${set.Bonus:N0}"));
        }
    }
}

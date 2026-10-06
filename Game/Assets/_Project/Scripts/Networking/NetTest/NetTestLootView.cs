using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace Abandoned.Networking
{
    /// <summary>Every spawned loot item one machine (<see cref="observer"/>) sees at one moment.</summary>
    [Serializable]
    public class NetTestLootView
    {
        public ulong observer;
        public List<NetTestLootSnapshot> items = new();

        public static NetTestLootView Take(NetworkManager manager)
        {
            var view = new NetTestLootView { observer = manager.LocalClientId };
            foreach (NetworkLoot loot in SpawnedLoot(manager).OrderBy(l => l.NetworkObjectId))
                view.items.Add(NetTestLootSnapshot.Of(loot));
            return view;
        }

        public static IEnumerable<NetworkLoot> SpawnedLoot(NetworkManager manager)
        {
            if (manager == null || manager.SpawnManager == null) yield break;
            foreach (NetworkObject no in manager.SpawnManager.SpawnedObjectsList)
                if (no != null && no.TryGetComponent(out NetworkLoot loot)) yield return loot;
        }
    }
}

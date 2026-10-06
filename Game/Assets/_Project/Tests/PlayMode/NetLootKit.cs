using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Abandoned.Tests
{
    /// <summary>
    /// Networked loot helpers for <see cref="NetTestHarness"/> sessions. Every in-process "machine" has
    /// its own copy of each item in one shared physics world, where a host copy and a client copy at the
    /// same spot would shove each other apart; <see cref="KeepMachinesApart"/> makes colliders of
    /// different machines ignore each other, as if they really were on different computers
    /// (<see cref="MachineSeparator"/> does it before every physics step that follows a spawn).
    /// </summary>
    public static class NetLootKit
    {
        public const string PrefabFolder = "Assets/_Project/Prefabs/Loot";

        public static NetworkObject Prefab(string lootId)
        {
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/Loot_{lootId}.prefab");
            Assert.IsNotNull(prefab, $"no loot prefab for {lootId}; run RebuildContent");
            return prefab.GetComponent<NetworkObject>();
#else
            return null;
#endif
        }

        /// <summary>Host spawns an item; returns once every machine has its copy (isolated from the others).</summary>
        public static IEnumerator Spawn(NetTestHarness net, string lootId, Vector3 position, System.Action<ulong> spawned)
        {
            NetworkObject item = net.Host.Manager.SpawnManager.InstantiateAndSpawn(Prefab(lootId), position: position, rotation: Quaternion.identity);
            ulong id = item.NetworkObjectId;
            yield return NetTestHarness.WaitFor(() => net.Machines.All(m => CopyOn(m, id) != null), $"item #{id} on every machine");
            KeepMachinesApart(net);
            spawned(id);
        }

        /// <summary>This machine's copy of item <paramref name="id"/>, or null (not spawned there / despawned).</summary>
        public static NetworkLoot CopyOn(NetworkBootstrap machine, ulong id) =>
            machine.Manager.SpawnManager != null && machine.Manager.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject no) && no != null
                ? no.GetComponent<NetworkLoot>()
                : null;

        public static IEnumerable<NetworkLoot> Copies(NetTestHarness net, ulong id) =>
            net.Machines.Select(m => CopyOn(m, id)).Where(c => c != null);

        /// <summary>Colliders on different machines never touch (players and loot alike).</summary>
        public static void KeepMachinesApart(NetTestHarness net)
        {
            var perMachine = net.Machines.Where(m => m != null && m.Manager != null && m.Manager.SpawnManager != null)
                .Select(m => m.Manager.SpawnManager.SpawnedObjectsList
                .Where(no => no != null && no.gameObject != null)
                .SelectMany(no => no.GetComponentsInChildren<Collider>(true))
                .ToList()).ToList();
            for (int a = 0; a < perMachine.Count; a++)
            for (int b = a + 1; b < perMachine.Count; b++)
            foreach (Collider x in perMachine[a])
            foreach (Collider y in perMachine[b])
                Physics.IgnoreCollision(x, y, true);
        }

        /// <summary>Waits until every machine agrees on the item's hold state.</summary>
        public static IEnumerator WaitForHold(NetTestHarness net, ulong id, LootHoldState expected, string what) =>
            NetTestHarness.WaitFor(() => Copies(net, id).Count() == net.Machines.Count
                && Copies(net, id).All(c => c.Hold.Mode == expected.Mode && c.Hold.HolderObjectId == expected.HolderObjectId), what, 5f);

        /// <summary>Lets an item land and stop.</summary>
        public static IEnumerator Settle(float seconds = 1f) => NetTestHarness.WaitSeconds(seconds);
    }
}

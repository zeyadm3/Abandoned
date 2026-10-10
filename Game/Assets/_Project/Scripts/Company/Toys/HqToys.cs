using System.Collections.Generic;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The HQ break room's toys (0.19.6): the host stands a basketball and three darts at their places when
    /// the HQ loads (and puts any that wander off for good back), and remembers who last held each, so the
    /// hoop and the board can say who scored. The toys are zero-value utility loot: carried and thrown like
    /// anything else.
    /// </summary>
    public class HqToys : MonoBehaviour
    {
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private GameObject dartPrefab;
        [SerializeField] private Transform ballSpot;
        [SerializeField] private Transform[] dartSpots;

        private readonly List<NetworkObject> spawned = new();
        private readonly Dictionary<LootItem, ulong> lastHolder = new();
        private float nextCheck;

        public static HqToys Current { get; private set; }

        private void OnEnable() => Current = this;
        private void OnDisable()
        {
            if (Current == this) Current = null;
        }

        /// <summary>Host: the player who last held this toy, or null.</summary>
        public string Thrower(LootItem toy) => toy != null && lastHolder.TryGetValue(toy, out ulong c) ? NetworkPlayer.NameOf(c) : null;

        private void Update()
        {
            NetworkManager net = NetworkManager.Singleton;
            if (net == null || !net.IsServer || !net.IsListening) return;
            foreach (NetworkObject o in spawned)
                if (o != null && o.TryGetComponent(out NetworkLoot loot) && loot.Hold.Mode != LootHoldMode.Free
                    && net.SpawnManager.SpawnedObjects.TryGetValue(loot.Hold.HolderObjectId, out NetworkObject holder))
                    lastHolder[loot.Item] = holder.OwnerClientId;
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + 5f;
            spawned.RemoveAll(o => o == null || !o.IsSpawned);
            int balls = 0, darts = 0;
            foreach (NetworkObject o in spawned) if (o.name.Contains("basketball")) balls++; else darts++;
            if (balls == 0) Spawn(ballPrefab, ballSpot);
            for (int i = darts; dartSpots != null && i < dartSpots.Length; i++) Spawn(dartPrefab, dartSpots[i]);
        }

        private void Spawn(GameObject prefab, Transform at)
        {
            if (prefab == null || at == null) return;
            GameObject instance = Instantiate(prefab, at.position, at.rotation);
            instance.GetComponent<LootItem>().Initialize(spawned.Count + 1);
            var networkObject = instance.GetComponent<NetworkObject>();
            networkObject.Spawn(destroyWithScene: true);
            spawned.Add(networkObject);
        }

#if UNITY_EDITOR
        public void EditorSetup(GameObject ball, GameObject dart, Transform ballAt, Transform[] dartsAt)
        {
            ballPrefab = ball;
            dartPrefab = dart;
            ballSpot = ballAt;
            dartSpots = dartsAt;
        }
#endif
    }
}

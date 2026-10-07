using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A level's run: whenever this machine hosts, it spawns the run's <see cref="RunState"/> (dynamic,
    /// so every host session gets a fresh run) and tells it the run's seed (the loot spawner's).
    /// </summary>
    public class RunDirector : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap bootstrap;
        [SerializeField] private NetworkObject runStatePrefab;
        [SerializeField] private NetworkLootSpawner lootSpawner;

        private NetworkObject spawned;

        public void Setup(NetworkBootstrap networkBootstrap, NetworkObject prefab, NetworkLootSpawner spawner)
        {
            bootstrap = networkBootstrap;
            runStatePrefab = prefab;
            lootSpawner = spawner;
        }

        private void Update()
        {
            NetworkManager manager = bootstrap != null ? bootstrap.Manager : null;
            if (manager == null || !manager.IsServer || !manager.IsListening || manager.ShutdownInProgress) return;
            if (spawned != null && spawned.IsSpawned) return;
            RunState.SeedSource = () => lootSpawner != null ? lootSpawner.Seed : 0;
            spawned = manager.SpawnManager.InstantiateAndSpawn(runStatePrefab);
        }
    }
}

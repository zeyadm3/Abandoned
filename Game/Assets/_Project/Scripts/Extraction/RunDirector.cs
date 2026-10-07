using System;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A level's run: whenever this machine hosts, it spawns the run's <see cref="RunState"/> (dynamic,
    /// so every host session gets a fresh run) and tells it the run's seed (the loot spawner's). The next
    /// run happens in place, everyone still connected: new loot from a new seed, the building restored
    /// and re-rolled, a fresh RunState, everyone back on their spawn point (PLAYBOOK 5.4: under 30 s).
    /// </summary>
    public class RunDirector : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap bootstrap;
        [SerializeField] private NetworkObject runStatePrefab;
        [SerializeField] private NetworkLootSpawner lootSpawner;

        [SerializeField] private StructureSimulation structure;

        private NetworkObject spawned;
        private readonly RunStats stats = new();

        /// <summary>Host: a new run began (seed). Monsters and danger reset on this.</summary>
        public static event Action<int> RunStarted;

        public RunStats Stats => stats;

        public void Setup(NetworkBootstrap networkBootstrap, NetworkObject prefab, NetworkLootSpawner spawner, StructureSimulation simulation)
        {
            bootstrap = networkBootstrap;
            runStatePrefab = prefab;
            lootSpawner = spawner;
            structure = simulation;
        }

        private void OnEnable()
        {
            if (lootSpawner != null) lootSpawner.SpawnedRun += stats.Begin;
            RunState.RunStatsSource = stats.Lines;
        }

        private void OnDisable()
        {
            if (lootSpawner != null) lootSpawner.SpawnedRun -= stats.Begin;
            stats.End();
            if (RunState.RunStatsSource == (Func<RunResults, string[]>)stats.Lines) RunState.RunStatsSource = null;
        }

        /// <summary>Host: start the next run right here.</summary>
        public void StartNextRun()
        {
            NetworkManager manager = bootstrap != null ? bootstrap.Manager : null;
            if (manager == null || !manager.IsServer) return;
            int seed = new System.Random(Environment.TickCount ^ lootSpawner.Seed).Next(1, int.MaxValue);
            if (spawned != null && spawned.IsSpawned) spawned.Despawn(true);
            spawned = null;
            // Building first (collapsed floors come back), then the loot that stands on it.
            if (structure != null) structure.ApplyStability(structure.Stability, seed);
            lootSpawner.Respawn(seed);
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.NetworkManager == manager && bootstrap.Slots.TryGetSlot(p.OwnerClientId, out int slot))
                    p.ServerRespawn(PlayerSpawnPoint.PoseFor(slot));
            Debug.Log($"[Run] Next run, seed {seed}.");
            RunStarted?.Invoke(seed);
        }

        private void Update()
        {
            NetworkManager manager = bootstrap != null ? bootstrap.Manager : null;
            if (manager == null || !manager.IsServer || !manager.IsListening || manager.ShutdownInProgress) return;
            if (spawned != null && spawned.IsSpawned) return;
            RunState.SeedSource = () => lootSpawner != null ? lootSpawner.Seed : 0;
            RunState.RunStatsSource = stats.Lines;
            spawned = manager.SpawnManager.InstantiateAndSpawn(runStatePrefab);
        }

        private void LateUpdate()
        {
            NetworkManager manager = bootstrap != null ? bootstrap.Manager : null;
            if (manager != null && manager.IsServer && manager.IsListening) stats.Tick(manager);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => RunStarted = null;
    }
}

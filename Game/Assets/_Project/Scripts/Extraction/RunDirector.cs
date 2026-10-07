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
        [Tooltip("0 = a random seed for each session's first run (contracts set it from M6).")]
        [SerializeField] private int firstRunSeed;

        private NetworkObject spawned;
        private bool started;
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
            BeginRun(NewSeed(), respawnPlayers: true);
        }

        private int NewSeed() => new System.Random(Environment.TickCount ^ (lootSpawner != null ? lootSpawner.Seed : 0)).Next(1, int.MaxValue);

        // Building first (collapsed floors come back, pre-damage re-rolled), then the loot that stands on it.
        private void BeginRun(int seed, bool respawnPlayers)
        {
            NetworkManager manager = bootstrap.Manager;
            if (spawned != null && spawned.IsSpawned) spawned.Despawn(true);
            spawned = null;
            if (structure != null) structure.ApplyStability(structure.Stability, seed);
            if (lootSpawner != null) lootSpawner.Respawn(seed);
            if (respawnPlayers)
                foreach (NetworkPlayer p in NetworkPlayer.All)
                    if (p != null && p.NetworkManager == manager && bootstrap.Slots.TryGetSlot(p.OwnerClientId, out int slot))
                        p.ServerRespawn(PlayerSpawnPoint.PoseFor(slot));
            started = true;
            Debug.Log($"[Run] Run seed {seed}.");
            RunStarted?.Invoke(seed);
        }

        private void Update()
        {
            NetworkManager manager = bootstrap != null ? bootstrap.Manager : null;
            if (manager == null || !manager.IsServer || !manager.IsListening || manager.ShutdownInProgress) return;
            if (spawned != null && spawned.IsSpawned) return;
            // A session's first run: its own seed, not the one the scene was saved with.
            if (!started) BeginRun(firstRunSeed != 0 ? firstRunSeed : NewSeed(), respawnPlayers: false);
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

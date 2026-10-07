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
        private float stabilityOverride = -1f, lootFill = 1f, fragileRarity = 1f;
        private int extraJackpots;
        private readonly RunStats stats = new();

        /// <summary>
        /// A fixed seed for the session's first run (0 = random): tests, and `-seed N` to replay a run.
        /// </summary>
        public static int ForcedSeed { get; set; }

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
            NetworkManager manager = Session != null ? Session.Manager : null;
            if (manager == null || !manager.IsServer) return;
            BeginRun(NewSeed(), respawnPlayers: true);
        }

        private int NewSeed() => new System.Random(Environment.TickCount ^ (lootSpawner != null ? lootSpawner.Seed : 0)).Next(1, int.MaxValue);

        // Building first (collapsed floors come back, pre-damage re-rolled), then the loot that stands on it.
        private void BeginRun(int seed, bool respawnPlayers)
        {
            NetworkManager manager = Session.Manager;
            if (spawned != null && spawned.IsSpawned) spawned.Despawn(true);
            spawned = null;
            if (structure != null) structure.ApplyStability(stabilityOverride >= 0f ? stabilityOverride : structure.Stability, seed);
            if (lootSpawner != null) lootSpawner.Respawn(seed, lootFill, fragileRarity, extraJackpots);
            if (respawnPlayers)
                foreach (NetworkPlayer p in NetworkPlayer.All)
                    if (p != null && p.NetworkManager == manager && Session.Slots.TryGetSlot(p.OwnerClientId, out int slot))
                        p.ServerRespawn(PlayerSpawnPoint.PoseFor(slot));
            started = true;
            Debug.Log($"[Run] Run seed {seed}.");
            RunStarted?.Invoke(seed);
        }

        private NetworkBootstrap Session => NetworkBootstrap.Resolve(bootstrap);

        private void Update()
        {
            // Not until every machine has this level loaded (session travel).
            if (!SessionTravel.LevelReady) return;
            NetworkManager manager = Session != null ? Session.Manager : null;
            if (manager == null || !manager.IsServer || !manager.IsListening || manager.ShutdownInProgress) return;
            if (spawned != null && spawned.IsSpawned) return;
            // A session's first run: its own seed, not the one the scene was saved with.
            if (!started)
            {
                // On a contract (from the HQ): its seed and terms; otherwise a dev run with the defaults.
                Company.CompanyService company = Company.CompanyService.Current;
                Contracts.Contract contract = company != null ? company.Active : default;
                if (contract.IsValid)
                {
                    Contracts.ContractModifier m = company.ModifierOf(contract);
                    stabilityOverride = contract.Stability;
                    lootFill = m != null ? m.LootMultiplier : 1f;
                    fragileRarity = m != null ? m.FragileRarity : 1f;
                    extraJackpots = m != null ? m.ExtraJackpots : 0;
                    RunState.TermsSource = () => new RunState.RunTerms(contract.Quota, contract.WindowSeconds, contract.PayoutBonus, contract.PowerOff,
                        m != null && m.Night, m != null && m.Storm, m != null ? m.ExtraThreats : 0, m != null ? m.HearingMultiplier : 1f,
                        m != null ? m.DecayMultiplier : 1f);
                }
                else RunState.TermsSource = null;
                BeginRun(ForcedSeed != 0 ? ForcedSeed : contract.IsValid ? contract.Seed : firstRunSeed != 0 ? firstRunSeed : NewSeed(), respawnPlayers: false);
            }
            RunState.SeedSource = () => lootSpawner != null ? lootSpawner.Seed : 0;
            RunState.RunStatsSource = stats.Lines;
            spawned = manager.SpawnManager.InstantiateAndSpawn(runStatePrefab);
        }

        private void LateUpdate()
        {
            NetworkManager manager = Session != null ? Session.Manager : null;
            if (manager != null && manager.IsServer && manager.IsListening) stats.Tick(manager);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            RunStarted = null;
            ForcedSeed = SeedArgument(Environment.GetCommandLineArgs());
        }

        public static int SeedArgument(string[] args)
        {
            for (int i = 0; args != null && i < args.Length - 1; i++)
                if (args[i] == "-seed" && int.TryParse(args[i + 1], out int seed)) return seed;
            return 0;
        }
    }
}

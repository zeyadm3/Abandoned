using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Loot;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Host: fills the level's <see cref="LootSpawnPoint"/>s when the session starts, from the run seed
    /// (<see cref="LootSpawnPlanner"/>), spawning each item as a NetworkObject with its seeded value so
    /// every client receives the same loot. Clients do nothing: NGO replicates the spawns.
    /// </summary>
    public class NetworkLootSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap bootstrap;
        [SerializeField] private LootCatalog catalog;
        [SerializeField] private LootSpawnConfig config;
        [Tooltip("Run seed until the run flow sets one.")]
        [SerializeField] private int seed = 1;
        [Tooltip("Spawn when hosting starts. Off where a RunDirector starts runs (it picks each run's seed).")]
        [SerializeField] private bool spawnOnHostStart = true;

        private readonly List<NetworkLoot> spawned = new();
        private float fillMultiplier = 1f, fragileRarity = 1f;
        private int extraJackpots;
        private bool hooked, done;

        public IReadOnlyList<NetworkLoot> Spawned => spawned;
        public int Seed => seed;
        public int TotalValue => spawned.Where(l => l != null && l.Item.IsInitialized).Sum(l => l.Item.CurrentValue);

        public void Setup(NetworkBootstrap networkBootstrap, LootCatalog lootCatalog, LootSpawnConfig spawnConfig, bool autoSpawn = true)
        {
            bootstrap = networkBootstrap;
            catalog = lootCatalog;
            config = spawnConfig;
            spawnOnHostStart = autoSpawn;
        }

        /// <summary>Before the session starts (the run flow): which run this is.</summary>
        public void SetSeed(int runSeed) => seed = runSeed;

        private NetworkBootstrap Session => NetworkBootstrap.Resolve(bootstrap);

        private void Start()
        {
            if (Session == null || Session.Manager == null) return;
            Session.Manager.OnServerStarted += OnServerStarted;
            hooked = true;
            if (Session.Manager.IsServer) OnServerStarted();
        }

        private void OnDestroy()
        {
            if (hooked && Session != null && Session.Manager != null) Session.Manager.OnServerStarted -= OnServerStarted;
        }

        private void OnServerStarted()
        {
            if (done || !spawnOnHostStart) return;
            done = true;
            SpawnAll();
        }

        public IReadOnlyList<LootSpawnPoint> Points() =>
            FindObjectsByType<LootSpawnPoint>(FindObjectsSortMode.None)
                .Where(p => p.gameObject.scene == gameObject.scene)
                .OrderBy(p => p.name, System.StringComparer.Ordinal).ToList();

        /// <summary>Host, next run: every loot item in the session goes (held, pocketed, broken or not) and a new run's loot comes in.</summary>
        public void Respawn(int runSeed, float fillMultiplier = 1f, float fragileRarity = 1f, int extraJackpots = 0)
        {
            this.fillMultiplier = fillMultiplier;
            this.fragileRarity = fragileRarity;
            this.extraJackpots = extraJackpots;
            NetworkManager manager = Session != null ? Session.Manager : null;
            if (manager == null || !manager.IsServer) return;
            foreach (NetworkObject no in manager.SpawnManager.SpawnedObjectsList.ToList())
                if (no != null && no.GetComponent<NetworkLoot>() != null) no.Despawn(true);
            spawned.Clear();
            seed = runSeed;
            SpawnAll();
        }

        /// <summary>Raised on the host after each run's loot is in (stats tracking hooks every item).</summary>
        public event System.Action<IReadOnlyList<NetworkLoot>> SpawnedRun;

        private void SpawnAll()
        {
            IReadOnlyList<LootSpawnPoint> points = Points();
            List<LootDefinition> definitions = catalog.Entries.Select(e => e.definition).Where(d => d != null).ToList();
            var random = new System.Random(seed ^ 0x5f3759df);
            done = true;
            foreach (LootSpawnPlanner.Placement p in LootSpawnPlanner.Plan(points, definitions, config, seed, fillMultiplier, fragileRarity, extraJackpots))
            {
                GameObject prefab = catalog.PrefabFor(p.Definition);
                if (prefab == null)
                {
                    Debug.LogError($"[Loot] No prefab for {p.Definition.Id} in the catalog (Tools/Abandoned/Generate Loot Prefabs).");
                    continue;
                }
                Transform point = points[p.Point].transform;
                Vector3 at = point.position + Vector3.up * (p.Definition.Size.y / 2f + 0.02f);
                Quaternion yaw = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
                GameObject instance = Instantiate(prefab, at, yaw);
                instance.GetComponent<LootItem>().Initialize(p.ValueSeed);
                instance.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                spawned.Add(instance.GetComponent<NetworkLoot>());
            }
            Debug.Log($"[Loot] Run seed {seed}: {spawned.Count} items on {points.Count} points, ${TotalValue:N0} in the building.");
            SpawnedRun?.Invoke(spawned);
        }

        private void OnGUI()
        {
            if (!DebugView.Visible || !done) return;
            GUI.Label(new Rect(Screen.width - 360f, 10f, 350f, 22f), $"LOOT  seed {seed}  {spawned.Count(l => l != null)} items  ${TotalValue:N0}");
        }
    }
}

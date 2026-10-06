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

        private readonly List<NetworkLoot> spawned = new();
        private bool hooked, done;

        public IReadOnlyList<NetworkLoot> Spawned => spawned;
        public int Seed => seed;
        public int TotalValue => spawned.Where(l => l != null && l.Item.IsInitialized).Sum(l => l.Item.CurrentValue);

        public void Setup(NetworkBootstrap networkBootstrap, LootCatalog lootCatalog, LootSpawnConfig spawnConfig)
        {
            bootstrap = networkBootstrap;
            catalog = lootCatalog;
            config = spawnConfig;
        }

        /// <summary>Before the session starts (the run flow): which run this is.</summary>
        public void SetSeed(int runSeed) => seed = runSeed;

        private void Start()
        {
            if (bootstrap == null || bootstrap.Manager == null) return;
            bootstrap.Manager.OnServerStarted += OnServerStarted;
            hooked = true;
            if (bootstrap.Manager.IsServer) OnServerStarted();
        }

        private void OnDestroy()
        {
            if (hooked && bootstrap != null && bootstrap.Manager != null) bootstrap.Manager.OnServerStarted -= OnServerStarted;
        }

        private void OnServerStarted()
        {
            if (done) return;
            done = true;
            SpawnAll();
        }

        public IReadOnlyList<LootSpawnPoint> Points() =>
            FindObjectsByType<LootSpawnPoint>(FindObjectsSortMode.None)
                .Where(p => p.gameObject.scene == gameObject.scene)
                .OrderBy(p => p.name, System.StringComparer.Ordinal).ToList();

        private void SpawnAll()
        {
            IReadOnlyList<LootSpawnPoint> points = Points();
            List<LootDefinition> definitions = catalog.Entries.Select(e => e.definition).Where(d => d != null).ToList();
            var random = new System.Random(seed ^ 0x5f3759df);
            foreach (LootSpawnPlanner.Placement p in LootSpawnPlanner.Plan(points, definitions, config, seed))
            {
                GameObject prefab = catalog.PrefabFor(p.Definition);
                if (prefab == null) continue;
                Transform point = points[p.Point].transform;
                Vector3 at = point.position + Vector3.up * (p.Definition.Size.y / 2f + 0.02f);
                Quaternion yaw = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
                GameObject instance = Instantiate(prefab, at, yaw);
                instance.GetComponent<LootItem>().Initialize(p.ValueSeed);
                instance.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                spawned.Add(instance.GetComponent<NetworkLoot>());
            }
            Debug.Log($"[Loot] Run seed {seed}: {spawned.Count} items on {points.Count} points, ${TotalValue:N0} in the building.");
        }

        private void OnGUI()
        {
            if (!DebugView.Visible || !done) return;
            GUI.Label(new Rect(Screen.width - 360f, 10f, 350f, 22f), $"LOOT  seed {seed}  {spawned.Count(l => l != null)} items  ${TotalValue:N0}");
        }
    }
}

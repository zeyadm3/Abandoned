using System.Linq;
using Abandoned.Extraction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>Host roster with staggered opening waves, steady replenishment and hidden connected placement.</summary>
    public class ThreatDirector : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap bootstrap;
        [SerializeField] private NetworkObject blindOnePrefab;
        [SerializeField] private NetworkObject[] roster = System.Array.Empty<NetworkObject>();
        [SerializeField] private float[] openingWeights = System.Array.Empty<float>();
        [SerializeField] private ThreatCatalog catalog;
        [Tooltip("No living player may see a spawn point or be closer than this.")]
        [SerializeField, Min(0f)] private float minSpawnDistance = 14f;
        private float spawnAt = -1f, nextArrival, openingRetryAt;
        private int runSeed, spawnedTier, openingRemaining = -1;
        private System.Random arrivals = new(1);
        private bool finalSpawned;
        private ThreatSpawnPoint[] points = System.Array.Empty<ThreatSpawnPoint>();
        public static ThreatDirector Current { get; private set; }
        public int PlacementRetries { get; private set; }
        public void Setup(NetworkBootstrap networkBootstrap, NetworkObject prefab, NetworkObject[] threats = null, float[] weights = null)
        {
            bootstrap = networkBootstrap; blindOnePrefab = prefab;
            roster = threats ?? new[] { prefab }; openingWeights = weights ?? new[] { 1f };
        }
        public void SetupCatalog(ThreatCatalog definitions)
        {
            catalog = definitions;
            if (catalog == null) return;
            roster = catalog.Definitions.Select(d => d != null ? d.Prefab : null).ToArray();
            openingWeights = catalog.Definitions.Select(d => d != null && !d.FinalOnly && d.MinimumDanger == 0 ? d.OpeningWeight : 0f).ToArray();
        }
        private void OnEnable()
        {
            Current = this;
            RunDirector.RunStarted += OnRunStarted;
            if (!TryGetComponent(out ThreatPacing _)) gameObject.AddComponent<ThreatPacing>();
        }
        private static ThreatPacingConfig Pacing => ThreatPacingConfig.Current;
        public float NextArrivalIn => spawnAt != 0f ? -1f : Mathf.Max(0f, nextArrival - Time.time);
        public int Alive => Threat.All.Count(t => t != null && t.IsSpawned && t.NetworkManager == Manager && !(t is LastHunter));
        public int AliveCap => Pacing.AliveCap(NetworkPlayer.All.Count(p => p != null && p.NetworkManager == Manager), Level);
        private static int Level => RunState.Current != null ? RunState.Current.Terms.ThreatLevel : 1;
        private void OnDisable() { if (Current == this) Current = null; RunDirector.RunStarted -= OnRunStarted; }
        private NetworkManager Manager => NetworkBootstrap.Resolve(bootstrap) is NetworkBootstrap b ? b.Manager : null;
        private bool IsHost => Manager != null && Manager.IsServer && Manager.IsListening;
        private void Update()
        {
            if (!IsHost) return;
            RunState run = RunState.Current;
            if (spawnAt < 0f && run != null && run.IsSpawned) Schedule(run.State.Seed);
            if (run == null || run.State.Phase != RunPhase.Running) return;
            int level = run.Terms.ThreatLevel;
            Threat.ContractDamageScale = Pacing.DamageScale(level);
            if (spawnAt > 0f)
            {
                float firstAt = spawnAt + Pacing.OpeningDelayFor(level, run.Terms.FirstJob) - Pacing.OpeningDelay;
                if (Time.time < firstAt || Time.time < openingRetryAt) return;
                if (openingRemaining < 0)
                    openingRemaining = Mathf.Min(AliveCap, Pacing.OpeningCount + run.Terms.ExtraThreats + Pacing.OpeningExtras(level));
                if (openingRemaining > 0 && Alive < AliveCap)
                {
                    Threat opened = Alive == 0 ? Spawn(Opening(runSeed)) : SpawnExtra(true);
                    if (opened == null) { openingRetryAt = Time.time + Pacing.SpawnRetry; return; }
                    openingRemaining--;
                    openingRetryAt = Time.time + Pacing.OpeningStagger;
                    if (openingRemaining > 0) return;
                }
                spawnAt = 0f;
                nextArrival = Time.time + NextGap();
            }
            if (Time.time >= nextArrival)
            {
                // A blocked point is retried soon; it cannot quietly lose a full arrival interval.
                bool full = Alive >= AliveCap;
                Threat arrived = full ? null : SpawnExtra();
                nextArrival = Time.time + (full || arrived != null ? NextGap() : Pacing.SpawnRetry);
            }
            if (spawnedTier < run.State.Danger && Alive < AliveCap)
            {
                // Keep an unlocked wave pending if all fair spots are temporarily blocked.
                if (Time.time < openingRetryAt) return;
                Threat arrived = SpawnExtra();
                openingRetryAt = Time.time + Pacing.SpawnRetry;
                if (arrived != null) spawnedTier++;
            }
            else if (Alive >= AliveCap) spawnedTier = run.State.Danger;
        }
        private float NextGap() => Pacing.NextGap(arrivals) * Pacing.GapScale(Level);
        private void OnRunStarted(int seed)
        {
            if (!IsHost) return;
            foreach (Threat t in Threat.All.ToList())
                if (t != null && t.IsSpawned && t.NetworkManager == Manager) t.NetworkObject.Despawn(true);
            Schedule(seed);
        }
        private void Schedule(int seed)
        {
            runSeed = seed; spawnedTier = 0; finalSpawned = false; openingRemaining = -1;
            PlacementRetries = 0; openingRetryAt = 0f;
            arrivals = new System.Random(seed ^ 0x51ED);
            points = FindObjectsByType<ThreatSpawnPoint>(FindObjectsSortMode.None)
                .Where(p => p.gameObject.scene == gameObject.scene).OrderBy(p => p.name).ToArray();
            spawnAt = Time.time + Pacing.OpeningDelay;
        }
        public int Opening(int seed)
        {
            if (roster.Length == 0) return -1;
            float total = 0f;
            for (int i = 0; i < roster.Length; i++) total += OpeningWeight(i);
            if (total <= 0f) return -1;
            double roll = new System.Random(seed).NextDouble() * total;
            for (int i = 0; i < roster.Length; i++)
            {
                float weight = OpeningWeight(i);
                if (weight <= 0f) continue;
                roll -= weight;
                if (roll <= 0) return i;
            }
            return -1;
        }
        private float OpeningWeight(int index)
        {
            if (roster[index] == null) return 0f;
            ThreatDefinition definition = DefinitionAt(index);
            // Every opener can hurt the crew. One-touch killers arrive only after the opening wave.
            if (definition != null && (definition.LethalContact || definition.Kind == ThreatKind.Collector || definition.FinalOnly)) return 0f;
            return index < openingWeights.Length ? openingWeights[index] : 0f;
        }
        public Threat SpawnExtra() => SpawnExtra(false);
        private Threat SpawnExtra(bool opening)
        {
            if (!IsHost || Alive >= AliveCap) return null;
            int danger = RunState.Current != null ? RunState.Current.State.Danger : 0;
            int[] candidates = Enumerable.Range(0, roster.Length)
                .Where(i => roster[i] != null && Eligible(i, danger) && (!opening || OpeningWeight(i) > 0f)).ToArray();
            if (candidates.Length == 0) return null;
            int[] novel = candidates.Where(i => !Threat.All.Any(t => t != null && t.IsSpawned && t.GetType() == roster[i].GetComponent<Threat>().GetType())).ToArray();
            int[] pool = novel.Length > 0 ? novel : candidates;
            return Spawn(pool[arrivals.Next(pool.Length)]);
        }
        private ThreatDefinition DefinitionAt(int index) => catalog != null && index < catalog.Definitions.Length ? catalog.Definitions[index] : roster[index].GetComponent<Threat>()?.Definition;
        private bool Eligible(int index, int danger)
        {
            ThreatDefinition definition = DefinitionAt(index);
            if (definition == null) return !(roster[index].GetComponent<Threat>() is LastHunter);
            if (definition.FinalOnly || definition.MinimumDanger > danger) return false;
            int count = Threat.All.Count(t => t != null && t.IsSpawned && t.Definition != null && t.Definition.Kind == definition.Kind);
            // Theft and instant-kill creatures supplement the damaging population; they cannot fill it.
            int limit = definition.LethalContact || definition.Kind == ThreatKind.Collector || definition.Kind == ThreatKind.Weight ? 1
                : definition.Kind == ThreatKind.Stalker || definition.Kind == ThreatKind.Thing ? 2 : 3;
            return count < limit;
        }
        public Threat Relocate(Threat threat)
        {
            if (!IsHost || threat == null || !threat.IsSpawned) return null;
            NetworkObject prefab = PrefabOf(threat);
            if (prefab == null || !TryPlacement(prefab, out Vector3 at, out Quaternion facing, out string marker)) return null;
            string name = threat.DisplayName;
            threat.NetworkObject.Despawn(true);
            NetworkObject spawned = Manager.SpawnManager.InstantiateAndSpawn(prefab, position: at, rotation: facing);
            Debug.Log($"[Threat] The {name} returns near the crew at {marker}.");
            return spawned != null ? spawned.GetComponent<Threat>() : null;
        }
        private NetworkObject PrefabOf(Threat threat)
        {
            foreach (NetworkObject prefab in roster)
                if (prefab != null && prefab.GetComponent<Threat>() is Threat t && t.GetType() == threat.GetType()) return prefab;
            return null;
        }
        public LastHunter SpawnFinalHunter()
        {
            if (!IsHost || finalSpawned) return null;
            LastHunter result = SpawnOf<LastHunter>() as LastHunter;
            if (result != null) finalSpawned = true;
            return result;
        }
        public BlindOne Spawn() => Spawn(blindOnePrefab) as BlindOne;
        public Threat Spawn(int index) => index >= 0 && index < roster.Length ? Spawn(roster[index]) : null;
        public Threat SpawnOf<T>() where T : Threat
        {
            foreach (NetworkObject prefab in roster)
                if (prefab != null && prefab.GetComponent<T>() != null) return Spawn(prefab);
            return null;
        }
        private bool TryPlacement(NetworkObject prefab, out Vector3 at, out Quaternion facing, out string marker)
        {
            if (points.Length == 0) points = FindObjectsByType<ThreatSpawnPoint>(FindObjectsSortMode.None)
                .Where(p => p.gameObject.scene == gameObject.scene).OrderBy(p => p.name).ToArray();
            bool found = ThreatSpawnSelector.Choose(points, prefab, Manager, gameObject.scene, minSpawnDistance, arrivals, out at, out facing, out marker);
            if (!found) PlacementRetries++;
            return found;
        }
        private Threat Spawn(NetworkObject prefab)
        {
            if (!IsHost || prefab == null || !TryPlacement(prefab, out Vector3 at, out Quaternion facing, out string marker)) return null;
            NetworkObject spawned = Manager.SpawnManager.InstantiateAndSpawn(prefab, position: at, rotation: facing);
            if (spawned == null) return null;
            Threat threat = spawned.GetComponent<Threat>();
            if (threat is LastHunter final) final.SetHuntBounds(HuntBounds());
            Debug.Log($"[Threat] The {threat.DisplayName} appears at {marker}.");
            return threat;
        }
        private static Bounds HuntBounds()
        {
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            bool first = true;
            foreach (Abandoned.Structure.StructuralSection section in FindObjectsByType<Abandoned.Structure.StructuralSection>(FindObjectsSortMode.None))
            {
                if (first) { bounds = new Bounds(section.transform.position, Vector3.one * 4f); first = false; }
                else bounds.Encapsulate(section.transform.position);
            }
            bounds.Expand(new Vector3(3f, 12f, 3f));
            return bounds;
        }
    }
}

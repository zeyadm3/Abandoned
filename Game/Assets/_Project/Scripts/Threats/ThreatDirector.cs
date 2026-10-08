using System.Linq;
using Abandoned.Extraction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>Seeded host roster with a head start, danger tier arrivals and a separately gated final creature.</summary>
    public class ThreatDirector : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap bootstrap;
        [SerializeField] private NetworkObject blindOnePrefab;
        [SerializeField] private NetworkObject[] roster = System.Array.Empty<NetworkObject>();
        [SerializeField] private float[] openingWeights = System.Array.Empty<float>();
        [SerializeField] private ThreatCatalog catalog;
        [Tooltip("A monster never appears closer than this to a living player, nor where one can see the spot (QA B-13).")]
        [SerializeField, Min(0f)] private float minSpawnDistance = 14f;
        private float spawnAt = -1f;
        private int runSeed, spawnedTier;
        private bool finalSpawned;
        public static ThreatDirector Current { get; private set; }
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
        private void OnEnable() { Current = this; RunDirector.RunStarted += OnRunStarted; }
        private void OnDisable() { if (Current == this) Current = null; RunDirector.RunStarted -= OnRunStarted; }
        private NetworkManager Manager => NetworkBootstrap.Resolve(bootstrap) is NetworkBootstrap b ? b.Manager : null;
        private bool IsHost => Manager != null && Manager.IsServer && Manager.IsListening;
        private void Update()
        {
            if (!IsHost) return;
            RunState run = RunState.Current;
            if (spawnAt < 0f && run != null && run.IsSpawned) Schedule(run.State.Seed);
            if (run == null || run.State.Phase != RunPhase.Running) return;
            if (spawnAt > 0f && Time.time >= spawnAt)
            {
                spawnAt = 0f; Spawn(Opening(runSeed));
                for (int i = 0; i < run.Terms.ExtraThreats; i++) SpawnExtra();
            }
            if (spawnAt != 0f) return;
            while (spawnedTier < run.State.Danger)
            {
                spawnedTier++;
                SpawnExtra();
            }
        }
        private void OnRunStarted(int seed)
        {
            if (!IsHost) return;
            foreach (Threat t in Threat.All.ToList()) if (t != null && t.IsSpawned) t.NetworkObject.Despawn(true);
            Schedule(seed);
        }
        private void Schedule(int seed)
        {
            runSeed = seed; spawnedTier = 0; finalSpawned = false;
            BlindOne listener = blindOnePrefab != null ? blindOnePrefab.GetComponent<BlindOne>() : null;
            spawnAt = Time.time + (listener != null ? Mathf.Max(0.01f, listener.Config.SpawnDelay) : 45f);
        }
        public int Opening(int seed)
        {
            if (roster.Length == 0) return -1;
            float total = openingWeights.Sum();
            if (total <= 0f) return -1;
            double roll = new System.Random(seed).NextDouble() * total;
            for (int i = 0; i < roster.Length; i++)
            {
                float weight = i < openingWeights.Length ? openingWeights[i] : 0f;
                if (weight <= 0f) continue;
                roll -= weight;
                if (roll <= 0) return i;
            }
            return -1;
        }
        public Threat SpawnExtra()
        {
            if (!IsHost || Threat.All.Count(t => !(t is LastHunter)) >= 8) return null;
            int danger = RunState.Current != null ? RunState.Current.State.Danger : 0;
            int[] candidates = Enumerable.Range(0, roster.Length).Where(i => roster[i] != null && Eligible(i, danger)).ToArray();
            if (candidates.Length == 0) return null;
            // Newly unlocked silhouettes arrive first, so danger introduces different rules before duplicates.
            int[] newKinds = candidates.Where(i => !Threat.All.Any(t => t != null && t.GetType() == roster[i].GetComponent<Threat>().GetType())).ToArray();
            int[] pool = newKinds.Length > 0 ? newKinds : candidates;
            return Spawn(pool[new System.Random(runSeed + Threat.All.Count * 7919 + spawnedTier).Next(pool.Length)]);
        }
        private bool Eligible(int index, int danger)
        {
            if (catalog == null || index >= catalog.Definitions.Length) return !(roster[index].GetComponent<Threat>() is LastHunter);
            ThreatDefinition definition = catalog.Definitions[index];
            return definition != null && !definition.FinalOnly && definition.MinimumDanger <= danger;
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
            foreach (NetworkObject prefab in roster) if (prefab != null && prefab.GetComponent<T>() != null) return Spawn(prefab);
            return null;
        }
        private Threat Spawn(NetworkObject prefab)
        {
            if (!IsHost || prefab == null) return null;
            ThreatSpawnPoint[] points = FindObjectsByType<ThreatSpawnPoint>(FindObjectsSortMode.None).OrderBy(p => p.name).ToArray();
            if (points.Length == 0) return null;
            Transform at = ChooseSpawn(points, (int)((uint)(runSeed + Threat.All.Count) % (uint)points.Length));
            NetworkObject spawned = Manager.SpawnManager.InstantiateAndSpawn(prefab, position: at.position, rotation: at.rotation);
            Threat threat = spawned.GetComponent<Threat>();
            if (threat is LastHunter final) final.SetHuntBounds(HuntBounds());
            Debug.Log($"[Threat] The {threat.DisplayName} appears at {at.name}.");
            return threat;
        }
        // From the seeded pick, the first point no living player is near or can see; failing that, the one
        // farthest from everyone. Same seed and same crew positions give the same choice.
        private Transform ChooseSpawn(ThreatSpawnPoint[] points, int start)
        {
            int walls = ~LayerMask.GetMask(Core.GameLayers.Player, Core.GameLayers.Loot, Core.GameLayers.Debris, "Ignore Raycast");
            Transform farthest = points[start].transform;
            float farthestDistance = -1f;
            for (int k = 0; k < points.Length; k++)
            {
                Transform t = points[(start + k) % points.Length].transform;
                float nearest = float.MaxValue;
                bool seen = false;
                foreach (NetworkPlayer p in NetworkPlayer.All)
                {
                    if (p == null || p.IsDead || p.NetworkManager != Manager) continue;
                    Vector3 eye = p.transform.position + Vector3.up * 1.6f;
                    nearest = Mathf.Min(nearest, Vector3.Distance(eye, t.position));
                    if (!Physics.Linecast(eye, t.position + Vector3.up * 1.2f, walls, QueryTriggerInteraction.Ignore)) seen = true;
                }
                if (nearest >= minSpawnDistance && !seen) return t;
                if (nearest > farthestDistance) (farthest, farthestDistance) = (t, nearest);
            }
            return farthest;
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

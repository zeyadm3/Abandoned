using System.Linq;
using Abandoned.Extraction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// Host: the run's threats. Each run opens with one threat picked by the run seed from the roster
    /// (Blind One, Stalker, Collector, by weight; GDD 9: runs start with 0-1 threats), appearing at a
    /// seeded spawn point after a head start; a new run clears the old ones. Danger adds another
    /// (<see cref="SpawnExtra"/>) and sharpens them all.
    /// </summary>
    public class ThreatDirector : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap bootstrap;
        [SerializeField] private NetworkObject blindOnePrefab;
        [Tooltip("Every threat that can appear here, and how likely each is to open a run (0 = never first).")]
        [SerializeField] private NetworkObject[] roster = System.Array.Empty<NetworkObject>();
        [SerializeField] private float[] openingWeights = System.Array.Empty<float>();

        private float spawnAt = -1f;
        private int runSeed;

        public static ThreatDirector Current { get; private set; }

        public void Setup(NetworkBootstrap networkBootstrap, NetworkObject prefab, NetworkObject[] threats = null, float[] weights = null)
        {
            bootstrap = networkBootstrap;
            blindOnePrefab = prefab;
            roster = threats ?? new[] { prefab };
            openingWeights = weights ?? new[] { 1f };
        }

        private void OnEnable()
        {
            Current = this;
            RunDirector.RunStarted += OnRunStarted;
        }

        private void OnDisable()
        {
            if (Current == this) Current = null;
            RunDirector.RunStarted -= OnRunStarted;
        }

        private NetworkManager Manager => NetworkBootstrap.Resolve(bootstrap) is NetworkBootstrap b ? b.Manager : null;
        private bool IsHost => Manager != null && Manager.IsServer && Manager.IsListening;

        private void Update()
        {
            if (!IsHost) return;
            // First run of a session: the run state exists once hosting starts.
            if (spawnAt < 0f && RunState.Current != null && RunState.Current.IsSpawned) Schedule(RunState.Current.State.Seed);
            if (spawnAt > 0f && Time.time >= spawnAt && RunState.Current != null && RunState.Current.State.Phase == RunPhase.Running)
            {
                spawnAt = 0f;
                Spawn(Opening(runSeed));
                // A night job (M9.2) wakes more than one thing.
                for (int i = 0; i < RunState.Current.Terms.ExtraThreats; i++) SpawnExtra();
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
            runSeed = seed;
            spawnAt = Time.time + Mathf.Max(0.01f, blindOnePrefab.GetComponent<BlindOne>().Config.SpawnDelay);
        }

        /// <summary>The roster index that opens a run with this seed (weighted).</summary>
        public int Opening(int seed)
        {
            if (roster.Length == 0) return -1;
            float total = 0f;
            for (int i = 0; i < roster.Length; i++) total += i < openingWeights.Length ? openingWeights[i] : 0f;
            if (total <= 0f) return 0;
            double roll = new System.Random(seed).NextDouble() * total;
            for (int i = 0; i < roster.Length; i++)
            {
                roll -= i < openingWeights.Length ? openingWeights[i] : 0f;
                if (roll <= 0) return i;
            }
            return 0;
        }

        /// <summary>Host, danger: another threat, any kind.</summary>
        public Threat SpawnExtra() => Spawn(new System.Random(runSeed + Threat.All.Count * 7919).Next(roster.Length));

        /// <summary>Host: one more Blind One now (tests, and the old single-threat behaviour).</summary>
        public BlindOne Spawn() => Spawn(blindOnePrefab) as BlindOne;

        /// <summary>Host: the roster's threat at <paramref name="index"/>, now.</summary>
        public Threat Spawn(int index) => index >= 0 && index < roster.Length ? Spawn(roster[index]) : null;

        public Threat SpawnOf<T>() where T : Threat
        {
            foreach (NetworkObject p in roster) if (p != null && p.GetComponent<T>() != null) return Spawn(p);
            return null;
        }

        private Threat Spawn(NetworkObject prefab)
        {
            if (!IsHost || prefab == null) return null;
            ThreatSpawnPoint[] points = FindObjectsByType<ThreatSpawnPoint>(FindObjectsSortMode.None).OrderBy(p => p.name).ToArray();
            if (points.Length == 0) return null;
            Transform at = points[(int)((uint)(runSeed + Threat.All.Count) % (uint)points.Length)].transform;
            NetworkObject spawned = Manager.SpawnManager.InstantiateAndSpawn(prefab, position: at.position, rotation: at.rotation);
            Threat threat = spawned.GetComponent<Threat>();
            Debug.Log($"[Threat] The {threat.DisplayName} appears at {at.name}.");
            return threat;
        }
    }
}

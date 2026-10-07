using System.Linq;
using Abandoned.Extraction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// Host: the run's threats. Each run starts with the Blind One appearing at a seeded spawn point after
    /// a head start (GDD 9: runs start with 0-1 threats); a new run clears the old ones. Danger (5.6) adds
    /// more and makes them sharper through <see cref="Spawn"/> and the threats' scales.
    /// </summary>
    public class ThreatDirector : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap bootstrap;
        [SerializeField] private NetworkObject blindOnePrefab;

        private float spawnAt = -1f;
        private int runSeed;

        public static ThreatDirector Current { get; private set; }

        public void Setup(NetworkBootstrap networkBootstrap, NetworkObject prefab)
        {
            bootstrap = networkBootstrap;
            blindOnePrefab = prefab;
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

        private NetworkManager Manager => bootstrap != null ? bootstrap.Manager : null;
        private bool IsHost => Manager != null && Manager.IsServer && Manager.IsListening;

        private void Update()
        {
            if (!IsHost) return;
            // First run of a session: the run state exists once hosting starts.
            if (spawnAt < 0f && RunState.Current != null && RunState.Current.IsSpawned) Schedule(RunState.Current.State.Seed);
            if (spawnAt > 0f && Time.time >= spawnAt && RunState.Current != null && RunState.Current.State.Phase == RunPhase.Running)
            {
                spawnAt = 0f;
                Spawn();
            }
        }

        private void OnRunStarted(int seed)
        {
            if (!IsHost) return;
            foreach (BlindOne b in BlindOne.All.ToList()) if (b != null && b.IsSpawned) b.NetworkObject.Despawn(true);
            Schedule(seed);
        }

        private void Schedule(int seed)
        {
            runSeed = seed;
            spawnAt = Time.time + Mathf.Max(0.01f, blindOnePrefab.GetComponent<BlindOne>().Config.SpawnDelay);
        }

        /// <summary>Host: one more Blind One now, at the run's next spawn point.</summary>
        public BlindOne Spawn()
        {
            if (!IsHost) return null;
            ThreatSpawnPoint[] points = FindObjectsByType<ThreatSpawnPoint>(FindObjectsSortMode.None).OrderBy(p => p.name).ToArray();
            if (points.Length == 0) return null;
            Transform at = points[(int)((uint)(runSeed + BlindOne.All.Count) % (uint)points.Length)].transform;
            NetworkObject spawned = Manager.SpawnManager.InstantiateAndSpawn(blindOnePrefab, position: at.position, rotation: at.rotation);
            Debug.Log($"[Threat] The Blind One appears at {at.name}.");
            return spawned.GetComponent<BlindOne>();
        }
    }
}

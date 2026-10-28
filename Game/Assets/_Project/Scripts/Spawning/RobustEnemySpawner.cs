using System;
using System.Collections;
using System.Collections.Generic;
using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Abandoned.Spawning
{
    /// <summary>
    /// A defensive monster spawner for scenes or tests that need monsters outside the run's ThreatDirector (which stays
    /// the game's own pacing and placement). Every spawn is snapped onto the NavMesh for the prefab's own agent type and
    /// warped there, so an agent never starts off the mesh; spawned monsters are forced visible and always animated, so a
    /// monster can't stand invisible or frozen in its bind pose. Host-authoritative like every threat: in a network session
    /// only the host spawns, through NGO; a networked prefab is never instantiated without a session (it would stand inert).
    /// F1 shows its state; gizmos mark each spawn point green (on the NavMesh) or red (nothing can spawn there).
    /// </summary>
    public class RobustEnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] monsterPrefabs = Array.Empty<GameObject>();
        [SerializeField] private Transform[] spawnPoints = Array.Empty<Transform>();
        [Tooltip("How far from a spawn point the NavMesh may be and still count (metres).")]
        [SerializeField, Min(0.1f)] private float navMeshSampleRadius = 5f;
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField, Min(0f)] private float initialDelaySeconds = 2f;
        [SerializeField, Min(0.1f)] private float spawnIntervalSeconds = 30f;
        [SerializeField, Min(1)] private int maxConcurrentAlive = 3;
        [Tooltip("0 = no limit.")]
        [SerializeField, Min(0)] private int maxTotalSpawns = 10;
        [Tooltip("Layer a spawned monster's renderers move to when the main camera doesn't draw theirs.")]
        [SerializeField] private string targetVisibleLayerName = "Default";

        private readonly List<GameObject> alive = new();
        private int total;
        private string lastProblem = "";

        public int Alive => alive.Count;

        private void Start()
        {
            if (spawnOnStart) StartCoroutine(Loop());
        }

        private IEnumerator Loop()
        {
            yield return new WaitForSeconds(initialDelaySeconds);
            while (maxTotalSpawns == 0 || total < maxTotalSpawns)
            {
                alive.RemoveAll(m => m == null);
                if (alive.Count < maxConcurrentAlive) SpawnOne();
                yield return new WaitForSeconds(spawnIntervalSeconds);
            }
        }

        /// <summary>Spawns one random monster at a random valid point. Returns it, or null (the reason is logged).</summary>
        public GameObject SpawnOne()
        {
            NetworkManager net = NetworkManager.Singleton;
            bool session = net != null && net.IsListening;
            if (session && !net.IsServer) return null; // the host spawns; clients see its copies

            GameObject prefab = PickPrefab();
            if (prefab == null) return Fail($"{name}: no monster prefab assigned (every entry of monsterPrefabs is empty).");
            bool networked = prefab.TryGetComponent(out NetworkObject netPrefab);
            if (networked && !session) return Fail($"{name}: {prefab.name} is a NetworkObject and no session is running; host first.");
            if (!TryPoint(prefab, out Vector3 at, out Quaternion facing)) return Fail($"{name}: no spawn point has NavMesh within {navMeshSampleRadius} m for {prefab.name}.");

            GameObject monster = null;
            try
            {
                monster = networked ? net.SpawnManager.InstantiateAndSpawn(netPrefab, position: at, rotation: facing).gameObject
                    : Instantiate(prefab, at, facing);
                // Threats switch their agent on in OnNetworkSpawn (host only); warp whichever agent is live now.
                if (monster.TryGetComponent(out NavMeshAgent agent) && agent.enabled && !agent.Warp(at))
                    Debug.LogWarning($"[Spawner] {monster.name} could not warp onto the NavMesh at {at}.", monster);
                MakeVisible(monster);
                alive.Add(monster);
                total++;
                Debug.Log($"[Spawner] {prefab.name} spawned at {at} ({alive.Count}/{maxConcurrentAlive} alive, {total} in all).", monster);
                return monster;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Spawner] {name}: spawning {prefab.name} at {at} failed: {e}", this);
                if (monster != null && !(monster.TryGetComponent(out NetworkObject spawned) && spawned.IsSpawned)) Destroy(monster);
                lastProblem = e.Message;
                return null;
            }
        }

        private GameObject PickPrefab()
        {
            int start = monsterPrefabs.Length > 0 ? Random.Range(0, monsterPrefabs.Length) : 0;
            for (int i = 0; i < monsterPrefabs.Length; i++)
                if (monsterPrefabs[(start + i) % monsterPrefabs.Length] is GameObject p && p != null) return p;
            return null;
        }

        // Tries every point once, from a random start, against the prefab's own agent type (a big monster's mesh differs).
        private bool TryPoint(GameObject prefab, out Vector3 at, out Quaternion facing)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = 0, areaMask = NavMesh.AllAreas };
            if (prefab.TryGetComponent(out NavMeshAgent agent)) filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            int start = spawnPoints.Length > 0 ? Random.Range(0, spawnPoints.Length) : 0;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform point = spawnPoints[(start + i) % spawnPoints.Length];
                if (point == null || !NavMesh.SamplePosition(point.position, out NavMeshHit hit, navMeshSampleRadius, filter)) continue;
                at = hit.position;
                facing = Quaternion.Euler(0f, point.eulerAngles.y, 0f);
                return true;
            }
            at = default;
            facing = Quaternion.identity;
            return false;
        }

        private void MakeVisible(GameObject monster)
        {
            Camera eye = Camera.main;
            int layer = LayerMask.NameToLayer(targetVisibleLayerName);
            if (eye != null && layer >= 0 && (eye.cullingMask & (1 << layer)) != 0)
                foreach (Renderer r in monster.GetComponentsInChildren<Renderer>(true))
                    if ((eye.cullingMask & (1 << r.gameObject.layer)) == 0) r.gameObject.layer = layer; // only what the camera skips
            // Off-screen bounds and animator culling are what leave a skinned monster frozen or popping in on approach.
            foreach (SkinnedMeshRenderer skin in monster.GetComponentsInChildren<SkinnedMeshRenderer>(true)) skin.updateWhenOffscreen = true;
            foreach (Animator animator in monster.GetComponentsInChildren<Animator>(true)) animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        private GameObject Fail(string problem)
        {
            if (problem != lastProblem) Debug.LogWarning("[Spawner] " + problem, this);
            lastProblem = problem;
            return null;
        }

        private void OnDrawGizmos()
        {
            foreach (Transform point in spawnPoints)
            {
                if (point == null) continue;
                bool ok = NavMesh.SamplePosition(point.position, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas);
                Gizmos.color = ok ? Color.green : Color.red;
                Gizmos.DrawWireSphere(point.position, 0.4f);
                if (ok) Gizmos.DrawLine(point.position, hit.position);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private GUIStyle style;

        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.UpperRight };
            GUI.Label(new Rect(Screen.width - 620, 94, 610, 22),
                $"spawner {name}: {alive.Count}/{maxConcurrentAlive} alive, {total}/{(maxTotalSpawns == 0 ? "∞" : maxTotalSpawns.ToString())} spawned" +
                (lastProblem.Length > 0 ? $" ({lastProblem})" : ""), style);
        }
#endif
    }
}

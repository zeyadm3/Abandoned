using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Player;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Networking
{
    /// <summary>
    /// Keeps every machine on the same level without NGO scene management (decision M6.0): the host
    /// clears the level's network objects, names the next level, and every machine loads it itself
    /// (players and this object survive: DontDestroyOnLoad). Clients report when they have it; the
    /// host's level directors only spawn the level's objects (structure sync, run, loot, threats) once
    /// every machine is there (<see cref="LevelReady"/>), then everyone is put on the level's spawns.
    /// </summary>
    public class SessionTravel : NetworkBehaviour
    {
        [Tooltip("Host: a machine that still hasn't loaded the level after this long is dropped, so one stuck player can't hold up the crew.")]
        [SerializeField, Min(10f)] private float loadTimeout = 90f;

        private readonly NetworkVariable<FixedString64Bytes> level = new();
        private readonly NetworkVariable<int> travel = new();
        // Host-written: who the crew is waiting for ("" when nobody), for the travel screen.
        private readonly NetworkVariable<FixedString128Bytes> waitingFor = new();
        private readonly Dictionary<ulong, int> readyFor = new();
        private int loadedTravel = -1;
        private bool placed;
        private float travelStarted, nextWaitingCheck;
        private AsyncOperation loading;

        public static SessionTravel Current { get; private set; }

        public string Level => level.Value.ToString();

        /// <summary>Names of the players everyone is waiting on to finish loading (empty when nobody).</summary>
        public string WaitingFor => waitingFor.Value.ToString();

        /// <summary>This machine's progress loading the level (0-1; 1 when not loading).</summary>
        public float LoadProgress => loading == null || loading.isDone ? 1f : Mathf.Clamp01(loading.progress / 0.9f);

        /// <summary>Host: the level this session began on (the HQ in the real game; a dev level in tests).</summary>
        public string StartLevel { get; private set; }
        public int TravelId => travel.Value;

        /// <summary>
        /// True when the level's network objects may be spawned: no travel system (a level played on its
        /// own, tests), or on the host once every connected machine has loaded the current level.
        /// </summary>
        public static bool LevelReady => Current == null || !Current.IsSpawned || Current.AllReady;

        /// <summary>Every machine: a level finished loading here (the travel id it belongs to).</summary>
        public static event Action<string> Arrived;

        private bool AllReady
        {
            get
            {
                if (!IsServer) return loadedTravel == travel.Value;
                if (loadedTravel != travel.Value) return false;
                foreach (ulong id in NetworkManager.ConnectedClientsIds)
                    if (id != NetworkManager.ServerClientId && (!readyFor.TryGetValue(id, out int t) || t != travel.Value)) return false;
                return true;
            }
        }

        public override void OnNetworkSpawn()
        {
            Current = this;
            DontDestroyOnLoad(gameObject);
            if (IsServer)
            {
                level.Value = SceneManager.GetActiveScene().name;
                StartLevel = Level;
                loadedTravel = travel.Value;
                placed = true; // players were placed by their spawn when they connected
                NetworkManager.OnClientDisconnectCallback += OnClientLeft;
                return;
            }
            travel.OnValueChanged += OnTravelChanged;
            Follow();
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
            travel.OnValueChanged -= OnTravelChanged;
            if (NetworkManager != null) NetworkManager.OnClientDisconnectCallback -= OnClientLeft;
        }

        private void OnClientLeft(ulong id) => readyFor.Remove(id);

        /// <summary>Host: everyone goes to <paramref name="sceneName"/> (a level in the build).</summary>
        public void Travel(string sceneName)
        {
            if (!IsServer || !Application.CanStreamedLevelBeLoaded(sceneName)) return;
            // The level's objects belong to the level; the session's (players, travel, company) live in
            // DontDestroyOnLoad and stay.
            foreach (NetworkObject no in NetworkManager.SpawnManager.SpawnedObjectsList.ToList())
                if (no != null && !no.IsPlayerObject && no.gameObject.scene.name != "DontDestroyOnLoad") no.Despawn(true);
            placed = false;
            travelStarted = Time.unscaledTime;
            travel.Value++;
            level.Value = sceneName;
            Debug.Log($"[Travel] Everyone to {sceneName} (travel {travel.Value}).");
            Load(sceneName, travel.Value);
        }

        private void OnTravelChanged(int previous, int current) => Follow();

        // Client: load whatever level the host is on (also on joining).
        private void Follow()
        {
            if (loadedTravel == travel.Value) return;
            string scene = Level;
            if (SceneManager.GetActiveScene().name == scene && loadedTravel < 0)
            {
                // Joined while already on the host's level (same first scene): nothing to load.
                loadedTravel = travel.Value;
                ReadyRpc(travel.Value);
                Arrived?.Invoke(scene);
                return;
            }
            Load(scene, travel.Value);
        }

        private void Load(string scene, int id)
        {
            void OnLoaded(Scene s, LoadSceneMode mode)
            {
                if (s.name != scene) return;
                SceneManager.sceneLoaded -= OnLoaded;
                loadedTravel = id;
                if (!IsServer) ReadyRpc(id);
                Arrived?.Invoke(scene);
            }
            SceneManager.sceneLoaded += OnLoaded;
            // QA B-05: async, so the travel screen keeps animating and the connection keeps ticking while it loads.
            loading = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            if (loading == null)
            {
                SceneManager.sceneLoaded -= OnLoaded;
                Debug.LogError($"[Travel] Could not start loading {scene}.");
            }
        }

        [Rpc(SendTo.Server)]
        private void ReadyRpc(int id, RpcParams rpcParams = default) => readyFor[rpcParams.Receive.SenderClientId] = id;

        // Host: once everyone is in, put them on this level's spawn points (each keeps their slot).
        private void Update()
        {
            if (!IsServer || placed) return;
            if (!AllReady)
            {
                WatchStragglers();
                return;
            }
            placed = true;
            waitingFor.Value = default;
            NetworkBootstrap session = NetworkBootstrap.Persistent != null ? NetworkBootstrap.Persistent : NetworkBootstrap.Instance;
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.NetworkManager == NetworkManager && session != null && session.Slots.TryGetSlot(p.OwnerClientId, out int slot))
                    p.ServerRespawn(PlayerSpawnPoint.PoseFor(slot));
            Debug.Log($"[Travel] Everyone is in {Level}.");
        }

        // Host, while the crew loads (QA B-04): name who we're waiting for, and drop anyone stuck past the timeout.
        private void WatchStragglers()
        {
            if (Time.unscaledTime < nextWaitingCheck || loadedTravel != travel.Value) return;
            nextWaitingCheck = Time.unscaledTime + 0.5f;
            var late = new List<ulong>();
            foreach (ulong id in NetworkManager.ConnectedClientsIds)
                if (id != NetworkManager.ServerClientId && (!readyFor.TryGetValue(id, out int t) || t != travel.Value)) late.Add(id);
            if (late.Count == 0) return;
            if (Time.unscaledTime - travelStarted > loadTimeout)
            {
                foreach (ulong id in late)
                {
                    Debug.LogWarning($"[Travel] {NetworkPlayer.NameOf(id)} (client {id}) didn't load {Level} in {loadTimeout:0} s; dropping them.");
                    NetworkManager.DisconnectClient(id, SessionMessages.LoadTimedOut);
                    readyFor.Remove(id);
                }
                return;
            }
            string names = string.Join(", ", late.Select(NetworkPlayer.NameOf));
            if (names.Length > 100) names = names.Substring(0, 100) + "...";
            var text = new FixedString128Bytes();
            text.CopyFromTruncated(names); // names may be any script: cut at a character, never mid-byte
            if (!text.Equals(waitingFor.Value)) waitingFor.Value = text;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Current = null;
            Arrived = null;
        }
    }
}

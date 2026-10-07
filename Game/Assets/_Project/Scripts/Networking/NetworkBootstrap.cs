using System;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Player;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Starts and stops network sessions: picks the transport (<see cref="TransportMode"/>), hosts,
    /// joins, disconnects, seats players on spawn points and tells <see cref="GameAuthority"/> who the
    /// host is. Owns its NetworkManager's lifetime, so leaving the scene ends the session.
    /// Never throws for expected failures: they become <see cref="LastError"/> for the menu.
    /// Robustness: clients send their build's compatibility key and the host refuses other builds and
    /// a full game with a reason; a host leaving tells every client why; <see cref="SessionEnded"/>
    /// lets the menu flow (<see cref="ReturnToMenu"/>) take the player back.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public sealed class NetworkBootstrap : MonoBehaviour
    {
        [SerializeField] private NetworkConfig config;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private UnityTransport unityTransport;
        [SerializeField] private FacepunchTransport facepunchTransport;
        [Tooltip("Scene bootstraps: the first one becomes the game's persistent session (it survives level loads); later levels' copies remove themselves. Also applies the auto-host/-connect launch rules.")]
        [SerializeField] private bool sceneSession = true;
        [Tooltip("Spawned when hosting starts: keeps every machine on the same level (6.0 session travel).")]
        [OptionalReference, SerializeField] private NetworkObject sessionTravelPrefab;

        private const string LoopbackAddress = "127.0.0.1";

        private SpawnSlots slots;
        private string compatibilityKey;
        private bool wasInSession, leavingOnPurpose;
        private static bool quitting;

        /// <summary>The first bootstrap alive; it answers <see cref="GameAuthority.IsHost"/>.</summary>
        public static NetworkBootstrap Instance { get; private set; }

        /// <summary>The game's session, kept across level loads (null in tests that build their own machines).</summary>
        public static NetworkBootstrap Persistent { get; private set; }

        /// <summary>A level's components use this: their own scene's bootstrap, or the session that loaded them.</summary>
        public static NetworkBootstrap Resolve(NetworkBootstrap own) => own != null ? own : Persistent != null ? Persistent : Instance;

        /// <summary>Tests: end and remove the persistent session so the next scene starts its own.</summary>
        public static void DestroyPersistent()
        {
            if (Persistent == null) return;
            NetworkBootstrap p = Persistent;
            Persistent = null;
            // Unhook first: this isn't a game ending (no "back to the menu", no notice for the next scene).
            NetworkManager manager = p.networkManager;
            p.networkManager = null;
            p.Unhook(manager);
            if (manager != null) DestroyImmediate(manager.gameObject);
            DestroyImmediate(p.gameObject);
            SessionEndNotice.Clear();
            // Players (and the travel object) outlive scenes on purpose; not past the session.
            foreach (NetworkObject no in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
                if (no != null && no.gameObject.scene.name == "DontDestroyOnLoad") DestroyImmediate(no.gameObject);
        }

        public NetworkConfig Config => config;
        public NetworkManager Manager => networkManager;
        public TransportMode Transport { get; private set; }
        public SpawnSlots Slots => slots;
        public string LastError { get; private set; } = string.Empty;
        public string JoinTarget { get; private set; } = string.Empty;

        /// <summary>What this machine tells the host it is (tests and nettest can pretend to be another build).</summary>
        public string CompatibilityKey
        {
            get => compatibilityKey ?? VersionInfo.CompatibilityKey;
            set => compatibilityKey = value;
        }

        public bool IsRunning => networkManager != null && networkManager.IsListening;

        /// <summary>Port the host actually listens on (differs from the config when started on port 0).</summary>
        public ushort HostPort => networkManager != null && networkManager.IsServer && Transport == TransportMode.UnityTransport
            ? unityTransport.GetLocalEndpoint().Port
            : (ushort)0;

        public event Action StateChanged;

        /// <summary>A session this machine had been in has ended: (left on purpose, why if not).</summary>
        public event Action<bool, string> SessionEnded;

        /// <summary>Host rules apply offline (solo) and on the host; never on a connected client.</summary>
        public static bool IsHostOrOffline(NetworkManager manager) =>
            manager == null || !manager.IsListening || manager.IsServer;

        /// <summary>Wires the references (scene builder and <see cref="NetworkBootstrapFactory"/>).</summary>
        public void Setup(NetworkConfig networkConfig, NetworkManager manager, UnityTransport utp, FacepunchTransport facepunch, bool sceneSession)
        {
            config = networkConfig;
            networkManager = manager;
            unityTransport = utp;
            facepunchTransport = facepunch;
            this.sceneSession = sceneSession;
        }

        private void Awake()
        {
            // A level loaded into a running game: the game's session stays; this scene's copy goes.
            if (sceneSession && Persistent != null && Persistent != this)
            {
                gameObject.SetActive(false);
                // Immediately, so it can't claim NetworkManager.Singleton for even a frame.
                if (networkManager != null) DestroyImmediate(networkManager.gameObject);
                Destroy(gameObject);
                if (Persistent.networkManager != null) Persistent.networkManager.SetSingleton();
                return;
            }
            if (sceneSession && transform.parent == null)
            {
                Persistent = this;
                DontDestroyOnLoad(gameObject);
            }
            if (Instance == null)
            {
                Instance = this;
                GameAuthority.SetHostCheck(() => IsHostOrOffline(networkManager));
            }
            // Behaves like the single-player handler for anything that isn't a spawned network object.
            InteractionService.Handler = new NetworkInteractionHandler();
            if (networkManager == null || config == null)
            {
                LastError = "Network setup is incomplete (no NetworkManager or NetworkConfig).";
                Debug.LogError($"[Net] {LastError}", this);
                return;
            }
            if (sceneSession) ReplaceStaleManager();

            slots = new SpawnSlots(config.MaxPlayers);
            ApplyConfig();
            SelectTransport(config.DefaultTransport);
            networkManager.ConnectionApprovalCallback = OnApproval;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnServerStopped += OnStopped;
            networkManager.OnClientStopped += OnStopped;
            networkManager.OnTransportFailure += OnTransportFailure;
            networkManager.OnServerStarted += OnServerStarted;
        }

        // Every session gets the object that keeps everyone on the same level.
        private void OnServerStarted()
        {
            if (sessionTravelPrefab != null) networkManager.SpawnManager.InstantiateAndSpawn(sessionTravelPrefab);
        }

        private void Start()
        {
            if (!sceneSession || networkManager == null || config == null) return;
            var args = NetworkLaunchArgs.Parse(Environment.GetCommandLineArgs());
            if (args.Transport.HasValue) SelectTransport(args.Transport.Value);

            // Back at the menu after a game: wait for the player instead of rejoining or hosting on our own.
            if (SessionEndNotice.ReturnedFromSession) return;
            if (!string.IsNullOrEmpty(args.ConnectAddress))
                StartClient(args.ConnectAddress, args.ConnectPort);
            else if (AutoHostPolicy.ShouldAutoHost(config.AutoHostInEditor, Application.isEditor, IsMainEditor(), args))
                // Automated runs host on a free loopback port: the user's own editor may be hosting on the real one.
                StartHost(Application.isBatchMode ? (ushort)0 : config.Port, Application.isBatchMode ? LoopbackAddress : config.ListenAddress);
        }

        private void ReplaceStaleManager()
        {
            NetworkManager current = NetworkManager.Singleton;
            if (current == networkManager) return;
            // NetworkManagers are DontDestroyOnLoad; one from a scene we left (still being torn down) must not linger.
            if (current != null) DestroyImmediate(current.gameObject);
            networkManager.SetSingleton();
        }

        private void ApplyConfig()
        {
            Unity.Netcode.NetworkConfig ngo = networkManager.NetworkConfig;
            ngo.ConnectionApproval = true;
            // Every instance loads its own scene for now (dev scenes, MPPM); the HQ/run flow decides scene sync in M5.
            ngo.EnableSceneManagement = false;
            ngo.TickRate = (uint)config.TickRate;
            unityTransport.MaxConnectAttempts = config.MaxConnectAttempts;
            unityTransport.ConnectTimeoutMS = config.ConnectTimeoutMs;
        }

        /// <summary>Chooses the transport for the next session. Refused while one is running.</summary>
        public bool SelectTransport(TransportMode mode)
        {
            if (IsRunning) return Fail("Disconnect before switching transport.");
            Transport = mode;
            networkManager.NetworkConfig.NetworkTransport = mode == TransportMode.Steam
                ? facepunchTransport
                : unityTransport;
            StateChanged?.Invoke();
            return true;
        }

        /// <summary>Hosts with the configured port (solo play is hosting with nobody else).</summary>
        public bool StartHost() => StartHost(config.Port, config.ListenAddress);

        public bool StartHost(ushort port, string listenAddress)
        {
            if (!CanStart()) return false;
            if (Transport == TransportMode.Steam)
            {
                if (!EnsureSteam()) return false;
            }
            else
            {
                unityTransport.SetConnectionData(true, LoopbackAddress, port, listenAddress);
            }

            slots.Clear();
            LastError = string.Empty;
            JoinTarget = string.Empty;
            BeginSession();
            if (!networkManager.StartHost())
                return Fail(Transport == TransportMode.UnityTransport
                    ? $"Couldn't host on port {port}. Is another game already using it?"
                    : "Couldn't host over Steam.");
            Debug.Log($"[Net] Hosting over {Transport}{(Transport == TransportMode.UnityTransport ? $" on port {HostPort}" : "")}.");
            StateChanged?.Invoke();
            return true;
        }

        /// <summary>Joins a host: "address[:port]" for Unity Transport, a SteamID64 for Steam.</summary>
        public bool StartClient(string address, ushort port = 0)
        {
            if (!CanStart()) return false;
            if (string.IsNullOrWhiteSpace(address)) return Fail("Enter an address to join.");
            NetworkLaunchArgs.SplitAddress(address, out string host, out ushort parsedPort);
            if (port == 0) port = parsedPort != 0 ? parsedPort : config.Port;

            if (Transport == TransportMode.Steam)
            {
                if (!ulong.TryParse(host, out ulong steamId) || steamId == 0)
                    return Fail("For Steam, enter the host's SteamID64 (invites arrive with the lobby).");
                facepunchTransport.targetSteamId = steamId;
                JoinTarget = $"Steam {steamId}";
                if (!EnsureSteam()) return false;
            }
            else
            {
                unityTransport.SetConnectionData(true, host, port);
                JoinTarget = $"{host}:{port}";
            }

            LastError = string.Empty;
            BeginSession();
            if (!networkManager.StartClient()) return Fail($"Couldn't start joining {JoinTarget}.");
            Debug.Log($"[Net] Joining {JoinTarget} over {Transport}.");
            StateChanged?.Invoke();
            return true;
        }

        public void Disconnect()
        {
            if (networkManager == null || !networkManager.IsListening) return;
            leavingOnPurpose = true;
            TellClientsTheHostLeft();
            networkManager.Shutdown();
            StateChanged?.Invoke();
        }

        private void BeginSession()
        {
            networkManager.NetworkConfig.ConnectionData = ConnectionGate.Payload(CompatibilityKey);
            wasInSession = leavingOnPurpose = false;
            SessionEndNotice.Clear();
        }

        // Without a reason clients can only guess between "host quit" and "network died".
        private void TellClientsTheHostLeft()
        {
            if (networkManager == null || !networkManager.IsListening || !networkManager.IsServer) return;
            foreach (ulong id in networkManager.ConnectedClientsIds.ToArray())
                if (id != NetworkManager.ServerClientId) networkManager.DisconnectClient(id, SessionMessages.HostLeft);
        }

        /// <summary>One line for menus and the F1 view.</summary>
        public string Status
        {
            get
            {
                if (networkManager == null) return "No NetworkManager";
                if (networkManager.ShutdownInProgress) return "Disconnecting...";
                if (!networkManager.IsListening) return "Offline";
                if (networkManager.IsHost)
                    return $"Hosting ({networkManager.ConnectedClientsIds.Count}/{config.MaxPlayers}) over {Transport}" +
                           (Transport == TransportMode.UnityTransport ? $", port {HostPort}" : "");
                return networkManager.IsConnectedClient ? $"Connected to {JoinTarget}" : $"Connecting to {JoinTarget}...";
            }
        }

        private bool CanStart()
        {
            if (networkManager == null || config == null) return Fail("Network setup is incomplete.");
            if (networkManager.IsListening || networkManager.ShutdownInProgress) return Fail("Already in a session; disconnect first.");
            return true;
        }

        private bool EnsureSteam()
        {
            SteamBootstrap steam = SteamBootstrap.Instance != null ? SteamBootstrap.Instance : SteamBootstrap.Create(config);
            if (steam.IsAvailable || steam.TryInitialize()) return true;
            return Fail(string.IsNullOrEmpty(steam.LastError) ? SteamErrorMessages.NotRunning : steam.LastError);
        }

        private void OnApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (!ConnectionGate.Admit(request.ClientNetworkId, request.Payload, CompatibilityKey, slots, config.MaxPlayers,
                    out int slot, out string reason))
            {
                response.Approved = false;
                response.Reason = reason;
                Debug.Log($"[Net] Refused client {request.ClientNetworkId}: {reason}");
                return;
            }
            Pose pose = PlayerSpawnPoint.PoseFor(slot);
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = pose.position;
            response.Rotation = pose.rotation;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (clientId == networkManager.LocalClientId) wasInSession = true;
            StateChanged?.Invoke();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (networkManager.IsServer)
            {
                slots.Release(clientId);
            }
            else if (clientId == networkManager.LocalClientId || clientId == NetworkManager.ServerClientId)
            {
                string reason = SessionMessages.FromHost(networkManager.DisconnectReason);
                LastError = reason.Length > 0 ? reason
                    : wasInSession || networkManager.IsConnectedClient ? SessionMessages.LostHost
                    : SessionMessages.CouldNotJoin(JoinTarget, VersionInfo.Display);
            }
            StateChanged?.Invoke();
        }

        private void OnStopped(bool wasHost)
        {
            slots?.Clear();
            bool ended = wasInSession && !quitting;
            wasInSession = false;
            StateChanged?.Invoke();
            if (ended) SessionEnded?.Invoke(leavingOnPurpose, leavingOnPurpose ? string.Empty : LastError);
        }

        private void OnApplicationQuit() => TellClientsTheHostLeft();

        private void OnTransportFailure()
        {
            LastError = "The network transport failed; the session ended.";
            StateChanged?.Invoke();
        }

        private bool Fail(string message)
        {
            LastError = message;
            Debug.LogWarning($"[Net] {message}");
            StateChanged?.Invoke();
            return false;
        }

        private static bool IsMainEditor()
        {
#if UNITY_EDITOR
            return Unity.Multiplayer.PlayMode.CurrentPlayer.IsMainEditor;
#else
            return true;
#endif
        }

        private void Unhook(NetworkManager manager)
        {
            if (manager == null) return;
            manager.OnClientDisconnectCallback -= OnClientDisconnected;
            manager.OnClientConnectedCallback -= OnClientConnected;
            manager.OnServerStopped -= OnStopped;
            manager.OnClientStopped -= OnStopped;
            manager.OnTransportFailure -= OnTransportFailure;
            manager.OnServerStarted -= OnServerStarted;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                GameAuthority.SetHostCheck(null);
            }
            if (Persistent == this) Persistent = null;
            if (networkManager == null) return;
            networkManager.OnServerStarted -= OnServerStarted;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnServerStopped -= OnStopped;
            networkManager.OnClientStopped -= OnStopped;
            networkManager.OnTransportFailure -= OnTransportFailure;
            // The manager is DontDestroyOnLoad; it goes with the bootstrap that owns it.
            if (!quitting) Destroy(networkManager.gameObject);
        }

        private static void OnQuitting() => quitting = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
            Persistent = null;
            quitting = false;
            Application.quitting -= OnQuitting;
            Application.quitting += OnQuitting;
        }
    }
}

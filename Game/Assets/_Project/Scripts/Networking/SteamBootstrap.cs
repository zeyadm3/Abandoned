using System;
using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Owns Steam's lifetime for the whole game: Init once, RunCallbacks every frame, Shutdown on quit.
    /// The Facepunch transport fork deliberately doesn't touch Steam's lifetime, so lobbies and rich
    /// presence survive the end of a network session. Never throws: failures become
    /// <see cref="LastError"/>, a message the menu can show the player as-is.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class SteamBootstrap : MonoBehaviour
    {
        [SerializeField] private NetworkConfig config;

        private ISteamClient client;
        private bool initialized;

        public static SteamBootstrap Instance { get; private set; }

        /// <summary>True while Steam is initialised and usable (lobbies, relay transport, voice).</summary>
        public bool IsAvailable => initialized && client != null && client.IsValid;

        /// <summary>Why Steam isn't available, readable by players. Empty when it is.</summary>
        public string LastError { get; private set; } = string.Empty;

        public ulong LocalSteamId => IsAvailable ? client.SteamId : 0UL;
        public string LocalPlayerName => IsAvailable ? client.PlayerName : string.Empty;
        public NetworkConfig Config => config;

        /// <summary>Opens a store page in the Steam overlay; false when Steam isn't running here.</summary>
        public bool OpenStorePage(uint appId)
        {
            if (!IsAvailable || appId == 0) return false;
            client.OpenStoreOverlay(appId);
            return true;
        }

        public event Action<bool> AvailabilityChanged;

        /// <summary>Creates the persistent bootstrap object (or returns the existing one).</summary>
        public static SteamBootstrap Create(NetworkConfig networkConfig, ISteamClient steamClient = null)
        {
            if (Instance != null) return Instance;
            var go = new GameObject(nameof(SteamBootstrap));
            var bootstrap = go.AddComponent<SteamBootstrap>();
            bootstrap.config = networkConfig;
            bootstrap.client = steamClient;
            return bootstrap;
        }

        /// <summary>Swaps the Steam API (tests). Only before Steam has been started.</summary>
        public void UseClient(ISteamClient steamClient)
        {
            if (initialized) throw new InvalidOperationException("Steam is already initialised; can't swap the client.");
            client = steamClient;
        }

        /// <summary>
        /// Starts Steam if this session is allowed to. Safe to call again (e.g. a "Retry" button after
        /// the player starts Steam). Returns whether Steam is available afterwards.
        /// </summary>
        public bool TryInitialize() => TryInitialize(Application.isBatchMode, Environment.GetCommandLineArgs());

        public bool TryInitialize(bool isBatchMode, string[] args)
        {
            if (IsAvailable) return true;

            // The test-run guard protects the real Steam account; an injected fake can't touch it, so
            // bootstrap tests can still exercise the Init path from the editor Test Runner.
            bool testGuard = SteamInitPolicy.TestRunActive && (client == null || client is FacepunchSteamClient);
            if (!SteamInitPolicy.Allows(isBatchMode, testGuard, args, out string reason)) return Fail(reason, null);
            if (config == null) return Fail(SteamErrorMessages.ConfigMissing, null);

            try
            {
                client ??= new FacepunchSteamClient();
                client.Init(config.SteamAppId);
            }
            catch (Exception e)
            {
                // Expected whenever Steam is closed; a warning, not an error, and only the message.
                return Fail(SteamErrorMessages.FromException(e), e.Message);
            }

            if (!client.IsValid) return Fail(SteamErrorMessages.NotRunning, "Init returned but the client isn't valid.");

            initialized = true;
            LastError = string.Empty;
            Debug.Log($"[Steam] Initialised (app {config.SteamAppId}) as {client.PlayerName} ({client.SteamId}).");
            // Achievements (M9.5) reach Steam while it runs; unlocks before that stay local only.
            Core.Achievements.Backend = a =>
            {
                if (IsAvailable && !string.IsNullOrEmpty(a.SteamName)) client.SetAchievement(a.SteamName);
            };
            AvailabilityChanged?.Invoke(true);
            return true;
        }

        /// <summary>
        /// Shuts Steam down. Called on quit; public so tests and a future "go offline" can use it.
        /// If a network session is still running (possibly over the Facepunch transport), NGO is shut
        /// down first and Steam only after NGO reports it has stopped: closing Steam under live Steam
        /// sockets makes NGO's disconnect/transport shutdown throw and clients get no clean disconnect.
        /// </summary>
        public void ShutdownSteam()
        {
            if (!initialized) return;
            initialized = false;

            NetworkManager network = NetworkManager.Singleton;
            if (network != null && network.IsListening)
            {
                DeferredSteamShutdown.After(network, client);
                // Deferred NGO shutdown; on quit, NGO's own OnApplicationQuit runs it synchronously
                // (whichever of the two quit callbacks Unity calls first), firing the stop event.
                if (!network.ShutdownInProgress) network.Shutdown();
            }
            else
            {
                ShutdownClient(client);
            }
            AvailabilityChanged?.Invoke(false);
        }

        internal static void ShutdownClient(ISteamClient steamClient)
        {
            try
            {
                steamClient.Shutdown();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Steam] Shutdown failed: {e.Message}");
            }
        }

        private bool Fail(string playerMessage, string detail)
        {
            LastError = playerMessage;
            if (detail != null) Debug.LogWarning($"[Steam] Not available: {playerMessage} ({detail})");
            else Debug.Log($"[Steam] Not available: {playerMessage}");
            return false;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (Application.isPlaying && transform.parent == null) DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (config != null && config.InitSteamOnStart) TryInitialize();
        }

        private void Update()
        {
            if (!initialized) return;
            try
            {
                client.RunCallbacks();
            }
            catch (Exception e)
            {
                // Steam closing under us mid-game: stop pumping instead of throwing every frame.
                Debug.LogWarning($"[Steam] RunCallbacks failed: {e.Message}");
                initialized = false;
                LastError = SteamErrorMessages.LostConnection;
                AvailabilityChanged?.Invoke(false);
            }
        }

        private void OnApplicationQuit() => ShutdownSteam();

        private void OnDestroy()
        {
            ShutdownSteam();
            if (Instance == this) Instance = null;
        }

        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            string text = IsAvailable ? $"Steam: on  {LocalPlayerName} ({LocalSteamId})" : $"Steam: off  {LastError}";
            GUI.Label(new Rect(10f, Screen.height - 30f, 900f, 24f), text);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;
    }
}

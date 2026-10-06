using System;
using Abandoned.Core;
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

            if (!SteamInitPolicy.Allows(isBatchMode, args, out string reason)) return Fail(reason, null);
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
            AvailabilityChanged?.Invoke(true);
            return true;
        }

        /// <summary>Shuts Steam down. Called on quit; public so tests and a future "go offline" can use it.</summary>
        public void ShutdownSteam()
        {
            if (!initialized) return;
            initialized = false;
            try
            {
                client.Shutdown();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Steam] Shutdown failed: {e.Message}");
            }
            AvailabilityChanged?.Invoke(false);
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

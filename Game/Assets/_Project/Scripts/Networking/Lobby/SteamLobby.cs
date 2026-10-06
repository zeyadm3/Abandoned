using System;
using System.Threading.Tasks;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Connects <see cref="SteamLobbyFlow"/> to the scene's <see cref="NetworkBootstrap"/> and to Steam:
    /// starts Steam with the game (invites need it before anyone hosts), attaches the lobby flow once
    /// Steam is up, opens a lobby whenever we host over Steam, leaves it when the session ends, and
    /// follows invites from the overlay or a <c>+connect_lobby</c> launch.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SteamLobby : MonoBehaviour, ILobbySession
    {
        [SerializeField] private NetworkBootstrap bootstrap;

        private SteamLobbyFlow flow;
        private ISteamLobbies lobbies;
        private bool ownsLobbies;
        private ulong pendingLobby;
        private bool hooked;

        /// <summary>Null until Steam is available (or a test injected lobbies).</summary>
        public SteamLobbyFlow Flow => flow;

        public bool IsRunning => bootstrap != null && bootstrap.IsRunning;
        public bool IsHost => IsRunning && bootstrap.Manager.IsHost;
        public bool UsesSteamTransport => bootstrap != null && bootstrap.Transport == TransportMode.Steam;
        public int MaxPlayers => bootstrap != null && bootstrap.Config != null ? bootstrap.Config.MaxPlayers : 1;
        public ulong LocalSteamId => SteamBootstrap.Instance != null ? SteamBootstrap.Instance.LocalSteamId : 0UL;
        public string CompatibilityKey => VersionInfo.CompatibilityKey;

        public bool JoinSteamHost(ulong hostSteamId, out string error)
        {
            error = string.Empty;
            if (bootstrap.SelectTransport(TransportMode.Steam) && bootstrap.StartClient(hostSteamId.ToString())) return true;
            error = bootstrap.LastError;
            return false;
        }

        /// <summary>Wires the bootstrap at runtime (tests); scenes get it from the scene builder.</summary>
        public void Setup(NetworkBootstrap networkBootstrap) => bootstrap = networkBootstrap;

        /// <summary>Tests (and anything without real Steam): drive the flow through other lobbies.</summary>
        public void UseLobbies(ISteamLobbies replacement)
        {
            DetachFlow();
            Attach(replacement, owns: false);
        }

        private void Start()
        {
            if (bootstrap == null || bootstrap.Manager == null) return;
            pendingLobby = NetworkLaunchArgs.Parse(Environment.GetCommandLineArgs()).ConnectLobby;
            bootstrap.Manager.OnServerStarted += OnServerStarted;
            bootstrap.Manager.OnServerStopped += OnStopped;
            bootstrap.Manager.OnClientStopped += OnStopped;
            hooked = true;

            // Never in batch mode or test runs (SteamInitPolicy): those must not touch the real Steam account.
            string[] args = Environment.GetCommandLineArgs();
            if (flow == null && bootstrap.Config != null && bootstrap.Config.InitSteamOnStart &&
                SteamInitPolicy.Allows(Application.isBatchMode, SteamInitPolicy.TestRunActive, args, out _))
                SteamBootstrap.Create(bootstrap.Config).TryInitialize();
        }

        private void Update()
        {
            SteamBootstrap steam = SteamBootstrap.Instance;
            bool steamUp = steam != null && steam.IsAvailable;
            if (ownsLobbies && !steamUp) DetachFlow();
            else if (flow == null && steamUp) Attach(new FacepunchSteamLobbies(), owns: true);

            if (flow != null && pendingLobby != 0UL && !IsRunning)
            {
                ulong id = pendingLobby;
                pendingLobby = 0UL;
                Run(flow.JoinAsync(id));
            }
        }

        private void Attach(ISteamLobbies replacement, bool owns)
        {
            lobbies = replacement;
            ownsLobbies = owns;
            flow = new SteamLobbyFlow(replacement, this);
            // Steam came up after we started hosting over it (e.g. the first Steam host starts Steam).
            if (IsHost) Run(flow.OnHostStarted());
        }

        private void DetachFlow()
        {
            if (flow == null) return;
            try
            {
                flow.Detach();
            }
            catch (Exception e)
            {
                // Steam already gone (quitting, Steam closed): there's no lobby left to leave.
                Debug.LogWarning($"[Steam] Leaving the lobby failed: {e.Message}");
            }
            if (ownsLobbies && lobbies is IDisposable disposable) disposable.Dispose();
            flow = null;
            lobbies = null;
            ownsLobbies = false;
        }

        private void OnServerStarted()
        {
            if (flow != null) Run(flow.OnHostStarted());
        }

        private void OnStopped(bool wasHost) => flow?.OnSessionStopped();

        private static async void Run(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Steam] Lobby operation failed: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            if (hooked && bootstrap != null && bootstrap.Manager != null)
            {
                bootstrap.Manager.OnServerStarted -= OnServerStarted;
                bootstrap.Manager.OnServerStopped -= OnStopped;
                bootstrap.Manager.OnClientStopped -= OnStopped;
            }
            DetachFlow();
        }
    }
}

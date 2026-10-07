using System;
using Abandoned.Networking;

namespace Abandoned.Tests
{
    /// <summary>Stand-in for Steam so bootstrap lifetime tests never touch the real client on this machine.</summary>
    public sealed class FakeSteamClient : ISteamClient
    {
        public Exception InitException;
        public Exception RunCallbacksException;
        public int InitCalls, RunCallbacksCalls, ShutdownCalls;
        public uint InitAppId;
        /// <summary>Runs inside Shutdown, so tests can check what else was still alive at that moment.</summary>
        public Action OnShutdown;
        private bool valid;

        public bool IsValid => valid;
        public ulong SteamId => valid ? 76561190000000001UL : 0UL;
        public string PlayerName => valid ? "Tester" : string.Empty;

        public void Init(uint appId)
        {
            InitCalls++;
            InitAppId = appId;
            if (InitException != null) throw InitException;
            valid = true;
        }

        public void RunCallbacks()
        {
            RunCallbacksCalls++;
            if (RunCallbacksException != null) throw RunCallbacksException;
        }

        public uint StoreOverlayOpened { get; private set; }
        public void OpenStoreOverlay(uint appId) => StoreOverlayOpened = appId;
        public void Shutdown()
        {
            ShutdownCalls++;
            OnShutdown?.Invoke();
            valid = false;
        }
    }
}

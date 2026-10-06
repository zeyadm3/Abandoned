using Abandoned.Networking;

namespace Abandoned.Tests
{
    /// <summary>A pretend network session for <see cref="SteamLobbyFlow"/> tests.</summary>
    public sealed class FakeLobbySession : ILobbySession
    {
        public bool IsRunning { get; set; }
        public bool IsHost { get; set; }
        public bool UsesSteamTransport { get; set; } = true;
        public int MaxPlayers { get; set; } = 4;
        public ulong LocalSteamId { get; set; } = 76561190000000001UL;
        public string CompatibilityKey { get; set; } = "0.3.0+abc1234";
        public ulong JoinedHost;
        public string RefuseJoin;

        public void Host()
        {
            IsRunning = true;
            IsHost = true;
        }

        public void Stop()
        {
            IsRunning = false;
            IsHost = false;
        }

        public bool JoinSteamHost(ulong hostSteamId, out string error)
        {
            error = RefuseJoin ?? string.Empty;
            if (RefuseJoin != null) return false;
            JoinedHost = hostSteamId;
            IsRunning = true;
            return true;
        }
    }
}

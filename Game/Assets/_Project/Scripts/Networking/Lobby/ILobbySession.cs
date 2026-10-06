namespace Abandoned.Networking
{
    /// <summary>What the Steam lobby flow needs from the network session (NetworkBootstrap in the game).</summary>
    public interface ILobbySession
    {
        /// <summary>A session is running or starting (hosting, connected or connecting).</summary>
        bool IsRunning { get; }
        bool IsHost { get; }
        bool UsesSteamTransport { get; }
        int MaxPlayers { get; }
        ulong LocalSteamId { get; }
        string CompatibilityKey { get; }

        /// <summary>Switches to the Steam transport and connects to the host's SteamID64.</summary>
        bool JoinSteamHost(ulong hostSteamId, out string error);
    }
}

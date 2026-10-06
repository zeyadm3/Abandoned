namespace Abandoned.Networking
{
    /// <summary>
    /// Player-readable reasons a session didn't start or ended, shared by the host (approval refusals,
    /// leaving), clients and the Steam lobby so the menu and tests agree on the wording.
    /// </summary>
    public static class SessionMessages
    {
        public const string HostLeft = "The host left the game.";
        public const string LostHost = "Lost connection to the host.";

        public static string Full(int maxPlayers) => $"The game is full ({maxPlayers} players).";

        public static string VersionMismatch(string hostKey, string ourKey) =>
            $"The host is on version {Describe(hostKey)}; you have {Describe(ourKey)}. Both players need the same build.";

        // "0.3.0+abc1234@time" -> "0.3.0 (abc1234)": the build time only matters to the comparison.
        private static string Describe(string key)
        {
            if (string.IsNullOrEmpty(key)) return "unknown";
            int plus = key.LastIndexOf('+');
            if (plus < 0) return key;
            string commit = key.Substring(plus + 1);
            int at = commit.IndexOf('@');
            if (at >= 0) commit = commit.Substring(0, at);
            return $"{key.Substring(0, plus)} ({commit})";
        }
    }
}

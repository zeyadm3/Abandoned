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

        /// <summary>
        /// Refused before our approval check ever ran (NGO's own prefab/config hash check fails for most
        /// other builds without giving a reason), or the host simply isn't there.
        /// </summary>
        public static string CouldNotJoin(string target, string ourVersion) =>
            $"Couldn't join {target}. Is the host running, and on the same build as you ({ourVersion})?";

        /// <summary>
        /// NGO hands clients either the reason the host sent or, over Unity Transport, its own debug text
        /// ("[Disconnect Event][Client-1]... ProtocolTimeout"). Only the host's reasons are for players.
        /// </summary>
        public static string FromHost(string disconnectReason) =>
            string.IsNullOrEmpty(disconnectReason) || disconnectReason.StartsWith("[") ? string.Empty : disconnectReason;

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

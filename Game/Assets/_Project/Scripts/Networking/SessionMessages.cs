namespace Abandoned.Networking
{
    /// <summary>
    /// Player-readable reasons a session didn't start or ended, shared by the host (approval refusals,
    /// leaving), clients and the Steam lobby so the menu and tests agree on the wording. They are kept and sent
    /// as English with their values (<see cref="Core.Loc.Wire"/>); the menu words them (0.24.8).
    /// </summary>
    public static class SessionMessages
    {
        public const string HostLeft = "The host left the game."; // @loc
        public const string LostHost = "Lost connection to the host."; // @loc
        public const string Kicked = "The host removed you from the game."; // @loc
        public const string LoadTimedOut = "Your game took too long to load the level, so the crew went on without you. Rejoin at the HQ."; // @loc

        /// <summary>
        /// Refused before our approval check ever ran (NGO's own prefab/config hash check fails for most
        /// other builds without giving a reason), or the host simply isn't there.
        /// </summary>
        public static string CouldNotJoin(string target, string ourVersion) =>
            Core.Loc.Wire("Couldn't join {0}. Is the host running, and on the same build as you ({1})?", target, ourVersion);

        /// <summary>
        /// NGO hands clients either the reason the host sent or, over Unity Transport, its own debug text
        /// ("[Disconnect Event][Client-1]... ProtocolTimeout"). Only the host's reasons are for players.
        /// </summary>
        public static string FromHost(string disconnectReason) =>
            string.IsNullOrEmpty(disconnectReason) || disconnectReason.StartsWith("[") ? string.Empty : disconnectReason;

        public static string Full(int maxPlayers) => Core.Loc.Wire("The game is full ({0} players).", maxPlayers);

        public static string VersionMismatch(string hostKey, string ourKey) =>
            Core.Loc.Wire("The host is on version {0}; you have {1}. Both players need the same build.", Describe(hostKey), Describe(ourKey));

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

using System;

namespace Abandoned.Networking
{
    /// <summary>
    /// Turns Steam init failures into messages a player can act on; the raw exception goes to the log.
    /// </summary>
    public static class SteamErrorMessages
    {
        public const string NotRunning = "Steam isn't running - start Steam and try again.";
        public const string LibraryMissing = "Steam couldn't be loaded on this computer - reinstall the game and try again.";
        public const string NeedsUpdate = "Steam needs an update - restart Steam to update it and try again.";
        public const string ConfigMissing = "Steam settings are missing from this build (NetworkConfig).";
        public const string Generic = "Couldn't connect to Steam - make sure Steam is running and you're logged in, then try again.";
        public const string LostConnection = "Lost connection to Steam - restart Steam and the game.";

        public static string FromException(Exception e)
        {
            if (e is DllNotFoundException || e is EntryPointNotFoundException || e is BadImageFormatException || e is TypeLoadException)
                return LibraryMissing;

            string message = e?.Message ?? string.Empty;
            // Facepunch 2.5.2 reports "SteamApi_Init failed with <ESteamAPIInitResult> - error: <text>".
            if (Contains(message, "NoSteamClient") || Contains(message, "install directory") || Contains(message, "not running"))
                return NotRunning;
            if (Contains(message, "VersionMismatch"))
                return NeedsUpdate;
            return Generic;
        }

        private static bool Contains(string text, string part) => text.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

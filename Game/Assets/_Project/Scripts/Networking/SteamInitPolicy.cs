using System;
using System.Collections.Generic;

namespace Abandoned.Networking
{
    /// <summary>
    /// Decides whether this process may start Steam. Batch mode (CI, automated tests) never does
    /// unless asked with -steam: the developer's own Steam account may be logged in on this machine,
    /// and an unattended Init would show them as playing the dev app.
    /// </summary>
    public static class SteamInitPolicy
    {
        public const string EnableFlag = "-steam";
        public const string DisableFlag = "-nosteam";

        public const string DisabledByFlagMessage = "Steam is turned off for this session (-nosteam).";
        public const string DisabledInBatchMessage = "Steam is off in batch mode and automated tests (run with -steam to allow it).";

        public static bool Allows(bool isBatchMode, IReadOnlyList<string> args, out string reason)
        {
            if (HasFlag(args, DisableFlag))
            {
                reason = DisabledByFlagMessage;
                return false;
            }
            if (isBatchMode && !HasFlag(args, EnableFlag))
            {
                reason = DisabledInBatchMessage;
                return false;
            }
            reason = string.Empty;
            return true;
        }

        private static bool HasFlag(IReadOnlyList<string> args, string flag)
        {
            if (args == null) return false;
            for (int i = 0; i < args.Count; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}

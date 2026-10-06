using System;
using System.Collections.Generic;

namespace Abandoned.Networking
{
    /// <summary>
    /// Decides whether this process may start Steam. Batch mode (CI) and automated test runs, including
    /// ones started from the editor's Test Runner window, never do unless asked with -steam: the
    /// developer's own Steam account may be logged in on this machine, and an unattended Init would
    /// show them as playing the dev app.
    /// </summary>
    public static class SteamInitPolicy
    {
        public const string EnableFlag = "-steam";
        public const string DisableFlag = "-nosteam";

        public const string DisabledByFlagMessage = "Steam is turned off for this session (-nosteam).";
        public const string DisabledInBatchMessage = "Steam is off in batch mode and automated tests (run with -steam to allow it).";

        /// <summary>
        /// True while a Unity Test Framework run is in progress. Set by the test assemblies' run callback
        /// (the only code that knows a GUI run has started; isBatchMode is false there).
        /// </summary>
        public static bool TestRunActive { get; set; }

        public static bool Allows(bool isBatchMode, IReadOnlyList<string> args, out string reason) =>
            Allows(isBatchMode, TestRunActive, args, out reason);

        public static bool Allows(bool isBatchMode, bool testRunActive, IReadOnlyList<string> args, out string reason)
        {
            if (HasFlag(args, DisableFlag))
            {
                reason = DisabledByFlagMessage;
                return false;
            }
            if ((isBatchMode || testRunActive) && !HasFlag(args, EnableFlag))
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

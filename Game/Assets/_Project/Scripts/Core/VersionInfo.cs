using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// The game's version for menus, logs, nettest results and (M3.6) refusing clients from another
    /// build. The version number itself is Player Settings > Version (<see cref="Application.version"/>);
    /// the commit and build time come from the stamped <see cref="BuildInfo"/> asset.
    /// </summary>
    public static class VersionInfo
    {
        private static BuildInfo cached;
        private static bool loaded;

        public static BuildInfo Build
        {
            get
            {
                if (!loaded)
                {
                    cached = Resources.Load<BuildInfo>(BuildInfo.ResourcePath);
                    loaded = true;
                }
                return cached;
            }
        }

        public static string Version => Application.version;
        public static string Commit => Build != null ? Build.Commit : BuildInfo.EditorCommit;

        /// <summary>"0.3.0 (abc1234)" - short enough for a menu corner.</summary>
        public static string Display => $"{Version} ({Commit})";

        public static string BuiltAtUtc => Build != null ? Build.BuiltAtUtc : "";

        /// <summary>
        /// What two machines compare before playing together. Version + commit, so two dev builds
        /// from different commits don't silently desync; the editor ("editor" commit) is compared on
        /// the version alone so MPPM and editor-vs-build testing keep working. A commit that doesn't
        /// name one exact tree ("-dirty" edits on top of it, or "unknown") also carries the build time,
        /// so only builds from the same build run match each other.
        /// </summary>
        public static string CompatibilityKey => FormatKey(Version, Commit, BuiltAtUtc);

        public const string DirtySuffix = "-dirty";
        public const string UnknownCommit = "unknown";

        public static string FormatKey(string version, string commit, string builtAtUtc = "") =>
            IsExactCommit(commit) ? $"{version}+{commit}" : $"{version}+{commit}@{builtAtUtc}";

        /// <summary>False for "unknown" and "-dirty" commits, which can stand for many different builds.</summary>
        public static bool IsExactCommit(string commit) =>
            !string.IsNullOrEmpty(commit) && commit != UnknownCommit &&
            !commit.EndsWith(DirtySuffix, System.StringComparison.Ordinal);

        public static bool AreCompatible(string keyA, string keyB)
        {
            Split(keyA, out string versionA, out string commitA, out string builtA);
            Split(keyB, out string versionB, out string commitB, out string builtB);
            if (versionA.Length == 0 || versionA != versionB) return false;
            if (commitA == BuildInfo.EditorCommit || commitB == BuildInfo.EditorCommit) return true;
            if (commitA != commitB) return false;
            // Same inexact label is not proof of the same code; without a build time there's nothing to match.
            return IsExactCommit(commitA) || (builtA.Length > 0 && builtA == builtB);
        }

        private static void Split(string key, out string version, out string commit, out string builtAtUtc)
        {
            key ??= string.Empty;
            int plus = key.LastIndexOf('+');
            version = plus < 0 ? key : key.Substring(0, plus);
            string rest = plus < 0 ? string.Empty : key.Substring(plus + 1);
            int at = rest.IndexOf('@');
            commit = at < 0 ? rest : rest.Substring(0, at);
            builtAtUtc = at < 0 ? string.Empty : rest.Substring(at + 1);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            cached = null;
            loaded = false;
        }
    }
}

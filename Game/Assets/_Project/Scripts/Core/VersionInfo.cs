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

        /// <summary>
        /// What two machines compare before playing together. Version + commit, so two dev builds
        /// from different commits don't silently desync; the editor ("editor" commit) is compared on
        /// the version alone so MPPM and editor-vs-build testing keep working.
        /// </summary>
        public static string CompatibilityKey => FormatKey(Version, Commit);

        public static string FormatKey(string version, string commit) => $"{version}+{commit}";

        public static bool AreCompatible(string keyA, string keyB)
        {
            if (keyA == keyB) return true;
            Split(keyA, out string versionA, out string commitA);
            Split(keyB, out string versionB, out string commitB);
            bool editorInvolved = commitA == BuildInfo.EditorCommit || commitB == BuildInfo.EditorCommit;
            return editorInvolved && versionA == versionB;
        }

        private static void Split(string key, out string version, out string commit)
        {
            key ??= string.Empty;
            int plus = key.LastIndexOf('+');
            version = plus < 0 ? key : key.Substring(0, plus);
            commit = plus < 0 ? string.Empty : key.Substring(plus + 1);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            cached = null;
            loaded = false;
        }
    }
}

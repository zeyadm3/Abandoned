using System.IO;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>Early Access helpers (M10.11): the community links and the player's log folder for bug reports.</summary>
    public static class Launch
    {
        private static LaunchConfig config;

        public static LaunchConfig Config => config != null ? config : config = Resources.Load<LaunchConfig>(LaunchConfig.ResourcePath);

        public static bool HasDiscord => Config != null && !string.IsNullOrWhiteSpace(Config.DiscordUrl);
        public static bool HasFeedback => Config != null && !string.IsNullOrWhiteSpace(Config.FeedbackUrl);

        public static void OpenDiscord()
        {
            if (HasDiscord) Application.OpenURL(Config.DiscordUrl);
        }

        public static void OpenFeedback()
        {
            if (HasFeedback) Application.OpenURL(Config.FeedbackUrl);
        }

        /// <summary>The folder holding Player.log (attach it to a bug report).</summary>
        public static string LogFolder
        {
            get
            {
                string log = Application.consoleLogPath;
                return string.IsNullOrEmpty(log) ? Application.persistentDataPath : Path.GetDirectoryName(log);
            }
        }

        public static void OpenLogFolder()
        {
            string folder = LogFolder;
            if (!string.IsNullOrEmpty(folder)) Application.OpenURL("file://" + folder.Replace('\\', '/'));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => config = null;
    }
}

using System;
using System.IO;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>Early Access helpers (M10.11): the community links and the player's log folder for bug reports.</summary>
    public static class Launch
    {
        private const string MailScheme = "mailto:";

        private static LaunchConfig config;
        private static bool? forcedPublic;

        public static LaunchConfig Config => config != null ? config : config = Resources.Load<LaunchConfig>(LaunchConfig.ResourcePath);

        /// <summary>
        /// The builds Steam hands to strangers (release and demo). Friends' builds may show a private
        /// address for feedback; these never do.
        /// </summary>
        public static bool IsPublicBuild => forcedPublic ?? (VersionInfo.Build != null &&
            (VersionInfo.Build.Flavor == "Release" || VersionInfo.Build.Flavor == "Demo"));

        public static bool HasDiscord => Config != null && !string.IsNullOrWhiteSpace(Config.DiscordUrl);
        public static bool HasFeedback => FeedbackUrl.Length > 0;

        /// <summary>
        /// Where feedback goes in this build ("" = nowhere, the button is hidden). A Steam build without a
        /// link of its own sends players to the game's Steam discussions.
        /// </summary>
        public static string FeedbackUrl
        {
            get
            {
                LaunchConfig c = Config;
                if (!IsPublicBuild) return c != null ? Clean(c.FeedbackUrl) : "";
                string own = c != null ? Clean(c.ReleaseFeedbackUrl) : "";
                if (own.Length > 0) return own;
                // set-app-id writes the game's App ID here too (the demo's wishlist button needs it).
                uint app = Demo.Config != null ? Demo.Config.StoreAppId : 0;
                return app != 0 ? $"https://steamcommunity.com/app/{app}/discussions/" : "";
            }
        }

        /// <summary>The address when feedback goes by e-mail ("" when it is a web page or nothing).</summary>
        public static string FeedbackEmail
        {
            get
            {
                string url = FeedbackUrl;
                if (!url.StartsWith(MailScheme, StringComparison.OrdinalIgnoreCase)) return "";
                int query = url.IndexOf('?');
                return (query < 0 ? url.Substring(MailScheme.Length) : url.Substring(MailScheme.Length, query - MailScheme.Length)).Trim();
            }
        }

        public static void OpenDiscord()
        {
            if (HasDiscord) Application.OpenURL(Config.DiscordUrl);
        }

        public static void OpenFeedback()
        {
            if (HasFeedback) Application.OpenURL(FeedbackUrl);
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
            if (string.IsNullOrEmpty(folder)) return;
            // An escaped URI: the folder has a space ("Zeyad Games"), and an unescaped one opens nothing on macOS.
            try { Application.OpenURL(new Uri(folder).AbsoluteUri); }
            catch (UriFormatException) { Application.OpenURL("file://" + folder.Replace('\\', '/')); }
        }

        /// <summary>Tests/editor: act as (or not as) a Steam build. Null = the build's own flavour.</summary>
        public static void ForcePublicBuild(bool? isPublic) => forcedPublic = isPublic;

        private static string Clean(string url) => string.IsNullOrWhiteSpace(url) ? "" : url.Trim();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            config = null;
            forcedPublic = null;
        }
    }
}

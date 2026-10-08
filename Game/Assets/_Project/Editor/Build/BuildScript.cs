using System;
using System.IO;
using System.Linq;
using Abandoned.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Player builds (Mono, CLAUDE.md) into Game/Builds/&lt;folder&gt;/ (gitignored). Scenes come from
    /// <see cref="BuildScenes"/>, the version from Player Settings > Version plus the stamped
    /// <see cref="BuildInfo"/>. Non-release builds get steam_appid.txt (480) beside the executable so
    /// Steam knows the app when it isn't launched from Steam. Tools/unity.sh build-mac|build-win|build
    /// call these through <see cref="BatchCommands"/> and zip the folders.
    /// </summary>
    public static class BuildScript
    {
        public const string ProductName = "Abandoned";
        /// <summary>0.&lt;milestone being built&gt;.&lt;patch&gt;; bump the middle number when a milestone starts.</summary>
        public const string Version = "0.12.7";
        public const string CompanyName = "Zeyad Games";
        public const string BundleId = "com.zeyadgames.abandoned";
        public const string MicrophoneUsage = "Abandoned uses your microphone for proximity voice chat with your crew.";
        public const string BuildsFolder = "Builds";
        public const string InfoFileName = "BUILD.txt";
        private const string Menu = "Tools/Abandoned/Build/";

        [MenuItem(Menu + "Mac")]
        public static void BuildMacMenu() => Report(Build(BuildPlatform.Mac, BuildFlavor.Shareable));

        [MenuItem(Menu + "Windows")]
        public static void BuildWindowsMenu() => Report(Build(BuildPlatform.Windows, BuildFlavor.Shareable));

        [MenuItem(Menu + "Both (Mac + Windows)")]
        public static void BuildBothMenu() => Report(BuildBoth());

        [MenuItem(Menu + "Dev Mac (for tests)")]
        public static void BuildDevMacMenu() => Report(Build(BuildPlatform.Mac, BuildFlavor.Dev));

        [MenuItem(Menu + "Release (Mac + Windows, real App ID)")]
        public static void BuildReleaseMenu() => Report(BuildRelease());

        /// <summary>
        /// The Steam release (M10.11): both platforms, no steam_appid.txt, Game/Builds/MacRelease and
        /// WindowsRelease, ready to upload as depots. Refuses while the App ID is still Spacewar (480).
        /// </summary>
        public static bool BuildRelease()
        {
            var network = AssetDatabase.LoadAssetAtPath<Abandoned.Networking.NetworkConfig>(NetworkContentBuilder.ConfigPath);
            if (network == null || network.SteamAppId == Abandoned.Networking.NetworkConfig.DevelopmentAppId)
            {
                Debug.LogError("[Build] A release build needs the game's real Steam App ID in Data/Networking/NetworkConfig " +
                               "(still 480, Spacewar). See Docs/STEAM_SETUP.md and Docs/LAUNCH.md.");
                return false;
            }
            string builtAtUtc = BuildInfoAsset.Now();
            return Build(BuildPlatform.Mac, BuildFlavor.Release, builtAtUtc) &&
                   Build(BuildPlatform.Windows, BuildFlavor.Release, builtAtUtc);
        }

        [MenuItem(Menu + "Demo (Mac + Windows)")]
        public static void BuildDemoMenu() => Report(BuildDemo());

        /// <summary>The demo (M8.3) for both platforms: Game/Builds/MacDemo and WindowsDemo.</summary>
        public static bool BuildDemo()
        {
            string builtAtUtc = BuildInfoAsset.Now();
            return Build(BuildPlatform.Mac, BuildFlavor.Demo, builtAtUtc) &&
                   Build(BuildPlatform.Windows, BuildFlavor.Demo, builtAtUtc);
        }

        public static bool BuildBoth()
        {
            string builtAtUtc = BuildInfoAsset.Now();
            return Build(BuildPlatform.Mac, BuildFlavor.Shareable, builtAtUtc) &&
                   Build(BuildPlatform.Windows, BuildFlavor.Shareable, builtAtUtc);
        }

        /// <summary>Game/Builds/Mac, Game/Builds/MacDev, Game/Builds/WindowsRelease...</summary>
        public static string OutputFolder(BuildPlatform platform, BuildFlavor flavor) =>
            Path.Combine(BuildsFolder, platform + (flavor == BuildFlavor.Shareable ? "" : flavor.ToString()));

        public static string ExecutablePath(BuildPlatform platform, BuildFlavor flavor) =>
            Path.Combine(OutputFolder(platform, flavor), ProductName + (platform == BuildPlatform.Mac ? ".app" : ".exe"));

        /// <summary>Release builds are launched by Steam with the real App ID; a stray 480 file would override it.</summary>
        public static bool ShipsSteamAppId(BuildFlavor flavor) => flavor != BuildFlavor.Release;

        public static BuildTarget TargetOf(BuildPlatform platform) =>
            platform == BuildPlatform.Mac ? BuildTarget.StandaloneOSX : BuildTarget.StandaloneWindows64;

        public static BuildOptions OptionsOf(BuildFlavor flavor) =>
            BuildOptions.StrictMode | (flavor == BuildFlavor.Dev ? BuildOptions.Development : BuildOptions.None);

        /// <summary>Settings every build relies on; stored in ProjectSettings so editor and builds agree.</summary>
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = Version;
            // Company + bundle id decide the save-data folder (Application.persistentDataPath), so they
            // must not change once players have saves (user decision 2026-10-06).
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, BundleId);
            // macOS asks the player before the game may use the microphone (proximity voice); without a
            // usage description the request is refused silently.
            PlayerSettings.iOS.microphoneUsageDescription = MicrophoneUsage;
            // Several instances on one Mac (MPPM, nettest, LAN tests): an unfocused one must keep simulating.
            PlayerSettings.runInBackground = true;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        }

        public static bool Build(BuildPlatform platform, BuildFlavor flavor, string builtAtUtc = null)
        {
            ApplyPlayerSettings();
            BuildScenes.ApplyToEditorSettings();
            string missing = BuildScenes.All.FirstOrDefault(s => !File.Exists(s));
            if (missing != null)
            {
                Debug.LogError($"[Build] Scene {missing} doesn't exist; run Tools/unity.sh rebuild.");
                return false;
            }

            string folder = OutputFolder(platform, flavor);
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
            if (platform == BuildPlatform.Mac) SetMacArchitecture(flavor);

            var options = new BuildPlayerOptions
            {
                scenes = BuildScenes.ForPlayer(flavor),
                locationPathName = ExecutablePath(platform, flavor),
                target = TargetOf(platform),
                targetGroup = BuildTargetGroup.Standalone,
                options = OptionsOf(flavor),
            };

            BuildReport report;
            BuildInfo stamp = BuildInfoAsset.Stamp(flavor, builtAtUtc);
            string commit = stamp.Commit;
            string builtAt = stamp.BuiltAtUtc;
            try
            {
                report = BuildPipeline.BuildPlayer(options);
            }
            finally
            {
                BuildInfoAsset.Reset();
            }

            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[Build] {platform} {flavor} failed: {summary.result}, {summary.totalErrors} error(s).");
                return false;
            }
            RemoveDoNotShipFolders(platform, flavor);
            if (ShipsSteamAppId(flavor)) CopySteamAppId(platform, flavor);
            if (platform == BuildPlatform.Mac && MacSignature.Problem(options.locationPathName) is string signature)
            {
                Debug.LogError($"[Build] {options.locationPathName} signature is broken: {signature}");
                return false;
            }
            WriteInfoFile(platform, flavor, summary, commit, builtAt);
            Debug.Log($"[Build] {platform} {flavor} OK: {Path.GetFullPath(options.locationPathName)} " +
                      $"({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:0} s).");
            return true;
        }

        // Dev builds only run on this Mac (Apple Silicon); shared builds must also run on Intel Macs.
        private static void SetMacArchitecture(BuildFlavor flavor) =>
            UnityEditor.OSXStandalone.UserBuildSettings.architecture =
                flavor == BuildFlavor.Dev ? OSArchitecture.ARM64 : OSArchitecture.x64ARM64;

        // Burst writes debug symbols next to the player; they are for us, not for players' zips.
        private static void RemoveDoNotShipFolders(BuildPlatform platform, BuildFlavor flavor)
        {
            foreach (string dir in Directory.GetDirectories(OutputFolder(platform, flavor), "*DoNotShip*"))
                Directory.Delete(dir, true);
        }

        private static void CopySteamAppId(BuildPlatform platform, BuildFlavor flavor)
        {
            string source = NetworkContentBuilder.SteamAppIdFile;
            if (!File.Exists(source))
            {
                Debug.LogWarning($"[Build] {source} is missing; Steam won't know the App ID outside Steam.");
                return;
            }
            string folder = OutputFolder(platform, flavor);
            // Only beside the .app, never inside it: a file added to the bundle breaks Unity's ad-hoc
            // signature seal and macOS then calls the downloaded app "damaged". Facepunch's
            // SteamClient.Init also sets SteamAppId in the environment, so a Finder launch still works.
            File.Copy(source, Path.Combine(folder, source), true);
        }

        // Repeats the stamp baked into the player rather than asking git again: the zip name and the
        // in-game version check must describe the same build.
        private static void WriteInfoFile(BuildPlatform platform, BuildFlavor flavor, BuildSummary summary,
                                          string commit, string builtAt)
        {
            string text =
                $"version={Application.version}\n" +
                $"commit={commit}\n" +
                $"platform={platform}\n" +
                $"flavor={flavor}\n" +
                $"built={builtAt}\n" +
                $"unity={Application.unityVersion}\n" +
                $"sizeMB={summary.totalSize / (1024 * 1024)}\n";
            File.WriteAllText(Path.Combine(OutputFolder(platform, flavor), InfoFileName), text);
        }

        private static void Report(bool ok)
        {
            if (ok) EditorUtility.RevealInFinder(BuildsFolder);
            else EditorUtility.DisplayDialog("Build failed", "See the Console for the build errors.", "OK");
        }
    }
}

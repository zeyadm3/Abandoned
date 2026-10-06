using System;
using System.Diagnostics;
using Abandoned.Core;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The commit label builds are stamped with: short HEAD, plus "-dirty" when anything that ships
    /// has uncommitted changes. Without the suffix a build made from edits on top of a commit claims
    /// to BE that commit, and the join version check would let it play with a clean build of it.
    /// </summary>
    public static class GitInfo
    {
        public const string Unknown = VersionInfo.UnknownCommit;
        public const string DirtySuffix = VersionInfo.DirtySuffix;

        // What ends up in a player, minus the files Unity rewrites on its own (PROGRESS.md "churn")
        // and the BuildInfo asset the build script itself stamps; those would mark every build dirty.
        private const string ShippedPaths =
            "\":(top)Game/Assets\" \":(top)Game/Packages\" \":(top)Game/ProjectSettings\" " +
            "\":(top,exclude)Game/Assets/Settings/PC_RPAsset.asset\" " +
            "\":(top,exclude)Game/Assets/Settings/UniversalRenderPipelineGlobalSettings.asset\" " +
            "\":(top,exclude)Game/Assets/Settings/DefaultVolumeProfile.asset\" " +
            "\":(top,exclude)Game/ProjectSettings/Packages\" " +
            "\":(top,exclude)Game/ProjectSettings/SceneTemplateSettings.json\" " +
            "\":(top,exclude)" + BuildInfoAssetPathFromRoot + "\" " +
            "\":(top,exclude)" + BuildInfoAssetPathFromRoot + ".meta\"";

        private const string BuildInfoAssetPathFromRoot = "Game/" + BuildInfoAsset.Path;

        /// <summary>"abc1234", "abc1234-dirty", or "unknown" when git isn't available.</summary>
        public static string CommitLabel()
        {
            string head = Run("rev-parse --short HEAD");
            if (head == null) return Unknown;
            string status = Run("status --porcelain -- " + ShippedPaths);
            // A failed status can't prove the tree is clean, so it counts as dirty.
            return Label(head, status ?? "?");
        }

        /// <summary>Pure part of <see cref="CommitLabel"/>, split out for tests.</summary>
        public static string Label(string head, string porcelainStatus)
        {
            if (string.IsNullOrWhiteSpace(head)) return Unknown;
            return string.IsNullOrWhiteSpace(porcelainStatus) ? head.Trim() : head.Trim() + DirtySuffix;
        }

        private static string Run(string arguments)
        {
            try
            {
                var start = new ProcessStartInfo("git", arguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using Process git = Process.Start(start);
                string output = git.StandardOutput.ReadToEnd().Trim();
                git.WaitForExit(5000);
                return git.HasExited && git.ExitCode == 0 ? output : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

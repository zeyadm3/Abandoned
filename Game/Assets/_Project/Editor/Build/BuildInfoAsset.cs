using System;
using System.Diagnostics;
using Abandoned.Core;
using UnityEditor;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>Creates, stamps and resets the BuildInfo asset the game reads its commit/build time from.</summary>
    public static class BuildInfoAsset
    {
        public const string Folder = "Assets/_Project/Data/Core/Resources";
        public const string Path = Folder + "/BuildInfo.asset";

        public static BuildInfo CreateMissing()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Data/Core", "Resources");
            var info = LoadOrCreateAsset<BuildInfo>(Path);
            AssetDatabase.SaveAssets();
            return info;
        }

        public static void Stamp(BuildFlavor flavor)
        {
            BuildInfo info = CreateMissing();
            info.Stamp(GitCommit(), DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), flavor.ToString());
            Save(info);
        }

        /// <summary>Back to editor defaults so a build never leaves the committed asset modified.</summary>
        public static void Reset()
        {
            BuildInfo info = CreateMissing();
            info.Stamp(BuildInfo.EditorCommit, "", "");
            Save(info);
        }

        private static void Save(BuildInfo info)
        {
            EditorUtility.SetDirty(info);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Short HEAD commit, or "unknown" when git isn't available.</summary>
        public static string GitCommit()
        {
            try
            {
                var start = new ProcessStartInfo("git", "rev-parse --short HEAD")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using Process git = Process.Start(start);
                string output = git.StandardOutput.ReadToEnd().Trim();
                git.WaitForExit(5000);
                return git.ExitCode == 0 && output.Length > 0 ? output : "unknown";
            }
            catch (Exception)
            {
                return "unknown";
            }
        }
    }
}

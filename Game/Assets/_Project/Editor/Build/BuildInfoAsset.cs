using System;
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
            // The demo's rules sit beside it (Resources: read before any scene exists).
            LoadOrCreateAsset<Abandoned.Core.DemoConfig>(Folder + "/" + Abandoned.Core.DemoConfig.ResourcePath + ".asset");
            // ...and the Early Access links (M10.11; empty until the Discord and the form exist).
            LoadOrCreateAsset<Abandoned.Core.LaunchConfig>(Folder + "/" + Abandoned.Core.LaunchConfig.ResourcePath + ".asset");
            AssetDatabase.SaveAssets();
            return info;
        }

        /// <summary>UTC build time in the format BuildInfo and BUILD.txt use.</summary>
        public static string Now() => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        /// <summary>
        /// Stamps the asset and returns it so BUILD.txt can repeat the exact same commit and time.
        /// Pass one builtAtUtc to every platform of a shared pair: a dirty build is only compatible
        /// with builds carrying the same stamp, and the Mac and Windows zips must play together.
        /// </summary>
        public static BuildInfo Stamp(BuildFlavor flavor, string builtAtUtc = null)
        {
            BuildInfo info = CreateMissing();
            info.Stamp(GitInfo.CommitLabel(), builtAtUtc ?? Now(), flavor.ToString());
            Save(info);
            return info;
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
    }
}

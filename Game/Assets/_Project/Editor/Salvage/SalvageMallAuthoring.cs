using System;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Authors only the salvage release; testing and play review remain a separate user step.</summary>
    public static class SalvageMallAuthoring
    {
        [MenuItem(ToolMenu.MallUpdates + "0.15.5 Build Salvage Content", priority = ToolMenu.MallP + 20)]
        public static void AuthorContent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Author salvage content in Edit mode.");
            SalvageAssetBuilder.AuthorAll();
            // This preserves shared HQ/neighbourhood material tuning while rebuilding only Mall.
            MeridianMallOverhaul.Author();
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[Salvage0155] Authored 45 asset families, their dressing/loot prefabs and the five-zone Mall scene. User review is pending.");
        }

        public static void AuthorContentAndExit() => RunBatch(false);

        public static void AuthorAndBuildPlayersAndExit() => RunBatch(true);

        private static void RunBatch(bool makePlayers)
        {
            try
            {
                AuthorContent();
                if (makePlayers)
                {
                    string stamp = BuildInfoAsset.Now();
                    // The user's current instruction leaves optional artifact inspection and all QA to them.
                    bool mac = BuildScript.Build(BuildPlatform.Mac, BuildFlavor.Shareable, stamp, false);
                    bool windows = BuildScript.Build(BuildPlatform.Windows, BuildFlavor.Shareable, stamp, false);
                    if (!mac || !windows) throw new InvalidOperationException("One or more 0.15.5 player builds could not be produced.");
                }
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}

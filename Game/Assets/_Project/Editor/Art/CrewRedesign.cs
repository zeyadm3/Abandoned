using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Batch entry for the 2026-10-22 crew redesign: refits the wardrobe and rebuilds the player prefab (which also
    /// saves the wardrobe mannequin and the first-person gloves). Same steps as Characters > Build Cosmetics, then
    /// Prefabs > Create Player Prefab. Run: Tools/unity.sh exec Abandoned.EditorTools.CrewRedesign.AuthorAndExit
    /// </summary>
    public static class CrewRedesign
    {
        public static void AuthorAndExit()
        {
            int code = 0;
            try
            {
                AssetDatabase.ImportAsset(WorkerVisualBuilder.ArtFolder, ImportAssetOptions.ImportRecursive);
                CosmeticsBuilder.CreateMissing();
                PlayerPrefabBuilder.Create();
                AssetDatabase.SaveAssets();
                Debug.Log("[CrewRedesign] wardrobe refitted, player prefab rebuilt.");
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                code = 1;
            }
            EditorApplication.Exit(code);
        }
    }
}

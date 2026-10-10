using System;
using Abandoned.Extraction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>More usable salvage and a commercial collection truck over the saved 0.15.6 mall.</summary>
    public static class SalvageRelease0157
    {
        [MenuItem("Tools/Abandoned/Mall/More Pickups and Collection Truck (0.15.7)")]
        public static void AuthorContent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Author 0.15.7 in Edit mode.");
            Scene scene = SceneManager.GetSceneByPath(MallBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            Transform mall = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "Mall") mall = root.transform;
            if (mall == null) throw new InvalidOperationException("The saved Mall root is missing.");

            SalvagePickables0157.Author();
            SalvageLootPlacement0157.Build(mall);
            TruckCargo cargo = mall.GetComponentInChildren<TruckCargo>(true);
            if (cargo == null) throw new InvalidOperationException("The saved collection truck is missing.");
            CollectionTruck0157.Upgrade(cargo.transform);
            MeridianLightLayers.Apply(mall);
            ShadowCasterPass.Apply(mall, "Mall");
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the 0.15.7 mall.");
            // Only pickup markers and the excluded truck changed; the interior navigation stays authored.
            if (!OcclusionBake.Bake())
                throw new InvalidOperationException("Unity could not author 0.15.7 occlusion data.");
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save 0.15.7 scene data.");
            Debug.Log("[Salvage0157] Saved more recoverable loot and the realistic collection truck as 0.15.7.EEA. User gameplay review is pending.");
        }

        public static void AuthorAndBuildPlayersAndExit() => RunBatch(true);
        public static void BuildPlayersAndExit() => RunBatch(false);

        private static void RunBatch(bool authorContent)
        {
            try
            {
                if (authorContent) AuthorContent();
                string stamp = BuildInfoAsset.Now();
                bool mac = BuildScript.Build(BuildPlatform.Mac, BuildFlavor.Shareable, stamp, false);
                bool windows = BuildScript.Build(BuildPlatform.Windows, BuildFlavor.Shareable, stamp, false);
                if (!mac || !windows) throw new InvalidOperationException("One or more 0.15.7 player builds failed.");
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

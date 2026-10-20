using System;
using Abandoned.Extraction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>Replaces only the saved collection truck and ships the revised commercial vehicle.</summary>
    public static class CollectionTruckRedoRelease
    {
        [MenuItem(ToolMenu.MallUpdates + "0.15.7b Replace Collection Truck", priority = ToolMenu.MallP + 24)]
        public static void AuthorContent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Author the replacement truck in Edit mode.");
            Scene scene = SceneManager.GetSceneByPath(MallBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            Transform mall = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "Mall") mall = root.transform;
            if (mall == null) throw new InvalidOperationException("The saved Mall root is missing.");
            TruckCargo cargo = mall.GetComponentInChildren<TruckCargo>(true);
            if (cargo == null) throw new InvalidOperationException("The saved collection truck is missing.");

            CollectionTruck0157.Upgrade(cargo.transform);
            MeridianLightLayers.Apply(mall);
            ShadowCasterPass.Apply(mall, "Mall");
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the 0.15.7b collection truck.");
            // The truck remains excluded from interior navigation; replacing its art needs no mall rebake.
            if (!OcclusionBake.Bake())
                throw new InvalidOperationException("Unity could not author 0.15.7b occlusion data.");
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save 0.15.7b scene data.");
            Debug.Log("[Truck0157b] Saved the replacement commercial collection truck as 0.15.7b.EEA. User gameplay review is pending.");
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
                if (!mac || !windows) throw new InvalidOperationException("One or more 0.15.7b player builds failed.");
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

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>Authors the stronger-threat release over the saved mall without replacing its approved layout.</summary>
    public static class SalvageRelease0156
    {
        [MenuItem(ToolMenu.MallUpdates + "0.15.6 Finish Threat and Content Update", priority = ToolMenu.MallP + 22)]
        public static void AuthorContent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Author 0.15.6 in Edit mode.");
            Scene scene = SceneManager.GetSceneByPath(MallBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            Transform mall = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "Mall") mall = root.transform;
            if (mall == null) throw new InvalidOperationException("The saved Mall root is missing.");

            ThreatPressure0156.Apply();
            ThreatPressure0156.AuthorSpawns(mall);
            MallExpansion0156.Build(mall);
            // Content bakes make the new solid fittings and district part of the saved player scene.
            MallSurfaceBuilder.Assign(mall);
            MallBuilder.BakeNavMesh(mall);
            MeridianLightLayers.Apply(mall);
            ShadowCasterPass.Apply(mall, "Mall");
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the expanded mall scene.");
            if (!OcclusionBake.Bake())
                throw new InvalidOperationException("Unity could not author 0.15.6 occlusion data.");
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save 0.15.6 scene data.");
            Debug.Log("[Salvage0156] Saved stronger threat pacing, spawn coverage and additive content as 0.15.6.EEA. User gameplay review is pending.");
        }

        public static void AuthorAndBuildPlayersAndExit()
        {
            RunBatch(true);
        }

        public static void BuildPlayersAndExit()
        {
            RunBatch(false);
        }

        private static void RunBatch(bool authorContent)
        {
            try
            {
                if (authorContent) AuthorContent();
                string stamp = BuildInfoAsset.Now();
                bool mac = BuildScript.Build(BuildPlatform.Mac, BuildFlavor.Shareable, stamp, false);
                bool windows = BuildScript.Build(BuildPlatform.Windows, BuildFlavor.Shareable, stamp, false);
                if (!mac || !windows) throw new InvalidOperationException("One or more 0.15.6 player builds failed.");
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

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>Installs generated store campaigns over the saved mall without rebuilding gameplay.</summary>
    public static class MallBrandingRelease0158
    {
        [MenuItem("Tools/Abandoned/Mall/Store Branding and Sale Artwork (0.15.8)")]
        public static void AuthorContent() => Author(true);

        [MenuItem("Tools/Abandoned/Mall/Remove Decorative Text (0.15.8b)")]
        public static void AuthorTextCleanup() => Author(false);

        private static void Author(bool installArtwork)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Author mall advertising in Edit mode.");
            AssetDatabase.Refresh();
            Scene scene = SceneManager.GetSceneByPath(MallBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            Transform mall = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "Mall") mall = root.transform;
            if (mall == null) throw new InvalidOperationException("The saved Mall root is missing.");

            MallTextCleanup0158.Build(mall);
            if (installArtwork) MallRetailArtwork0158.Build(mall);
            MeridianLightLayers.Apply(mall);
            ShadowCasterPass.Apply(mall, "Mall");
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save mall retail presentation.");
            // Print media has no blocking colliders; interior navigation remains the existing authored data.
            if (!OcclusionBake.Bake())
                throw new InvalidOperationException("Unity could not author mall occlusion data.");
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save mall scene data.");
            Debug.Log($"[Mall0158] Saved {BuildScript.Version} retail presentation: image artwork, essential directions and functional UI; decorative world text removed. User review is pending.");
        }

        public static void AuthorAndBuildPlayersAndExit() => RunBatch(true, true);
        public static void DeclutterAndBuildPlayersAndExit() => RunBatch(true, false);
        public static void BuildPlayersAndExit() => RunBatch(false, false);

        private static void RunBatch(bool authorContent, bool installArtwork)
        {
            try
            {
                if (authorContent) Author(installArtwork);
                string stamp = BuildInfoAsset.Now();
                bool mac = BuildScript.Build(BuildPlatform.Mac, BuildFlavor.Shareable, stamp, false);
                bool windows = BuildScript.Build(BuildPlatform.Windows, BuildFlavor.Shareable, stamp, false);
                if (!mac || !windows) throw new InvalidOperationException("One or more player builds failed.");
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

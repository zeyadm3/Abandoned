using System;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>An additive density layer over the saved, approved mall; no gameplay or QA operation.</summary>
    public static class SalvageMallDensity
    {
        public const string GroupName = "_SALVAGE_DENSITY";

        public static void Build(Transform mall)
        {
            Transform previous = mall.Find(GroupName);
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            Transform root = GreyboxFactory.Group(GroupName, mall);
            var fit = new SalvageDensityPlacement(mall, root);
            SalvageRetailDensity.Build(fit);
            SalvageWorkshopDensity.Build(fit);
            SalvageConcourseDensity.Build(fit);
            SalvageDensityBackdrops.Build(fit);
            int batches = SalvageDensityBatcher.Build(root);
            root.gameObject.AddComponent<SalvageDensityDebug>().EditorSetup(fit.Props, fit.Details, batches);
            Debug.Log($"[Salvage0155b] Added {fit.Props} stock props and {fit.Details} packing/demolition details in {batches} section batches to the existing mall.");
        }

        [MenuItem(ToolMenu.MallUpdates + "0.15.5b Fill Salvage Mall", priority = ToolMenu.MallP + 21)]
        public static void AuthorCurrentMall()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Fill the saved mall in Edit mode.");
            Scene scene = SceneManager.GetSceneByPath(MallBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            Transform mall = null;
            foreach (GameObject candidate in scene.GetRootGameObjects())
                if (candidate.name == "Mall") mall = candidate.transform;
            if (mall == null) throw new InvalidOperationException("The saved Mall root is missing.");
            Build(mall);
            // These are content bakes needed by the additional solid fittings, not QA checks.
            MallSurfaceBuilder.Assign(mall);
            MallBuilder.BakeNavMesh(mall);
            MeridianLightLayers.Apply(mall);
            ShadowCasterPass.Apply(mall, "Mall");
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath);
            if (!OcclusionBake.Bake()) throw new InvalidOperationException("Unity could not author the expanded mall's occlusion data.");
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the filled mall scene.");
            Debug.Log("[Salvage0155b] Saved the denser mall as 0.15.5b.EEA. User review remains pending.");
        }

        public static void AuthorAndBuildPlayersAndExit()
        {
            try
            {
                AuthorCurrentMall();
                string stamp = BuildInfoAsset.Now();
                bool mac = BuildScript.Build(BuildPlatform.Mac, BuildFlavor.Shareable, stamp, false);
                bool windows = BuildScript.Build(BuildPlatform.Windows, BuildFlavor.Shareable, stamp, false);
                if (!mac || !windows) throw new InvalidOperationException("One or more 0.15.5b player builds could not be produced.");
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

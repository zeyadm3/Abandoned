using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.11: level fixes found by the restored audits (<see cref="MallAudit"/>). Cuts the basement openings out of the
    /// district ground's collider in the saved mall (<see cref="DistrictGroundHole"/>), makes walk-through props solid in
    /// the mall and the Factory (<see cref="SolidObjectsPass"/>, <see cref="LevelSolidity"/>), and rebuilds the School and
    /// Prison from the fixed location builder. Details: Docs/PROGRESS.md "0.24.11".
    /// </summary>
    public static class Release02411
    {
        [MenuItem(ToolMenu.MallUpdates + "0.24.11 Basement Ground and Solid Props", priority = ToolMenu.MallP + 43)]
        public static void Author()
        {
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();

            Scene scene = SceneManager.GetSceneByPath(MallBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded) scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            Transform mall = scene.GetRootGameObjects().FirstOrDefault(r => r.name == "Mall")?.transform;
            if (mall == null) throw new InvalidOperationException("The saved Mall root is missing.");
            bool cut = DistrictGroundHole.Apply(mall);
            Debug.Log($"[Ground02411] Basement openings {(cut ? "cut out of" : "already clear of")} the district ground.");
            // The 0.14.0 rule (anything with volume a player can reach is solid) had stopped being applied to the saved
            // mall: the 0.15.5b density props and the 0.24.9 stand-ins could be walked through. Same route guard as a rebuild.
            int solid = SolidObjectsPass.Apply(mall, "Mall", new MallRouteClearance(mall).Conflict);
            Debug.Log($"[Solid02411] {solid} objects made solid.");
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the mall.");

            foreach (string path in new[] { SchoolBuilder.ScenePath, FactoryBuilder.ScenePath, PrisonBuilder.ScenePath })
            {
                Scene level = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                GameObject root = level.GetRootGameObjects().OrderByDescending(r => r.GetComponentsInChildren<Renderer>(true).Length).First();
                Debug.Log($"[Solid02411] {level.name}: {LevelSolidity.Apply(root.transform, level.name)} objects made solid.");
                if (!EditorSceneManager.SaveScene(level, path)) throw new InvalidOperationException($"Unity could not save {level.name}.");
            }
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        /// <summary>
        /// The School and Prison rebuilt from the fixed location builder (stairs lined up with their shafts and doorways,
        /// no wall or dressing across a stairwell, solid props). Nothing had changed either scene since it was built.
        /// </summary>
        public static void RebuildLevels()
        {
            SchoolBuilder.Build();
            PrisonBuilder.Build();
        }

        public static void RebuildLevelsAndExit() => ReleaseKit.RunAndExit(RebuildLevels);
    }
}

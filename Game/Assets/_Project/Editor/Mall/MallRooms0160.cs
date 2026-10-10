using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.16.0: closable doors and hiding lockers, added over the saved 0.15.8c mall. Nothing existing is
    /// moved or cut: the doors stand in doorways the building already has and the lockers against bare
    /// walls, so the baked NavMesh and occlusion data stay valid (doors and lockers carve the NavMesh at run
    /// time). The full mall builder calls <see cref="Build"/> too, so a regeneration keeps them.
    /// </summary>
    public static class MallRooms0160
    {
        public const string GroupName = "_ROOMS_0160";

        [MenuItem("Tools/Abandoned/Mall/Add Doors and Lockers (0.16.0)")]
        public static void AuthorContent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Author 0.16.0 in Edit mode.");
            Scene scene = SceneManager.GetSceneByPath(MallBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                // Another scene is open: don't throw its unsaved changes away without asking.
                if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            }
            SceneManager.SetActiveScene(scene);
            Transform mall = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "Mall") mall = root.transform;
            if (mall == null) throw new InvalidOperationException("The saved Mall root is missing.");

            // The config asset, the run's door state on the RunState prefab, and the new sound rows.
            RoomsContentBuilder.CreateMissing();
            Build(mall);
            MeridianLightLayers.Apply(mall);
            ShadowCasterPass.Apply(mall, "Mall");
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the 0.16.0 mall.");
            Debug.Log("[Rooms0160] Saved the mall with sliding doors and hiding lockers as " + BuildScript.Version + ". Nothing was baked or tested.");
        }

        /// <summary>Batch mode: <c>-executeMethod Abandoned.EditorTools.MallRooms0160.AuthorAndExit</c>.</summary>
        public static void AuthorAndExit()
        {
            try
            {
                AuthorContent();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>Adds (or replaces) the doors and lockers under the mall's root.</summary>
        public static void Build(Transform mall)
        {
            if (mall == null) throw new ArgumentNullException(nameof(mall));
            Transform previous = mall.Find(GroupName);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            Transform root = GreyboxFactory.Group(GroupName, mall);
            int doors = MallDoorBuilder.Build(GreyboxFactory.Group("Doors", root));
            int lockers = MallLockerBuilder.Build(mall, GreyboxFactory.Group("Lockers", root));
            Debug.Log($"[Rooms0160] {doors} sliding doors (8 store, 2 service) and {lockers} hiding lockers stand in the mall.");
        }
    }
}

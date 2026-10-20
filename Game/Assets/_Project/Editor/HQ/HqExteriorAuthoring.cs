using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>Add the entrance and estate to the saved HQ without regenerating its interior.</summary>
    public static class HqExteriorAuthoring
    {
        [MenuItem(ToolMenu.Hq + "Add Staff Door and Exterior", priority = ToolMenu.HqP + 20)]
        public static void AuthorExterior()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("The HQ entrance and exterior must be authored in Edit mode.");

            Scene scene = SceneManager.GetSceneByPath(HqBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(HqBuilder.ScenePath, OpenSceneMode.Additive);

            Transform hq = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "HQ") { hq = root.transform; break; }
            if (hq == null) throw new InvalidOperationException("The saved HQ scene has no HQ root.");

            // Use the current hierarchy, including any unsaved hand placements in an open HQ.
            HqStaffEntryDoor.Build(hq);
            HqExteriorBuilder.Build(hq);
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, HqBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the HQ entrance and exterior.");
            Debug.Log("[HQ] Closed staff entrance and surrounding industrial estate saved; interior retained.");
        }
    }
}

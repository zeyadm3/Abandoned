using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Repairs boxes an earlier architecture run left drawing nothing: their renderer was taken off, but the run's record
    /// was lost and its bank (with the drawings' meshes) was replaced, so the next run neither restored nor redrew them
    /// (found by the triage audit: 316 in the Mall's surrounding district on 2026-10-29). Each such box gets its renderer
    /// back with the orphan drawing's materials, the orphan goes, and the map's architecture and coplanar pass run again.
    /// </summary>
    public static class ArchOrphanRepair
    {
        [MenuItem(ToolMenu.Art + "Remake: Repair Lost Architecture Drawings in All Maps", priority = ToolMenu.ArtP + 110)]
        public static void RepairAllMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            RepairAll();
        }

        /// <summary>Batch: Tools/unity.sh exec Abandoned.EditorTools.Remake.ArchOrphanRepair.RepairAllAndExit</summary>
        public static void RepairAllAndExit()
        {
            RepairAll();
            EditorApplication.Exit(0);
        }

        public static void RepairAll()
        {
            foreach (string path in AvOverhaul.MapScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int fixedBoxes = Restore(scene);
                if (fixedBoxes == 0) continue;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[ArchRepair] {scene.name}: {fixedBoxes} box renderer(s) restored from lost drawings; redrawing.");
                Debug.Log(ArchRebuild.ApplyScene(path));
                AvOverhaul.MapPasses(path);
            }
            AssetDatabase.SaveAssets();
        }

        /// <summary>Gives each box under a meshless drawing its renderer back and removes the drawing.</summary>
        public static int Restore(Scene scene)
        {
            var orphans = new List<MeshFilter>();
            foreach (GameObject root in scene.GetRootGameObjects())
                orphans.AddRange(root.GetComponentsInChildren<MeshFilter>(true)
                    .Where(f => f.name == ArchRebuild.DrawingName && f.sharedMesh == null));
            int restored = 0;
            foreach (MeshFilter orphan in orphans)
            {
                Transform box = orphan.transform.parent;
                // Not ?.: a missing component is Unity's fake null.
                MeshFilter boxFilter = box != null ? box.GetComponent<MeshFilter>() : null;
                if (boxFilter == null || boxFilter.sharedMesh == null || box.GetComponent<MeshRenderer>() != null) continue;
                MeshRenderer drawn = orphan.GetComponent<MeshRenderer>();
                MeshRenderer renderer = box.gameObject.AddComponent<MeshRenderer>();
                if (drawn != null)
                {
                    renderer.sharedMaterials = drawn.sharedMaterials;
                    renderer.renderingLayerMask = drawn.renderingLayerMask;
                    renderer.shadowCastingMode = drawn.shadowCastingMode;
                    renderer.receiveShadows = drawn.receiveShadows;
                }
                Object.DestroyImmediate(orphan.gameObject);
                restored++;
            }
            return restored;
        }
    }
}

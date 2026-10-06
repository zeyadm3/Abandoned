using System.Reflection;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Writes NGO's ids into generated content. NGO fills them in NetworkObject.OnValidate, which our
    /// builders never trigger at the right moment: a scene-placed object only counts as in-scene placed
    /// (and gets its source prefab's hash) once its scene is saved and in Build Settings. Without that,
    /// a joining client keeps its own scene copies and also spawns the host's, so every item is doubled.
    /// </summary>
    public static class NetworkObjectIds
    {
        private static readonly MethodInfo Validate =
            typeof(NetworkObject).GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        /// <summary>Call after the scene is saved and listed in Build Settings; saves it again.</summary>
        public static int StampScene(Scene scene)
        {
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (NetworkObject no in root.GetComponentsInChildren<NetworkObject>(true))
            {
                if (no.GetComponent<NetworkManager>() != null) continue;
                Validate.Invoke(no, null);
                // Prefab instances keep these as overrides; record them so the save writes them.
                if (PrefabUtility.IsPartOfPrefabInstance(no)) PrefabUtility.RecordPrefabInstancePropertyModifications(no);
                count++;
            }
            if (count > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            return count;
        }

        /// <summary>Prefab assets get their hash on import; this also writes it to disk.</summary>
        public static void StampPrefab(GameObject prefab)
        {
            if (prefab == null || !prefab.TryGetComponent(out NetworkObject no)) return;
            Validate.Invoke(no, null);
            EditorUtility.SetDirty(no);
        }
    }
}

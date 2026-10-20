using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Undoes the 0.24.12 probe-volume experiment in a saved map: removes its probe volume and the baked lighting data,
    /// so the map lights from its own tuned ambient again. Baked bounce light made every interior wall black (the
    /// fixtures bounce far less than the hand-tuned flat ambient gave), so the game stays realtime-only (M7.2).
    /// </summary>
    public static class LightingBake
    {
        public const string Folder = "Assets/_Project/Art/Lighting/Probes";

        public static void Clear(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (ProbeVolume v in Object.FindObjectsByType<ProbeVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (v.gameObject.name == "_AV_ProbeVolume") Object.DestroyImmediate(v.gameObject);
            Lightmapping.Clear();
            Lightmapping.ClearLightingDataAsset();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Lighting] {scene.name}: baked probe data cleared.");
        }
    }
}

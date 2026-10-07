using Abandoned.Company;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Keeps hand placement in the generated HQ: move the GarageDoor in Scenes/HQ (and set its Open slider),
    /// save the scene, and its pose goes into Data/Company/HqPlacements, which every HQ rebuild reads back.
    /// </summary>
    [InitializeOnLoad]
    public static class HqPlacementSync
    {
        /// <summary>True while HqBuilder is generating the scene: its own save must not overwrite your placement.</summary>
        public static bool Building;

        static HqPlacementSync() => EditorSceneManager.sceneSaving += OnSceneSaving;

        public static HqPlacements Load() => SerializedWiring.LoadOrCreateAsset<HqPlacements>(HqPlacements.Path);

        private static void OnSceneSaving(Scene scene, string path)
        {
            if (Building || path != HqBuilder.ScenePath) return;
            Capture(scene);
        }

        [MenuItem("Tools/Abandoned/HQ/Save Garage Door Placement")]
        public static void SaveFromOpenScene()
        {
            Scene scene = SceneManager.GetSceneByPath(HqBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded) { Debug.LogWarning("[HQ] Open Scenes/HQ first."); return; }
            Capture(scene);
        }

        [MenuItem("Tools/Abandoned/HQ/Reset Garage Door Placement")]
        public static void ResetPlacement()
        {
            HqPlacements placements = Load();
            placements.ResetGarageDoor();
            EditorUtility.SetDirty(placements);
            AssetDatabase.SaveAssets();
            Debug.Log("[HQ] Garage door placement reset to the garage opening; it applies on the next HQ rebuild.");
        }

        private static void Capture(Scene scene)
        {
            GarageDoor door = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if ((door = root.GetComponentInChildren<GarageDoor>(true)) != null) break;
            if (door == null) return;
            HqPlacements placements = Load();
            if (!placements.SetGarageDoor(door.transform, door.Open)) return;
            EditorUtility.SetDirty(placements);
            AssetDatabase.SaveAssetIfDirty(placements);
            Debug.Log($"[HQ] Garage door placement saved: {door.transform.position}, yaw {door.transform.eulerAngles.y:0.#}, open {door.Open:P0}.");
        }
    }
}

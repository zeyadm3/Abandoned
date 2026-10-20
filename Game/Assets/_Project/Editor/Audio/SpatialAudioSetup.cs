using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Makes Steam Audio the project's spatializer (Project Settings > Audio), which is the only place a player build
    /// takes it from. Run by RenderPipelineSetup.Apply with the rest of the 0.24.12 project settings.
    /// </summary>
    public static class SpatialAudioSetup
    {
        private const string Wanted = "Steam Audio Spatializer";

        public static void Apply()
        {
            Object manager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset") is { Length: > 0 } a ? a[0] : null;
            if (manager == null) { Debug.LogWarning("[Audio] ProjectSettings/AudioManager.asset not found."); return; }
            bool known = false;
            foreach (string name in AudioSettings.GetSpatializerPluginNames()) if (name == Wanted) known = true;
            if (!known) { Debug.LogWarning($"[Audio] {Wanted} is not installed; the game keeps Unity's panning."); return; }
            var so = new SerializedObject(manager);
            SerializedProperty p = so.FindProperty("m_SpatializerPlugin");
            if (p == null) { Debug.LogWarning("[Audio] AudioManager has no m_SpatializerPlugin field."); return; }
            if (p.stringValue == Wanted) return;
            p.stringValue = Wanted;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Audio] Spatializer set to {Wanted}.");
        }
    }
}

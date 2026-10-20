using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Adds a renderer feature to a renderer asset the way URP's own inspector does (sub-asset + its id in the
    /// feature map), so a rebuild can install features without anyone clicking "Add Renderer Feature".
    /// </summary>
    public static class RendererFeatures
    {
        public static T Ensure<T>(ScriptableRendererData renderer) where T : ScriptableRendererFeature
        {
            T existing = renderer.rendererFeatures.OfType<T>().FirstOrDefault();
            if (existing != null) return existing;

            T feature = ScriptableObject.CreateInstance<T>();
            feature.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(feature, renderer);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out string _, out long localId);

            var so = new SerializedObject(renderer);
            SerializedProperty list = so.FindProperty("m_RendererFeatures");
            SerializedProperty map = so.FindProperty("m_RendererFeatureMap");
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = feature;
            map.arraySize++;
            map.GetArrayElementAtIndex(map.arraySize - 1).longValue = localId;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(renderer);
            return feature;
        }

        /// <summary>Sets serialised fields the feature keeps private (shaders, URP's internal settings).</summary>
        public static bool Set(Object target, string path, System.Action<SerializedProperty> write)
        {
            var so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(path);
            if (property == null)
            {
                Debug.LogWarning($"[Render] {target.name} has no field {path}; left as it was.");
                return false;
            }
            write(property);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
            return true;
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Sets [SerializeField] private fields from Editor builders, so runtime classes keep their
    /// fields private and builders fail loudly when a field is renamed.
    /// </summary>
    public static class SerializedWiring
    {
        public static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"{target.GetType().Name} has no serialized field '{field}'.");
                return;
            }
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static Object Get(Object target, string field) =>
            new SerializedObject(target).FindProperty(field)?.objectReferenceValue;

        /// <summary>Loads a ScriptableObject asset, creating it with defaults if it doesn't exist.</summary>
        public static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            string folder = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(folder))
                Debug.LogError($"Folder {folder} does not exist; cannot create {path}.");

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}

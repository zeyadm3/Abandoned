using UnityEditor;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    public static class MeridianMallOverhaul
    {
        [MenuItem("Tools/Abandoned/Mall/Rebuild Mall and Parking (0.15.0)")]
        public static void Author()
        {
            // Shared asset tuning belongs to the HQ and approved neighbourhood too. Rebuild scene content.
            string[] paths = AssetDatabase.FindAssets("t:Material", new[] { "Assets/_Project/Art" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(path => path.EndsWith(".mat")).Distinct().ToArray();
            var originals = new Material[paths.Length];
            var copies = new Material[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                originals[i] = AssetDatabase.LoadAssetAtPath<Material>(paths[i]);
                copies[i] = new Material(originals[i]) { name = originals[i].name, hideFlags = HideFlags.HideAndDontSave };
            }
            try { MallBuilder.Build(); }
            finally
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    // Opening the replacement scene unloads ordinary transient materials and old asset instances.
                    Material original = AssetDatabase.LoadAssetAtPath<Material>(paths[i]);
                    copies[i].hideFlags = HideFlags.None;
                    EditorUtility.CopySerialized(copies[i], original);
                    EditorUtility.SetDirty(original);
                    Object.DestroyImmediate(copies[i]);
                }
                AssetDatabase.SaveAssets();
            }
        }
    }
}

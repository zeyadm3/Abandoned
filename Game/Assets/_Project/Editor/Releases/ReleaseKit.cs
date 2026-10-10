using System;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What every 0.16.1+ version step shares: creating its tunables asset when missing, and running a step
    /// from batch mode (<c>Tools/unity.sh exec Abandoned.EditorTools.Release0161.AuthorAndExit</c>).
    /// </summary>
    public static class ReleaseKit
    {
        public const string ExtractionResources = "Assets/_Project/Data/Extraction/Resources";

        /// <summary>The config at <paramref name="folder"/>/<paramref name="name"/>.asset, made with code defaults when missing.</summary>
        public static T Config<T>(string folder, string name) where T : ScriptableObject
        {
            PolishAssets.EnsureFolder(folder);
            return SerializedWiring.LoadOrCreateAsset<T>($"{folder}/{name}.asset");
        }

        public static void RunAndExit(Action step)
        {
            try
            {
                step();
                AssetDatabase.SaveAssets();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}

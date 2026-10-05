using System.Linq;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Creates our physics layers in TagManager at fixed indices. Collision rules between them are
    /// applied at runtime by <see cref="GameLayers.ApplyCollisionRules"/>.
    /// </summary>
    public static class ProjectLayersSetup
    {
        private const int FirstLayerIndex = 8;

        [MenuItem("Tools/Abandoned/Setup Physics Layers")]
        public static void Apply()
        {
            Object tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset").FirstOrDefault();
            if (tagManager == null)
            {
                Debug.LogError("TagManager.asset not found.");
                return;
            }

            var so = new SerializedObject(tagManager);
            SerializedProperty layers = so.FindProperty("layers");
            for (int i = 0; i < GameLayers.All.Length; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(FirstLayerIndex + i);
                string wanted = GameLayers.All[i];
                if (slot.stringValue == wanted) continue;
                if (!string.IsNullOrEmpty(slot.stringValue))
                {
                    Debug.LogError($"Layer {FirstLayerIndex + i} is '{slot.stringValue}', expected '{wanted}'. Fix by hand.");
                    continue;
                }
                slot.stringValue = wanted;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        public static bool AllPresent() => GameLayers.All.All(n => LayerMask.NameToLayer(n) >= 0);
    }
}

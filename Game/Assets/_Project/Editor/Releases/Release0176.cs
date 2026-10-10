using Abandoned.Equipment;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>0.17.6 spray chalk: the catalog item and icon, the marks' unlit material, RunChalk on the RunState prefab.</summary>
    public static class Release0176
    {
        public const string MaterialPath = "Assets/_Project/Art/Greybox/ChalkMarks.mat";

        [MenuItem("Tools/Abandoned/Content/Spray Chalk (0.17.6)")]
        public static void Author()
        {
            var config = ReleaseKit.Config<ToolConfig>(ReleaseKit.EquipmentResources, ToolConfig.ResourcePath);
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                // Particles/Unlit multiplies in the vertex colour: each crew seat's dots keep their own colour.
                material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")) { name = "ChalkMarks" };
                material.SetColor("_BaseColor", Color.white);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            config.EditorSetChalkMaterial(material);
            EditorUtility.SetDirty(config);
            NetworkContentBuilder.CreateRunStatePrefab();
            ReleaseKit.Gear();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}

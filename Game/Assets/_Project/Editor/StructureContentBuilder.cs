using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>Creates the structure configs, dust material and fractured tile if they don't exist yet.</summary>
    public static class StructureContentBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Structure/StructureConfig.asset";
        public const string VisualConfigPath = "Assets/_Project/Data/Structure/StructureVisualConfig.asset";
        public const string DustMaterialPath = GreyboxFactory.MaterialFolder + "/Greybox_Dust.mat";

        private const string ParticleShader = "Universal Render Pipeline/Particles/Unlit";

        [MenuItem("Tools/Abandoned/Create Structure Content")]
        public static void CreateMissing()
        {
            LoadOrCreateAsset<StructureConfig>(ConfigPath);
            var visuals = LoadOrCreateAsset<StructureVisualConfig>(VisualConfigPath);
            if (visuals.DustMaterial == null) Set(visuals, "<DustMaterial>k__BackingField", DustMaterial());
            if (AssetDatabase.LoadAssetAtPath<GameObject>(FracturedTileGenerator.PrefabPath) == null)
                FracturedTileGenerator.Generate();
            AssetDatabase.SaveAssets();
        }

        private static Material DustMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(DustMaterialPath);
            if (material != null) return material;
            Shader shader = Shader.Find(ParticleShader);
            if (shader == null)
            {
                Debug.LogError($"Shader '{ParticleShader}' not found.");
                return null;
            }
            material = new Material(shader);
            material.SetColor("_BaseColor", new Color(0.78f, 0.72f, 0.64f, 1f));
            AssetDatabase.CreateAsset(material, DustMaterialPath);
            return material;
        }
    }
}

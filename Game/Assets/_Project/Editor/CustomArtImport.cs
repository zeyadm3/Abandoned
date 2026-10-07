using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Import original environment FBX as visual geometry; builders author gameplay collisions.</summary>
    public sealed class CustomArtImport : AssetPostprocessor
    {
        public override uint GetVersion() => 2;
        private bool IsEnvironment => assetPath.StartsWith(CustomMallArt.Folder + "/");

        private void OnPreprocessModel()
        {
            if (!IsEnvironment) return;
            var model = (ModelImporter)assetImporter;
            model.globalScale = 1f;
            model.useFileScale = true;
            model.bakeAxisConversion = true;
            model.importAnimation = false;
            model.animationType = ModelImporterAnimationType.None;
            model.importCameras = false;
            model.importLights = false;
            model.importBlendShapes = false;
            model.addCollider = false;
            model.isReadable = false;
            model.importNormals = ModelImporterNormals.Import;
            model.importTangents = ModelImporterTangents.CalculateMikk;
            model.meshCompression = ModelImporterMeshCompression.Low;
            model.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            model.materialLocation = ModelImporterMaterialLocation.InPrefab;
        }

        private void OnPostprocessMaterial(Material material)
        {
            if (IsEnvironment && material.name.StartsWith("ENV_")) CustomMallArt.Configure(material, material.name);
        }

        private Material OnAssignMaterialModel(Material source, Renderer renderer)
        {
            if (!IsEnvironment || source == null || !source.name.StartsWith("ENV_")) return null;
            return AssetDatabase.LoadAssetAtPath<Material>($"{CustomMallArt.Folder}/Materials/{source.name}.mat");
        }

        private void OnPreprocessTexture()
        {
            if (!IsEnvironment) return;
            var texture = (TextureImporter)assetImporter;
            texture.textureType = TextureImporterType.Default;
            texture.sRGBTexture = true;
            texture.mipmapEnabled = true;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            texture.maxTextureSize = 256;
        }
    }
}

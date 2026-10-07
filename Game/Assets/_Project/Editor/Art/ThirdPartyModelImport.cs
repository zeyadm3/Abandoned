using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Import settings for third-party CC0 models (Art/ThirdParty): meshes and their own materials
    /// only. No animation, cameras, lights or colliders (gameplay colliders come from our own boxes),
    /// and not readable (saves memory).
    /// </summary>
    public class ThirdPartyModelImport : AssetPostprocessor
    {
        public const string Root = "Assets/_Project/Art/ThirdParty/";

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Root)) return;
            var importer = (ModelImporter)assetImporter;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.addCollider = false;
            importer.isReadable = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Import settings for everything under Art/Remake (Tools/Blender/remake exports). Models are visual only: builders
    /// author colliders and gameplay. Skinned areas keep their armature (Generic rig, optimised transforms off, so
    /// builders can bind bones to WorkerRig joints or threat rigs); threats also import their takes.
    /// Materials named "RM_&lt;set&gt;" map to RemakeMaterials; textures are typed by their suffix.
    /// </summary>
    public sealed class RemakeImport : AssetPostprocessor
    {
        public override uint GetVersion() => 1;

        private void OnPreprocessModel()
        {
            if (!RemakePaths.IsRemake(assetPath)) return;
            var model = (ModelImporter)assetImporter;
            model.globalScale = 1f;
            model.useFileScale = true;
            model.bakeAxisConversion = true;
            model.importCameras = false;
            model.importLights = false;
            model.importVisibility = false;
            model.addCollider = false;
            model.isReadable = RemakePaths.NeedsReadable(assetPath);
            model.importNormals = ModelImporterNormals.Import;
            model.importTangents = ModelImporterTangents.CalculateMikk;
            model.meshCompression = ModelImporterMeshCompression.Off;
            model.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            model.materialLocation = ModelImporterMaterialLocation.InPrefab;
            model.sortHierarchyByName = false;
            bool skinned = RemakePaths.IsSkinned(assetPath);
            model.importBlendShapes = skinned;
            model.animationType = skinned ? ModelImporterAnimationType.Generic : ModelImporterAnimationType.None;
            model.optimizeGameObjects = false;
            model.importAnimation = skinned && RemakePaths.IsAnimated(assetPath);
            model.skinWeights = ModelImporterSkinWeights.Standard;
        }

        private Material OnAssignMaterialModel(Material source, Renderer renderer)
        {
            if (!RemakePaths.IsRemake(assetPath) || source == null) return null;
            string name = source.name;
            if (!name.StartsWith("RM_")) return null;
            return RemakeMaterials.GetOrCreate(name.Substring(3), RemakePaths.AreaFolder(assetPath));
        }

        private void OnPreprocessTexture()
        {
            if (!RemakePaths.IsRemake(assetPath)) return;
            var texture = (TextureImporter)assetImporter;
            string file = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            texture.mipmapEnabled = true;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 4;
            texture.textureCompression = TextureImporterCompression.CompressedHQ;
            texture.maxTextureSize = 2048;
            if (assetPath.StartsWith(RemakePaths.Icons + "/"))
            {
                texture.textureType = TextureImporterType.Sprite;
                texture.alphaIsTransparency = true;
                texture.mipmapEnabled = false;
                texture.wrapMode = TextureWrapMode.Clamp;
                return;
            }
            if (file.EndsWith("_Normal"))
            {
                texture.textureType = TextureImporterType.NormalMap;
                texture.sRGBTexture = false;
            }
            else if (file.EndsWith("_Mask"))
            {
                texture.textureType = TextureImporterType.Default;
                texture.sRGBTexture = false;
                texture.alphaSource = TextureImporterAlphaSource.FromInput;
            }
            else
            {
                texture.textureType = TextureImporterType.Default;
                texture.sRGBTexture = true;
                texture.alphaSource = file.Contains("Hair") || file.Contains("Decal")
                    ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            }
        }

        private void OnPostprocessTexture(Texture2D texture)
        {
            if (!RemakePaths.IsRemake(assetPath)) return;
            string file = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            int cut = file.LastIndexOf('_');
            if (cut <= 0) return;
            string set = file.Substring(0, cut);
            string folder = System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/');
            if (!folder.EndsWith("/Textures")) return;
            string area = folder.Substring(0, folder.Length - "/Textures".Length);
            // Textures can arrive after the model that uses them: top up an existing material on the next tick.
            EditorApplication.delayCall += () =>
            {
                foreach (string name in new[] { set, set + "_Tint" })
                {
                    var m = AssetDatabase.LoadAssetAtPath<Material>($"{area}/Materials/{name}.mat");
                    if (m != null) RemakeMaterials.Fill(m, set, folder);
                }
            };
        }
    }
}

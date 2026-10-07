using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Custom Blender rigs use Generic animation: no retargeting can normalize their wrong proportions.</summary>
    public class ThreatModelImporter : AssetPostprocessor
    {
        public override uint GetVersion() => 2;
        private bool IsThreat => assetPath.StartsWith("Assets/_Project/Art/Custom/Threats/", StringComparison.Ordinal) && assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);
        private void OnPreprocessModel()
        {
            if (!IsThreat) return;
            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.bakeAxisConversion = true;
            importer.preserveHierarchy = true;
            importer.optimizeGameObjects = false;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            importer.importCameras = importer.importLights = false;
            importer.isReadable = false;
            importer.addCollider = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        }
        private Material OnAssignMaterialModel(Material source, Renderer renderer)
        {
            if (!IsThreat || source == null || !source.name.StartsWith("THREAT_", StringComparison.Ordinal)) return null;
            return AssetDatabase.LoadAssetAtPath<Material>(ThreatRigBuilder.MaterialPath(source.name));
        }
        private void OnPostprocessMaterial(Material material)
        {
            if (IsThreat && material.name.StartsWith("THREAT_", StringComparison.Ordinal))
                ThreatRigBuilder.ConfigureMaterial(material, material.name);
        }
        private void OnPreprocessTexture()
        {
            if (assetPath != ThreatRigBuilder.ArtFolder + "/ThreatGrime.png") return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 256;
        }
        private void OnPreprocessAnimation()
        {
            if (!IsThreat) return;
            var importer = (ModelImporter)assetImporter;
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            foreach (ModelImporterClipAnimation clip in clips)
            {
                string name = clip.name.Split('|').Last();
                foreach (string candidate in Enum.GetNames(typeof(Abandoned.Threats.ThreatMotion)))
                    if (name.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0) { name = candidate; break; }
                clip.name = name;
                clip.loopTime = clip.loopPose = name != "Attack";
                clip.lockRootPositionXZ = clip.lockRootRotation = true;
                clip.keepOriginalPositionXZ = clip.keepOriginalOrientation = true;
            }
            importer.clipAnimations = clips;
        }
    }
}

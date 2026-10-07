using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Custom Blender rigs use Generic animation: no retargeting can normalize their wrong proportions.</summary>
    public class ThreatModelImporter : AssetPostprocessor
    {
        private bool IsThreat => assetPath.StartsWith("Assets/_Project/Art/Custom/Threats/", StringComparison.Ordinal) && assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);
        private void OnPreprocessModel()
        {
            if (!IsThreat) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            importer.importCameras = importer.importLights = false;
            importer.isReadable = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
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

using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Configures rendering reach, camera distance (LevelCameraRange 5000m),
    /// and occlusion culling tagging for Pine Hollow Middle School.
    /// Ensures distant Blue Ridge mountains and winter surroundings are always visible without pop-in.
    /// </summary>
    public static class BurleySchoolRendering
    {
        public const float FarClip = 5000f;

        public static void Apply(Transform root, BurleySchoolArt art)
        {
            // 1. Scene-Owned Camera Reach (LevelCameraRange)
            var reach = root.GetComponent<LevelCameraRange>();
            if (reach == null) reach = root.gameObject.AddComponent<LevelCameraRange>();
            reach.EditorSetup(FarClip);

            foreach (GameObject sceneRoot in root.gameObject.scene.GetRootGameObjects())
            foreach (Camera camera in sceneRoot.GetComponentsInChildren<Camera>(true))
                if (camera.CompareTag("MainCamera")) camera.farClipPlane = FarClip;

            // 2. Static Occlusion Culling Flags
            var opaqueMass = new HashSet<Material>
            {
                art.Brick, art.BrickDark, art.Limestone, art.Concrete, art.PlasterWall,
                art.FloorTile, art.Terrazzo, art.GymWood, art.StageWood, art.OakWood,
                art.AcousticCeiling, art.RoofGravel, art.Snow, art.SnowDrift
            };

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                GameObject go = renderer.gameObject;
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
                bool farScenery = Under(renderer.transform, root, "BlueRidgeMountainPanorama") ||
                                  Under(renderer.transform, root, "CampusSurroundings");
                SectionProp sectionProp = renderer.GetComponentInParent<SectionProp>();
                bool section = renderer.GetComponentInParent<StructuralSection>() != null ||
                               sectionProp != null && sectionProp.Section != null;
                bool moving = renderer.GetComponentInParent<LightFixture>() != null ||
                              renderer.GetComponentInParent<Abandoned.Extraction.HidingLocker>() != null ||
                              renderer.GetComponentInParent<ParticleSystem>() != null;
                bool decorative = Under(renderer.transform, root, "WinterAccents") ||
                                  Under(renderer.transform, root, "WinterFoliage");

                bool opaque = true;
                foreach (Material material in renderer.sharedMaterials)
                    opaque &= material != null && opaqueMass.Contains(material) && material.renderQueue < (int)RenderQueue.AlphaTest;

                Vector3 size = renderer.bounds.size;
                int broadAxes = (size.x >= 1.4f ? 1 : 0) + (size.y >= 1.4f ? 1 : 0) + (size.z >= 1.4f ? 1 : 0);
                bool substantial = broadAxes >= 2 && Mathf.Min(size.x, Mathf.Min(size.y, size.z)) >= 0.08f;

                if (farScenery || section || moving || decorative || !opaque || !substantial)
                    flags &= ~StaticEditorFlags.OccluderStatic;
                if (farScenery || section || moving)
                    flags &= ~StaticEditorFlags.OccludeeStatic;
                if (section || moving)
                    flags &= ~(StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.ReflectionProbeStatic);

                GameObjectUtility.SetStaticEditorFlags(go, flags);

                // Distant scenery must never vanish when moving between rooms
                if (farScenery) renderer.allowOcclusionWhenDynamic = false;
                renderer.reflectionProbeUsage = farScenery ? ReflectionProbeUsage.Off : ReflectionProbeUsage.BlendProbes;
            }
        }

        private static bool Under(Transform item, Transform root, string group)
        {
            for (Transform t = item; t != null && t != root; t = t.parent)
                if (t.name == group) return true;
            return false;
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Keep the playable island in the culling bake and the open bay visible to every main-camera lens.</summary>
    public static class IslandPrisonRendering
    {
        public const float FarClip = 5000f;

        public static void Apply(Transform root, IslandPrisonArt art)
        {
            var reach = root.GetComponent<LevelCameraRange>();
            if (reach == null) reach = root.gameObject.AddComponent<LevelCameraRange>();
            reach.EditorSetup(FarClip);
            foreach (GameObject sceneRoot in root.gameObject.scene.GetRootGameObjects())
            foreach (Camera camera in sceneRoot.GetComponentsInChildren<Camera>(true))
                if (camera.CompareTag("MainCamera")) camera.farClipPlane = FarClip;

            var opaqueMass = new HashSet<Material>
            {
                art.Facade, art.FacadeDark, art.Plaster, art.Dado, art.Ceiling, art.Floor, art.Terrazzo,
                art.Tile, art.Block, art.Brick, art.BurntBrick, art.Rock, art.RockDark, art.Ground, art.Grass, art.Sand,
            };
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                GameObject go = renderer.gameObject;
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
                bool farScenery = Under(renderer.transform, root, IslandBackdrop.GroupName) || Under(renderer.transform, root, "Sea");
                SectionProp sectionProp = renderer.GetComponentInParent<SectionProp>();
                bool section = renderer.GetComponentInParent<StructuralSection>() != null ||
                    sectionProp != null && sectionProp.Section != null;
                bool moving = renderer.GetComponentInParent<LightFixture>() != null ||
                    renderer.GetComponentInParent<Abandoned.Extraction.HidingLocker>() != null ||
                    renderer.GetComponentInParent<IdleSway>() != null || renderer.GetComponentInParent<SwingingLamp>() != null;
                bool decorative = Under(renderer.transform, root, "Decay") || Under(renderer.transform, root, IslandVegetation.GroupName);
                bool opaque = true;
                foreach (Material material in renderer.sharedMaterials)
                    opaque &= material != null && opaqueMass.Contains(material) && material.renderQueue < (int)RenderQueue.AlphaTest;
                Vector3 size = renderer.bounds.size;
                // Wall thickness may be only 14 cm. Require two broad axes so a bar or cornice is never an occluder.
                int broadAxes = (size.x >= 1.4f ? 1 : 0) + (size.y >= 1.4f ? 1 : 0) + (size.z >= 1.4f ? 1 : 0);
                bool substantial = broadAxes >= 2 && Mathf.Min(size.x, Mathf.Min(size.y, size.z)) >= .08f;
                if (farScenery || section || moving || decorative || !opaque || !substantial)
                    flags &= ~StaticEditorFlags.OccluderStatic;
                if (farScenery || section || moving) flags &= ~StaticEditorFlags.OccludeeStatic;
                if (section || moving) flags &= ~(StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.ReflectionProbeStatic);
                GameObjectUtility.SetStaticEditorFlags(go, flags);
                // A 6 km water slab or 1.4 km bridge must not inflate Umbra's cells or vanish with its island chunk.
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

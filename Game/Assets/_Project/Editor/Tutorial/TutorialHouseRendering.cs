using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Render reach and static flags for the training house: the shell, rooms and neighbourhood batch and occlude;
    /// floors, things that fall with them, doors, closets, lights and particles never do (they move or vanish).
    /// Only big opaque pieces occlude; the town round the lot is never in the occlusion bake.
    /// </summary>
    public static class TutorialHouseRendering
    {
        public const float FarClip = 1200f;

        private static readonly string[] StaticGroups = { "Walls", "Roof", "Ceilings", "Rooms", "BasementDressing", "Basement", "Bulkhead", "Grounds", "Porch", "Yard", "Neighbourhood" };

        public static void Apply(Transform root, TutorialHouseArt art)
        {
            var reach = root.GetComponent<LevelCameraRange>() ?? root.gameObject.AddComponent<LevelCameraRange>();
            reach.EditorSetup(FarClip);
            foreach (GameObject sceneRoot in root.gameObject.scene.GetRootGameObjects())
            foreach (Camera camera in sceneRoot.GetComponentsInChildren<Camera>(true))
                if (camera.CompareTag("MainCamera")) camera.farClipPlane = FarClip;

            foreach (string name in StaticGroups)
            {
                Transform group = root.Find(name);
                if (group != null) GreyboxFactory.MarkStatic(group);
            }

            var occluders = new HashSet<Material> { art.Plaster, art.Siding, art.BasementWall, art.Concrete, art.Snow, art.Brick, art.Ceiling };
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                GameObject go = renderer.gameObject;
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
                bool moving = renderer.GetComponentInParent<StructuralSection>() != null
                              || renderer.GetComponentInParent<SectionProp>() != null
                              || renderer.GetComponentInParent<LightFixture>() != null
                              || renderer.GetComponentInParent<Extraction.SlidingDoor>() != null
                              || renderer.GetComponentInParent<Extraction.HidingLocker>() != null
                              || renderer.GetComponentInParent<Extraction.BreakerPanel>() != null
                              || renderer.GetComponentInParent<ParticleSystem>() != null;
                // The town is big and mostly in the fog: batched, never part of the occlusion bake (it would stretch
                // the bake over the whole world for nothing).
                bool far = Under(renderer.transform, root, "Town");
                bool opaque = true;
                foreach (Material m in renderer.sharedMaterials)
                    opaque &= m != null && occluders.Contains(m) && m.renderQueue < (int)RenderQueue.AlphaTest;
                Vector3 size = renderer.bounds.size;
                int broad = (size.x >= 1.2f ? 1 : 0) + (size.y >= 1.2f ? 1 : 0) + (size.z >= 1.2f ? 1 : 0);
                if (moving)
                {
                    flags = 0;
                }
                else
                {
                    if (!opaque || broad < 2 || far) flags &= ~StaticEditorFlags.OccluderStatic;
                    if (far) flags &= ~StaticEditorFlags.OccludeeStatic;
                    // The street's copies of the house all draw the same saved meshes: static batching would bake a
                    // copy of each into the scene per house (a hundred and more), so they stay as they are and the
                    // SRP batcher draws them.
                    if (far && renderer.GetComponentInParent<LODGroup>() != null) flags &= ~StaticEditorFlags.BatchingStatic;
                }
                GameObjectUtility.SetStaticEditorFlags(go, flags);
                if (far) renderer.allowOcclusionWhenDynamic = false;
                renderer.reflectionProbeUsage = far ? ReflectionProbeUsage.Off : ReflectionProbeUsage.BlendProbes;
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

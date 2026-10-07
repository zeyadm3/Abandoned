using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Bakes occlusion culling for a level (M7.6). Only geometry that never breaks occludes: walls,
    /// columns, roof, exterior (OccluderStatic via GreyboxFactory.MarkStatic). Floor tiles carry no
    /// static flags, so a collapsed floor never hides what's visible through its hole.
    /// </summary>
    public static class OcclusionBake
    {
        /// <summary>Bakes the open (saved) scene; the data asset lands in a folder named after it.</summary>
        public static bool Bake()
        {
            StaticOcclusionCulling.smallestOccluder = 1.5f;
            StaticOcclusionCulling.smallestHole = 0.25f;
            StaticOcclusionCulling.backfaceThreshold = 100f;
            bool baked = StaticOcclusionCulling.Compute();
            if (!baked) Debug.LogError("[Occlusion] Bake failed.");
            return baked;
        }

        [MenuItem("Tools/Abandoned/Art/Bake Occlusion (open scene)")]
        private static void BakeOpenScene() => Debug.Log($"[Occlusion] {(Bake() ? "baked" : "failed")}.");
    }
}

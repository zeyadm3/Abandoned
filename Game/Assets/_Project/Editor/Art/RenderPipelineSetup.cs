using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Render pipeline budget (M7.6), applied by the rebuild so it can't drift: the sun's shadows only
    /// matter near windows and the skylight indoors, so two cascades out to 35 m instead of four to
    /// 50 m (each cascade redraws every caster). Shadowed spot lights (flashlights) stay on.
    /// </summary>
    public static class RenderPipelineSetup
    {
        public const float ShadowDistance = 35f;
        public const int Cascades = 2;
        public const string PcAssetPath = "Assets/Settings/PC_RPAsset.asset";

        [MenuItem("Tools/Abandoned/Art/Apply Render Budget")]
        public static void Apply()
        {
            // The PC quality level's asset (quality levels override the default pipeline).
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PcAssetPath);
            if (urp == null)
            {
                Debug.LogError($"[Render] No URP asset at {PcAssetPath}.");
                return;
            }
            urp.shadowDistance = ShadowDistance;
            urp.shadowCascadeCount = Cascades;
            urp.cascade2Split = 0.3f;
            EditorUtility.SetDirty(urp);
            AssetDatabase.SaveAssets();
        }
    }
}

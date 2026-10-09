using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Render pipeline budget (M7.6), applied by the rebuild so it can't drift: the sun's shadows only
    /// matter near windows and the skylight indoors, so two cascades out to 35 m instead of four to
    /// 50 m (each cascade redraws every caster). Since 0.14.0 the shadowed local lights are the player's
    /// flashlight plus the ceiling lights nearest the camera (Structure.ShadowDirector), so the atlas they
    /// share is larger, and ambient occlusion is a little stronger so things under lights that cast no
    /// shadows still sit on the floor. Also makes Data/Core/Resources/ShadowBudgetConfig.
    /// </summary>
    public static class RenderPipelineSetup
    {
        public const float ShadowDistance = 35f;
        public const int Cascades = 2;
        // Sixteen 1024 px maps: the flashlight and up to six ceiling lights at full size, with room to spare.
        public const int LocalShadowAtlas = 4096;
        public const string PcAssetPath = "Assets/Settings/PC_RPAsset.asset";
        public const string PcRendererPath = "Assets/Settings/PC_Renderer.asset";
        public const string BudgetPath = "Assets/_Project/Data/Core/Resources/ShadowBudgetConfig.asset";

        // Ambient occlusion: reach, darkness, and how much it bites into direct light (it was 0.3 / 0.4 / 0.25).
        private const float AoRadius = 0.4f, AoIntensity = 0.55f, AoDirect = 0.5f;

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
            urp.additionalLightsShadowmapResolution = LocalShadowAtlas;
            EditorUtility.SetDirty(urp);
            AmbientOcclusion();
            if (AssetDatabase.LoadAssetAtPath<Abandoned.Core.ShadowBudgetConfig>(BudgetPath) == null)
            {
                PolishAssets.EnsureFolder("Assets/_Project/Data/Core/Resources");
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<Abandoned.Core.ShadowBudgetConfig>(), BudgetPath);
            }
            AssetDatabase.SaveAssets();
        }

        // The renderer feature's settings aren't public, so they are set through its serialised fields.
        private static void AmbientOcclusion()
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(PcRendererPath))
            {
                if (asset == null || asset.name != "ScreenSpaceAmbientOcclusion") continue;
                var feature = new SerializedObject(asset);
                bool set = Set(feature, "m_Settings.Radius", AoRadius) & Set(feature, "m_Settings.Intensity", AoIntensity)
                           & Set(feature, "m_Settings.DirectLightingStrength", AoDirect);
                feature.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                if (!set) Debug.LogWarning("[Render] Ambient occlusion settings not found on the PC renderer; left as they were.");
                return;
            }
            Debug.LogWarning($"[Render] No ambient occlusion feature on {PcRendererPath}; left as it was.");
        }

        private static bool Set(SerializedObject target, string path, float value)
        {
            SerializedProperty property = target.FindProperty(path);
            if (property == null) return false;
            property.floatValue = value;
            return true;
        }
    }
}

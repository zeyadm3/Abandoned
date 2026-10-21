using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The PC renderer, applied by the rebuild so it can't drift. Since 0.24.12: Forward+ (fixture-dense rooms are
    /// no longer cut to four lights per object), HDR colour grading, two sun cascades out to 35 m (M7.6)
    /// (VideoApplier lowers it per the Shadows setting), and three renderer features: URP decals, VolumetricFogFeature
    /// and AutoExposureFeature.
    /// Since 0.14.0 the shadowed local lights are the flashlight plus the ceiling lights nearest the camera
    /// (Structure.ShadowDirector), so the atlas they share is large. Also makes ShadowBudgetConfig.
    /// </summary>
    public static class RenderPipelineSetup
    {
        // M7.6's budget stands: every cascade redraws every caster, and 4 cascades to 50 m put the mall's concourse over
        // its draw budget (MallRenderBudgetTests) for sun shadows that only matter near windows.
        public const float ShadowDistance = 35f;
        public const int Cascades = 2;
        // Sixteen 1024 px maps. A spot light is one and a point light six, so the flashlight, two point lights
        // and three spots fill it: ShadowDirector counts maps, not lights, to stay inside.
        public const int LocalShadowAtlas = 4096;
        public const string PcAssetPath = "Assets/Settings/PC_RPAsset.asset";
        public const string PcRendererPath = "Assets/Settings/PC_Renderer.asset";
        public const string BudgetPath = "Assets/_Project/Data/Core/Resources/ShadowBudgetConfig.asset";
        public const string FogShaderPath = "Assets/_Project/Art/Shaders/VolumetricFog.shader";
        public const string ExposureShaderPath = "Assets/_Project/Art/Shaders/AutoExposure.shader";

        // Ambient occlusion: reach, darkness, and how much it bites into direct light. 0.24.13: twelve samples at full
        // resolution and a little more reach, so the foot of every wall, shelf and crate sits in its own contact shadow.
        private const float AoRadius = 0.6f, AoIntensity = 0.8f, AoDirect = 0.35f;
        private const int AoSamplesHigh = 0;
        // 0.24.13: URP's own bias is 1/1; 0.1 drew shadow acne (fine stripes that crawl as the flashlight moves).
        // Half keeps contact shadows tight without the acne.
        private const float ShadowDepthBias = 0.5f, ShadowNormalBias = 0.5f;
        // Decals fade out beyond this; grime twenty metres away is never read and still costs.
        private const float DecalDistance = 40f;

        [MenuItem(ToolMenu.Art + "Apply Render Budget", priority = ToolMenu.ArtP)]
        public static void Apply()
        {
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PcAssetPath);
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(PcRendererPath);
            if (urp == null || renderer == null)
            {
                Debug.LogError($"[Render] No URP asset at {PcAssetPath} or renderer at {PcRendererPath}.");
                return;
            }
            Pipeline(urp);
            renderer.renderingMode = RenderingMode.ForwardPlus;
            EditorUtility.SetDirty(renderer);
            AmbientOcclusion();
            Features(renderer);
            DefaultVolume();
            SpatialAudioSetup.Apply();
            if (AssetDatabase.LoadAssetAtPath<ShadowBudgetConfig>(BudgetPath) == null)
            {
                PolishAssets.EnsureFolder("Assets/_Project/Data/Core/Resources");
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ShadowBudgetConfig>(), BudgetPath);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Render] PC pipeline: Forward+, HDR grading, 2 cascades / 35 m, decals, volumetric fog, eye adaptation.");
        }

        private static void Pipeline(UniversalRenderPipelineAsset urp)
        {
            urp.supportsHDR = true;
            urp.colorGradingMode = ColorGradingMode.HighDynamicRange;
            urp.colorGradingLutSize = 32;
            urp.shadowDistance = ShadowDistance;
            urp.shadowCascadeCount = Cascades;
            urp.cascade2Split = 0.3f;
            urp.mainLightShadowmapResolution = 4096;
            urp.additionalLightsShadowmapResolution = LocalShadowAtlas;
            RendererFeatures.Set(urp, "m_SoftShadowsSupported", p => p.boolValue = true);
            RendererFeatures.Set(urp, "m_SoftShadowQuality", p => p.intValue = (int)SoftShadowQuality.High);
            RendererFeatures.Set(urp, "m_ShadowDepthBias", p => p.floatValue = ShadowDepthBias);
            RendererFeatures.Set(urp, "m_ShadowNormalBias", p => p.floatValue = ShadowNormalBias);
            // Room reflections (RoomReflections): blended, box-projected, through Forward+'s probe atlas.
            RendererFeatures.Set(urp, "m_ReflectionProbeBlending", p => p.boolValue = true);
            RendererFeatures.Set(urp, "m_ReflectionProbeBoxProjection", p => p.boolValue = true);
            RendererFeatures.Set(urp, "m_ReflectionProbeAtlas", p => p.boolValue = true);
            // 0.24.12 tried baked probe volumes for bounce light: they replaced each map's tuned flat ambient with the
            // little a dim fixture bounces, and every wall went black. Legacy probes (the scene's ambient) stay.
            RendererFeatures.Set(urp, "m_LightProbeSystem", p => p.intValue = (int)LightProbeSystem.LegacyLightProbes);
            RendererFeatures.Set(urp, "m_SupportProbeVolumeScenarios", p => p.boolValue = false);
            RendererFeatures.Set(urp, "m_SupportProbeVolumeScenarioBlending", p => p.boolValue = false);
            EditorUtility.SetDirty(urp);
        }

        private static void Features(UniversalRendererData renderer)
        {
            var decals = RendererFeatures.Ensure<DecalRendererFeature>(renderer);
            RendererFeatures.Set(decals, "m_Settings.maxDrawDistance", p => p.floatValue = DecalDistance);
            // Screen space, not DBuffer: the wear and crack decals are colour only, and DBuffer's extra depth-normals
            // pass put the mall's concourse over its draw budget (MallRenderBudgetTests).
            RendererFeatures.Set(decals, "m_Settings.technique", p => p.enumValueIndex = 2);

            var fog = RendererFeatures.Ensure<VolumetricFogFeature>(renderer);
            RendererFeatures.Set(fog, "shader", p => p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Shader>(FogShaderPath));

            var exposure = RendererFeatures.Ensure<AutoExposureFeature>(renderer);
            RendererFeatures.Set(exposure, "shader", p => p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Shader>(ExposureShaderPath));
        }

        // Unity 6 builds every volume stack from the types in the pipeline's default profile: a component missing there
        // is never blended, whatever a map's profile says. Ours go in inert (no fog, no adaptation); maps switch them on.
        public const string DefaultVolumePath = "Assets/Settings/DefaultVolumeProfile.asset";

        private static void DefaultVolume()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(DefaultVolumePath);
            if (profile == null)
            {
                Debug.LogError($"[Render] No default volume profile at {DefaultVolumePath}; volumetric fog and eye adaptation will not run.");
                return;
            }
            Add<VolumetricFogOverride>(profile);
            Add<AutoExposureOverride>(profile);
            EditorUtility.SetDirty(profile);
        }

        private static void Add<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.Has<T>()) return;
            T component = profile.Add<T>();
            component.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(component, profile);
        }

        // The SSAO feature's settings aren't public, so they are set through its serialised fields.
        private static void AmbientOcclusion()
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(PcRendererPath))
            {
                if (asset == null || asset.name != "ScreenSpaceAmbientOcclusion") continue;
                RendererFeatures.Set(asset, "m_Settings.Radius", p => p.floatValue = AoRadius);
                RendererFeatures.Set(asset, "m_Settings.Intensity", p => p.floatValue = AoIntensity);
                RendererFeatures.Set(asset, "m_Settings.DirectLightingStrength", p => p.floatValue = AoDirect);
                RendererFeatures.Set(asset, "m_Settings.Samples", p => p.enumValueIndex = AoSamplesHigh);
                RendererFeatures.Set(asset, "m_Settings.Downsample", p => p.boolValue = false);
                RendererFeatures.Set(asset, "m_Settings.BlurQuality", p => p.enumValueIndex = 0); // bilateral: keeps edges
                return;
            }
            Debug.LogWarning($"[Render] No ambient occlusion feature on {PcRendererPath}; left as it was.");
        }
    }
}

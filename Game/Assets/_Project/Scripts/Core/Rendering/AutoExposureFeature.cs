using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Core
{
    /// <summary>
    /// Eye adaptation on the PC renderer (0.24.12), added by EditorTools.RenderPipelineSetup. Game cameras only,
    /// where the volume enables <see cref="AutoExposureOverride"/> and the player's Eye adaptation setting is on.
    /// It runs before bloom and tonemapping, so a bright window still blooms after the eye has closed down.
    /// </summary>
    public class AutoExposureFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader shader;
        private Material material;
        private AutoExposurePass pass;

        /// <summary>F1 overlay: read the exposure back now and then.</summary>
        public static bool WantShownStops { get; set; }

        public override void Create()
        {
            pass = new AutoExposurePass { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing, requiresIntermediateTexture = true };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (shader == null || !VideoSettings.EyeAdaptation || !VideoSettings.PostProcessing) return;
            RenderTexture rt = renderingData.cameraData.camera.targetTexture;
            if (renderingData.cameraData.cameraType != CameraType.Game || (rt != null && rt.width < 1024)) return;
            if (renderingData.cameraData.renderType == CameraRenderType.Overlay) return; // the base camera already adapted
            if (material == null) material = UnityEngine.Rendering.CoreUtils.CreateEngineMaterial(shader);
            if (!pass.Setup(material)) return;
            renderer.EnqueuePass(pass);
            if (WantShownStops && Time.frameCount % 15 == 0) pass.RequestShownStops();
        }

        protected override void Dispose(bool disposing)
        {
            pass?.Dispose();
            if (material != null) UnityEngine.Rendering.CoreUtils.Destroy(material);
            material = null;
        }
    }
}

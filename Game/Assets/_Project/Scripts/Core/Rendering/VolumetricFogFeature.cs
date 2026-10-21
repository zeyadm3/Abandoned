using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Core
{
    /// <summary>
    /// Volumetric light (0.24.12) on the PC renderer, added by EditorTools.RenderPipelineSetup. It runs only where
    /// the camera's volume has a <see cref="VolumetricFogOverride"/> with density, and only on game and scene
    /// cameras drawing at full size: the wardrobe preview, the truck monitor and reflection probes don't pay for it.
    /// </summary>
    public class VolumetricFogFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader shader;
        private Material material;
        private VolumetricFogPass pass;

        /// <summary>F1 + V: show the fog alone (RenderDebugOverlay).</summary>
        public static bool DebugFogOnly { get; set; }

        /// <summary>Why the last camera got no fog ("running" when it did); for F1 and the review renders.</summary>
        public static string LastSkip { get; private set; } = "never asked";

        public override void Create()
        {
            // It blends over the camera colour, so it needs one even when post-processing is off.
            pass = new VolumetricFogPass { renderPassEvent = RenderPassEvent.BeforeRenderingTransparents, requiresIntermediateTexture = true };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (shader == null || VideoSettings.Volumetrics <= 0) { LastSkip = "no shader or setting off"; return; }
            CameraType type = renderingData.cameraData.cameraType;
            if (type != CameraType.Game && type != CameraType.SceneView) { LastSkip = "not a game or scene camera"; return; }
            // An overlay draws on top of a base camera that already has its fog.
            if (renderingData.cameraData.renderType == CameraRenderType.Overlay) { LastSkip = "overlay camera"; return; }
            // The wardrobe preview and the truck's monitor are small render textures; a full-size target (a capture) gets fog.
            RenderTexture rt = renderingData.cameraData.camera.targetTexture;
            if (rt != null && rt.width < 1024) { LastSkip = "small monitor or preview"; return; }
            if (material == null) material = UnityEngine.Rendering.CoreUtils.CreateEngineMaterial(shader);
            if (!pass.Setup(material)) { LastSkip = "no fog in this volume"; return; }
            LastSkip = "running";
            pass.ConfigureInput(ScriptableRenderPassInput.Depth);
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            if (material != null) UnityEngine.Rendering.CoreUtils.Destroy(material);
            material = null;
        }
    }
}

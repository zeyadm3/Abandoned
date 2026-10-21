using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Core
{
    /// <summary>
    /// The four draws of <see cref="VolumetricFogFeature"/>: march at reduced resolution, blur across, blur down,
    /// then upsample and blend over the camera colour. Each draw carries its own property block.
    /// </summary>
    public class VolumetricFogPass : ScriptableRenderPass
    {
        private const int MarchPass = 0, BlurPass = 1, CompositePass = 2;
        private static readonly int Corner00 = Shader.PropertyToID("_FogCorner00"), Corner10 = Shader.PropertyToID("_FogCorner10"),
            Corner01 = Shader.PropertyToID("_FogCorner01"), CamPos = Shader.PropertyToID("_FogCamPos"),
            SourceSize = Shader.PropertyToID("_FogSourceSize"), Input = Shader.PropertyToID("_FogInput"),
            Params0 = Shader.PropertyToID("_FogParams0"), Params1 = Shader.PropertyToID("_FogParams1"),
            Params2 = Shader.PropertyToID("_FogParams2"), Albedo = Shader.PropertyToID("_FogAlbedo"),
            Ambient = Shader.PropertyToID("_FogAmbient"), Wind = Shader.PropertyToID("_FogWind"),
            BlurDir = Shader.PropertyToID("_FogBlurDir"), DebugId = Shader.PropertyToID("_FogDebug");

        private readonly Vector3[] corners = new Vector3[4];
        // Reused every frame: URP records and runs one camera's graph before it records the next.
        private readonly MaterialPropertyBlock march = new(), across = new(), down = new(), composite = new();
        private Material material;
        private VolumetricFogOverride fog;

        private class PassData
        {
            public Material Material;
            public int Pass;
            public TextureHandle Input;
            public MaterialPropertyBlock Block;
        }

        /// <summary>False when the camera's volume asks for no fog this frame.</summary>
        public bool Setup(Material fogMaterial)
        {
            material = fogMaterial;
            fog = VolumeManager.instance.stack.GetComponent<VolumetricFogOverride>();
            return fog != null && fog.IsActive;
        }

        public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
        {
            var resources = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            if (!resources.cameraDepthTexture.IsValid() || resources.isActiveTargetBackBuffer) return;

            int level = VideoSettings.Volumetrics;
            int divisor = VideoSettings.VolumetricDivisor[level];
            int width = Mathf.Max(1, cameraData.cameraTargetDescriptor.width / divisor);
            int height = Mathf.Max(1, cameraData.cameraTargetDescriptor.height / divisor);
            var desc = new TextureDesc(width, height)
            {
                colorFormat = GraphicsFormat.R16G16B16A16_SFloat,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                clearBuffer = false,
                name = "_VolumetricFogA"
            };
            TextureHandle a = graph.CreateTexture(desc);
            desc.name = "_VolumetricFogB";
            TextureHandle b = graph.CreateTexture(desc);

            Camera camera = cameraData.camera;
            View(march, camera, width, height);
            View(across, camera, width, height);
            View(down, camera, width, height);
            View(composite, camera, width, height);
            Medium(march, VideoSettings.VolumetricSteps[level], cameraData.maxShadowDistance);
            across.SetVector(BlurDir, new Vector4(1f, 0f));
            down.SetVector(BlurDir, new Vector4(0f, 1f));
            composite.SetFloat(DebugId, VolumetricFogFeature.DebugFogOnly ? 1f : 0f);

            Draw(graph, resources, "Volumetric Fog March", MarchPass, TextureHandle.nullHandle, a, march, true);
            Draw(graph, resources, "Volumetric Fog Blur X", BlurPass, a, b, across, false);
            Draw(graph, resources, "Volumetric Fog Blur Y", BlurPass, b, a, down, false);
            Draw(graph, resources, "Volumetric Fog Composite", CompositePass, a, resources.activeColorTexture, composite, false);
        }

        private void Draw(RenderGraph graph, UniversalResourceData resources, string name, int pass, TextureHandle input,
            TextureHandle target, MaterialPropertyBlock block, bool needsLights)
        {
            using IRasterRenderGraphBuilder builder = graph.AddRasterRenderPass(name, out PassData data);
            data.Material = material;
            data.Pass = pass;
            data.Input = input;
            data.Block = block;
            builder.UseTexture(resources.cameraDepthTexture);
            if (input.IsValid()) builder.UseTexture(input);
            if (needsLights)
            {
                if (resources.mainShadowsTexture.IsValid()) builder.UseTexture(resources.mainShadowsTexture);
                if (resources.additionalShadowsTexture.IsValid()) builder.UseTexture(resources.additionalShadowsTexture);
            }
            builder.SetRenderAttachment(target, 0);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc(static (PassData d, RasterGraphContext ctx) =>
            {
                if (d.Input.IsValid()) d.Block.SetTexture(Input, d.Input);
                ctx.cmd.DrawProcedural(Matrix4x4.identity, d.Material, d.Pass, MeshTopology.Triangles, 3, 1, d.Block);
            });
        }

        private void View(MaterialPropertyBlock block, Camera camera, int width, int height)
        {
            // Rays to the view's corners at view depth 1, so position = camera + ray * eye depth.
            camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), 1f, Camera.MonoOrStereoscopicEye.Mono, corners);
            Transform t = camera.transform;
            block.SetVector(Corner00, t.TransformVector(corners[0])); // bottom left
            block.SetVector(Corner01, t.TransformVector(corners[1])); // top left
            block.SetVector(Corner10, t.TransformVector(corners[3])); // bottom right
            block.SetVector(CamPos, t.position);
            block.SetVector(SourceSize, new Vector4(width, height, 1f / width, 1f / height));
        }

        private void Medium(MaterialPropertyBlock block, int steps, float shadowDistance)
        {
            block.SetVector(Params0, new Vector4(fog.density.value, fog.baseHeight.value, fog.heightFalloff.value, fog.maxDistance.value));
            block.SetVector(Params1, new Vector4(fog.anisotropy.value, fog.mainLight.value, fog.localLights.value, fog.unshadowedLights.value));
            block.SetVector(Params2, new Vector4(fog.noise.value, fog.noiseScale.value, steps, Mathf.Max(1f, shadowDistance)));
            block.SetVector(Albedo, (Vector4)fog.albedo.value.linear);
            // The dust glows the colour the map's ordinary fog already is, so far rooms fade into it instead of to black.
            block.SetVector(Ambient, (Vector4)(RenderSettings.fogColor.linear * fog.ambient.value));
            block.SetVector(Wind, fog.wind.value * Time.time);
        }
    }
}

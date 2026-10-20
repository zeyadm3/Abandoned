using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Core
{
    /// <summary>
    /// The three draws of <see cref="AutoExposureFeature"/>. The exposure lives in two 1 x 1 textures that swap each
    /// frame (last frame's value eases toward the new target), so it survives between frames without a CPU readback.
    /// </summary>
    public class AutoExposurePass : ScriptableRenderPass
    {
        private const int LuminanceSize = 64; // averaged in the adapt pass by 32 x 32 bilinear taps
        private static readonly int Source = Shader.PropertyToID("_ExposureSource"), Previous = Shader.PropertyToID("_ExposurePrevious"),
            Params0 = Shader.PropertyToID("_ExposureParams0"), Params1 = Shader.PropertyToID("_ExposureParams1"),
            Reset = Shader.PropertyToID("_ExposureReset");

        private readonly MaterialPropertyBlock luminance = new(), adapt = new(), apply = new();
        private RTHandle[] exposure = new RTHandle[2];
        private int current;
        private float lastTime = -100f;
        private Material material;
        private AutoExposureOverride settings;

        /// <summary>The exposure in stops from the last frame read on the CPU side (F1 overlay only; never synced).</summary>
        public static float ShownStops { get; private set; }

        private class PassData
        {
            public Material Material;
            public int Pass;
            public TextureHandle Source, Previous;
            public MaterialPropertyBlock Block;
        }

        public bool Setup(Material exposureMaterial)
        {
            material = exposureMaterial;
            settings = VolumeManager.instance.stack.GetComponent<AutoExposureOverride>();
            return settings != null && settings.IsActive;
        }

        public void Dispose()
        {
            foreach (RTHandle h in exposure) h?.Release();
            exposure = new RTHandle[2];
        }

        public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
        {
            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer) return;
            for (int i = 0; i < 2; i++)
                if (exposure[i] == null)
                    exposure[i] = RTHandles.Alloc(1, 1, colorFormat: GraphicsFormat.R16_SFloat, name: "_AutoExposure" + i);

            // A long gap (a level load, the game paused in the editor) starts from the target instead of easing to it.
            bool reset = Time.unscaledTime - lastTime > 0.5f;
            lastTime = Time.unscaledTime;
            TextureHandle previous = graph.ImportTexture(exposure[current]);
            current = 1 - current;
            TextureHandle next = graph.ImportTexture(exposure[current]);

            var desc = new TextureDesc(LuminanceSize, LuminanceSize)
            {
                colorFormat = GraphicsFormat.R16G16_SFloat,
                filterMode = FilterMode.Bilinear,
                clearBuffer = false,
                name = "_ExposureLuminance"
            };
            TextureHandle lum = graph.CreateTexture(desc);

            var p0 = new Vector4(settings.key.value, settings.strength.value, settings.minEv.value, settings.maxEv.value);
            adapt.SetVector(Params0, p0);
            adapt.SetVector(Params1, new Vector4(settings.speedDown.value, settings.speedUp.value, Mathf.Min(Time.unscaledDeltaTime, 0.1f), 0f));
            adapt.SetFloat(Reset, reset ? 1f : 0f);

            Draw(graph, "Exposure Luminance", 0, resources.activeColorTexture, TextureHandle.nullHandle, lum, luminance);
            Draw(graph, "Exposure Adapt", 1, lum, previous, next, adapt);
            Draw(graph, "Exposure Apply", 2, TextureHandle.nullHandle, next, resources.activeColorTexture, apply);
        }

        private void Draw(RenderGraph graph, string name, int pass, TextureHandle source, TextureHandle prev, TextureHandle target, MaterialPropertyBlock block)
        {
            using IRasterRenderGraphBuilder builder = graph.AddRasterRenderPass(name, out PassData data);
            data.Material = material;
            data.Pass = pass;
            data.Source = source;
            data.Previous = prev;
            data.Block = block;
            if (source.IsValid()) builder.UseTexture(source);
            if (prev.IsValid()) builder.UseTexture(prev);
            builder.SetRenderAttachment(target, 0);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc(static (PassData d, RasterGraphContext ctx) =>
            {
                if (d.Source.IsValid()) d.Block.SetTexture(Source, d.Source);
                if (d.Previous.IsValid()) d.Block.SetTexture(Previous, d.Previous);
                ctx.cmd.DrawProcedural(Matrix4x4.identity, d.Material, d.Pass, MeshTopology.Triangles, 3, 1, d.Block);
            });
        }

        /// <summary>For the F1 overlay: reads the GPU value back asynchronously, never blocking a frame.</summary>
        public void RequestShownStops()
        {
            RTHandle h = exposure[current];
            if (h?.rt == null) return;
            AsyncGPUReadback.Request(h.rt, 0, r =>
            {
                if (!r.hasError) ShownStops = Mathf.HalfToFloat(r.GetData<ushort>()[0]);
            });
        }
    }
}

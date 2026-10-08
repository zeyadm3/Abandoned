using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Measured asset/render census; static Editor art counters never imply multiplayer FPS.</summary>
    public static class MeridianPerformanceReport
    {
        public static void Write(Transform mall, string folder, IReadOnlyList<MeridianRenderSample> samples)
        {
            Renderer[] renderers = mall.GetComponentsInChildren<Renderer>(true);
            var materials = new HashSet<Material>(renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null));
            if (RenderSettings.skybox != null) materials.Add(RenderSettings.skybox);
            var textures = new HashSet<Texture>();
            foreach (Material material in materials)
                foreach (string property in material.GetTexturePropertyNames())
                { Texture texture = material.GetTexture(property); if (texture != null) textures.Add(texture); }
            var meshes = new HashSet<Mesh>(mall.GetComponentsInChildren<MeshFilter>(true).Select(f => f.sharedMesh).Where(m => m != null));
            foreach (SkinnedMeshRenderer skin in mall.GetComponentsInChildren<SkinnedMeshRenderer>(true)) if (skin.sharedMesh != null) meshes.Add(skin.sharedMesh);
            long triangles = meshes.Sum(mesh => Enumerable.Range(0, mesh.subMeshCount).Sum(i => (long)mesh.GetIndexCount(i) / 3));
            long vertices = meshes.Sum(mesh => (long)mesh.vertexCount);
            long nativeBytes = textures.Sum(texture => Profiler.GetRuntimeMemorySizeLong(texture));
            long gpuBytes = textures.Sum(StorageBytes);
            Light[] sceneDirectional = Object.FindObjectsByType<Light>(FindObjectsSortMode.None)
                .Where(light => light.type == LightType.Directional && !light.transform.IsChildOf(mall)).ToArray();
            Light[] lights = mall.GetComponentsInChildren<Light>(true).Concat(sceneDirectional).ToArray();
            StructuralSection[] sections = mall.GetComponentsInChildren<StructuralSection>(true);
            LODGroup[] lods = mall.GetComponentsInChildren<LODGroup>(true);
            ReflectionProbe[] probes = mall.GetComponentsInChildren<ReflectionProbe>(true);
            long reflectionBytes = probes.Where(probe => probe.realtimeTexture != null)
                .Sum(probe => Profiler.GetRuntimeMemorySizeLong(probe.realtimeTexture));
            var text = new StringBuilder();
            text.AppendLine($"# Meridian {BuildScript.Version} — asset and Editor render report");
            text.AppendLine();
            text.AppendLine("Captured from the saved Mall in Edit mode at 1920 × 1080. No gameplay, network, EditMode/PlayMode or screenshot tests were run.");
            text.AppendLine("FPS and the 60 FPS four-player mid-range-PC-host target remain pending the user's in-game pass. These art captures do not measure host simulation or multiplayer cost.");
            text.AppendLine();
            text.AppendLine($"Graphics device: {SystemInfo.graphicsDeviceName}; API: {SystemInfo.graphicsDeviceType}; Unity: {Application.unityVersion}.");
            text.AppendLine($"Referenced mall and scene-sky textures: {textures.Count}; format/block/mipmap GPU storage: {MiB(gpuBytes)} MiB; Profiler runtime/native allocation: {MiB(nativeBytes)} MiB.");
            text.AppendLine("GPU storage is calculated from each actual imported graphics format, dimensions and mip count. Profiler allocation includes Editor/native overhead; realtime reflection targets and transient render buffers are reported separately from imported material maps.");
            text.AppendLine($"Unique instantiated meshes, including inactive LODs: {meshes.Count}; stored vertices: {vertices:N0}; stored triangles: {triangles:N0}.");
            text.AppendLine($"Renderers: {renderers.Length}; material slots: {renderers.Sum(r => r.sharedMaterials.Length)}; unique materials including sky: {materials.Count}; instancing-enabled materials: {materials.Count(m => m.enableInstancing)}.");
            text.AppendLine($"LOD groups: {lods.Length}; realtime fixtures: {mall.GetComponentsInChildren<LightFixture>(true).Length}; mall plus global directional lights: {lights.Length}; global directionals: {sceneDirectional.Length}; shadow-capable lights: {lights.Count(l => l.shadows != LightShadows.None)}.");
            text.AppendLine($"Reflection probes: {probes.Length}; allocated realtime probe textures: {MiB(reflectionBytes)} MiB; sections: {sections.Length}; collapsible: {sections.Count(s => s.CanCollapse)}; permanent: {sections.Count(s => !s.CanCollapse)}.");
            text.AppendLine($"Static renderer flags — batching: {Flagged(renderers, StaticEditorFlags.BatchingStatic)}; occluder: {Flagged(renderers, StaticEditorFlags.OccluderStatic)}; occludee: {Flagged(renderers, StaticEditorFlags.OccludeeStatic)}.");
            text.AppendLine();
            text.AppendLine("| Art view | Flashlights | Editor batches | SetPass | Triangles | Vertices |");
            text.AppendLine("|---|---:|---:|---:|---:|---:|");
            foreach (MeridianRenderSample sample in samples)
                text.AppendLine($"| {sample.Name} | {sample.Flashlights} | {sample.Batches} | {sample.SetPass} | {sample.Triangles} | {sample.Vertices} |");
            text.AppendLine();
            text.AppendLine("UnityStats are raw Editor counters sampled after synchronous art rendering and readback; they may include Editor/probe work and are not an isolated gameplay profiler measurement. All-zero counters mean the batch renderer did not expose usable statistics.");
            text.AppendLine("Every normal view uses one owner flashlight. The extra atrium view uses four static beams, one shadowed and three unshadowed, matching the current runtime flashlight policy. Power-on respects dead fixtures and depicts faulty fixtures at a steady 75%; power-off removes fixture lamps and all their diffuser emission. The scene file is byte-checked unchanged after capture.");
            text.AppendLine("Gallery stills show the authored collapse wound. The three close terrazzo stills swap the existing stable/cracking/failing art variants without advancing structural simulation or damage.");
            text.AppendLine();
            text.AppendLine("| Texture | Imported size / mips | Actual format | GPU MiB | Profiler MiB | Import type / sRGB / mipmaps | Default max | Standalone override / max / format |");
            text.AppendLine("|---|---|---|---:|---:|---|---:|---|");
            foreach (Texture texture in textures.OrderBy(t => AssetDatabase.GetAssetPath(t)).ThenBy(t => t.name))
            {
                string path = AssetDatabase.GetAssetPath(texture);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                TextureImporterPlatformSettings platform = importer != null ? importer.GetPlatformTextureSettings("Standalone") : null;
                string settings = importer == null ? "generated/sub-asset" : $"{importer.textureType} / {importer.sRGBTexture} / {importer.mipmapEnabled}";
                string target = platform == null ? "n/a" : $"{platform.overridden} / {platform.maxTextureSize} / {platform.format}";
                string name = string.IsNullOrEmpty(path) ? texture.name : path;
                text.AppendLine($"| {name} | {texture.width} × {texture.height} / {MipCount(texture)} | {texture.graphicsFormat} | {MiB(StorageBytes(texture))} | {MiB(Profiler.GetRuntimeMemorySizeLong(texture))} | {settings} | {(importer != null ? importer.maxTextureSize : 0)} | {target} |");
            }
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "PERFORMANCE.md"), text.ToString());
            Debug.Log($"[MeridianPerformance] {textures.Count} texture references, {MiB(gpuBytes)} MiB imported GPU storage, {renderers.Length} renderers, {lods.Length} LOD groups; host FPS pending.");
        }

        private static int Flagged(IEnumerable<Renderer> renderers, StaticEditorFlags flag) =>
            renderers.Count(renderer => (GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) & flag) != 0);
        private static string MiB(long bytes) => (bytes / (1024d * 1024d)).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        private static int MipCount(Texture texture) => texture is Texture2D map ? map.mipmapCount : texture is Cubemap cube ? cube.mipmapCount : 1;
        private static long StorageBytes(Texture texture)
        {
            GraphicsFormat format = texture.graphicsFormat;
            if (format == GraphicsFormat.None) return 0;
            int blockWidth = (int)GraphicsFormatUtility.GetBlockWidth(format);
            int blockHeight = (int)GraphicsFormatUtility.GetBlockHeight(format);
            long blockBytes = GraphicsFormatUtility.GetBlockSize(format), total = 0;
            int width = texture.width, height = texture.height;
            for (int mip = 0; mip < MipCount(texture); mip++)
            {
                total += (long)Mathf.CeilToInt(width / (float)blockWidth) * Mathf.CeilToInt(height / (float)blockHeight) * blockBytes;
                width = Mathf.Max(1, width / 2); height = Mathf.Max(1, height / 2);
            }
            return total * (texture is Cubemap ? 6 : 1);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using Abandoned.Core;
using Abandoned.Player;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>Repeatable player-height views of the saved maps. No scene or preference changes are saved.</summary>
    public static class LightingReview
    {
        private const int Width = 1280, Height = 720;
        private static string scratch;
        private static int probeIndex;
        private static bool ArtReview => Environment.GetEnvironmentVariable("ART_REVIEW") == "1";

        public static void CaptureAndExit()
        {
            int failures = 0;
            var annotations = GizmoUtility.GetGizmoInfo().Select(info => (info, info.gizmoEnabled, info.iconEnabled)).ToArray();
            foreach (var entry in annotations)
            {
                entry.info.gizmoEnabled = false;
                entry.info.iconEnabled = false;
                GizmoUtility.ApplyGizmoInfo(entry.info);
            }
            string output = Environment.GetEnvironmentVariable("LIGHTING_REVIEW") ?? "lighting-before";
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Docs/renders", output));
            Directory.CreateDirectory(folder);
            string ablate = Environment.GetEnvironmentVariable("LIGHTING_ABLATE") ?? "";
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RenderPipelineSetup.PcRendererPath);
            foreach (ScriptableRendererFeature feature in renderer.rendererFeatures)
            {
                if ((ablate.Contains("fog") && feature is VolumetricFogFeature) ||
                    (ablate.Contains("eye") && feature is AutoExposureFeature)) feature.SetActive(false);
            }
            try
            {
                foreach (string path in AvOverhaul.MapScenes)
                {
                    string filter = Environment.GetEnvironmentVariable("LIGHTING_MAP");
                    if (!string.IsNullOrEmpty(filter) && !path.Contains(filter, StringComparison.OrdinalIgnoreCase)) continue;
                    try { Capture(path, folder); }
                    catch (Exception e) { failures++; Debug.LogException(e); }
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(scratch)) AssetDatabase.DeleteAsset(scratch);
                foreach (var entry in annotations)
                {
                    entry.info.gizmoEnabled = entry.gizmoEnabled;
                    entry.info.iconEnabled = entry.iconEnabled;
                    GizmoUtility.ApplyGizmoInfo(entry.info);
                }
            }
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }

        private static void Capture(string path, string folder)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            if ((Environment.GetEnvironmentVariable("LIGHTING_ABLATE") ?? "").Contains("bloom"))
                foreach (Volume volume in Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
                    if (volume.sharedProfile != null && volume.sharedProfile.TryGet(out Bloom bloom)) bloom.intensity.Override(0f);
            Physics.SyncTransforms();
            DynamicGI.UpdateEnvironment();
            if (ArtReview)
            {
                foreach (LightFixture fixture in Object.FindObjectsByType<LightFixture>(FindObjectsSortMode.None)) fixture.SetPowered(true);
                foreach (ParticleSystem particles in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
                {
                    if (!particles.main.playOnAwake || !particles.gameObject.activeInHierarchy) continue;
                    var particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
                    if (VisualEffectsRemaster.Kind(particles.name) == null && VisualEffectsRemaster.Kind(particleRenderer?.sharedMaterial?.name) == null) continue;
                    particles.Simulate(6f, false, true, true);
                    particles.Pause(false);
                }
            }
            // Cold cubes are unpublished by RoomReflections in play as well (URP 17.3 cluster-count bug).
            foreach (ReflectionProbe probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None))
                if (probe.texture == null) probe.enabled = false;
            var go = new GameObject("LightingReviewCamera");
            Camera camera = go.AddComponent<Camera>();
            camera.fieldOfView = 70f;
            camera.nearClipPlane = .08f;
            camera.farClipPlane = 1200f;
            camera.allowHDR = true;
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = !(Environment.GetEnvironmentVariable("LIGHTING_ABLATE") ?? "").Contains("post");
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            // The pipeline still shades in HDR internally. The PNG is a display image and needs an sRGB final target.
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            try
            {
                var spawn = Object.FindFirstObjectByType<PlayerSpawnPoint>();
                if (spawn != null)
                {
                    camera.transform.SetPositionAndRotation(spawn.transform.position + Vector3.up * 1.65f,
                        Quaternion.Euler(4f, spawn.transform.eulerAngles.y, 0f));
                    Draw(camera, target, Path.Combine(folder, scene.name + "-outside.png"));
                }
                LightFixture[] fixtures = Object.FindObjectsByType<LightFixture>(FindObjectsSortMode.None)
                    .Where(f => f.Lamp != null && !f.Dead && f.Lamp.type == LightType.Spot)
                    .OrderBy(f => f.transform.position.x).ThenBy(f => f.transform.position.z).ThenBy(f => f.transform.position.y).ToArray();
                int shot = 0;
                foreach (LightFixture fixture in fixtures)
                {
                    Light lamp = fixture.Lamp;
                    if (lamp.transform.forward.y > -.6f) continue;
                    if (!Physics.Raycast(lamp.transform.position, Vector3.down, out RaycastHit floor, 14f,
                            ~LayerMask.GetMask("Ignore Raycast"), QueryTriggerInteraction.Ignore) || floor.distance < 2f) continue;
                    if (!NavMesh.SamplePosition(floor.point + Vector3.right * 2f, out NavMeshHit nav, 2f, NavMesh.AllAreas)) continue;
                    Vector3 at = nav.position + Vector3.up * 1.65f;
                    if (Physics.CheckSphere(at, .18f, ~LayerMask.GetMask("Ignore Raycast"), QueryTriggerInteraction.Ignore)) continue;
                    Vector3 best = Vector3.forward;
                    float far = 0;
                    for (int a = 0; a < 16; a++)
                    {
                        Vector3 dir = Quaternion.Euler(0, a * 22.5f, 0) * Vector3.forward;
                        float reach = Physics.Raycast(at, dir, out RaycastHit wall, 24f, ~0, QueryTriggerInteraction.Ignore) ? wall.distance : 24f;
                        if (reach > far) { best = dir; far = reach; }
                    }
                    camera.transform.SetPositionAndRotation(at, Quaternion.LookRotation(best) * Quaternion.Euler(5f, 0, 0));
                    Debug.Log($"[LightingReview] {scene.name} {shot}: {at} looking {best}; lamp {lamp.intensity:0.00}, floor {floor.distance:0.00}m");
                    Draw(camera, target, Path.Combine(folder, $"{scene.name}-room-{shot}.png"));
                    if (ArtReview && shot == 0)
                    {
                        var flashlight = camera.gameObject.AddComponent<Light>();
                        flashlight.type = LightType.Spot;
                        flashlight.range = 26f;
                        flashlight.intensity = 33.6f;
                        flashlight.spotAngle = 68f;
                        flashlight.innerSpotAngle = 40.8f;
                        flashlight.color = new Color(.95f, .98f, 1f);
                        flashlight.shadows = LightShadows.Soft;
                        flashlight.cookie = LightCookies.Flashlight;
                        LevelStoreys.LightEverything(flashlight);
                        Draw(camera, target, Path.Combine(folder, $"{scene.name}-flashlight.png"));
                        CloseSurface(camera, target, folder, scene.name);
                        CloseProp(camera, target, folder, scene.name);
                        Object.DestroyImmediate(flashlight);
                    }
                    if (++shot == 2) break;
                }
            }
            finally { Object.DestroyImmediate(go); target.Release(); Object.DestroyImmediate(target); }
        }

        private static void Draw(Camera camera, RenderTexture target, string path)
        {
            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
            if (RenderPipelineManager.currentPipeline == null) RenderPipeline.SubmitRenderRequest(camera, request);
            if (Environment.GetEnvironmentVariable("LIGHTING_REVIEW") != null)
            {
                var director = camera.GetComponent<ShadowDirector>() ?? camera.gameObject.AddComponent<ShadowDirector>();
                director.Preview(camera);
                foreach (IndirectLighting rig in Object.FindObjectsByType<IndirectLighting>(FindObjectsSortMode.None)) rig.Preview(camera);
            }
            VolumeManager.instance.ResetMainStack();
            VolumeManager.instance.Update(camera.transform, camera.GetUniversalAdditionalCameraData().volumeLayerMask);
            LightShaftSwitch.Apply(VideoSettings.Volumetrics > 0);
            if (ArtReview) BakeNearestReflections(camera);
            string ablate = Environment.GetEnvironmentVariable("LIGHTING_ABLATE") ?? "";
            if (ablate.Contains("fog")) VolumeManager.instance.stack.GetComponent<VolumetricFogOverride>().density.value = 0f;
            if (ablate.Contains("eye")) VolumeManager.instance.stack.GetComponent<AutoExposureOverride>().enabled.value = false;
            if (ablate.Contains("bloom")) VolumeManager.instance.stack.GetComponent<Bloom>().intensity.value = 0f;
            for (int i = 0; i < 8; i++) RenderPipeline.SubmitRenderRequest(camera, request);
            Debug.Log($"[LightingReview] {Path.GetFileName(path)}: fog {VolumeManager.instance.stack.GetComponent<VolumetricFogOverride>()?.density.value}, " +
                      $"bloom {VolumeManager.instance.stack.GetComponent<Bloom>()?.intensity.value}, eye {VolumeManager.instance.stack.GetComponent<AutoExposureOverride>()?.enabled.value}");
            RenderTexture previous = RenderTexture.active;
            var pixels = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(path, pixels.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; Object.DestroyImmediate(pixels); }
        }

        private static void CloseSurface(Camera camera, RenderTexture target, string folder, string scene)
        {
            Vector3 original = camera.transform.position;
            Quaternion rotation = camera.transform.rotation;
            float nearest = 16f;
            RaycastHit closest = default;
            for (int i = 0; i < 16; i++)
            {
                Vector3 dir = Quaternion.Euler(0, i * 22.5f, 0) * Vector3.forward;
                if (Physics.Raycast(original, dir, out RaycastHit wall, nearest, ~0, QueryTriggerInteraction.Ignore) && wall.distance > 1.2f)
                { closest = wall; nearest = wall.distance; }
            }
            if (nearest < 16f)
            {
                camera.transform.position = closest.point + closest.normal * 1.4f;
                camera.transform.rotation = Quaternion.LookRotation(-closest.normal) * Quaternion.Euler(9f, 18f, 0);
                Draw(camera, target, Path.Combine(folder, scene + "-surface-detail.png"));
            }
            camera.transform.SetPositionAndRotation(original, rotation);
        }

        private static void CloseProp(Camera camera, RenderTexture target, string folder, string scene)
        {
            Vector3 original = camera.transform.position;
            Quaternion rotation = camera.transform.rotation;
            foreach (MeshRenderer renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)
                         .Where(r => r.enabled && r.TryGetComponent<MeshFilter>(out var filter) && filter.sharedMesh != null)
                         .OrderBy(r => r.bounds.SqrDistance(original)))
            {
                Mesh mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                if (!AssetDatabase.GetAssetPath(mesh).StartsWith(ObjectRemaster.Folder + "/Prop_", StringComparison.Ordinal)) continue;
                Bounds bounds = renderer.bounds;
                float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (size < .5f || size > 2.5f || bounds.size.y < .4f || bounds.SqrDistance(original) > 144f) continue;
                Vector3 direction = original - bounds.center;
                if (Physics.Raycast(original, -direction.normalized, out RaycastHit hit, direction.magnitude) && bounds.SqrDistance(hit.point) > .08f) continue;
                Vector3 at = bounds.center + direction.normalized * Mathf.Max(1.1f, size * 1.2f) + Vector3.up * .15f;
                if (Physics.CheckSphere(at, .15f, ~LayerMask.GetMask("Ignore Raycast"), QueryTriggerInteraction.Ignore)) continue;
                camera.transform.SetPositionAndRotation(at, Quaternion.LookRotation(bounds.center - at));
                Draw(camera, target, Path.Combine(folder, scene + "-prop-detail.png"));
                Debug.Log($"[LightingReview] {scene} prop detail: {renderer.name} ({mesh.name})");
                break;
            }
            camera.transform.SetPositionAndRotation(original, rotation);
        }

        private static void BakeNearestReflections(Camera camera)
        {
            if (string.IsNullOrEmpty(scratch))
            {
                string name = "_ArtReview_" + Guid.NewGuid().ToString("N");
                AssetDatabase.CreateFolder("Assets", name);
                scratch = "Assets/" + name;
            }
            foreach (ReflectionProbe probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)
                         .OrderBy(p => p.bounds.SqrDistance(camera.transform.position))
                         .ThenBy(p => (p.transform.position - camera.transform.position).sqrMagnitude).Take(2))
            {
                if (probe.customBakedTexture != null && AssetDatabase.GetAssetPath(probe.customBakedTexture).StartsWith(scratch)) continue;
                string path = scratch + "/probe_" + probeIndex++ + ".exr";
                probe.enabled = true;
                probe.mode = ReflectionProbeMode.Realtime;
                if (!Lightmapping.BakeReflectionProbe(probe, path)) throw new InvalidOperationException("Couldn't render reflection " + probe.name);
                AssetDatabase.ImportAsset(path);
                probe.mode = ReflectionProbeMode.Custom;
                probe.customBakedTexture = AssetDatabase.LoadAssetAtPath<Texture>(path);
            }
        }
    }
}

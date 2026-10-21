using System.IO;
using Abandoned.Core;
using Abandoned.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.12 review renders (batch: Tools/unity.sh exec Abandoned.EditorTools.AvRenders.RenderAndExit): four views per
    /// map (a player spawn, two seeded walkable spots looking down their longest clear line, and one holding a lit
    /// flashlight) into Game/Logs/renders/av. Never saves a scene.
    /// </summary>
    public static class AvRenders
    {
        private const int Width = 1600, Height = 900;
        private const string ProbeTemp = "Assets/_RenderTemp";
        // RENDER_OUT=<folder name> keeps one set of renders apart from another (before/after a change).
        private static readonly string Folder = Path.Combine(Application.dataPath, "..", "Logs", "renders",
            string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("RENDER_OUT")) ? "av" : System.Environment.GetEnvironmentVariable("RENDER_OUT"));

        public static void RenderAndExit()
        {
            Directory.CreateDirectory(Folder);
            foreach (string path in AvOverhaul.MapScenes)
            {
                try { RenderScene(path); }
                catch (System.Exception e) { Debug.LogException(e); }
            }
            AssetDatabase.DeleteAsset(ProbeTemp);
            EditorApplication.Exit(0);
        }

        // A batch session never draws realtime probes; bake each once into a scratch folder so the renders show the
        // room reflections the game draws (the scene is never saved).
        private static void BakeProbes()
        {
            AssetDatabase.DeleteAsset(ProbeTemp);
            AssetDatabase.CreateFolder("Assets", "_RenderTemp");
            int i = 0;
            foreach (ReflectionProbe probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None))
            {
                if (probe.mode != UnityEngine.Rendering.ReflectionProbeMode.Realtime) continue;
                string file = $"{ProbeTemp}/probe_{i++}.exr";
                if (!Lightmapping.BakeReflectionProbe(probe, file)) continue;
                AssetDatabase.ImportAsset(file);
                probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;
                probe.customBakedTexture = AssetDatabase.LoadAssetAtPath<Texture>(file);
            }
            Debug.Log($"[AvRenders] {i} probes baked for the renders.");
        }

        private static void RenderScene(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            // In play Unity derives the ambient from the scene's colours itself; a batch session has to be told to.
            DynamicGI.UpdateEnvironment();
            BakeProbes();
            var go = new GameObject("AvRenderCamera") { hideFlags = HideFlags.HideAndDontSave };
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 400f;
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            cam.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            try
            {
                var spawn = Object.FindFirstObjectByType<PlayerSpawnPoint>();
                if (spawn != null)
                {
                    go.transform.SetPositionAndRotation(spawn.transform.position + Vector3.up * 1.65f, Quaternion.Euler(5f, spawn.transform.eulerAngles.y, 0f));
                    Render(cam, target, $"{scene.name}_0_spawn");
                }
                NavMeshTriangulation nav = NavMesh.CalculateTriangulation();
                var rng = new System.Random(1212);
                for (int i = 1; i <= 3 && nav.indices.Length >= 3; i++)
                {
                    int tri = rng.Next(nav.indices.Length / 3) * 3;
                    Vector3 p = (nav.vertices[nav.indices[tri]] + nav.vertices[nav.indices[tri + 1]] + nav.vertices[nav.indices[tri + 2]]) / 3f + Vector3.up * 1.65f;
                    Vector3 best = Vector3.forward;
                    float far = 0f;
                    for (int a = 0; a < 16; a++)
                    {
                        Vector3 d = Quaternion.Euler(0f, a * 22.5f, 0f) * Vector3.forward;
                        float reach = Physics.Raycast(p, d, out RaycastHit hit, 80f, mask, QueryTriggerInteraction.Ignore) ? hit.distance : 80f;
                        if (reach > far) { far = reach; best = d; }
                    }
                    go.transform.SetPositionAndRotation(p, Quaternion.LookRotation(best) * Quaternion.Euler(4f, 0f, 0f));
                    Light torch = null;
                    if (i == 3)
                    {
                        torch = go.AddComponent<Light>();
                        torch.type = LightType.Spot;
                        torch.range = 26f;
                        torch.intensity = 28f;
                        torch.spotAngle = 68f;
                        torch.innerSpotAngle = 40f;
                        torch.shadows = LightShadows.Soft;
                        torch.cookie = LightCookies.Flashlight;
                    }
                    Render(cam, target, $"{scene.name}_{i}{(torch != null ? "_flashlight" : "")}");
                    if (i == 1)
                    {
                        // A/B: the same view without the two new effects, to see what they add or take away.
                        VideoSettings.Volumetrics = 0;
                        VideoSettings.EyeAdaptation = false;
                        Render(cam, target, $"{scene.name}_{i}_without_fog_and_eye");
                        VideoSettings.Volumetrics = 2;
                        VideoSettings.EyeAdaptation = true;                    }
                    if (torch != null)
                    {
                        VolumetricFogFeature.DebugFogOnly = true; // the fog alone, to see what the march adds
                        Render(cam, target, $"{scene.name}_{i}_fogonly");
                        VolumetricFogFeature.DebugFogOnly = false;
                    }
                    if (torch != null) Object.DestroyImmediate(torch);
                }
                // 0.24.13: two close views of whatever wall is nearest, to judge surface detail and any flicker.
                for (int i = 4; i <= 5 && nav.indices.Length >= 3; i++)
                {
                    int tri = rng.Next(nav.indices.Length / 3) * 3;
                    Vector3 p = (nav.vertices[nav.indices[tri]] + nav.vertices[nav.indices[tri + 1]] + nav.vertices[nav.indices[tri + 2]]) / 3f + Vector3.up * 1.65f;
                    Vector3 nearest = Vector3.forward;
                    float close = 80f;
                    for (int a = 0; a < 16; a++)
                    {
                        Vector3 d = Quaternion.Euler(0f, a * 22.5f, 0f) * Vector3.forward;
                        if (Physics.Raycast(p, d, out RaycastHit hit, 80f, mask, QueryTriggerInteraction.Ignore) && hit.distance > 1f && hit.distance < close)
                        {
                            close = hit.distance;
                            nearest = d;
                        }
                    }
                    Vector3 at = close < 80f ? p + nearest * Mathf.Max(0f, close - 1.8f) : p;
                    go.transform.SetPositionAndRotation(at, Quaternion.LookRotation(nearest) * Quaternion.Euler(12f, 25f, 0f));
                    Render(cam, target, $"{scene.name}_{i}_close");
                }
                // 0.24.13: two ceiling fixtures from below, where overlapping fittings showed worst.
                var fixtures = Object.FindObjectsByType<Abandoned.Structure.LightFixture>(FindObjectsSortMode.InstanceID);
                for (int i = 0; i < 2 && fixtures.Length > 0; i++)
                {
                    Transform f = fixtures[rng.Next(fixtures.Length)].transform;
                    go.transform.SetPositionAndRotation(f.position + new Vector3(0.6f, -2.2f, 0.4f), Quaternion.Euler(-62f, 200f, 0f));
                    Render(cam, target, $"{scene.name}_{6 + i}_fixture");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
                target.Release();
                Object.DestroyImmediate(target);
            }
        }

        private static void Render(Camera cam, RenderTexture target, string name)
        {
            // URP updates volumes only on its camera-stack path (what the game uses), not for a single-camera request.
            VolumeManager.instance.ResetMainStack();
            VolumeManager.instance.Update(cam.transform, cam.GetUniversalAdditionalCameraData().volumeLayerMask);
            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
            bool supported = RenderPipeline.SupportsRenderRequest(cam, request);
            for (int frame = 0; frame < 3; frame++)
            {
                if (supported) RenderPipeline.SubmitRenderRequest(cam, request);
                else cam.Render();
            }
            RenderTexture previous = RenderTexture.active;
            var pixels = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(Path.Combine(Folder, name + ".png"), pixels.EncodeToPNG());
                Debug.Log($"[AvRenders] {name}: fog {VolumetricFogFeature.LastSkip}");
            }
            finally
            {
                RenderTexture.active = previous;
                Object.DestroyImmediate(pixels);
            }
        }
    }
}

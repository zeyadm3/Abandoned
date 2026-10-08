using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>Saved-level art review only: no Play mode, network spawning or scene saves.</summary>
    public static class MeridianArtRenders
    {
        private const int Width = 1920, Height = 1080;
        public static string Folder => Path.GetFullPath(Path.Combine(Application.dataPath, "../../Docs/renders/0.13.0"));

        public static void RenderAndExit()
        {
            int code = 0;
            try { Capture(); }
            catch (Exception exception) { Debug.LogException(exception); code = 1; }
            finally { EditorApplication.Exit(code); }
        }

        public static void Capture()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Meridian art capture requires Edit mode.");
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", MallBuilder.ScenePath));
            string before = Hash(path);
            Directory.CreateDirectory(Folder);
            Scene scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            Transform mall = scene.GetRootGameObjects().First(g => g.name == "Mall").transform;
            var samples = new System.Collections.Generic.List<MeridianRenderSample>();
            var cameraObject = new GameObject("MeridianArtCamera") { hideFlags = HideFlags.HideAndDontSave };
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.07f;
            camera.farClipPlane = 130f;
            camera.allowHDR = true;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var texture = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) { name = "MeridianArtTarget" };
            texture.Create();
            camera.targetTexture = texture;
            try
            {
                using (var state = new MeridianRenderState(mall, camera))
                {
                    foreach (MeridianRenderView view in MeridianRenderView.Authored)
                    foreach (bool power in new[] { true, false })
                    {
                        state.Apply(view, power, 1);
                        samples.Add(Render(camera, texture, view.Name + (power ? "-power-on" : "-power-off"), 1));
                    }
                    MeridianRenderView atrium = MeridianRenderView.Authored[0];
                    state.Apply(atrium, true, 4);
                    samples.Add(Render(camera, texture, "atrium-four-flashlights", 4));
                    CaptureSurfaceStages(mall, state, camera, texture, samples);
                }
                MeridianPerformanceReport.Write(mall, Folder, samples);
                MeridianFloorPlans.Write();
            }
            finally
            {
                camera.targetTexture = null;
                texture.Release();
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                // Discard all temporary state, including property blocks and fog, without saving.
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                if (Hash(path) != before) throw new InvalidOperationException("Saved Mall scene changed during art capture.");
            }
            Debug.Log($"[MeridianArt] Captured {samples.Count} art stills in {Folder}; saved Mall bytes unchanged.");
        }

        private static MeridianRenderSample Render(Camera camera, RenderTexture target, string name, int flashlights)
        {
            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
            if (RenderPipeline.SupportsRenderRequest(camera, request)) RenderPipeline.SubmitRenderRequest(camera, request);
            else camera.Render();
            RenderTexture previous = RenderTexture.active;
            var pixels = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(Path.Combine(Folder, name + ".png"), pixels.EncodeToPNG());
                var sample = new MeridianRenderSample(name, UnityStats.batches, UnityStats.setPassCalls,
                    UnityStats.triangles, UnityStats.vertices, flashlights);
                Debug.Log($"[MeridianArt] {name}: {sample.Batches} Editor batches, {sample.SetPass} SetPass, {sample.Triangles} triangles.");
                return sample;
            }
            finally { RenderTexture.active = previous; UnityEngine.Object.DestroyImmediate(pixels); }
        }

        private static string Hash(string path)
        {
            using SHA256 hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path)));
        }

        private static void CaptureSurfaceStages(Transform mall, MeridianRenderState state, Camera camera,
            RenderTexture texture, System.Collections.Generic.List<MeridianRenderSample> samples)
        {
            var cell = new Vector2Int(6, 3);
            Transform tile = mall.Find($"Tiles/Floor_0/{MallBuilder.TileName(cell, 0)}");
            StructureStageVisuals stages = tile != null ? tile.GetComponent<StructureStageVisuals>() : null;
            if (stages == null) return;
            bool stable = stages.Stable != null && stages.Stable.gameObject.activeSelf;
            bool cracking = stages.Cracking != null && stages.Cracking.gameObject.activeSelf;
            bool failing = stages.Failing != null && stages.Failing.gameObject.activeSelf;
            Vector3 at = MallLayout.TileTopCenter(cell, 0);
            var view = new MeridianRenderView("terrazzo-surface", at + new Vector3(0f, 2.5f, -2f), at);
            try
            {
                // Swap only authored art variants; no structural simulation, damage or gameplay is advanced.
                foreach (StructuralStage stage in new[] { StructuralStage.Stable, StructuralStage.Cracking, StructuralStage.Failing })
                {
                    stages.Show(stage);
                    state.Apply(view, true, 1);
                    samples.Add(Render(camera, texture, "terrazzo-surface-" + stage.ToString().ToLowerInvariant(), 1));
                }
            }
            finally
            {
                if (stages.Stable != null) stages.Stable.gameObject.SetActive(stable);
                if (stages.Cracking != null) stages.Cracking.gameObject.SetActive(cracking);
                if (stages.Failing != null) stages.Failing.gameObject.SetActive(failing);
            }
        }
    }
}

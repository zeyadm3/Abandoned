using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools
{
    /// <summary>Repeatable saved-scene art views. Temporary review lights never get saved into the level.</summary>
    public static class TutorialHouseReview
    {
        public static void BeforeAndExit() => Run("before");
        public static void AfterAndExit() => Run("after");
        public static void RebuildAndExit() => Run("after", true);

        private static void Run(string stage, bool rebuild = false)
        {
            try
            {
                if (rebuild) TutorialHouseBuilder.Build();
                Capture(stage);
                EditorApplication.Exit(0);
            }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        private static void Capture(string stage)
        {
            var scene = EditorSceneManager.OpenScene(TutorialHouseLayout.ScenePath, OpenSceneMode.Single);
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Docs/renders/tutorial-refinement", stage));
            Directory.CreateDirectory(folder);
            var camera = new GameObject("ReviewCamera").AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = 68;
            camera.nearClipPlane = .06f;
            camera.farClipPlane = 1200;
            camera.allowHDR = true;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            // A neutral inspection lamp at the player's viewpoint reveals the unpowered interior finishes.
            var lamp = new GameObject("ReviewLamp").AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.range = 14;
            lamp.intensity = 2.2f;
            lamp.color = new Color(1f, .93f, .83f);
            lamp.shadows = LightShadows.Soft;
            var target = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32);
            target.Create();
            camera.targetTexture = target;
            (string name, Vector3 eye, Vector3 aim)[] views = {
                ("exterior", new Vector3(39, 2.2f, 19), new Vector3(21, 1.5f, 10)),
                ("living", new Vector3(20, 1.65f, 9.8f), new Vector3(24, .9f, 13)),
                ("kitchen", new Vector3(13.5f, 1.65f, 8.4f), new Vector3(10, 1.1f, 13)),
                ("dining", new Vector3(18.7f, 1.65f, 8), new Vector3(16.5f, 1, 12)),
                ("bedroom", new Vector3(5.4f, 1.65f, 12.5f), new Vector3(2.6f, .8f, 16.2f)),
                ("bathroom", new Vector3(5.05f, 1.65f, 9.5f), new Vector3(1.7f, .8f, 8.5f)),
                ("master", new Vector3(20, 1.65f, 5.5f), new Vector3(24.5f, .8f, 2.7f)),
                ("basement", new Vector3(6.5f, -.75f, 11.6f), new Vector3(1.5f, -1.1f, 14.8f))
            };
            foreach (var view in views)
            {
                camera.transform.position = view.eye;
                camera.transform.LookAt(view.aim);
                lamp.transform.position = view.eye + Vector3.up * .25f;
                lamp.enabled = view.name != "exterior";
                var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
                for (int i = 0; i < 3; i++) RenderPipeline.SubmitRenderRequest(camera, request);
                var previous = RenderTexture.active;
                RenderTexture.active = target;
                var pixels = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(Path.Combine(folder, view.name + ".png"), pixels.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(pixels);
                RenderTexture.active = previous;
            }
            int broken = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true))
                .Count(r => r.sharedMaterials.Any(m => m == null || m.shader == null || m.shader.name.Contains("InternalError")));
            if (broken > 0) throw new InvalidOperationException($"{broken} renderers have missing materials/shaders.");
            camera.targetTexture = null;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log($"[HouseReview] {views.Length} views saved to {folder}; missing materials: {broken}.");
        }
    }
}

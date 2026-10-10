using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.4 store art and screenshots, rendered from the saved mall in the editor (nothing saved back): six
    /// 1920×1080 screenshots with staged loot, crew and monsters, and Steam's capsule set at the current sizes,
    /// the title set in Saira Stencil as world text over a render. Written to Docs/StoreArt.
    /// </summary>
    public static class Release0244
    {
        internal readonly struct Shot
        {
            public readonly string Name;
            public readonly Vector3 Eye, Target;
            public readonly bool Power;
            public readonly int Torches;
            public readonly (string prefab, Vector3 at, float yaw)[] Props;
            public Shot(string name, Vector3 eye, Vector3 target, bool power, int torches, params (string, Vector3, float)[] props)
            { Name = name; Eye = eye; Target = target; Power = power; Torches = torches; Props = props; }
        }

        private const string P = "Assets/_Project/Prefabs/", L = P + "Loot/Loot_", T = P + "Threats/", Crew = P + "Player.prefab";

        private static readonly Shot[] Shots =
        {
            new("01-atrium-the-hunter", new Vector3(28f, 1.7f, 4f), new Vector3(28f, 3.4f, 26f), true, 0,
                (T + "Hunter.prefab", new Vector3(28f, 0f, 21f), 200f), (Crew, new Vector3(27f, 0f, 12f), 10f), (Crew, new Vector3(29.4f, 0f, 12.6f), 340f),
                (L + "corner_sofa.prefab", new Vector3(28.2f, 0.45f, 13.4f), 0f)),
            new("02-glass-bridge-piano", new Vector3(18f, 5.7f, 26f), new Vector3(36f, 4.6f, 26f), true, 0,
                (L + "grand_piano.prefab", new Vector3(26f, 4.8f, 26f), 90f), (Crew, new Vector3(24.6f, 4f, 26.9f), 90f),
                (Crew, new Vector3(27.4f, 4f, 25.1f), 270f), (Crew, new Vector3(26.2f, 4f, 27.3f), 180f)),
            new("03-loading-the-truck", new Vector3(26f, 1.7f, -22f), new Vector3(36f, 1.6f, -11f), true, 0,
                (Crew, new Vector3(35.2f, 0f, -6.5f), 180f), (Crew, new Vector3(36.6f, 0f, -6.7f), 180f),
                (L + "flatscreen_tv.prefab", new Vector3(35.9f, 1.1f, -7.1f), 0f), (L + "large_painting.prefab", new Vector3(36f, 1.7f, -13f), 0f)),
            new("04-dark-store-the-stalker", new Vector3(12f, 1.7f, 26f), new Vector3(7f, 1.3f, 27f), false, 1,
                (T + "Stalker.prefab", new Vector3(5.5f, 0f, 28f), 80f), (L + "antique_vase.prefab", new Vector3(8.5f, 0.3f, 26.5f), 0f)),
            new("05-food-court-the-blind-one", new Vector3(18f, 1.7f, 41.5f), new Vector3(24f, 1.8f, 47.5f), true, 0,
                (T + "BlindOne.prefab", new Vector3(23f, 0f, 46f), 220f), (Crew, new Vector3(20f, 0f, 43.5f), 45f)),
            new("06-flooded-basement", new Vector3(42f, -2.3f, 29f), new Vector3(49f, -2.5f, 34f), false, 2,
                (T + "Crawlers.prefab", new Vector3(47f, -4f, 33f), 230f), (L + "server_rack.prefab", new Vector3(45f, -3.1f, 31f), 15f)),
        };

        private static readonly (string name, int w, int h, int shot, bool title, bool transparent)[] Capsules =
        {
            ("header_capsule", 920, 430, 0, true, false), ("small_capsule", 462, 174, 1, true, false),
            ("main_capsule", 1232, 706, 0, true, false), ("vertical_capsule", 748, 896, 1, true, false),
            ("library_capsule", 600, 900, 3, true, false), ("library_hero", 3840, 1240, 2, false, false),
            ("library_logo", 1280, 720, -1, true, true), ("page_background", 1438, 810, 4, false, false),
        };

        [MenuItem("Tools/Abandoned/Store/Render Store Art and Screenshots (0.24.4)")]
        public static void Author()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Docs/StoreArt"));
            Directory.CreateDirectory(Path.Combine(root, "screenshots"));
            Scene scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            Transform mall = scene.GetRootGameObjects().First(g => g.name == "Mall").transform;
            var cameraObject = new GameObject("StoreArtCamera") { hideFlags = HideFlags.HideAndDontSave };
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = 68f;
            camera.nearClipPlane = 0.07f;
            camera.farClipPlane = 160f;
            camera.allowHDR = true;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var staged = new List<GameObject>();
            // A lift in exposure for the store: still dark, but readable on a small capsule.
            var liftObject = new GameObject("StoreExposure") { hideFlags = HideFlags.HideAndDontSave };
            var lift = liftObject.AddComponent<Volume>();
            lift.isGlobal = true;
            lift.priority = 200f;
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.Add<ColorAdjustments>(true).postExposure.Override(0.9f);
            lift.sharedProfile = profile;
            try
            {
                using var state = new MeridianRenderState(mall, camera);
                foreach (Shot shot in Shots)
                {
                    Stage(shot, staged);
                    state.Apply(new MeridianRenderView(shot.Name, shot.Eye, shot.Target), shot.Power, shot.Torches);
                    Texture2D still = Grab(camera, 1920, 1080, false);
                    File.WriteAllBytes(Path.Combine(root, "screenshots", shot.Name + ".png"), still.EncodeToPNG());
                    Object.DestroyImmediate(still);
                    Clear(staged);
                }
                foreach (var c in Capsules)
                {
                    GameObject title = null;
                    if (c.shot >= 0)
                    {
                        Shot shot = Shots[c.shot];
                        Stage(shot, staged);
                        state.Apply(new MeridianRenderView(shot.Name, shot.Eye, shot.Target), shot.Power, shot.Torches);
                    }
                    camera.fieldOfView = c.w > c.h * 2.5f ? 40f : c.h > c.w ? 80f : 62f;
                    string file = Path.Combine(root, c.name + ".png");
                    Texture2D backdrop = c.shot >= 0 ? Grab(camera, c.w, c.h, false) : null;
                    Texture2D words = null;
                    if (c.title)
                    {
                        // The title alone on a clear background, laid over the scene: nothing in the mall can cover it.
                        title = Title(camera, c.w, c.h, c.transparent || c.shot < 0);
                        int mask = camera.cullingMask;
                        CameraClearFlags clear = camera.clearFlags;
                        camera.cullingMask = 1 << TitleLayer;
                        camera.clearFlags = CameraClearFlags.SolidColor;
                        camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                        camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
                        words = Grab(camera, c.w, c.h, true);
                        camera.cullingMask = mask;
                        camera.clearFlags = clear;
                        camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
                    }
                    Texture2D result = Over(backdrop, words, c.w, c.h);
                    File.WriteAllBytes(file, result.EncodeToPNG());
                    foreach (Texture2D t in new[] { backdrop, words, result }) if (t != null) Object.DestroyImmediate(t);
                    camera.fieldOfView = 68f;
                    if (title != null) Object.DestroyImmediate(title);
                    Clear(staged);
                }
            }
            finally
            {
                Clear(staged);
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(liftObject);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // nothing saved
            }
            BuildScript.ApplyPlayerSettings();
            Debug.Log($"[Store0244] {Shots.Length} screenshots and {Capsules.Length} capsules in {root}.");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        internal const int TitleLayer = 5; // UI: nothing in the mall is on it

        internal static void Stage(Shot shot, List<GameObject> staged)
        {
            foreach ((string prefab, Vector3 at, float yaw) in shot.Props)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
                if (asset == null) { Debug.LogWarning($"[Store0244] {prefab} missing; left out of {shot.Name}."); continue; }
                var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
                go.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
                staged.Add(go);
            }
        }

        internal static void Clear(List<GameObject> staged)
        {
            foreach (GameObject go in staged) if (go != null) Object.DestroyImmediate(go);
            staged.Clear();
        }

        // "ABANDONED" in the stencil face, with a drop shadow, floating in front of the lens.
        internal static GameObject Title(Camera camera, int w, int h, bool alone, string words = "ABANDONED", float scale = 1f)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(UiContentBuilder.TitleFontPath);
            var root = new GameObject("StoreTitle") { layer = TitleLayer };
            float aspect = (float)w / h;
            Transform cam = camera.transform;
            float distance = 3f;
            float halfHeight = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance, halfWidth = halfHeight * aspect;
            root.transform.SetPositionAndRotation(cam.position + cam.forward * distance + (alone ? Vector3.zero : -cam.up * halfHeight * 0.55f), cam.rotation);
            float size = Mathf.Min(halfWidth * 1.7f / Mathf.Max(9, words.Length), halfHeight * 0.5f) * scale;
            for (int layerIndex = 0; layerIndex < 2; layerIndex++)
            {
                var text = new GameObject(layerIndex == 0 ? "Shadow" : "Face") { layer = TitleLayer };
                text.transform.SetParent(root.transform, false);
                text.transform.localPosition = layerIndex == 0 ? new Vector3(size * 0.06f, -size * 0.06f, 0.01f) : Vector3.zero;
                var mesh = text.AddComponent<TextMesh>();
                mesh.text = words;
                mesh.font = font;
                mesh.fontSize = 120;
                mesh.characterSize = size / 12f;
                mesh.anchor = TextAnchor.MiddleCenter;
                mesh.alignment = TextAlignment.Center;
                mesh.color = layerIndex == 0 ? new Color(0f, 0f, 0f, 0.85f) : new Color(0.95f, 0.75f, 0.16f);
                text.GetComponent<MeshRenderer>().sharedMaterial = WorldTextMaterial.For(font);
            }
            return root;
        }

        internal static Texture2D Grab(Camera camera, int w, int h, bool alpha)
        {
            var target = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            target.Create();
            camera.targetTexture = target;
            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
            bool supported = RenderPipeline.SupportsRenderRequest(camera, request);
            for (int frame = 0; frame < 2; frame++)
            {
                if (supported) RenderPipeline.SubmitRenderRequest(camera, request);
                else camera.Render();
            }
            RenderTexture previous = RenderTexture.active;
            var pixels = new Texture2D(w, h, alpha ? TextureFormat.RGBA32 : TextureFormat.RGB24, false);
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            pixels.Apply();
            RenderTexture.active = previous;
            camera.targetTexture = null;
            target.Release();
            Object.DestroyImmediate(target);
            return pixels;
        }

        // The title over the scene by its alpha (or alone, keeping its alpha, for the transparent logo).
        internal static Texture2D Over(Texture2D scene, Texture2D words, int w, int h)
        {
            var result = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] under = scene != null ? scene.GetPixels() : null, over = words != null ? words.GetPixels() : null;
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++)
            {
                Color b = under != null ? under[i] : new Color(0f, 0f, 0f, 0f);
                if (over == null) { px[i] = new Color(b.r, b.g, b.b, under != null ? 1f : b.a); continue; }
                Color t = over[i];
                px[i] = under != null ? new Color(Mathf.Lerp(b.r, t.r, t.a), Mathf.Lerp(b.g, t.g, t.a), Mathf.Lerp(b.b, t.b, t.a), 1f) : t;
            }
            result.SetPixels(px);
            result.Apply();
            return result;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Media;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.5 trailer, a draft: the roadmap cuts it from captured play, which needs people playing; until then this
    /// renders a 34 s, 1080p30 animatic from the saved mall (slow camera moves through staged shots, title cards
    /// between) and encodes it with the editor's own MediaEncoder (H.264 MP4, silent). Docs/StoreArt/trailer_draft.mp4.
    /// </summary>
    public static class Release0245
    {
        private const int W = 1920, H = 1080, Fps = 30;

        private readonly struct Move
        {
            public readonly Vector3 EyeFrom, EyeTo, Target;
            public readonly bool Power;
            public readonly int Torches;
            public readonly Release0244.Shot Props;
            public readonly float Seconds;
            public Move(Vector3 from, Vector3 to, Vector3 target, bool power, int torches, Release0244.Shot props, float seconds = 4.5f)
            { EyeFrom = from; EyeTo = to; Target = target; Power = power; Torches = torches; Props = props; Seconds = seconds; }
        }

        private const string P = "Assets/_Project/Prefabs/", L = P + "Loot/Loot_", T = P + "Threats/", Crew = P + "Player.prefab";

        private static readonly (string card, Move move)[] Script =
        {
            ("THE COMPANY NEEDS SALVAGE", new Move(new(28f, 1.7f, 3f), new(28f, 1.7f, 9f), new(28f, 3.4f, 26f), false, 4,
                new Release0244.Shot("a", default, default, false, 0, (T + "Hunter.prefab", new Vector3(28f, 0f, 22f), 200f)))),
            ("SOME OF IT WEIGHS A TON", new Move(new(18f, 5.7f, 26f), new(21f, 5.7f, 26f), new(36f, 4.6f, 26f), true, 0,
                new Release0244.Shot("b", default, default, false, 0, (L + "grand_piano.prefab", new Vector3(26f, 4.8f, 26f), 90f),
                    (Crew, new Vector3(24.6f, 4f, 26.9f), 90f), (Crew, new Vector3(27.4f, 4f, 25.1f), 270f)))),
            ("THE FLOORS DON'T LIKE IT", new Move(new(17f, 1.7f, 22f), new(20f, 1.2f, 24f), new(26f, 4.5f, 26f), true, 0,
                new Release0244.Shot("c", default, default, false, 0, (L + "grand_piano.prefab", new Vector3(26f, 4.8f, 26f), 90f)))),
            ("SOMETHING HEARS YOU", new Move(new(12f, 1.7f, 26f), new(10.4f, 1.7f, 26.4f), new(7f, 1.3f, 27f), false, 1,
                new Release0244.Shot("d", default, default, false, 0, (T + "Stalker.prefab", new Vector3(5.5f, 0f, 28f), 80f)))),
            ("THE TRUCK WON'T WAIT", new Move(new(26f, 1.7f, -22f), new(30f, 1.7f, -17f), new(36f, 1.6f, -11f), true, 0,
                new Release0244.Shot("e", default, default, false, 0, (Crew, new Vector3(35.2f, 0f, -6.5f), 180f), (Crew, new Vector3(36.6f, 0f, -6.7f), 180f)))),
        };

        [MenuItem("Tools/Abandoned/Store/Render Draft Trailer (0.24.5)")]
        public static void Author()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Docs/StoreArt"));
            Directory.CreateDirectory(root);
            string file = Path.Combine(root, "trailer_draft.mp4");
            if (File.Exists(file)) File.Delete(file);
            Scene scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            Transform mall = scene.GetRootGameObjects().First(g => g.name == "Mall").transform;
            var cameraObject = new GameObject("TrailerCamera") { hideFlags = HideFlags.HideAndDontSave };
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.07f;
            camera.farClipPlane = 160f;
            camera.allowHDR = true;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var liftObject = new GameObject("TrailerExposure") { hideFlags = HideFlags.HideAndDontSave };
            var lift = liftObject.AddComponent<Volume>();
            lift.isGlobal = true;
            lift.priority = 200f;
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.Add<ColorAdjustments>(true).postExposure.Override(0.8f);
            lift.sharedProfile = profile;
            var staged = new List<GameObject>();
            int frames = 0;
            var attributes = new VideoTrackAttributes
            {
                frameRate = new MediaRational(Fps), width = W, height = H, includeAlpha = false, bitRateMode = VideoBitrateMode.High,
            };
            try
            {
                using var state = new MeridianRenderState(mall, camera);
                using var encoder = new MediaEncoder(file, attributes);
                foreach ((string card, Move move) in Script)
                {
                    frames += Card(camera, encoder, card, 1.6f);
                    Release0244.Stage(move.Props, staged);
                    int count = Mathf.RoundToInt(move.Seconds * Fps);
                    for (int f = 0; f < count; f++)
                    {
                        float t = Mathf.SmoothStep(0f, 1f, f / (float)(count - 1));
                        Vector3 eye = Vector3.Lerp(move.EyeFrom, move.EyeTo, t);
                        state.Apply(new MeridianRenderView("trailer", eye, move.Target), move.Power, move.Torches);
                        Texture2D frame = Release0244.Grab(camera, W, H, true);
                        encoder.AddFrame(frame);
                        Object.DestroyImmediate(frame);
                        frames++;
                    }
                    Release0244.Clear(staged);
                }
                frames += Card(camera, encoder, "ABANDONED", 3f, 1.4f);
                frames += Card(camera, encoder, "WISHLIST ON STEAM", 2f);
            }
            finally
            {
                Release0244.Clear(staged);
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(liftObject);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // nothing saved
            }
            BuildScript.ApplyPlayerSettings();
            Debug.Log($"[Trailer0245] {frames} frames ({frames / (float)Fps:0.0} s) written to {file}.");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        // A title card: the words in the stencil face on black, held for a moment.
        private static int Card(Camera camera, MediaEncoder encoder, string words, float seconds, float scale = 0.7f)
        {
            int mask = camera.cullingMask;
            CameraClearFlags clear = camera.clearFlags;
            camera.transform.SetPositionAndRotation(new Vector3(0f, -200f, 0f), Quaternion.identity);
            GameObject title = Release0244.Title(camera, W, H, true, words, scale);
            camera.cullingMask = 1 << Release0244.TitleLayer;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            Texture2D frame = Release0244.Grab(camera, W, H, true);
            camera.cullingMask = mask;
            camera.clearFlags = clear;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            Object.DestroyImmediate(title);
            // Opaque black under the words for the video.
            Color[] px = frame.GetPixels();
            for (int i = 0; i < px.Length; i++) px[i] = new Color(px[i].r * px[i].a, px[i].g * px[i].a, px[i].b * px[i].a, 1f);
            frame.SetPixels(px);
            frame.Apply();
            int count = Mathf.RoundToInt(seconds * Fps);
            for (int f = 0; f < count; f++) encoder.AddFrame(frame);
            Object.DestroyImmediate(frame);
            return count;
        }
    }
}

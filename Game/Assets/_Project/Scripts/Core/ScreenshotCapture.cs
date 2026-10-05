using System.IO;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Renders a camera to a PNG in Game/Screenshots (gitignored). Used by batch-mode checks and
    /// tests so visuals can be reviewed without opening the editor.
    /// </summary>
    public static class ScreenshotCapture
    {
        public static string Folder => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Screenshots"));

        public static string Capture(Camera camera, string fileName, int width = 1280, int height = 720)
        {
            Directory.CreateDirectory(Folder);
            var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                string path = Path.Combine(Folder, fileName.EndsWith(".png") ? fileName : fileName + ".png");
                File.WriteAllBytes(path, image.EncodeToPNG());
                return path;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(image);
            }
        }

        /// <summary>Renders from a temporary camera at a pose, for viewpoints no scene camera has.</summary>
        public static string CaptureFrom(Vector3 position, Vector3 lookAt, string fileName, float fieldOfView = 70f)
        {
            var go = new GameObject("ScreenshotCamera");
            try
            {
                var camera = go.AddComponent<Camera>();
                camera.fieldOfView = fieldOfView;
                camera.nearClipPlane = 0.05f;
                go.transform.position = position;
                go.transform.LookAt(lookAt);
                return Capture(camera, fileName);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

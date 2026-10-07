using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Core
{
    /// <summary>
    /// Applies <see cref="VideoSettings"/> (UI step 5): the window, VSync and frame cap, the quality preset,
    /// the shadow distance (on a runtime copy of the render pipeline asset, so the project's asset is never
    /// edited) and a brightness volume. Made on first scene load; skipped in batch mode (tests, nettest).
    /// </summary>
    public class VideoApplier : MonoBehaviour
    {
        private static VideoApplier instance;
        private Volume brightness;
        private ColorAdjustments exposure;
        private UniversalRenderPipelineAsset runtimeAsset;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode || instance != null) return;
            var go = new GameObject("VideoSettings");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<VideoApplier>();
        }

        private void OnEnable()
        {
            VideoSettings.Changed += Apply;
            Apply();
        }

        private void OnDisable() => VideoSettings.Changed -= Apply;

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Apply()
        {
            // The window (not in the editor: the Game view owns its size).
            if (!Application.isEditor)
            {
                Vector2Int r = VideoSettings.Resolution;
                if (r.x <= 0 || r.y <= 0) r = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
                Screen.SetResolution(r.x, r.y, VideoSettings.WindowMode);
            }
            QualitySettings.vSyncCount = VideoSettings.VSync ? 1 : 0;
            Application.targetFrameRate = VideoSettings.VSync || VideoSettings.FrameCap <= 0 ? -1 : VideoSettings.FrameCap;
            int quality = VideoSettings.Quality;
            if (quality >= 0 && quality < QualitySettings.names.Length && quality != QualitySettings.GetQualityLevel())
            {
                QualitySettings.SetQualityLevel(quality, true);
                runtimeAsset = null; // the preset may bring its own pipeline asset
            }
            Shadows();
            Brightness();
        }

        private void Shadows()
        {
            var current = QualitySettings.renderPipeline as UniversalRenderPipelineAsset ?? GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (current == null) return;
            if (runtimeAsset == null || (current != runtimeAsset && current.name != runtimeAsset.name + " (runtime)"))
            {
                runtimeAsset = Instantiate(current);
                runtimeAsset.name = current.name + " (runtime)";
                QualitySettings.renderPipeline = runtimeAsset;
            }
            runtimeAsset.shadowDistance = VideoSettings.ShadowDistances[VideoSettings.Shadows];
        }

        private void Brightness()
        {
            if (brightness == null)
            {
                var go = new GameObject("BrightnessVolume");
                go.transform.SetParent(transform, false);
                brightness = go.AddComponent<Volume>();
                brightness.isGlobal = true;
                brightness.priority = 10f; // under night vision (50)
                VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
                exposure = profile.Add<ColorAdjustments>();
                exposure.postExposure.overrideState = true;
                brightness.sharedProfile = profile;
            }
            exposure.postExposure.value = VideoSettings.Brightness;
            brightness.weight = Mathf.Approximately(VideoSettings.Brightness, 0f) ? 0f : 1f;
        }
    }
}

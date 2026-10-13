using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Core
{
    /// <summary>
    /// The 0.23.4 video options, kept true every frame against levels that set their own atmosphere: fog off,
    /// bloom and film grain off (a top-priority volume), post-processing off on the main camera, texture detail,
    /// the monitor the window is on, and a small FPS counter.
    /// </summary>
    public class VideoExtras : MonoBehaviour
    {
        private Volume overrides;
        private Bloom bloom;
        private FilmGrain grain;
        private int shownMonitor = -1, shownTextures = -1;
        private float fps, nextFpsText;
        private UnityEngine.UIElements.Label fpsLabel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode) return;
            var go = new GameObject("VideoExtras");
            DontDestroyOnLoad(go);
            go.AddComponent<VideoExtras>();
        }

        private void LateUpdate()
        {
            if (!VideoSettings.Fog && RenderSettings.fog) RenderSettings.fog = false;
            Overrides();
            Camera cam = Camera.main;
            if (cam != null && cam.TryGetComponent(out UniversalAdditionalCameraData data) && data.renderPostProcessing != VideoSettings.PostProcessing)
                data.renderPostProcessing = VideoSettings.PostProcessing;
            if (shownTextures != VideoSettings.TextureQuality)
            {
                shownTextures = VideoSettings.TextureQuality;
                QualitySettings.globalTextureMipmapLimit = shownTextures;
            }
            if (shownMonitor != VideoSettings.Monitor) MoveToMonitor();
            Fps();
        }

        private void Overrides()
        {
            if (overrides == null)
            {
                overrides = gameObject.AddComponent<Volume>();
                overrides.isGlobal = true;
                overrides.priority = 100f;
                VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
                bloom = profile.Add<Bloom>();
                bloom.intensity.Override(0f);
                grain = profile.Add<FilmGrain>();
                grain.intensity.Override(0f);
                overrides.sharedProfile = profile;
            }
            bloom.active = !VideoSettings.Bloom;
            grain.active = !VideoSettings.Grain;
            overrides.weight = !VideoSettings.Bloom || !VideoSettings.Grain ? 1f : 0f;
        }

        private void MoveToMonitor()
        {
            shownMonitor = VideoSettings.Monitor;
            if (Application.isEditor) return;
            var displays = new List<DisplayInfo>();
            Screen.GetDisplayLayout(displays);
            if (shownMonitor >= displays.Count) return;
            DisplayInfo target = displays[shownMonitor];
            if (Screen.mainWindowDisplayInfo.Equals(target)) return;
            Screen.MoveMainWindowTo(target, Vector2Int.zero);
        }

        private void Fps()
        {
            fps = Mathf.Lerp(fps, Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f, 0.1f);
            UnityEngine.UIElements.VisualElement hud = UI.HudLayer.Root;
            if (!VideoSettings.ShowFps || hud == null)
            {
                if (fpsLabel != null) fpsLabel.style.display = UnityEngine.UIElements.DisplayStyle.None;
                return;
            }
            if (fpsLabel == null || fpsLabel.panel == null)
            {
                fpsLabel = UI.HudLayer.Label("fps-counter");
                fpsLabel.pickingMode = UnityEngine.UIElements.PickingMode.Ignore;
            }
            fpsLabel.style.display = UnityEngine.UIElements.DisplayStyle.Flex;
            if (Time.unscaledTime < nextFpsText) return;
            nextFpsText = Time.unscaledTime + 0.5f;
            fpsLabel.text = Loc.T("{0:0} FPS", fps);
        }
    }
}

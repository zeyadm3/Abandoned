using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.UI
{
    /// <summary>What the 3D view behind the menus should look like.</summary>
    public enum MenuBackdropMode { None, Title, Pause }

    /// <summary>
    /// A runtime URP volume over whatever camera is rendering behind the menus. Title: the drifting view
    /// darkened and drained, with grain, vignette and a little lens fringing. Pause: the live game (which
    /// keeps running) sinks further, loses its colour and blurs. Values ease between modes and the volume
    /// switches itself off when nothing is open, so play pays nothing for it.
    /// </summary>
    public class MenuPostEffects : MonoBehaviour
    {
        private const float NoBlurDistance = 60f;

        private Volume volume;
        private ColorAdjustments colour;
        private Vignette vignette;
        private FilmGrain grain;
        private ChromaticAberration fringe;
        private DepthOfField blur;
        private MenuBackdropMode mode;
        private float exposure, saturation, vignetteAmount, grainAmount, fringeAmount, blurEnd = NoBlurDistance;

        public MenuBackdropMode Mode
        {
            get => mode;
            set => mode = value;
        }

        private void Awake()
        {
            var holder = new GameObject("MenuPostVolume");
            holder.transform.SetParent(transform, false);
            volume = holder.AddComponent<Volume>();
            volume.isGlobal = true;
            // Above every level's own atmosphere volume; it only overrides what it sets.
            volume.priority = 100f;
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "MenuPost (runtime)";
            volume.sharedProfile = profile;
            // Only the parameters set below are overridden, so each level's own grading shows through.
            colour = profile.Add<ColorAdjustments>();
            vignette = profile.Add<Vignette>();
            vignette.smoothness.Override(0.45f);
            vignette.color.Override(Color.black);
            grain = profile.Add<FilmGrain>();
            grain.type.Override(FilmGrainLookup.Medium3);
            grain.response.Override(0.85f);
            fringe = profile.Add<ChromaticAberration>();
            blur = profile.Add<DepthOfField>();
            blur.mode.Override(DepthOfFieldMode.Gaussian);
            blur.gaussianStart.Override(0f);
            blur.highQualitySampling.Override(false);
            Apply();
        }

        private void OnDestroy()
        {
            if (volume != null && volume.sharedProfile != null) Destroy(volume.sharedProfile);
        }

        private void Update()
        {
            MenuEffectsConfig config = MenuEffectsConfig.Current;
            bool title = mode == MenuBackdropMode.Title, pause = mode == MenuBackdropMode.Pause;
            float ease = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 3f / Mathf.Max(0.01f, config.PostBlendSeconds));
            exposure = Mathf.Lerp(exposure, title ? config.TitleExposure : pause ? config.PauseExposure : 0f, ease);
            saturation = Mathf.Lerp(saturation, title ? config.TitleSaturation : pause ? config.PauseSaturation : 0f, ease);
            vignetteAmount = Mathf.Lerp(vignetteAmount, mode == MenuBackdropMode.None ? 0f : config.PostVignette, ease);
            grainAmount = Mathf.Lerp(grainAmount, mode == MenuBackdropMode.None ? 0f : config.PostGrain, ease);
            fringeAmount = Mathf.Lerp(fringeAmount, mode == MenuBackdropMode.None || MenuEffectsConfig.Calm ? 0f : config.PostChromatic, ease);
            blurEnd = Mathf.Lerp(blurEnd, pause ? config.PauseBlurEnd : NoBlurDistance, ease);
            Apply();
        }

        private void Apply()
        {
            if (volume == null) return;
            MenuEffectsConfig config = MenuEffectsConfig.Current;
            bool any = mode != MenuBackdropMode.None || Mathf.Abs(exposure) > 0.01f || Mathf.Abs(saturation) > 0.5f
                       || vignetteAmount > 0.005f || grainAmount > 0.005f || blurEnd < NoBlurDistance - 1f;
            volume.enabled = any;
            if (!any) return;
            colour.postExposure.Override(exposure);
            colour.saturation.Override(saturation);
            vignette.intensity.Override(vignetteAmount);
            grain.intensity.Override(grainAmount);
            fringe.intensity.Override(fringeAmount);
            // Gaussian depth of field is the only costly part: it runs only while something is blurred.
            bool blurring = blurEnd < NoBlurDistance - 1f;
            blur.active = blurring;
            blur.gaussianEnd.Override(blurEnd);
            blur.gaussianMaxRadius.Override(config.PauseBlurRadius);
        }
    }
}

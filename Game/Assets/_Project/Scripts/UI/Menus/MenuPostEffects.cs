using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abandoned.UI
{
    /// <summary>What the 3D view behind the menus should look like.</summary>
    public enum MenuBackdropMode { None, Title, Pause }

    /// <summary>
    /// A runtime URP volume over whatever camera is rendering behind the menus: darkened, drained and
    /// vignetted (no blur, grain or fringing: the pause blur cost a full-screen pass in a live game). Values
    /// ease between modes and the volume switches itself off when nothing is open, so play pays nothing for it.
    /// </summary>
    public class MenuPostEffects : MonoBehaviour
    {
        private Volume volume;
        private ColorAdjustments colour;
        private Vignette vignette;
        private MenuBackdropMode mode;
        private float exposure, saturation, vignetteAmount;

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
            Apply();
        }

        private void Apply()
        {
            if (volume == null) return;
            bool any = mode != MenuBackdropMode.None || Mathf.Abs(exposure) > 0.01f || Mathf.Abs(saturation) > 0.5f
                       || vignetteAmount > 0.005f;
            volume.enabled = any;
            if (!any) return;
            colour.postExposure.Override(exposure);
            colour.saturation.Override(saturation);
            vignette.intensity.Override(vignetteAmount);
        }
    }
}

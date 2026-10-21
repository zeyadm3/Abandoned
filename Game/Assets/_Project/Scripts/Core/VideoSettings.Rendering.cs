using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Video options added with the 0.24.12 renderer: volumetric light, the upscaler used below 100 % render
    /// scale, eye adaptation and motion blur. Read every frame by the renderer features (nothing is baked from them).
    /// </summary>
    public static partial class VideoSettings
    {
        public static readonly string[] VolumetricNames = { "Off", "Low", "Medium", "High" }; // @loc:quality
        public static readonly string[] UpscalerNames = { "STP", "FSR", "Bilinear" }; // @loc

        /// <summary>
        /// Per <see cref="VolumetricNames"/> level: the march's resolution divisor and its steps. Every step loops over
        /// the lights around it, so the pixel count is what costs: Medium marches a ninth of the screen.
        /// </summary>
        public static readonly int[] VolumetricDivisor = { 0, 4, 3, 2 };
        public static readonly int[] VolumetricSteps = { 0, 16, 24, 32 };

        // Read by the renderer features for every camera every frame, so through the settings cache, not Prefs.
        /// <summary>Light beams and lit dust. Off falls back to the old painted light shafts.</summary>
        public static int Volumetrics
        {
            get => Mathf.Clamp(GetInt("video.volumetrics", 2), 0, VolumetricNames.Length - 1);
            set { SetInt("video.volumetrics", Mathf.Clamp(value, 0, VolumetricNames.Length - 1)); Changed?.Invoke(); }
        }

        public static int Upscaler
        {
            get => Mathf.Clamp(GetInt("video.upscaler", 0), 0, UpscalerNames.Length - 1);
            set { SetInt("video.upscaler", Mathf.Clamp(value, 0, UpscalerNames.Length - 1)); Changed?.Invoke(); }
        }

        /// <summary>The eye adjusting between daylight and dark halls (AutoExposureFeature).</summary>
        public static bool EyeAdaptation
        {
            get => GetInt("video.eyeadapt", 1) == 1;
            set { SetInt("video.eyeadapt", value ? 1 : 0); Changed?.Invoke(); }
        }

        public static bool MotionBlur
        {
            get => GetInt("video.motionblur", 0) == 1;
            set { SetInt("video.motionblur", value ? 1 : 0); Changed?.Invoke(); }
        }
    }
}

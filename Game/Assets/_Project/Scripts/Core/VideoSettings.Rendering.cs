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

        /// <summary>Per <see cref="VolumetricNames"/> level: the march's resolution divisor and its steps.</summary>
        public static readonly int[] VolumetricDivisor = { 0, 4, 2, 2 };
        public static readonly int[] VolumetricSteps = { 0, 20, 28, 44 };

        /// <summary>Light beams and lit dust. Off falls back to the old painted light shafts.</summary>
        public static int Volumetrics
        {
            get => Mathf.Clamp(Prefs.GetInt("video.volumetrics", 2), 0, VolumetricNames.Length - 1);
            set { Prefs.SetInt("video.volumetrics", Mathf.Clamp(value, 0, VolumetricNames.Length - 1)); Changed?.Invoke(); }
        }

        public static int Upscaler
        {
            get => Mathf.Clamp(Prefs.GetInt("video.upscaler", 0), 0, UpscalerNames.Length - 1);
            set { Prefs.SetInt("video.upscaler", Mathf.Clamp(value, 0, UpscalerNames.Length - 1)); Changed?.Invoke(); }
        }

        /// <summary>The eye adjusting between daylight and dark halls (AutoExposureFeature).</summary>
        public static bool EyeAdaptation { get => Flag("video.eyeadapt", true); set => SetFlag("video.eyeadapt", value); }

        public static bool MotionBlur { get => Flag("video.motionblur", false); set => SetFlag("video.motionblur", value); }
    }
}

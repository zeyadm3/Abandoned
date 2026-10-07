using System;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// The player's video settings (UI step 5): window mode, resolution, VSync, frame cap, quality preset,
    /// shadow distance, brightness. Kept in Prefs and applied by <see cref="VideoApplier"/> at startup and
    /// whenever they change. Nothing here touches project assets, so the editor stays clean.
    /// </summary>
    public static class VideoSettings
    {
        public static readonly int[] FrameCaps = { 0, 30, 60, 90, 120, 144, 240 };
        public static readonly string[] ShadowNames = { "Off", "Low", "Medium", "High" };
        public static readonly float[] ShadowDistances = { 0f, 15f, 25f, 35f };
        public static readonly string[] AntiAliasingNames = { "Off", "MSAA 2x", "MSAA 4x" };
        public static readonly int[] AntiAliasingSamples = { 1, 2, 4 };
        public const float MinRenderScale = 0.5f;

        private const string ModeKey = "video.mode", WidthKey = "video.width", HeightKey = "video.height", VsyncKey = "video.vsync",
            CapKey = "video.cap", QualityKey = "video.quality", ShadowsKey = "video.shadows", BrightnessKey = "video.brightness",
            ScaleKey = "video.renderscale", AaKey = "video.aa";

        public static event Action Changed;

        public static FullScreenMode WindowMode
        {
            get => (FullScreenMode)Prefs.GetInt(ModeKey, (int)FullScreenMode.FullScreenWindow);
            set { Prefs.SetInt(ModeKey, (int)value); Changed?.Invoke(); }
        }

        /// <summary>0 x 0 = the desktop's resolution.</summary>
        public static Vector2Int Resolution
        {
            get => new(Prefs.GetInt(WidthKey, 0), Prefs.GetInt(HeightKey, 0));
            set { Prefs.SetInt(WidthKey, value.x); Prefs.SetInt(HeightKey, value.y); Changed?.Invoke(); }
        }

        public static bool VSync
        {
            get => Prefs.GetInt(VsyncKey, 1) == 1;
            set { Prefs.SetInt(VsyncKey, value ? 1 : 0); Changed?.Invoke(); }
        }

        /// <summary>Frames per second when VSync is off; 0 = unlimited.</summary>
        public static int FrameCap
        {
            get => Prefs.GetInt(CapKey, 0);
            set { Prefs.SetInt(CapKey, Mathf.Max(0, value)); Changed?.Invoke(); }
        }

        /// <summary>Index into QualitySettings.names; -1 = whatever the build ships with.</summary>
        public static int Quality
        {
            get => Prefs.GetInt(QualityKey, -1);
            set { Prefs.SetInt(QualityKey, value); Changed?.Invoke(); }
        }

        /// <summary>Index into <see cref="ShadowNames"/>; High is the shipped shadow budget (M7.6).</summary>
        public static int Shadows
        {
            get => Mathf.Clamp(Prefs.GetInt(ShadowsKey, ShadowNames.Length - 1), 0, ShadowNames.Length - 1);
            set { Prefs.SetInt(ShadowsKey, Mathf.Clamp(value, 0, ShadowNames.Length - 1)); Changed?.Invoke(); }
        }

        /// <summary>Exposure offset in stops, -1..+1.5 (power-off jobs can be very dark on some screens).</summary>
        public static float Brightness
        {
            get => Prefs.GetFloat(BrightnessKey, 0f);
            set { Prefs.SetFloat(BrightnessKey, Mathf.Clamp(value, -1f, 1.5f)); Changed?.Invoke(); }
        }

        /// <summary>3D resolution as a fraction of the window (UI stays sharp); lower is faster on weak GPUs.</summary>
        public static float RenderScale
        {
            get => Mathf.Clamp(Prefs.GetFloat(ScaleKey, 1f), MinRenderScale, 1f);
            set { Prefs.SetFloat(ScaleKey, Mathf.Clamp(value, MinRenderScale, 1f)); Changed?.Invoke(); }
        }

        /// <summary>Index into <see cref="AntiAliasingNames"/>.</summary>
        public static int AntiAliasing
        {
            get => Mathf.Clamp(Prefs.GetInt(AaKey, 1), 0, AntiAliasingNames.Length - 1);
            set { Prefs.SetInt(AaKey, Mathf.Clamp(value, 0, AntiAliasingNames.Length - 1)); Changed?.Invoke(); }
        }

        /// <summary>Back to the shipped look and window (Settings > Restore defaults on the Video/Graphics pages).</summary>
        public static void ResetAll()
        {
            foreach (string key in new[] { ModeKey, WidthKey, HeightKey, VsyncKey, CapKey, QualityKey, ShadowsKey, BrightnessKey, ScaleKey, AaKey })
                Prefs.Delete(key);
            Changed?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Changed = null;
    }
}

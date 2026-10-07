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

        private const string ModeKey = "video.mode", WidthKey = "video.width", HeightKey = "video.height", VsyncKey = "video.vsync",
            CapKey = "video.cap", QualityKey = "video.quality", ShadowsKey = "video.shadows", BrightnessKey = "video.brightness";

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Changed = null;
    }
}

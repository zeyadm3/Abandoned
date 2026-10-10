using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>Video options added in 0.23.4 (applied by VideoApplier and VideoExtras).</summary>
    public static partial class VideoSettings
    {
        public static readonly string[] TextureNames = { "Full", "Half", "Quarter" };

        private static bool Flag(string key, bool fallback) => Prefs.GetInt(key, fallback ? 1 : 0) == 1;
        private static void SetFlag(string key, bool value)
        {
            Prefs.SetInt(key, value ? 1 : 0);
            Changed?.Invoke();
        }

        /// <summary>Which display the game window is on (0 = the main one).</summary>
        public static int Monitor { get => Prefs.GetInt("video.monitor", 0); set { Prefs.SetInt("video.monitor", Mathf.Max(0, value)); Changed?.Invoke(); } }

        /// <summary>Texture detail: 0 full, 1 half, 2 quarter (mipmap limit).</summary>
        public static int TextureQuality { get => Prefs.GetInt("video.textures", 0); set { Prefs.SetInt("video.textures", Mathf.Clamp(value, 0, 2)); Changed?.Invoke(); } }

        public static bool Fog { get => Flag("video.fog", true); set => SetFlag("video.fog", value); }
        public static bool Bloom { get => Flag("video.bloom", true); set => SetFlag("video.bloom", value); }
        public static bool Grain { get => Flag("video.grain", true); set => SetFlag("video.grain", value); }
        /// <summary>All post-processing (also drops the low-health vision, night vision tint, brightness).</summary>
        public static bool PostProcessing { get => Flag("video.post", true); set => SetFlag("video.post", value); }
        public static bool ShowFps { get => Flag("video.fps", false); set => SetFlag("video.fps", value); }
    }
}

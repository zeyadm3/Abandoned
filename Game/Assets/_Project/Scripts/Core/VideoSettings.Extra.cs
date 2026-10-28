using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>Video options added in 0.23.4 (applied by VideoApplier and VideoExtras).</summary>
    public static partial class VideoSettings
    {
        public static readonly string[] TextureNames = { "Full", "Half", "Quarter" }; // @loc

        private const string MonitorKey = "video.monitor", TexturesKey = "video.textures", FogKey = "video.fog", BloomKey = "video.bloom",
            GrainKey = "video.grain", PostKey = "video.post", FpsKey = "video.fps";

        // VideoExtras reads these every frame: through the settings cache, not Prefs.
        private static bool Flag(string key, bool fallback) => GetInt(key, fallback ? 1 : 0) == 1;
        private static void SetFlag(string key, bool value)
        {
            SetInt(key, value ? 1 : 0);
            Changed?.Invoke();
        }

        /// <summary>Which display the game window is on (0 = the main one).</summary>
        public static int Monitor { get => Mathf.Max(0, GetInt(MonitorKey, 0)); set { SetInt(MonitorKey, Mathf.Max(0, value)); Changed?.Invoke(); } }

        /// <summary>Texture detail: 0 full, 1 half, 2 quarter (mipmap limit).</summary>
        public static int TextureQuality { get => Mathf.Clamp(GetInt(TexturesKey, 0), 0, 2); set { SetInt(TexturesKey, Mathf.Clamp(value, 0, 2)); Changed?.Invoke(); } }

        public static bool Fog { get => Flag(FogKey, true); set => SetFlag(FogKey, value); }
        public static bool Bloom { get => Flag(BloomKey, true); set => SetFlag(BloomKey, value); }
        public static bool Grain { get => Flag(GrainKey, true); set => SetFlag(GrainKey, value); }
        /// <summary>All post-processing (also drops the low-health vision, night vision tint, brightness).</summary>
        public static bool PostProcessing { get => Flag(PostKey, true); set => SetFlag(PostKey, value); }
        public static bool ShowFps { get => Flag(FpsKey, false); set => SetFlag(FpsKey, value); }
    }
}

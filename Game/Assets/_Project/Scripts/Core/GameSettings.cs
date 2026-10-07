using System;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// The player's own settings (GDD 20) that aren't sound or voice: look, camera comfort and
    /// accessibility. Kept in PlayerPrefs and cached; <see cref="Changed"/> lets live objects re-apply.
    /// Designer tunables stay in ScriptableObject configs; these only scale or switch them.
    /// </summary>
    public static class GameSettings
    {
        public const float MinSensitivity = 0.25f, MaxSensitivity = 3f;
        public const float MinFov = 60f, MaxFov = 100f, DefaultFov = 75f;

        private const string SensitivityKey = "settings.sensitivity", FovKey = "settings.fov", BobKey = "settings.headbob",
            ShakeKey = "settings.shake", SubtitlesKey = "settings.subtitles", ColorblindKey = "settings.colorblind";

        private static float? sensitivity, fov;
        private static bool? headBob, shake, subtitles, colorblind;

        public static event Action Changed;

        /// <summary>Multiplies the configured mouse sensitivity.</summary>
        public static float Sensitivity
        {
            get => sensitivity ??= PlayerPrefs.GetFloat(SensitivityKey, 1f);
            set => Set(ref sensitivity, SensitivityKey, Mathf.Clamp(value, MinSensitivity, MaxSensitivity));
        }

        public static float FieldOfView
        {
            get => fov ??= PlayerPrefs.GetFloat(FovKey, DefaultFov);
            set => Set(ref fov, FovKey, Mathf.Clamp(value, MinFov, MaxFov));
        }

        public static bool HeadBob
        {
            get => headBob ??= PlayerPrefs.GetInt(BobKey, 1) == 1;
            set => Set(ref headBob, BobKey, value);
        }

        public static bool CameraShake
        {
            get => shake ??= PlayerPrefs.GetInt(ShakeKey, 1) == 1;
            set => Set(ref shake, ShakeKey, value);
        }

        /// <summary>Captions for structural warnings and threat sounds.</summary>
        public static bool Subtitles
        {
            get => subtitles ??= PlayerPrefs.GetInt(SubtitlesKey, 0) == 1;
            set => Set(ref subtitles, SubtitlesKey, value);
        }

        /// <summary>The stress scanner uses a colourblind-safe palette (blue / orange / vermilion).</summary>
        public static bool ColorblindScanner
        {
            get => colorblind ??= PlayerPrefs.GetInt(ColorblindKey, 0) == 1;
            set => Set(ref colorblind, ColorblindKey, value);
        }

        /// <summary>Back to defaults (tests; a "reset" button later).</summary>
        public static void ResetAll()
        {
            foreach (string key in new[] { SensitivityKey, FovKey, BobKey, ShakeKey, SubtitlesKey, ColorblindKey }) PlayerPrefs.DeleteKey(key);
            sensitivity = fov = null;
            headBob = shake = subtitles = colorblind = null;
            Changed?.Invoke();
        }

        private static void Set(ref float? cache, string key, float value)
        {
            cache = value;
            PlayerPrefs.SetFloat(key, value);
            Changed?.Invoke();
        }

        private static void Set(ref bool? cache, string key, bool value)
        {
            cache = value;
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            Changed?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            sensitivity = fov = null;
            headBob = shake = subtitles = colorblind = null;
            Changed = null;
        }
    }
}

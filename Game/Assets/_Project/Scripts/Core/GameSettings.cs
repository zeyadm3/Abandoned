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
            ShakeKey = "settings.shake", SubtitlesKey = "settings.subtitles", ColorblindKey = "settings.colorblind", UiScaleKey = "settings.uiscale",
            ReduceMenuEffectsKey = "settings.reducemenufx";
        public const float MinUiScale = 0.75f, MaxUiScale = 1.5f;

        private static float? sensitivity, fov, uiScale;
        private static bool? headBob, shake, subtitles, colorblind, reduceMenuEffects;

        public static event Action Changed;

        /// <summary>Multiplies the configured mouse sensitivity.</summary>
        public static float Sensitivity
        {
            get => sensitivity ??= Prefs.GetFloat(SensitivityKey, 1f);
            set => Set(ref sensitivity, SensitivityKey, Mathf.Clamp(value, MinSensitivity, MaxSensitivity));
        }

        public static float FieldOfView
        {
            get => fov ??= Prefs.GetFloat(FovKey, DefaultFov);
            set => Set(ref fov, FovKey, Mathf.Clamp(value, MinFov, MaxFov));
        }

        public static bool HeadBob
        {
            get => headBob ??= Prefs.GetInt(BobKey, 1) == 1;
            set => Set(ref headBob, BobKey, value);
        }

        public static bool CameraShake
        {
            get => shake ??= Prefs.GetInt(ShakeKey, 1) == 1;
            set => Set(ref shake, ShakeKey, value);
        }

        /// <summary>Captions for structural warnings and threat sounds.</summary>
        public static bool Subtitles
        {
            get => subtitles ??= Prefs.GetInt(SubtitlesKey, 0) == 1;
            set => Set(ref subtitles, SubtitlesKey, value);
        }

        /// <summary>The stress scanner uses a colourblind-safe palette (blue / orange / vermilion).</summary>
        public static bool ColorblindScanner
        {
            get => colorblind ??= Prefs.GetInt(ColorblindKey, 0) == 1;
            set => Set(ref colorblind, ColorblindKey, value);
        }

        /// <summary>
        /// Calms the menus for players sensitive to flashing: no flicker, glitches, jitter, shakes or static
        /// cuts (the dark, grain and blur stay; nothing in them flashes).
        /// </summary>
        public static bool ReduceMenuEffects
        {
            get => reduceMenuEffects ??= Prefs.GetInt(ReduceMenuEffectsKey, 0) == 1;
            set => Set(ref reduceMenuEffects, ReduceMenuEffectsKey, value);
        }

        /// <summary>How big the menus and HUD are (UI step 5): 1 = designed size at 1080p, scaled with the screen.</summary>
        public static float UiScale
        {
            get => uiScale ??= Prefs.GetFloat(UiScaleKey, 1f);
            set => Set(ref uiScale, UiScaleKey, Mathf.Clamp(value, MinUiScale, MaxUiScale));
        }

        /// <summary>Back to defaults (tests; a "reset" button later).</summary>
        public static void ResetAll()
        {
            foreach (string key in new[] { SensitivityKey, FovKey, BobKey, ShakeKey, SubtitlesKey, ColorblindKey, UiScaleKey, ReduceMenuEffectsKey }) Prefs.Delete(key);
            Prefs.Save(); // a delete that isn't flushed can come back next launch
            sensitivity = fov = uiScale = null;
            headBob = shake = subtitles = colorblind = reduceMenuEffects = null;
            Changed?.Invoke();
        }

        private static void Set(ref float? cache, string key, float value)
        {
            cache = value;
            Prefs.SetFloat(key, value);
            Changed?.Invoke();
        }

        private static void Set(ref bool? cache, string key, bool value)
        {
            cache = value;
            Prefs.SetInt(key, value ? 1 : 0);
            Changed?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            sensitivity = fov = null;
            headBob = shake = subtitles = colorblind = reduceMenuEffects = null;
            Changed = null;
        }
    }
}

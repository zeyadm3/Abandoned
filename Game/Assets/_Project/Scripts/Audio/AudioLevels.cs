using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Player volume settings by category (master, effects, ambience), kept in PlayerPrefs. Voice has
    /// its own volume (VoiceSettings). The settings menu (M7.4) edits these.
    /// </summary>
    public static class AudioLevels
    {
        private const string MasterKey = "audio.master", SfxKey = "audio.sfx", AmbienceKey = "audio.ambience";

        public static float Master
        {
            get => PlayerPrefs.GetFloat(MasterKey, 1f);
            set { PlayerPrefs.SetFloat(MasterKey, Mathf.Clamp01(value)); AudioListener.volume = Master; }
        }

        // Read on every sound, so cached (PlayerPrefs reads aren't free).
        private static float? sfx, ambience;

        public static float Sfx
        {
            get => sfx ??= PlayerPrefs.GetFloat(SfxKey, 1f);
            set { sfx = Mathf.Clamp01(value); PlayerPrefs.SetFloat(SfxKey, sfx.Value); }
        }

        public static float Ambience
        {
            get => ambience ??= PlayerPrefs.GetFloat(AmbienceKey, 1f);
            set { ambience = Mathf.Clamp01(value); PlayerPrefs.SetFloat(AmbienceKey, ambience.Value); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyMaster()
        {
            sfx = ambience = null;
            AudioListener.volume = Master;
        }
    }
}

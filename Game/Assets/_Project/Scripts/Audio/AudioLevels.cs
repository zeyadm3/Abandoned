using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Player volume settings by category (master, effects, ambience, music), kept in Prefs. Voice has
    /// its own volume (VoiceSettings). The settings menu (M7.4) edits these.
    /// </summary>
    public static class AudioLevels
    {
        private const string MasterKey = "audio.master", SfxKey = "audio.sfx", AmbienceKey = "audio.ambience", MusicKey = "audio.music";

        public static float Master
        {
            get => Prefs.GetFloat(MasterKey, 1f);
            set { Prefs.SetFloat(MasterKey, Mathf.Clamp01(value)); AudioListener.volume = Master; }
        }

        // Read on every sound, so cached (PlayerPrefs reads aren't free).
        private static float? sfx, ambience, music;

        public static float Sfx
        {
            get => sfx ??= Prefs.GetFloat(SfxKey, 1f);
            set { sfx = Mathf.Clamp01(value); Prefs.SetFloat(SfxKey, sfx.Value); }
        }

        public static float Ambience
        {
            get => ambience ??= Prefs.GetFloat(AmbienceKey, 1f);
            set { ambience = Mathf.Clamp01(value); Prefs.SetFloat(AmbienceKey, ambience.Value); }
        }

        public static float Music
        {
            get => music ??= Prefs.GetFloat(MusicKey, 0.7f);
            set { music = Mathf.Clamp01(value); Prefs.SetFloat(MusicKey, music.Value); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyMaster()
        {
            sfx = ambience = music = null;
            AudioListener.volume = Master;
        }
    }
}

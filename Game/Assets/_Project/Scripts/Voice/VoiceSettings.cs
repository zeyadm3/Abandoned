using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// This player's own voice choices (not tuning): push-to-talk or open mic, how loud others are,
    /// and mute. Saved in PlayerPrefs until the settings menu and save system arrive (M6/M7).
    /// </summary>
    public static class VoiceSettings
    {
        private const string ModeKey = "voice.mode", VolumeKey = "voice.volume", MuteKey = "voice.mute";

        private static bool loaded;
        private static VoiceMode mode;
        private static float volume = 1f;
        private static bool muted;

        public static VoiceMode Mode
        {
            get { Load(); return mode; }
            set { Load(); mode = value; Save(); }
        }

        /// <summary>Everyone else's voices, 0..1.</summary>
        public static float Volume
        {
            get { Load(); return volume; }
            set { Load(); volume = Mathf.Clamp01(value); Save(); }
        }

        /// <summary>Never send our microphone.</summary>
        public static bool MicMuted
        {
            get { Load(); return muted; }
            set { Load(); muted = value; Save(); }
        }

        private static void Load()
        {
            if (loaded) return;
            loaded = true;
            // Tests and headless runs must not inherit whatever a player chose on this machine.
            if (Application.isBatchMode) return;
            mode = (VoiceMode)PlayerPrefs.GetInt(ModeKey, (int)VoiceMode.PushToTalk);
            volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
            muted = PlayerPrefs.GetInt(MuteKey, 0) != 0;
        }

        /// <summary>Writes the choices to disk (PlayerPrefs only flushes on a clean quit otherwise).</summary>
        public static void Flush()
        {
            if (!Application.isBatchMode) PlayerPrefs.Save();
        }

        private static void Save()
        {
            if (Application.isBatchMode) return;
            PlayerPrefs.SetInt(ModeKey, (int)mode);
            PlayerPrefs.SetFloat(VolumeKey, volume);
            PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            loaded = false;
            mode = VoiceMode.PushToTalk;
            volume = 1f;
            muted = false;
        }
    }
}

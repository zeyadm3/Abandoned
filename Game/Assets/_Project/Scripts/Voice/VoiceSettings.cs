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

        // Per crewmate (UI step 4, the pause menu's crew list): for this session only, by client id.
        private static readonly System.Collections.Generic.Dictionary<ulong, float> playerVolume = new();
        private static readonly System.Collections.Generic.HashSet<ulong> playerMuted = new();

        /// <summary>One crewmate's voice, 0..2 (on top of <see cref="Volume"/>); 1 by default.</summary>
        public static float PlayerVolume(ulong clientId) => playerVolume.TryGetValue(clientId, out float v) ? v : 1f;

        public static void SetPlayerVolume(ulong clientId, float value) => playerVolume[clientId] = Mathf.Clamp(value, 0f, 2f);

        public static bool PlayerMuted(ulong clientId) => playerMuted.Contains(clientId);

        public static void SetPlayerMuted(ulong clientId, bool mute)
        {
            if (mute) playerMuted.Add(clientId);
            else playerMuted.Remove(clientId);
        }

        /// <summary>What a crewmate's voice is multiplied by on this machine (0 when muted).</summary>
        public static float PlayerGain(ulong clientId) => PlayerMuted(clientId) ? 0f : PlayerVolume(clientId);

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

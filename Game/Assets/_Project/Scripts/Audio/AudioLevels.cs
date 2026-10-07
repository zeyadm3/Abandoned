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
        private static int duckFrame = -1;
        private static float backgroundDuck = 1f;

        /// <summary>Ease background sound beneath audible crew speech, without softening floor warnings.</summary>
        public static float BackgroundDuck
        {
            get
            {
                if (duckFrame == Time.frameCount) return backgroundDuck;
                duckFrame = Time.frameCount;
                bool audible = false;
                foreach (Voice.NetworkVoice speaker in Voice.NetworkVoice.All)
                {
                    if (speaker == null || speaker.IsOwner || !speaker.IsSpeaking || Voice.VoiceSettings.Volume * Voice.VoiceSettings.PlayerGain(speaker.OwnerClientId) <= 0.01f) continue;
                    bool radio = speaker.IsOnRadio && speaker.Radio != null && speaker.Radio.isActiveAndEnabled &&
                                 Networking.NetworkPlayer.Local != null && speaker.HasRadio(Networking.NetworkPlayer.Local.OwnerClientId);
                    bool nearby = speaker.Playback != null && speaker.Playback.isActiveAndEnabled && speaker.Playback.Gain > 0.08f;
                    if (radio || nearby) { audible = true; break; }
                }
                float target = audible ? 0.65f : 1f;
                backgroundDuck = Mathf.MoveTowards(backgroundDuck, target, Time.unscaledDeltaTime * (audible ? 3f : 1.2f));
                return backgroundDuck;
            }
        }

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
            duckFrame = -1;
            backgroundDuck = 1f;
            AudioListener.volume = Master;
        }
    }
}

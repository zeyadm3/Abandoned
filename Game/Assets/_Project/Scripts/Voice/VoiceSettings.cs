using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// This player's own voice choices (not tuning): push-to-talk or open mic, how loud others are, the
    /// microphone, and mute. Kept through <see cref="Prefs"/> like every other setting (in memory in batch
    /// mode). Per-crewmate volume and mute are remembered by that person's Steam id (or name off Steam),
    /// so they survive rejoins and later sessions (QA B-25).
    /// </summary>
    public static class VoiceSettings
    {
        private const string ModeKey = "voice.mode", VolumeKey = "voice.volume", MuteKey = "voice.mute", DeviceKey = "voice.device",
            PlayerKey = "voice.player.";

        private static bool loaded;
        private static VoiceMode mode;
        private static float volume = 1f;
        private static bool muted;

        // Identity -> (volume, muted), read from Prefs on first use.
        private static readonly System.Collections.Generic.Dictionary<string, (float volume, bool muted)> players = new();

        /// <summary>Resolves a client to a stable identity (the networking layer sets this); null = this session only.</summary>
        public static System.Func<ulong, string> IdentityOf { get; set; } = id => Networking.NetworkPlayer.IdentityOf(id);

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

        /// <summary>One crewmate's voice, 0..2 (on top of <see cref="Volume"/>); 1 by default.</summary>
        public static float PlayerVolume(ulong clientId) => Player(clientId).volume;

        public static void SetPlayerVolume(ulong clientId, float value)
        {
            (float _, bool mute) = Player(clientId);
            SetPlayer(clientId, Mathf.Clamp(value, 0f, 2f), mute);
        }

        public static bool PlayerMuted(ulong clientId) => Player(clientId).muted;

        public static void SetPlayerMuted(ulong clientId, bool mute)
        {
            (float v, bool _) = Player(clientId);
            SetPlayer(clientId, v, mute);
        }

        /// <summary>What a crewmate's voice is multiplied by on this machine (0 when muted).</summary>
        public static float PlayerGain(ulong clientId)
        {
            (float v, bool mute) = Player(clientId);
            return mute ? 0f : v;
        }

        private static string KeyOf(ulong clientId)
        {
            string identity = IdentityOf?.Invoke(clientId);
            return string.IsNullOrEmpty(identity) ? "client." + clientId : identity;
        }

        private static (float volume, bool muted) Player(ulong clientId)
        {
            string key = KeyOf(clientId);
            if (players.TryGetValue(key, out var p)) return p;
            p = (Prefs.GetFloat(PlayerKey + key + ".volume", 1f), Prefs.GetInt(PlayerKey + key + ".mute", 0) == 1);
            players[key] = p;
            return p;
        }

        private static void SetPlayer(ulong clientId, float v, bool mute)
        {
            string key = KeyOf(clientId);
            players[key] = (v, mute);
            // Session-only ids aren't worth keeping on disk.
            if (key.StartsWith("client.")) return;
            Prefs.SetFloat(PlayerKey + key + ".volume", v);
            Prefs.SetInt(PlayerKey + key + ".mute", mute ? 1 : 0);
        }

        /// <summary>
        /// The microphone to use with the built-in voice (UI step 5); "" = the system default. Steam voice
        /// always uses the system's default device. Applies the next time a game starts.
        /// </summary>
        public static string MicDevice
        {
            get => Prefs.GetString(DeviceKey, "");
            set => Prefs.SetString(DeviceKey, value ?? "");
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
            mode = (VoiceMode)Prefs.GetInt(ModeKey, (int)VoiceMode.PushToTalk);
            volume = Prefs.GetFloat(VolumeKey, 1f);
            muted = Prefs.GetInt(MuteKey, 0) != 0;
        }

        /// <summary>Writes the choices to disk (PlayerPrefs only flushes on a clean quit otherwise).</summary>
        public static void Flush() => Prefs.Save();

        private static void Save()
        {
            Prefs.SetInt(ModeKey, (int)mode);
            Prefs.SetFloat(VolumeKey, volume);
            Prefs.SetInt(MuteKey, muted ? 1 : 0);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            loaded = false;
            mode = VoiceMode.PushToTalk;
            volume = 1f;
            muted = false;
            players.Clear();
            IdentityOf = id => Networking.NetworkPlayer.IdentityOf(id);
        }
    }
}

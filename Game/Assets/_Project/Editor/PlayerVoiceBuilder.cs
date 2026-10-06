using Abandoned.Player;
using Abandoned.Voice;
using UnityEngine;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Adds proximity voice to the Player prefab: NetworkVoice (send/relay/receive), a "Voice" child at
    /// eye height with the AudioSource + VoicePlayback (remote copies speak from their head), and the
    /// owner-only VoiceTransmitter + VoiceHud.
    /// </summary>
    public static class PlayerVoiceBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Voice/VoiceConfig.asset";

        public readonly struct Result
        {
            public readonly Behaviour Transmitter, Hud;

            public Result(Behaviour transmitter, Behaviour hud)
            {
                Transmitter = transmitter;
                Hud = hud;
            }
        }

        public static Result Add(GameObject root, Transform cameraRoot, PlayerInputReader reader)
        {
            var config = LoadOrCreateAsset<VoiceConfig>(ConfigPath);

            var mouth = new GameObject("Voice");
            mouth.transform.SetParent(cameraRoot, false);
            mouth.AddComponent<AudioSource>();
            var playback = mouth.AddComponent<VoicePlayback>();
            Set(playback, "config", config);

            var voice = root.AddComponent<NetworkVoice>();
            Set(voice, "config", config);
            Set(voice, "playback", playback);

            var transmitter = root.AddComponent<VoiceTransmitter>();
            Set(transmitter, "config", config);
            Set(transmitter, "inputReader", reader);
            Set(transmitter, "voice", voice);

            var hud = root.AddComponent<VoiceHud>();
            Set(hud, "transmitter", transmitter);
            return new Result(transmitter, hud);
        }
    }
}

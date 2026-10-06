using Abandoned.Player;
using Abandoned.Voice;
using UnityEngine;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Adds proximity voice to the Player prefab: NetworkVoice (send/relay/receive), a "Voice" child at
    /// eye height with the AudioSource + low-pass + VoicePlayback (remote copies speak from their head,
    /// muffled by walls), a "Radio" child (2D, band-passed) for walkie-talkie transmissions, and the
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
            mouth.AddComponent<AudioLowPassFilter>().cutoffFrequency = VoiceMath.OpenCutoff;
            var playback = mouth.AddComponent<VoicePlayback>();
            Set(playback, "config", config);

            // The radio is in the listener's hand, not at the speaker: 2D, so where it hangs doesn't matter.
            var handset = new GameObject("Radio");
            handset.transform.SetParent(root.transform, false);
            handset.AddComponent<AudioSource>();
            handset.AddComponent<AudioHighPassFilter>();
            handset.AddComponent<AudioLowPassFilter>();
            var radio = handset.AddComponent<RadioPlayback>();
            Set(radio, "config", config);

            var voice = root.AddComponent<NetworkVoice>();
            Set(voice, "config", config);
            Set(voice, "playback", playback);
            Set(voice, "radio", radio);

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

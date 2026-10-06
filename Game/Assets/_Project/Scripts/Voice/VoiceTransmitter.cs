using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// The local player's microphone side: decides when we're talking (push-to-talk V, or open mic
    /// above the threshold with a short hang time), reads packets from the capture backend and hands
    /// them to <see cref="NetworkVoice"/>. Owner only.
    /// </summary>
    public class VoiceTransmitter : MonoBehaviour
    {
        // Enough to drain a frame's audio after a hitch without flooding the network.
        private const int MaxPacketsPerFrame = 6;

        [SerializeField] private VoiceConfig config;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private NetworkVoice voice;

        private IVoiceCapture capture;
        private byte[] packet;
        private float[] pcm = new float[24000];
        private float lastLoud = float.NegativeInfinity, lastSent = float.NegativeInfinity;

        public IVoiceCapture Capture => capture;
        /// <summary>Sent voice within the indicator hold time.</summary>
        public bool Transmitting => Time.time - lastSent <= config.IndicatorHold;
        public bool OnRadio { get; private set; }
        public float Level { get; private set; }

        // Opened lazily, once we know this is the spawned local player: remote copies are enabled for a
        // moment before NGO switches them off, and closing their capture would stop the shared microphone.
        private void EnsureCapture()
        {
            if (capture != null || voice == null || !voice.IsSpawned || !voice.IsOwner) return;
            bool steam = NetworkBootstrap.Instance != null && NetworkBootstrap.Instance.Transport == TransportMode.Steam;
            capture = VoiceBackends.CreateCapture(steam);
            // Steam hands over everything it buffered in one read and its packets can't be split.
            packet = new byte[capture.Codec == VoiceCodecId.Steam ? NetworkVoice.MaxPacketBytes : config.PacketBytes];
        }

        private void OnDisable()
        {
            capture?.Dispose();
            capture = null;
        }

        private void Update()
        {
            EnsureCapture();
            if (capture == null) return;
            PlayerInputFrame input = inputReader != null ? inputReader.Current : default;
            bool keyed = input.TalkHeld || input.RadioHeld;
            bool open = VoiceSettings.Mode == VoiceMode.OpenMic;
            bool muted = VoiceSettings.MicMuted;
            capture.Recording = !muted && (keyed || open);
            // Keyed speech always goes out, and so does the tail Steam still has after the key is let go.
            if (keyed) lastLoud = Time.time;
            OnRadio = input.RadioHeld && voice.HasRadio(voice.OwnerClientId);

            for (int i = 0; i < MaxPacketsPerFrame; i++)
            {
                int length = capture.ReadPacket(packet);
                if (length <= 0) break;
                if (muted) continue; // drain what was captured before muting, but never send it
                Level = MeasureLevel(length);
                // Open mic: only real speech goes out, plus a short tail so word endings aren't cut.
                if (!keyed)
                {
                    if (Level >= config.OpenMicThreshold) lastLoud = Time.time;
                    if (Time.time - lastLoud > config.OpenMicHangTime) continue;
                }
                voice.Send(packet, length, capture.Codec, Level, OnRadio);
                lastSent = Time.time;
            }
        }

        private float MeasureLevel(int length)
        {
            IVoiceCodec codec = VoiceBackends.Codec(capture.Codec);
            if (codec == null) return 0f;
            int n = codec.Decode(packet, length, pcm);
            return VoiceMath.Rms(pcm, n);
        }
    }
}

using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// Raw microphone through Unity's Microphone API (the system default input), mu-law encoded. Stands
    /// in for Steam voice when testing over Unity Transport (MPPM, LAN). The device may not do 16 kHz,
    /// so audio is captured at whatever it offers, low-passed and resampled.
    /// </summary>
    public sealed class MicrophoneVoiceCapture : IVoiceCapture
    {
        private const int BufferSeconds = 1;
        // A permission-denied or dead input delivers exact zeros; real rooms never do.
        private const float SilentInputSeconds = 2f;
        private const string Default = null; // Unity: null = the system's default input device

        private readonly AudioClip clip;
        private readonly int deviceRate;
        private readonly MicResampler resampler;
        private float[] raw = new float[0];
        private float[] resampled = new float[0];
        private int readPosition;
        private float silentFor;
        private bool recording;

        public string Name => "Microphone";
        public string Problem { get; private set; } = string.Empty;
        public VoiceCodecId Codec => VoiceCodecId.MuLaw;

        public MicrophoneVoiceCapture()
        {
            if (Microphone.devices.Length == 0)
            {
                Problem = "No microphone found.";
                return;
            }
            Microphone.GetDeviceCaps(Default, out int min, out int max);
            // 0/0 means "any rate".
            deviceRate = max == 0 ? MuLawCodec.Rate : Mathf.Clamp(MuLawCodec.Rate, min, max);
            resampler = new MicResampler((double)deviceRate / MuLawCodec.Rate);
            clip = Microphone.Start(Default, true, BufferSeconds, deviceRate);
            if (clip == null) Problem = "Couldn't open the microphone.";
        }

        public bool Recording
        {
            get => recording;
            set
            {
                if (value && !recording && clip != null) readPosition = Microphone.GetPosition(Default);
                recording = value;
            }
        }

        public int ReadPacket(byte[] packet)
        {
            if (clip == null) return 0;
            int position = Microphone.GetPosition(Default);
            int available = (position - readPosition + clip.samples) % clip.samples;
            if (!recording)
            {
                readPosition = position;
                return 0;
            }
            double step = resampler.Step;
            if (available < MuLawCodec.MinPacketSamples * step) return 0;
            int wanted = Mathf.Min(available, (int)(packet.Length * step));
            if (raw.Length != wanted) raw = new float[wanted]; // GetData reads exactly raw.Length samples
            clip.GetData(raw, readPosition); // wraps around the looping clip
            readPosition = (readPosition + wanted) % clip.samples;
            WatchForSilence(wanted);

            if (resampled.Length < (int)(wanted / step) + 2) resampled = new float[(int)(wanted / step) + 2];
            int produced = resampler.Process(raw, wanted, resampled);
            return MuLawCodec.Encode(resampled, produced, packet);
        }

        private void WatchForSilence(int count)
        {
            bool silent = true;
            for (int i = 0; i < count && silent; i++) silent = raw[i] == 0f;
            silentFor = silent ? silentFor + (float)count / deviceRate : 0f;
            Problem = silentFor >= SilentInputSeconds
                ? "The microphone sends nothing. Allow microphone access for Abandoned (System Settings > Privacy)."
                : string.Empty;
        }

        public void Dispose()
        {
            if (clip != null) Microphone.End(Default);
        }
    }
}

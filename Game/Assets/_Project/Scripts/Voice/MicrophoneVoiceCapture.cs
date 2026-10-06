using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// Raw microphone through Unity's Microphone API, mu-law encoded. Stands in for Steam voice when
    /// testing over Unity Transport (MPPM, LAN). The device may not do 16 kHz, so audio is captured at
    /// whatever it offers and resampled.
    /// </summary>
    public sealed class MicrophoneVoiceCapture : IVoiceCapture
    {
        private const int BufferSeconds = 1;

        private readonly string device;
        private readonly AudioClip clip;
        private readonly int deviceRate;
        private float[] raw = new float[0];
        private float[] resampled = new float[0];
        private int readPosition;
        private double resamplePhase;
        private bool recording;

        public string Name => "Microphone";
        public string Problem { get; } = string.Empty;
        public VoiceCodecId Codec => VoiceCodecId.MuLaw;

        public MicrophoneVoiceCapture()
        {
            if (Microphone.devices.Length == 0)
            {
                Problem = "No microphone found.";
                return;
            }
            device = Microphone.devices[0];
            Microphone.GetDeviceCaps(device, out int min, out int max);
            // 0/0 means "any rate".
            deviceRate = max == 0 ? MuLawCodec.Rate : Mathf.Clamp(MuLawCodec.Rate, min, max);
            clip = Microphone.Start(device, true, BufferSeconds, deviceRate);
            if (clip == null) Problem = $"Couldn't open the microphone '{device}'.";
        }

        public bool Recording
        {
            get => recording;
            set
            {
                if (value && !recording && clip != null) readPosition = Microphone.GetPosition(device);
                recording = value;
            }
        }

        public int ReadPacket(byte[] packet)
        {
            if (clip == null) return 0;
            int position = Microphone.GetPosition(device);
            int available = (position - readPosition + clip.samples) % clip.samples;
            if (!recording)
            {
                readPosition = position;
                return 0;
            }
            // Ask only for as much device audio as fits in one packet after resampling.
            double step = (double)deviceRate / MuLawCodec.Rate;
            if (available < MuLawCodec.MinPacketSamples * step) return 0;
            int wanted = Mathf.Min(available, (int)(packet.Length * step));
            if (raw.Length < wanted) raw = new float[wanted];
            ReadWrapped(readPosition, wanted);
            readPosition = (readPosition + wanted) % clip.samples;

            int produced = Resample(raw, wanted, step);
            return MuLawCodec.Encode(resampled, produced, packet);
        }

        // AudioClip.GetData doesn't wrap at the end of a looping clip, so read the two halves.
        private void ReadWrapped(int from, int count)
        {
            int first = Mathf.Min(count, clip.samples - from);
            var head = new float[first];
            clip.GetData(head, from);
            System.Array.Copy(head, raw, first);
            if (count == first) return;
            var tail = new float[count - first];
            clip.GetData(tail, 0);
            System.Array.Copy(tail, 0, raw, first, count - first);
        }

        // Linear interpolation; the phase carries over between packets so there's no click at the seams.
        private int Resample(float[] source, int count, double step)
        {
            int capacity = (int)(count / step) + 2;
            if (resampled.Length < capacity) resampled = new float[capacity];
            int n = 0;
            while (resamplePhase < count - 1)
            {
                int i = (int)resamplePhase;
                float t = (float)(resamplePhase - i);
                resampled[n++] = source[i] + (source[i + 1] - source[i]) * t;
                resamplePhase += step;
            }
            resamplePhase -= count;
            if (resamplePhase < 0) resamplePhase = 0;
            return n;
        }

        public void Dispose()
        {
            if (device != null) Microphone.End(device);
        }
    }
}

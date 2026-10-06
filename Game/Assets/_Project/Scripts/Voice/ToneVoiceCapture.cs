using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// A synthetic "voice" (a steady tone, mu-law) produced in real time while recording. Lets tests and
    /// the headless nettest exercise the whole voice path without a microphone.
    /// </summary>
    public sealed class ToneVoiceCapture : IVoiceCapture
    {
        private readonly float frequency, amplitude;
        private readonly float[] samples = new float[MuLawCodec.Rate];
        private double phase, owed;
        private float lastTime = -1f;
        private bool recording;

        public ToneVoiceCapture(float frequency = 220f, float amplitude = 0.3f)
        {
            this.frequency = frequency;
            this.amplitude = amplitude;
        }

        public string Name => "Test tone";
        public string Problem => string.Empty;
        public VoiceCodecId Codec => VoiceCodecId.MuLaw;

        public bool Recording
        {
            get => recording;
            set
            {
                if (value && !recording) lastTime = Time.realtimeSinceStartup;
                recording = value;
            }
        }

        public int ReadPacket(byte[] packet)
        {
            if (!recording) return 0;
            float now = Time.realtimeSinceStartup;
            owed += (now - lastTime) * MuLawCodec.Rate;
            lastTime = now;
            if (owed < MuLawCodec.MinPacketSamples) return 0;
            int n = Mathf.Min((int)owed, packet.Length, samples.Length);
            owed -= n;
            double step = 2 * System.Math.PI * frequency / MuLawCodec.Rate;
            for (int i = 0; i < n; i++)
            {
                samples[i] = amplitude * (float)System.Math.Sin(phase);
                phase += step;
            }
            phase %= 2 * System.Math.PI;
            return MuLawCodec.Encode(samples, n, packet);
        }

        public void Dispose() { }
    }
}

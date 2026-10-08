using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>Original heartbeat and breath; no soundtrack plays over the building's warnings.</summary>
    public static class PlayerVitalAudio
    {
        private const int Rate = 22050;
        private static AudioClip heartbeat, breath;

        public static AudioClip Heartbeat
        {
            get
            {
                if (heartbeat != null) return heartbeat;
                var samples = new float[(int)(Rate * 0.42f)];
                for (int i = 0; i < samples.Length; i++)
                {
                    float t = i / (float)Rate;
                    samples[i] = Pulse(t, 0f, 1f) + Pulse(t, 0.17f, 0.65f);
                }
                heartbeat = AudioClip.Create("Salvager heartbeat", samples.Length, 1, Rate, false);
                heartbeat.SetData(samples, 0);
                return heartbeat;
            }
        }

        private static float Pulse(float t, float start, float gain)
        {
            float age = t - start;
            if (age < 0f || age > 0.17f) return 0f;
            float envelope = Mathf.Exp(-age * 35f) * Mathf.Min(1f, age * 300f);
            return gain * envelope * (Mathf.Sin(age * 2f * Mathf.PI * 54f) * 0.65f + Mathf.Sin(age * 2f * Mathf.PI * 89f) * 0.22f);
        }

        public static AudioClip Breath
        {
            get
            {
                if (breath != null) return breath;
                var samples = new float[(int)(Rate * 1.6f)];
                var random = new System.Random(87231);
                float filtered = 0f;
                for (int i = 0; i < samples.Length; i++)
                {
                    float phase = i / (float)samples.Length;
                    filtered = Mathf.Lerp(filtered, (float)random.NextDouble() * 2f - 1f, 0.085f);
                    float envelope = Mathf.Sin(phase * Mathf.PI);
                    samples[i] = filtered * envelope * envelope * 0.55f;
                }
                breath = AudioClip.Create("Salvager breath", samples.Length, 1, Rate, false);
                breath.SetData(samples, 0);
                return breath;
            }
        }
    }
}

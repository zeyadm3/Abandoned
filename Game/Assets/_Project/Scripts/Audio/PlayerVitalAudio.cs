using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>Original heartbeat and breath; no soundtrack plays over the building's warnings.</summary>
    public static class PlayerVitalAudio
    {
        private const int Rate = 22050;
        private static AudioClip heartbeat, breath, labored;

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

        /// <summary>Near death (10 HP and below): a sharp, ragged breath in and a long, shaking one out.</summary>
        public static AudioClip LaboredBreath
        {
            get
            {
                if (labored != null) return labored;
                var samples = new float[(int)(Rate * 1.9f)];
                var random = new System.Random(55117);
                float air = 0f, chest = 0f;
                for (int i = 0; i < samples.Length; i++)
                {
                    float t = i / (float)Rate;
                    float noise = (float)random.NextDouble() * 2f - 1f;
                    // In: short and bright (0 - 0.62 s). A held gap. Out: longer, darker, with a wet catch in it (0.78 - 1.86 s).
                    bool inhale = t < 0.7f;
                    float phase = inhale ? Mathf.Clamp01(t / 0.62f) : Mathf.Clamp01((t - 0.78f) / 1.08f);
                    float swell = Mathf.Max(0f, Mathf.Sin(phase * Mathf.PI)); // sin(pi) is a hair below zero in floats
                    float envelope = inhale ? Mathf.Pow(swell, 0.7f) * (0.55f + 0.45f * phase) : swell * (1f - 0.35f * phase);
                    air = Mathf.Lerp(air, noise, inhale ? 0.22f : 0.1f);
                    chest = Mathf.Lerp(chest, noise, 0.02f);
                    float catchInThroat = inhale ? 1f : 0.72f + 0.28f * Mathf.Sin(t * 2f * Mathf.PI * 23f);
                    samples[i] = Mathf.Clamp((air * 0.85f + chest * (inhale ? 0.25f : 0.9f)) * envelope * catchInThroat * 0.9f, -1f, 1f);
                }
                labored = AudioClip.Create("Salvager labored breath", samples.Length, 1, Rate, false);
                labored.SetData(samples, 0);
                return labored;
            }
        }
    }
}

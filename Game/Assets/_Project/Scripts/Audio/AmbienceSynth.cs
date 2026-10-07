using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Seamless ambience loops made in code (no CC0 library fits a big empty building): wind moving
    /// through it, and the mains hum of its lights. Each loop's end crossfades into its start.
    /// </summary>
    public static class AmbienceSynth
    {
        private const int SampleRate = 22050;

        /// <summary>Low, gusting wind: brown noise whose loudness and brightness swell slowly.</summary>
        public static AudioClip Wind(float seconds, int seed)
        {
            var random = new System.Random(seed);
            float brown = 0f, lp = 0f;
            return Loop("Ambience_Wind", seconds, t =>
            {
                brown = Mathf.Clamp(brown + ((float)random.NextDouble() * 2f - 1f) * 0.02f, -1f, 1f) * 0.998f;
                float gust = 0.55f + 0.45f * Mathf.Sin(2f * Mathf.PI * t / seconds * 3f) * Mathf.Sin(2f * Mathf.PI * t / seconds * 2f + 1f);
                lp += (brown - lp) * (0.02f + 0.06f * gust);
                return lp * (0.5f + gust) * 3f;
            });
        }

        /// <summary>Fluorescent mains hum: 50 Hz and its harmonics with a little buzz (whole cycles, so it loops).</summary>
        public static AudioClip Hum(float seconds)
        {
            var random = new System.Random(3);
            return Loop("Ambience_Hum", seconds, t =>
            {
                float w = 2f * Mathf.PI * 50f * t;
                float tone = Mathf.Sin(w) * 0.5f + Mathf.Sin(2f * w) * 0.35f + Mathf.Sin(3f * w) * 0.15f + Mathf.Sin(6f * w) * 0.08f;
                float buzz = ((float)random.NextDouble() * 2f - 1f) * 0.04f * (0.5f + 0.5f * Mathf.Sin(2f * w));
                return (tone + buzz) * 0.5f;
            });
        }

        private static AudioClip Loop(string name, float seconds, System.Func<float, float> sample)
        {
            int count = Mathf.CeilToInt(seconds * SampleRate);
            int fade = SampleRate / 2;
            var data = new float[count + fade];
            for (int i = 0; i < data.Length; i++) data[i] = sample(i / (float)SampleRate);
            // The tail past the loop point fades into the head, so the jump back is inaudible.
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                data[i] = data[i] * k + data[count + i] * (1f - k);
            }
            var clipData = new float[count];
            for (int i = 0; i < count; i++) clipData[i] = Mathf.Clamp(data[i], -1f, 1f);
            AudioClip clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(clipData, 0);
            return clip;
        }
    }
}

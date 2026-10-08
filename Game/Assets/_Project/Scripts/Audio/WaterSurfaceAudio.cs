using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>Original shallow-water signatures for the mall's flooded surfaces.</summary>
    public static class WaterSurfaceAudio
    {
        private const int SampleRate = 44100;
        private static AudioClip step, impact;

        public static AudioClip Step() => step != null ? step : step = Splash("Step_Water", 0.28f, 0.45f, 53);
        public static AudioClip Impact() => impact != null ? impact : impact = Splash("Impact_Water", 0.65f, 0.8f, 97);

        private static AudioClip Splash(string name, float seconds, float gain, int seed)
        {
            var samples = new float[Mathf.CeilToInt(seconds * SampleRate)];
            var random = new System.Random(seed);
            float smooth = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)SampleRate;
                smooth += ((float)random.NextDouble() * 2f - 1f - smooth) * 0.32f;
                float spray = smooth * Mathf.Exp(-t * 14f);
                float slosh = Mathf.Sin(t * 410f + Mathf.Sin(t * 19f) * 3f) * Mathf.Exp(-t * 8f) * 0.12f;
                float envelope = Mathf.Clamp01(t / 0.005f) * Mathf.Clamp01((seconds - t) / 0.06f);
                samples[i] = Mathf.Clamp((spray + slosh) * gain * envelope, -1f, 1f);
            }
            var clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { step = null; impact = null; }
    }
}

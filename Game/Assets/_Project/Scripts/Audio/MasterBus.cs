using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The mastering stage (0.24.12), last on the listener after the room and the muffle: a stereo-linked compressor
    /// that pulls a collapse and a monster's scream into one loudness range with the footsteps, then a soft limiter
    /// so a pile of falling loot never clips. Unity can't author mixer assets from code, so the master bus is this
    /// filter; its numbers are in AudioConfig (Mastering). 2D sounds (UI, the player's own pulse) bypass it with the
    /// other listener effects.
    /// </summary>
    // Not DisallowMultipleComponent: ListenerSpace replaces it in one frame (the old one is destroyed at frame end).
    public class MasterBus : MonoBehaviour
    {
        // Copied from the config on the main thread; the audio thread only reads them.
        private volatile bool enabledFlag = true;
        private float threshold = 0.25f, ratio = 3f, makeup = 1.4f, ceiling = 0.9f, attackCoef, releaseCoef;
        private float envelope, gain = 1f;
        private int sampleRate;

        /// <summary>Gain reduction in dB right now; for the F1 overlay.</summary>
        public static float ReductionDb { get; private set; }

        private void Awake()
        {
            sampleRate = AudioSettings.outputSampleRate;
            Configure();
        }

        private void Update() => Configure();

        private void Configure()
        {
            AudioConfig c = AudioConfig.Current;
            enabledFlag = c.Mastering;
            threshold = Mathf.Pow(10f, c.CompressorThresholdDb / 20f);
            ratio = Mathf.Max(1f, c.CompressorRatio);
            makeup = Mathf.Pow(10f, c.MakeupDb / 20f);
            ceiling = Mathf.Pow(10f, c.CeilingDb / 20f);
            float rate = Mathf.Max(8000, sampleRate);
            attackCoef = Mathf.Exp(-1f / (Mathf.Max(0.0005f, c.AttackSeconds) * rate));
            releaseCoef = Mathf.Exp(-1f / (Mathf.Max(0.005f, c.ReleaseSeconds) * rate));
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!enabledFlag || channels <= 0) return;
            float env = envelope, g = gain, minGain = 1f;
            for (int i = 0; i < data.Length; i += channels)
            {
                // Stereo-linked peak detector, so the image never pulls to one side.
                float peak = 0f;
                for (int c = 0; c < channels; c++)
                {
                    float a = data[i + c] < 0f ? -data[i + c] : data[i + c];
                    if (a > peak) peak = a;
                }
                env = peak > env ? attackCoef * env + (1f - attackCoef) * peak : releaseCoef * env + (1f - releaseCoef) * peak;
                // Above the threshold, every extra 'ratio' dB in comes out as 1 dB.
                g = env > threshold ? Mathf.Pow(env / threshold, 1f / ratio - 1f) : 1f;
                if (g < minGain) minGain = g;
                float k = g * makeup;
                for (int c = 0; c < channels; c++)
                {
                    float x = data[i + c] * k;
                    // Soft limit: linear below the knee, eased into the ceiling above it.
                    float knee = ceiling * 0.8f;
                    float ax = x < 0f ? -x : x;
                    if (ax > knee)
                    {
                        float over = (ax - knee) / (ceiling - knee);
                        ax = knee + (ceiling - knee) * (over / (1f + over));
                        x = x < 0f ? -ax : ax;
                    }
                    data[i + c] = x;
                }
            }
            envelope = env;
            gain = g;
            ReductionDb = 20f * Mathf.Log10(Mathf.Max(1e-4f, minGain));
        }
    }
}

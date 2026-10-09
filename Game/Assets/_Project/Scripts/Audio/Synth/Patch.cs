using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>Recipes the sound patches share: what materials ring like, and scattering small sounds over time.</summary>
    internal static class Patch
    {
        // Partial ratios of a struck object. The further from whole numbers, the less it sounds like a note.
        public static readonly float[] Sheet = { 1f, 1.47f, 2.09f, 2.56f, 3.39f, 4.12f, 5.31f };
        public static readonly float[] Bar = { 1f, 2.76f, 5.4f, 8.93f };
        public static readonly float[] Glass = { 1f, 2.32f, 3.85f, 5.9f };
        public static readonly float[] Wood = { 1f, 1.58f, 2.41f, 3.14f };
        public static readonly float[] Hollow = { 1f, 2.7f, 4.3f };
        public static readonly float[] Coin = { 1f, 1.5f, 2.3f, 3.6f };

        public static SynthSamples Mono(SoundId id, int take, float[] data, float peak = 0.9f) =>
            new SynthSamples($"Synth_{id}_{take}", Synth.Normalize(data, peak));

        /// <summary>Adds one layer at a level (its loudest sample) and a time.</summary>
        public static void Layer(float[] dst, float[] layer, float level, float atSeconds = 0f) =>
            Synth.Mix(dst, Synth.Normalize(layer, level), atSeconds);

        /// <summary>Weight: pushed into soft clipping, then brought back to full level.</summary>
        public static float[] Heavy(float[] d, float drive) =>
            Synth.Normalize(Synth.Drive(Synth.Normalize(d, 1f), drive), 0.9f);

        /// <summary>Small noise grains scattered over a span, thinning out: rubble, grit, things still falling.</summary>
        public static void Debris(float[] dst, int seed, int count, float from, float span, float bright, float skew = 1.6f)
        {
            var rng = new Rng(seed);
            for (int i = 0; i < count; i++)
            {
                float late = Mathf.Pow(rng.Uni(), skew);
                float top = bright * rng.Range(0.5f, 1.2f);
                float[] grain = Synth.Noise(rng.Range(0.03f, 0.16f), seed * 31 + i, top, top * 0.25f, 0.001f, rng.Range(25f, 60f));
                Layer(dst, grain, rng.Range(0.15f, 0.6f) * (1f - 0.6f * late), from + late * span);
            }
        }

        /// <summary>Small struck pings scattered over a span: shards, coins, fixings, relays.</summary>
        public static void Pings(float[] dst, int seed, int count, float from, float span, float hzLow, float hzHigh,
            float[] ratios, float decay, float level, float skew = 1.4f)
        {
            var rng = new Rng(seed);
            for (int i = 0; i < count; i++)
            {
                float at = from + Mathf.Pow(rng.Uni(), skew) * span;
                float[] ping = Synth.Modal(Mathf.Min(0.5f, 6f / decay), rng.Range(hzLow, hzHigh), ratios, null,
                    decay * rng.Range(0.8f, 1.3f));
                Layer(dst, Synth.Fade(ping, 0f, 0.01f), level * rng.Range(0.4f, 1f), at);
            }
        }
    }
}

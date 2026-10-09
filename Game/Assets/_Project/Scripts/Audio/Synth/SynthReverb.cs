using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// A small room for rendered sounds: six damped feedback combs into two all-passes. Baked into the
    /// clip, so a roar or a collapse carries its own tail wherever it is played. The listener's live
    /// reverb (<see cref="ListenerSpace"/>) then puts the whole world in the space the player stands in.
    /// </summary>
    public static class SynthReverb
    {
        private static readonly float[] CombMs = { 29.7f, 37.1f, 41.1f, 43.7f, 31.3f, 34.9f };
        private static readonly float[] AllPassMs = { 5f, 1.7f };

        /// <summary>
        /// Dry plus wet, with the tail appended. <paramref name="spread"/> stretches the delay lengths:
        /// render left and right with different spreads for a wide stereo room.
        /// </summary>
        public static float[] Apply(float[] dry, float decaySeconds, float wet, float damping = 0.35f, float spread = 0f,
            int rate = Synth.Rate, float tailSeconds = -1f)
        {
            int tail = Synth.Samples(tailSeconds >= 0f ? tailSeconds : decaySeconds * 0.9f, rate);
            float[] d = Wet(dry, dry.Length + tail, decaySeconds, damping, spread, rate);
            for (int i = 0; i < d.Length; i++) d[i] = d[i] * wet + (i < dry.Length ? dry[i] : 0f);
            // The tail is cut at a fixed length; the last stretch fades so the cut can't click.
            return Synth.Fade(d, 0f, Mathf.Min(0.25f, tail / (float)rate), rate);
        }

        /// <summary>For seamless loops: the tail wraps round into the start instead of being appended.</summary>
        public static float[] ApplyLooped(float[] dry, float decaySeconds, float wet, float damping = 0.35f, float spread = 0f,
            int rate = Synth.LoopRate)
        {
            int tail = Mathf.Min(dry.Length, Synth.Samples(decaySeconds, rate));
            float[] full = Wet(dry, dry.Length + tail, decaySeconds, damping, spread, rate);
            var d = new float[dry.Length];
            for (int i = 0; i < dry.Length; i++) d[i] = dry[i] + full[i] * wet;
            for (int i = 0; i < tail; i++) d[i] += full[dry.Length + i] * wet * (1f - i / (float)tail);
            return d;
        }

        private static float[] Wet(float[] dry, int length, float decaySeconds, float damping, float spread, int rate)
        {
            var d = new float[length];
            int combs = CombMs.Length;
            var lines = new float[combs][];
            var index = new int[combs];
            var feedback = new float[combs];
            var store = new float[combs];
            for (int c = 0; c < combs; c++)
            {
                int size = Mathf.Max(8, Mathf.RoundToInt(CombMs[c] * (1f + spread) * 0.001f * rate));
                lines[c] = new float[size];
                // Each comb loses 60 dB over the decay time.
                feedback[c] = Mathf.Pow(10f, -3f * (size / (float)rate) / Mathf.Max(0.05f, decaySeconds));
            }
            for (int i = 0; i < length; i++)
            {
                float x = i < dry.Length ? dry[i] : 0f, sum = 0f;
                for (int c = 0; c < combs; c++)
                {
                    float[] line = lines[c];
                    int p = index[c];
                    float y = line[p];
                    // The damping inside the loop makes the tail lose its top first, as rooms do.
                    store[c] = y * (1f - damping) + store[c] * damping;
                    line[p] = x + store[c] * feedback[c];
                    index[c] = p + 1 == line.Length ? 0 : p + 1;
                    sum += y;
                }
                d[i] = sum / combs;
            }
            foreach (float ms in AllPassMs)
            {
                var line = new float[Mathf.Max(4, Mathf.RoundToInt(ms * (1f + spread) * 0.001f * rate))];
                int p = 0;
                for (int i = 0; i < length; i++)
                {
                    float delayed = line[p], x = d[i];
                    line[p] = x + delayed * 0.5f;
                    d[i] = delayed - x * 0.5f;
                    p = p + 1 == line.Length ? 0 : p + 1;
                }
            }
            return d;
        }
    }
}

using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// A piece of music being put together (0.14.0): notes are dropped onto a left/right pair at a time
    /// and a place across the stereo field. In a loop, whatever rings past the end comes back in at the
    /// start, so the loop is seamless by construction. <see cref="Finish"/> puts it in a room and levels it.
    /// </summary>
    public sealed class MusicScore
    {
        public readonly int Rate;
        public readonly float Seconds;
        private readonly float[] left, right;
        private readonly bool loop;

        public int Length => left.Length;

        public MusicScore(float seconds, bool loop, int rate = Synth.LoopRate)
        {
            Seconds = seconds;
            Rate = rate;
            this.loop = loop;
            left = Synth.Buffer(seconds, rate);
            right = Synth.Buffer(seconds, rate);
        }

        /// <summary>Pan runs from -1 (left) to 1 (right).</summary>
        public void Add(float[] note, float at, float gain = 1f, float pan = 0f)
        {
            float angle = (Mathf.Clamp(pan, -1f, 1f) + 1f) * Mathf.PI * 0.25f;
            float l = Mathf.Cos(angle) * gain, r = Mathf.Sin(angle) * gain;
            if (loop)
            {
                Synth.MixWrapped(left, note, at, l, Rate);
                Synth.MixWrapped(right, note, at, r, Rate);
            }
            else
            {
                Synth.Mix(left, note, at, l, Rate);
                Synth.Mix(right, note, at, r, Rate);
            }
        }

        /// <summary>The two sides get slightly different rooms, which is what makes it wide.</summary>
        public SynthSamples Finish(string name, float roomSeconds, float wet, float peak = 0.85f)
        {
            float[] l, r;
            if (loop)
            {
                l = SynthReverb.ApplyLooped(left, roomSeconds, wet, 0.4f, 0f, Rate);
                r = SynthReverb.ApplyLooped(right, roomSeconds, wet, 0.4f, 0.08f, Rate);
            }
            else
            {
                l = SynthReverb.Apply(left, roomSeconds, wet, 0.4f, 0f, Rate);
                r = SynthReverb.Apply(right, roomSeconds, wet, 0.4f, 0.08f, Rate);
            }
            Synth.NormalizePair(l, r, peak);
            return new SynthSamples(name, Synth.Interleave(l, r), 2, Rate);
        }
    }
}

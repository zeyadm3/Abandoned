namespace Abandoned.Voice
{
    /// <summary>
    /// Streaming linear resampler for the microphone, continuous across chunks (it remembers the last
    /// input sample and its phase), with a moving-average low-pass first when decimating so hiss and
    /// sibilants above the new Nyquist don't fold down into the speech band. Pure; unit-tested.
    /// </summary>
    public sealed class MicResampler
    {
        private readonly int smooth;
        private readonly float[] history;
        private int historyIndex;
        private float historySum;
        private float previous;
        private double phase; // position of the next output in input samples, relative to the chunk start (-1 = previous)

        public double Step { get; }

        /// <param name="step">Input samples per output sample (device rate / target rate).</param>
        public MicResampler(double step)
        {
            Step = step;
            smooth = step > 1.5 ? (int)System.Math.Round(step) : 1;
            history = new float[smooth];
        }

        public int Process(float[] input, int count, float[] output)
        {
            int n = 0;
            for (int i = 0; i < count; i++)
            {
                float x = Smooth(input[i]);
                // Emit every output that falls between the previous sample (i - 1) and this one.
                while (phase <= i && n < output.Length)
                {
                    float t = (float)(phase - (i - 1));
                    output[n++] = previous + (x - previous) * t;
                    phase += Step;
                }
                previous = x;
            }
            phase -= count;
            return n;
        }

        private float Smooth(float x)
        {
            if (smooth == 1) return x;
            historySum += x - history[historyIndex];
            history[historyIndex] = x;
            historyIndex = (historyIndex + 1) % smooth;
            return historySum / smooth;
        }
    }
}

namespace Abandoned.Voice
{
    /// <summary>
    /// Smooths a speaker's packets for playback. Packets arrive in bursts over the network but the
    /// audio thread pulls a steady stream, so playback starts only once <see cref="TargetSamples"/> are
    /// queued, re-primes after running dry (instead of crackling), and drops the oldest audio when too
    /// far behind (so latency can't grow forever). Written by the main thread, read by the audio thread.
    /// </summary>
    public sealed class VoiceJitterBuffer
    {
        private readonly object gate = new();
        private readonly float[] ring;
        private int readIndex, count;
        private bool playing;

        public int TargetSamples { get; private set; }
        public int Capacity => ring.Length;
        public int Underruns { get; private set; }
        public int Dropped { get; private set; }

        public int Count
        {
            get { lock (gate) return count; }
        }

        public bool Playing
        {
            get { lock (gate) return playing; }
        }

        public VoiceJitterBuffer(int targetSamples, int maxSamples)
        {
            TargetSamples = System.Math.Max(1, targetSamples);
            ring = new float[System.Math.Max(maxSamples, TargetSamples * 2)];
        }

        public void Write(float[] samples, int length)
        {
            lock (gate)
            {
                for (int i = 0; i < length; i++)
                {
                    if (count == ring.Length)
                    {
                        // Full: forget the oldest sample to stay near real time.
                        readIndex = (readIndex + 1) % ring.Length;
                        count--;
                        Dropped++;
                    }
                    ring[(readIndex + count) % ring.Length] = samples[i];
                    count++;
                }
                if (!playing && count >= TargetSamples) playing = true;
            }
        }

        /// <summary>Fills <paramref name="output"/> (silence where there's nothing to play); returns how many samples were real audio.</summary>
        public int Read(float[] output, int length)
        {
            lock (gate)
            {
                int i = 0;
                if (playing)
                {
                    for (; i < length && count > 0; i++)
                    {
                        output[i] = ring[readIndex];
                        readIndex = (readIndex + 1) % ring.Length;
                        count--;
                    }
                    if (i < length)
                    {
                        playing = false;
                        Underruns++;
                    }
                }
                int played = i;
                for (; i < length; i++) output[i] = 0f;
                return played;
            }
        }

        /// <summary>
        /// Audio thread: the clip pulls this many samples per read, so the cushion must be a whole read
        /// plus the jitter target or every read past the queue would underrun.
        /// </summary>
        public void EnsureTarget(int samples)
        {
            lock (gate)
            {
                if (samples > TargetSamples) TargetSamples = System.Math.Min(samples, ring.Length);
            }
        }

        /// <summary>A short utterance that never reached the target: play what there is.</summary>
        public void StartNow()
        {
            lock (gate)
            {
                if (count > 0) playing = true;
            }
        }

        public void Clear()
        {
            lock (gate)
            {
                readIndex = count = 0;
                playing = false;
            }
        }
    }
}

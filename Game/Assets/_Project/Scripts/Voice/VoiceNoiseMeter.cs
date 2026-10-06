namespace Abandoned.Voice
{
    /// <summary>
    /// Turns one speaker's stream of voice packets (~50 a second) into occasional noise events: the
    /// first packet after a quiet spell is heard at once, then at most one event per interval carrying
    /// the loudest moment since the last. Pure, so it's tested without a network.
    /// </summary>
    public sealed class VoiceNoiseMeter
    {
        private readonly float interval;
        private float lastEmit = float.NegativeInfinity, lastAdd = float.NegativeInfinity;
        private float peak;
        private bool pending;

        public VoiceNoiseMeter(float interval) => this.interval = interval;

        /// <summary>Adds one packet's loudness; returns true with the loudness to emit when it's time.</summary>
        public bool Add(float loudness, float time, out float emit)
        {
            emit = 0f;
            // A held-back peak belongs to the speech it came from; after a pause it's stale.
            if (time - lastAdd > interval) pending = false;
            lastAdd = time;
            if (!pending || loudness > peak) peak = loudness;
            pending = true;
            if (time - lastEmit < interval) return false;
            lastEmit = time;
            emit = peak;
            pending = false;
            peak = 0f;
            return emit > 0f;
        }
    }
}

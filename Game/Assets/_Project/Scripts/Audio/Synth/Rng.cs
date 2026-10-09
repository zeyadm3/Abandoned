namespace Abandoned.Audio
{
    /// <summary>
    /// A small deterministic noise source for offline synthesis: the same seed always renders the same
    /// sound, on any thread. It's a struct with state, so keep it in a local and never copy it mid-use.
    /// </summary>
    public struct Rng
    {
        private uint state;

        public Rng(int seed)
        {
            unchecked { state = (uint)seed * 2654435761u + 0x9E3779B9u; }
            if (state == 0u) state = 0x9E3779B9u; // xorshift never leaves zero
        }

        private uint Next()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        /// <summary>White noise, -1..1.</summary>
        public float Bi() => (Next() & 0xFFFFFF) / 8388607.5f - 1f;

        /// <summary>0..1.</summary>
        public float Uni() => (Next() & 0xFFFFFF) / 16777215f;

        public float Range(float min, float max) => min + (max - min) * Uni();

        public int Int(int maxExclusive) => maxExclusive <= 1 ? 0 : (int)(Next() % (uint)maxExclusive);
    }
}

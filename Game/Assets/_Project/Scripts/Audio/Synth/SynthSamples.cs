namespace Abandoned.Audio
{
    /// <summary>One rendered sound, as plain data (so it can be made off the main thread).</summary>
    public sealed class SynthSamples
    {
        public readonly string Name;
        /// <summary>Interleaved when <see cref="Channels"/> is 2.</summary>
        public readonly float[] Data;
        public readonly int Channels, Rate;

        public SynthSamples(string name, float[] data, int channels = 1, int rate = Synth.Rate)
        {
            Name = name;
            Data = data;
            Channels = channels;
            Rate = rate;
        }
    }
}

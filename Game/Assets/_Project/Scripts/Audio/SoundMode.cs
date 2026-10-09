namespace Abandoned.Audio
{
    /// <summary>Where a sound's audio comes from.</summary>
    public enum SoundMode
    {
        /// <summary>A recorded clip from the SoundBank; the synthesised version only stands in when the bank has none.</summary>
        Library,
        /// <summary>Synthesised. A clip put in the SoundBank for it replaces the synthesis.</summary>
        Synth,
        /// <summary>The SoundBank's recording and the synthesised layer play together.</summary>
        Layered,
    }
}

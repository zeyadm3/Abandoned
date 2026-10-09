namespace Abandoned.Audio
{
    /// <summary>What walls and floors between the sound and the listener do to it.</summary>
    public enum SoundOcclusion
    {
        /// <summary>Nothing: warnings and signals that must always read (the floor, the horn).</summary>
        None,
        /// <summary>Dulled, but never much quieter: monsters, so a wall hides where it is but not that it is there.</summary>
        Soft,
        /// <summary>Dulled and quietened.</summary>
        Full,
    }
}

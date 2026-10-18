namespace Abandoned.Extraction
{
    /// <summary>What a <see cref="HidingLocker"/> looks like to the player (its prompts). The hiding rules are the
    /// same for all of them. Append only: scenes store the number.</summary>
    public enum HideSpotKind : byte { Locker, Machine, Furnace, Cabinet }
}

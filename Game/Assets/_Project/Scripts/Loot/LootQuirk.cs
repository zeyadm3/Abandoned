namespace Abandoned.Loot
{
    /// <summary>
    /// Loot quirks (0.18.2): traits an item's definition can carry, any combination. Each is handled by
    /// <see cref="LootQuirks"/>, so a new quirky item is data only. Append only (stored by value).
    /// </summary>
    [System.Flags]
    public enum LootQuirk
    {
        None = 0,
        /// <summary>Rattles when moved: heard by monsters.</summary>
        Rattles = 1,
        /// <summary>Leaks while carried: loses a little value every second it's moving.</summary>
        Leaks = 2,
        /// <summary>Rings now and then, wherever it is: an alarm clock, a phone.</summary>
        Rings = 4,
        /// <summary>Top-heavy: its weight sits high, so it tips over easily.</summary>
        TopHeavy = 8,
    }
}

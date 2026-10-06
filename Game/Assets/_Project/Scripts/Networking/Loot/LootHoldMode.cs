namespace Abandoned.Networking
{
    /// <summary>Where a networked loot item is: lying in the world, in someone's hands, or in their pockets.</summary>
    public enum LootHoldMode : byte
    {
        Free = 0,
        Held = 1,
        Pocketed = 2,
    }
}

namespace Abandoned.Networking
{
    /// <summary>The kinds of loot request a client sends the host.</summary>
    public enum LootRequest
    {
        Pickup = 0,
        Release = 1,
        Unpocket = 2,
        Place = 3,
    }
}

namespace Abandoned.Core
{
    /// <summary>Anything with a current money value (loot), so HUD and extraction can total it.</summary>
    public interface IValuable
    {
        int CurrentValue { get; }
    }
}

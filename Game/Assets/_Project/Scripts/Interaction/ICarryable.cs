using Abandoned.Core;

namespace Abandoned.Interaction
{
    /// <summary>
    /// Carry properties of a grabbable object. Loot implements this from its LootDefinition;
    /// tools and props can implement it too without depending on the Loot area.
    /// </summary>
    public interface ICarryable : IWeighted
    {
        string DisplayName { get; }
        CarryClass CarryClass { get; }
    }
}

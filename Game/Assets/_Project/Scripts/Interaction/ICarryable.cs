namespace Abandoned.Interaction
{
    /// <summary>
    /// Carry properties of a grabbable object. Loot implements this from its LootDefinition;
    /// tools and props can implement it too without depending on the Loot area.
    /// </summary>
    public interface ICarryable
    {
        string DisplayName { get; }
        CarryClass CarryClass { get; }

        /// <summary>Gameplay weight in kg: drives carry speed, stamina and structural load. Not Rigidbody mass.</summary>
        float GameplayWeight { get; }
    }
}

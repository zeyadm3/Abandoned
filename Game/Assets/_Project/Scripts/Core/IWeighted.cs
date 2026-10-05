namespace Abandoned.Core
{
    /// <summary>
    /// Anything with a gameplay weight in kg (not Rigidbody mass): carried loads, structural load,
    /// how hard a falling object hits a player.
    /// </summary>
    public interface IWeighted
    {
        float GameplayWeight { get; }
    }
}

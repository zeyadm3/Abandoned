namespace Abandoned.UI
{
    /// <summary>
    /// Something a player can press E on that has its own one-time tip (0.16.1+): the tip shows the first
    /// time the player looks at one, like the door and locker tips, without HintDirector naming each type.
    /// </summary>
    public interface IHintSource
    {
        HintId Hint { get; }
    }
}

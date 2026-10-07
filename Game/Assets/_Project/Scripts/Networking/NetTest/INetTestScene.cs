namespace Abandoned.Networking
{
    /// <summary>A scenario that runs in a level other than <see cref="NetTestRunner.DefaultScene"/>.</summary>
    public interface INetTestScene
    {
        string Scene { get; }
    }
}

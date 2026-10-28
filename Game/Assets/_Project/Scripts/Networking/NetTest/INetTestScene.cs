#if UNITY_EDITOR || DEVELOPMENT_BUILD
// The nettest harness ships only in the editor and Development builds (Tools/unity.sh nettest uses a dev build).
namespace Abandoned.Networking
{
    /// <summary>A scenario that runs in a level other than <see cref="NetTestRunner.DefaultScene"/>.</summary>
    public interface INetTestScene
    {
        string Scene { get; }
    }
}

#endif

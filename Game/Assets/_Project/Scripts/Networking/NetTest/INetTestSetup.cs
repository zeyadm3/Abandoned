#if UNITY_EDITOR || DEVELOPMENT_BUILD
// The nettest harness ships only in the editor and Development builds (Tools/unity.sh nettest uses a dev build).
namespace Abandoned.Networking
{
    /// <summary>A scenario that has to change the game before the session starts (e.g. a fake microphone).</summary>
    public interface INetTestSetup
    {
        void Prepare();
    }
}

#endif

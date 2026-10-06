namespace Abandoned.Networking
{
    /// <summary>A scenario that has to change the game before the session starts (e.g. a fake microphone).</summary>
    public interface INetTestSetup
    {
        void Prepare();
    }
}

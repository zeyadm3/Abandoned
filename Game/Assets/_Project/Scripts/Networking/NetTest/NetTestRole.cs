#if UNITY_EDITOR || DEVELOPMENT_BUILD
// The nettest harness ships only in the editor and Development builds (Tools/unity.sh nettest uses a dev build).
namespace Abandoned.Networking
{
    /// <summary>What a process launched for the multi-process nettest does.</summary>
    public enum NetTestRole
    {
        None,
        Host,
        Client,
    }
}

#endif

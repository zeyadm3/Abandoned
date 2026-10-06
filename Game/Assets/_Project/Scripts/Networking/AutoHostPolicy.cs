namespace Abandoned.Networking
{
    /// <summary>
    /// Whether a scene should start hosting by itself. Solo play is just hosting with nobody else
    /// (CLAUDE.md: the solo player is the host), so in the editor Play goes straight in.
    /// </summary>
    public static class AutoHostPolicy
    {
        public static bool ShouldAutoHost(bool autoHostInEditor, bool isEditor, bool isMainEditor, NetworkLaunchArgs args)
        {
            if (args.IsClientLaunch) return false;
            if (args.Host) return true;
            // Multiplayer Play Mode virtual players are clients: they join the main editor's host.
            return isEditor && autoHostInEditor && isMainEditor;
        }
    }
}

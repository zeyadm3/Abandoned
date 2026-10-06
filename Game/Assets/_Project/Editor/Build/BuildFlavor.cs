namespace Abandoned.EditorTools
{
    /// <summary>
    /// Dev: development player for automated tests (nettest), host architecture only, fast to build.
    /// Shareable: normal player zipped for the user and the Windows tester, App ID 480 file included.
    /// Release: Steam build with the real App ID; never ships steam_appid.txt (Steam launches it).
    /// </summary>
    public enum BuildFlavor
    {
        Dev,
        Shareable,
        Release,
    }
}

using Unity.Netcode;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Spray chalk (0.17.6) was cut with the gear on 2026-10-22. This empty behaviour stays only because the
    /// saved RunState prefabs (the shared one, the Hospital's and the Motel's) still carry it, and a missing
    /// script on a network prefab breaks its behaviour list. Remove it from NetworkContentBuilder.RunStateExtras
    /// and those prefabs in a Unity rebuild, then delete this file.
    /// </summary>
    public class RunChalk : NetworkBehaviour
    {
    }
}

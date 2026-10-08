using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Whether developer keys work here (F1 overlay, K ragdoll, free camera mid-run, structure keys):
    /// the editor and Development builds only. Shareable, Demo and Release builds are what players get,
    /// and the overlay shows monsters through walls, so they never answer these keys.
    /// </summary>
    public static class DevTools
    {
        private static bool? forced;

        public static bool Enabled => forced ?? (Application.isEditor || Debug.isDebugBuild);

        /// <summary>Tests: pretend to be a player build (false) or a dev build (true); null = real answer.</summary>
        public static void Force(bool? enabled) => forced = enabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => forced = null;
    }
}

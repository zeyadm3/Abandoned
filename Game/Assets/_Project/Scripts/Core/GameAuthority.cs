using System;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Answers "is this machine the host?" for host-authoritative rules (loot value, structure,
    /// money). Single-player is always host; networking registers a real check in M3.
    /// </summary>
    public static class GameAuthority
    {
        private static Func<bool> isHostCheck;

        public static bool IsHost => isHostCheck?.Invoke() ?? true;

        public static void SetHostCheck(Func<bool> check) => isHostCheck = check;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => isHostCheck = null;
    }
}

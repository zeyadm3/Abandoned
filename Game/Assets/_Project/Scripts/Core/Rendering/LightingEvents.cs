using System;

namespace Abandoned.Core
{
    /// <summary>
    /// 0.24.13: "the level's lights just changed" (mains off or back, a breaker thrown, night). Raised by
    /// Extraction.PowerController; RoomReflections redraws its probes so floors stop reflecting lamps that are out.
    /// </summary>
    public static class LightingEvents
    {
        public static event Action Changed;

        public static void Raise() => Changed?.Invoke();

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Changed = null;
    }
}

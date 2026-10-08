using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// The local player's racing heartbeat as the low-health feedback drives it (PlayerHealthEffects.Trauma).
    /// Everything that pulses with it (the vitals bar, the vignette, the blood on the lens) reads this one
    /// envelope, so they stay in step with the sound. Owner-only and cosmetic.
    /// </summary>
    public static class PlayerPulse
    {
        /// <summary>1 on each beat, falling toward 0 before the next; 0 while health is above the critical threshold.</summary>
        public static float Beat { get; internal set; }

        /// <summary>0 = fine, 1 = critical, 2 = severe trauma, 3 = near death (PlayerHealthConfig.TraumaTier).</summary>
        public static int Tier { get; internal set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Beat = 0f;
            Tier = 0;
        }
    }
}

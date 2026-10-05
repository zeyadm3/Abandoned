using System;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// World event "something heavy hit here". Each local camera decides how much to shake from its
    /// own distance and settings, so the source doesn't need to know who's watching.
    /// </summary>
    public static class CameraShake
    {
        /// <summary>Position and momentum (gameplay kg × m/s) of the impact.</summary>
        public static event Action<Vector3, float> Impact;

        public static void Emit(Vector3 position, float momentum) => Impact?.Invoke(position, momentum);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Impact = null;
    }
}

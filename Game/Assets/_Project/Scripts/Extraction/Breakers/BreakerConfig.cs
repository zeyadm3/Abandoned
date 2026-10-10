using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Breaker rooms (0.16.1): how long a thrown breaker keeps its sector lit, how long the panel needs to
    /// cool down, and how loud it all is. Lives at Data/Extraction/Resources/BreakerConfig; the defaults
    /// below apply until that asset exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Breaker Config", fileName = "BreakerConfig")]
    public class BreakerConfig : ScriptableObject
    {
        public const string ResourcePath = "BreakerConfig";

        [Tooltip("Seconds a thrown breaker keeps its sector's lights on.")]
        [SerializeField, Min(5f)] private float restoreSeconds = 120f;
        [Tooltip("Seconds after the lights drop again before the same panel can be thrown.")]
        [SerializeField, Min(0f)] private float cooldownSeconds = 45f;
        [Tooltip("How close (m) a player must be to throw a breaker.")]
        [SerializeField, Min(0.5f)] private float reach = 2.6f;
        [Tooltip("How loud throwing it is to monsters (1 = heard 40 m away).")]
        [SerializeField, Range(0f, 1f)] private float throwNoise = 1f;
        [Tooltip("While a sector is restored, its panel hums this loud every few seconds: the light costs you.")]
        [SerializeField, Range(0f, 1f)] private float humNoise = 0.4f;
        [SerializeField, Min(1f)] private float humInterval = 6f;

        public float RestoreSeconds => restoreSeconds;
        public float CooldownSeconds => cooldownSeconds;
        public float Reach => reach;
        public float ThrowNoise => throwNoise;
        public float HumNoise => humNoise;
        public float HumInterval => humInterval;

        private static BreakerConfig current;

        public static BreakerConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<BreakerConfig>(ResourcePath);
                if (current == null) current = CreateInstance<BreakerConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

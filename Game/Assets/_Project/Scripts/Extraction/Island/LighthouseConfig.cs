using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The island prison's lighthouse: how fast the lamp turns, how far and wide its beam reaches, and how it
    /// dazzles monsters it sweeps across. How long it runs and how long it cools down are the breaker rules
    /// (<see cref="BreakerConfig"/>), since the lamp is thrown like a breaker. Lives at
    /// Data/Extraction/Resources/LighthouseConfig; the defaults below apply until that asset exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Lighthouse Config", fileName = "LighthouseConfig")]
    public class LighthouseConfig : ScriptableObject
    {
        public const string ResourcePath = "LighthouseConfig";

        [Tooltip("Degrees per second the lamp turns (a real light: one sweep every 10-20 s).")]
        [SerializeField, Range(5f, 120f)] private float turnSpeed = 30f;
        [Tooltip("How far (m) the beam reaches across the island.")]
        [SerializeField, Min(10f)] private float range = 110f;
        [Tooltip("Half-width (degrees) of the beam, side to side, for dazzling monsters.")]
        [SerializeField, Range(1f, 30f)] private float halfWidth = 7f;
        [Tooltip("Seconds a monster caught in the beam can't strike (it flinches away from the light).")]
        [SerializeField, Range(0f, 6f)] private float dazeSeconds = 2.5f;
        [Tooltip("Seconds before the same monster can be dazzled again: the beam buys a moment, not safety.")]
        [SerializeField, Range(0f, 60f)] private float dazeCooldown = 12f;
        [Tooltip("Seconds between the host's looks for monsters in the beam.")]
        [SerializeField, Range(0.05f, 1f)] private float checkInterval = 0.2f;
        [SerializeField, Min(0f)] private float beamIntensity = 60f;

        public float TurnSpeed => turnSpeed;
        public float Range => range;
        public float HalfWidth => halfWidth;
        public float DazeSeconds => dazeSeconds;
        public float DazeCooldown => dazeCooldown;
        public float CheckInterval => checkInterval;
        public float BeamIntensity => beamIntensity;

        private static LighthouseConfig current;

        public static LighthouseConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<LighthouseConfig>(ResourcePath);
                if (current == null) current = CreateInstance<LighthouseConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

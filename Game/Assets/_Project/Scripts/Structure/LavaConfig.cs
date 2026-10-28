using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// The Iron Factory's molten channel: how often the host looks for anything that went in, the death line,
    /// and how the surface moves. Lives at Data/Structure/Resources/LavaConfig (the Iron Factory build makes it);
    /// the defaults below apply until that asset exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Structure/Lava Config", fileName = "LavaConfig")]
    public class LavaConfig : ScriptableObject
    {
        public const string ResourcePath = "LavaConfig";

        [Tooltip("Seconds between the host's checks for players and loot in the lava.")]
        [SerializeField, Min(0.02f)] private float checkInterval = 0.1f;
        [Tooltip("UV units per second the crust drifts along the channel.")]
        [SerializeField] private Vector2 crustDrift = new(0.004f, 0.018f);
        [Tooltip("How fast the glow lights breathe.")]
        [SerializeField, Min(0f)] private float flickerSpeed = 0.9f;
        [Tooltip("How far the glow lights dip and swell around their set intensity (0.2 = +/-20 %).")]
        [SerializeField, Range(0f, 0.6f)] private float flickerAmount = 0.22f;

        [field: SerializeField] public string DeathLine { get; private set; } = "You fell into the molten iron."; // @loc

        public float CheckInterval => checkInterval;
        public Vector2 CrustDrift => crustDrift;
        public float FlickerSpeed => flickerSpeed;
        public float FlickerAmount => flickerAmount;

        private static LavaConfig cached;

        public static LavaConfig Current
        {
            get
            {
                if (cached != null) return cached;
                cached = Resources.Load<LavaConfig>(ResourcePath);
                if (cached == null) cached = CreateInstance<LavaConfig>();
                return cached;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => cached = null;
    }
}

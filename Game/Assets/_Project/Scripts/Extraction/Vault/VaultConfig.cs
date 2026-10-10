using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>The vault drill (0.16.6): how long it drills, how often it jams, what is inside. Data/Extraction/Resources/VaultConfig.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Vault Config", fileName = "VaultConfig")]
    public class VaultConfig : ScriptableObject
    {
        public const string ResourcePath = "VaultConfig";

        [Tooltip("Seconds of drilling to open the safe.")]
        [SerializeField, Min(5f)] private float drillSeconds = 90f;
        [Tooltip("Every this many seconds of drilling, the drill may jam and wait for someone to free it.")]
        [SerializeField, Min(1f)] private float jamCheckSeconds = 12f;
        [SerializeField, Range(0f, 1f)] private float jamChance = 0.4f;
        [Tooltip("How loud the drill is to monsters, every couple of seconds while it runs.")]
        [SerializeField, Range(0f, 1f)] private float drillNoise = 1f;
        [SerializeField, Min(0.5f)] private float noiseInterval = 2f;
        [SerializeField, Min(0.5f)] private float reach = 2.6f;
        [Tooltip("How many valuables are inside (rolled per job), and their value against the same item found loose.")]
        [SerializeField] private Vector2Int contents = new(4, 6);
        [SerializeField, Min(1f)] private float valueScale = 1.6f;

        public float DrillSeconds => drillSeconds;
        public float JamCheckSeconds => jamCheckSeconds;
        public float JamChance => jamChance;
        public float DrillNoise => drillNoise;
        public float NoiseInterval => noiseInterval;
        public float Reach => reach;
        public Vector2Int Contents => contents;
        public float ValueScale => valueScale;

        private static VaultConfig current;

        public static VaultConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<VaultConfig>(ResourcePath);
                if (current == null) current = CreateInstance<VaultConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

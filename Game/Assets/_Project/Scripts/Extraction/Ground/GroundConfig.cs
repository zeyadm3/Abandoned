using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Noisy ground (0.17.3) and live water (0.17.5): how loud each kind is underfoot, and what standing in
    /// powered water costs. Data/Extraction/Resources/GroundConfig; code defaults until then.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Ground Config", fileName = "GroundConfig")]
    public class GroundConfig : ScriptableObject
    {
        public const string ResourcePath = "GroundConfig";

        [Tooltip("Noise per step to monsters (broken glass, loose tiles, puddle); 1 = heard 40 m away.")]
        [SerializeField] private float[] stepNoise = { 0.55f, 0.4f, 0.3f };
        [Tooltip("Crouching takes this share of it.")]
        [SerializeField, Range(0f, 1f)] private float crouchShare = 0.15f;
        [Tooltip("Metres moved between two steps' sounds.")]
        [SerializeField, Min(0.3f)] private float stride = 0.9f;

        [Header("Live water (0.17.5)")]
        [Tooltip("Damage per second to anyone standing in water while its sector has power.")]
        [SerializeField, Min(0f)] private float shockDamage = 22f;
        [SerializeField, Min(0.1f)] private float shockInterval = 0.5f;

        public float StepNoise(GroundKind kind) => (int)kind < stepNoise.Length ? stepNoise[(int)kind] : 0.3f;
        public float CrouchShare => crouchShare;
        public float Stride => stride;
        public float ShockDamage => shockDamage;
        public float ShockInterval => shockInterval;

        private static GroundConfig current;

        public static GroundConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<GroundConfig>(ResourcePath);
                if (current == null) current = CreateInstance<GroundConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

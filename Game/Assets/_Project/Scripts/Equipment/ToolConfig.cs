using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// Tunables for the gear added from 0.17 on (sledgehammer, crash mat, chalk, glow sticks and flares,
    /// later the floodlight, lockpick, thermal camera and drone). Data/Equipment/Resources/ToolConfig;
    /// the code defaults apply until that asset exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Equipment/Tool Config", fileName = "ToolConfig")]
    public class ToolConfig : ScriptableObject
    {
        public const string ResourcePath = "ToolConfig";

        [Header("Sledgehammer (0.17.0)")]
        [Tooltip("Reach (m) of a swing.")]
        [SerializeField] private float sledgeReach = 2.8f;
        [Tooltip("Strike momentum (kg·m/s) into the floor section hit; the crowbar's is 1500.")]
        [SerializeField] private float sledgeMomentum = 5200f;
        [SerializeField] private float sledgeCooldown = 1.3f;
        [Tooltip("How loud each blow is to monsters.")]
        [SerializeField, Range(0f, 1f)] private float sledgeNoise = 0.9f;

        public float SledgeReach => sledgeReach;
        public float SledgeMomentum => sledgeMomentum;
        public float SledgeCooldown => sledgeCooldown;
        public float SledgeNoise => sledgeNoise;

        [Header("Crash mat (0.17.1)")]
        [Tooltip("Share of its speed an item (or a falling player) keeps landing on the mat.")]
        [SerializeField, Range(0.02f, 1f)] private float matCushion = 0.12f;
        [Tooltip("How high above the mat (m) the soft zone reaches: what lands from above is caught.")]
        [SerializeField, Min(0.2f)] private float matDepth = 0.9f;

        public float MatCushion => matCushion;
        public float MatDepth => matDepth;

        // @@fields

        private static ToolConfig current;

        public static ToolConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<ToolConfig>(ResourcePath);
                if (current == null) current = CreateInstance<ToolConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>The host's difficulty preset (0.19.4). Stored in the company save; append only.</summary>
    public enum Difficulty : byte { Relaxed, Standard, Brutal }

    /// <summary>
    /// What each difficulty preset scales (0.19.4), all numbers the configs already have: quota, monster
    /// damage, how soon the next monster comes, and how fast the building decays. Index = preset.
    /// Data/Core/Resources/DifficultyConfig; code defaults until then.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Core/Difficulty Config", fileName = "DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        public const string ResourcePath = "DifficultyConfig";

        [SerializeField] private float[] quota = { 0.8f, 1f, 1.25f };
        [SerializeField] private float[] monsterDamage = { 0.7f, 1f, 1.3f };
        [Tooltip("Gap between monster arrivals (bigger = calmer).")]
        [SerializeField] private float[] monsterGap = { 1.4f, 1f, 0.75f };
        [SerializeField] private float[] decay = { 0.7f, 1f, 1.4f };

        /// <summary>Host: the preset this session plays at (set from the company save).</summary>
        public static Difficulty Active { get; set; } = Difficulty.Standard;

        private static float At(float[] a) => a != null && (int)Active < a.Length ? a[(int)Active] : 1f;
        public float Quota => At(quota);
        /// <summary>The quota scale of one preset: for every machine, which knows the preset from the company's synced state, not from <see cref="Active"/>.</summary>
        public float QuotaAt(Difficulty preset) => quota != null && (int)preset < quota.Length ? quota[(int)preset] : 1f;
        public float MonsterDamage => At(monsterDamage);
        public float MonsterGap => At(monsterGap);
        public float Decay => At(decay);

        private static DifficultyConfig current;

        public static DifficultyConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<DifficultyConfig>(ResourcePath);
                if (current == null) current = CreateInstance<DifficultyConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            current = null;
            Active = Difficulty.Standard;
        }
    }
}

using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Pay on top of the quota (0.18): boss requests, rush orders, loot sets and overtime.
    /// Data/Extraction/Resources/BonusConfig; code defaults until then.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Bonus Config", fileName = "BonusConfig")]
    public class BonusConfig : ScriptableObject
    {
        public const string ResourcePath = "BonusConfig";

        [Header("Boss requests (0.18.0)")]
        [Tooltip("The request's bonus: this share of the item's value, never less than the minimum.")]
        [SerializeField, Range(0f, 3f)] private float requestValueShare = 0.8f;
        [SerializeField, Min(0)] private int requestMinBonus = 1500;
        [Tooltip("Seconds into the job before the boss calls it in (the loot must have spawned).")]
        [SerializeField, Min(0f)] private float requestDelay = 6f;

        public float RequestValueShare => requestValueShare;
        public int RequestMinBonus => requestMinBonus;
        public float RequestDelay => requestDelay;

        // @@bonusfields

        private static BonusConfig current;

        public static BonusConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<BonusConfig>(ResourcePath);
                if (current == null) current = CreateInstance<BonusConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

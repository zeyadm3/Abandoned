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

        [Header("Rush orders (0.18.1)")]
        [Tooltip("Seconds into the job before the first rush order, then between rush orders (rolled between).")]
        [SerializeField, Min(0f)] private float rushFirst = 300f;
        [SerializeField] private Vector2 rushGap = new(360f, 540f);
        [Tooltip("Seconds a rush order lasts: what reaches the truck in that time pays this many times its value.")]
        [SerializeField, Min(10f)] private float rushSeconds = 180f;
        [SerializeField, Min(1f)] private float rushMultiplier = 2f;

        public float RushFirst => rushFirst;
        public Vector2 RushGap => rushGap;
        public float RushSeconds => rushSeconds;
        public float RushMultiplier => rushMultiplier;

        [Header("Overtime (0.18.6)")]
        [Tooltip("Once quota is met, the multiplier on everything over quota climbs this much per minute you stay...")]
        [SerializeField, Min(0f)] private float overtimePerMinute = 0.1f;
        [Tooltip("...up to this.")]
        [SerializeField, Min(1f)] private float overtimeMax = 2f;

        public float OvertimePerMinute => overtimePerMinute;
        public float OvertimeMax => overtimeMax;

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

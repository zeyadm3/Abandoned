using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>Company progression tuning (GDD 13). One asset in Data/Company.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Company/Company Config", fileName = "CompanyConfig")]
    public class CompanyConfig : ScriptableObject, IValidatable
    {
        [Tooltip("Experience to reach each level: index 0 = level 2, index 1 = level 3...")]
        [field: SerializeField] public int[] LevelXp { get; private set; } =
            { 20, 50, 90, 140, 200, 270, 350, 440, 540, 650, 770, 900, 1040, 1190, 1350, 1520, 1700, 1890, 2090 };
        [Tooltip("Experience per $1,000 hauled.")]
        [field: SerializeField, Min(0f)] public float XpPerThousand { get; private set; } = 1f;
        [Tooltip("Extra experience for meeting the quota.")]
        [field: SerializeField, Min(0)] public int QuotaXp { get; private set; } = 10;
        [Tooltip("Missed quotas in a row before the company goes under (GDD 13).")]
        [field: SerializeField, Range(1, 10)] public int MissesToBankruptcy { get; private set; } = 3;
        [Tooltip("A missed quota costs this share of the shortfall (debt).")]
        [field: SerializeField, Range(0f, 2f)] public float ShortfallPenalty { get; private set; } = 0.5f;

        [Tooltip("Every job costs this much (fuel, rent, insurance), paid at payday whatever the haul (M10.9).")]
        [field: SerializeField, Min(0)] public int RunningCostBase { get; private set; } = 6000;
        [Tooltip("...plus this much per company level (bigger jobs, bigger bills).")]
        [field: SerializeField, Min(0)] public int RunningCostPerLevel { get; private set; } = 1200;

        [Tooltip("What a full mall run tends to hold ($), for the board's loot estimates.")]
        [field: SerializeField, Min(0)] public int LootEstimate { get; private set; } = 300000;

        public void Validate(List<string> errors)
        {
            for (int i = 1; i < LevelXp.Length; i++)
                if (LevelXp[i] <= LevelXp[i - 1]) errors.Add($"{name}: LevelXp must increase.");
        }

        public int RunningCost(int level) => RunningCostBase + RunningCostPerLevel * Mathf.Max(0, level - 1);

        public int LevelFor(int xp)
        {
            int level = 1;
            foreach (int need in LevelXp) if (xp >= need) level++;
            return level;
        }
    }
}

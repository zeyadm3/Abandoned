using UnityEngine;

namespace Abandoned.World
{
    /// <summary>Tunables for the crew's van in the open world: fuel, damage and breakdowns (Docs/OPEN_WORLD_PLAN.md).</summary>
    [CreateAssetMenu(menuName = "Abandoned/World/Van Config")]
    public class VanConfig : ScriptableObject
    {
        [Header("Fuel")]
        [SerializeField] private float tankLitres = 60f;
        [SerializeField] private float litresPerKm = .18f;
        [SerializeField] private float litresPerMinuteIdle = .05f;

        [Header("Damage")]
        [Tooltip("Impacts slower than this (m/s) do no damage.")]
        [SerializeField] private float safeImpactSpeed = 4f;
        [Tooltip("Damage (0..100) per m/s above the safe speed.")]
        [SerializeField] private float damagePerSpeed = 3f;

        [Header("Breakdowns")]
        [Tooltip("Chance per km of a breakdown at full damage; scales with damage squared.")]
        [SerializeField] private float breakdownPerKmAtFullDamage = .35f;
        [Tooltip("Chance per km for an undamaged van (old engine).")]
        [SerializeField] private float baseBreakdownPerKm = .01f;
        [SerializeField] private float repairSeconds = 12f;

        public float TankLitres => tankLitres;
        public float LitresPerKm => litresPerKm;
        public float LitresPerMinuteIdle => litresPerMinuteIdle;
        public float SafeImpactSpeed => safeImpactSpeed;
        public float DamagePerSpeed => damagePerSpeed;
        public float BreakdownPerKmAtFullDamage => breakdownPerKmAtFullDamage;
        public float BaseBreakdownPerKm => baseBreakdownPerKm;
        public float RepairSeconds => repairSeconds;

        public static VanConfig Load()
        {
            var config = Resources.Load<VanConfig>("VanConfig");
            return config != null ? config : CreateInstance<VanConfig>();
        }
    }
}

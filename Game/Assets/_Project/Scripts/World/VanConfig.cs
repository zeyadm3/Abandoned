using UnityEngine;

namespace Abandoned.World
{
    /// <summary>Tunables for the crew's van in the open world: driving, fuel, damage and breakdowns (Docs/OPEN_WORLD_PLAN.md).</summary>
    [CreateAssetMenu(menuName = "Abandoned/World/Van Config")]
    public class VanConfig : ScriptableObject
    {
        [Header("Driving")]
        [Tooltip("Rigidbody mass (kg). A loaded box van; keeps PhysX stable against loot of up to a few hundred kg.")]
        [SerializeField] private float mass = 3600f;
        [Tooltip("Torque per driven (rear) wheel at full throttle (N m).")]
        [SerializeField] private float motorTorque = 2600f;
        [SerializeField] private float brakeTorque = 9000f;
        [Tooltip("Brake applied when nobody presses anything, so a parked van stays put (N m).")]
        [SerializeField] private float idleBrakeTorque = 600f;
        [SerializeField] private float topSpeedKmh = 95f;
        [SerializeField] private float reverseSpeedKmh = 18f;
        [Tooltip("Front wheel lock at a standstill (degrees); it narrows with speed so the van doesn't flip.")]
        [SerializeField] private float maxSteer = 34f;
        [SerializeField] private float steerAtTopSpeed = 9f;
        [Tooltip("Degrees per second the front wheels turn toward the wanted angle.")]
        [SerializeField] private float steerRate = 90f;
        [Tooltip("Anti-roll bar stiffness per axle (N per metre of difference in compression).")]
        [SerializeField] private float antiRoll = 26000f;
        [Tooltip("Downward push per (m/s)^2, to keep the wheels planted at speed.")]
        [SerializeField] private float downforce = 6f;

        [Header("Fuel")]
        [SerializeField] private float tankLitres = 40f;
        [Tooltip("A worn box van, driven hard: about twenty kilometres (four jobs) on a full tank.")]
        [SerializeField] private float litresPerKm = 2f;
        [SerializeField] private float litresPerMinuteIdle = .3f;
        [Tooltip("Litres a pump puts in per second.")]
        [SerializeField] private float pumpLitresPerSecond = 6f;

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
        [Tooltip("Damage a roadside repair takes off (0..100).")]
        [SerializeField] private float repairPatch = 10f;

        [Header("Seats")]
        [Tooltip("How close (m) a player must be to a door to get in.")]
        [SerializeField] private float seatReach = 3.5f;
        [Tooltip("A driver's input older than this (s) counts as let go: the van brakes.")]
        [SerializeField] private float inputTimeout = .5f;

        public float Mass => mass;
        public float MotorTorque => motorTorque;
        public float BrakeTorque => brakeTorque;
        public float IdleBrakeTorque => idleBrakeTorque;
        public float TopSpeed => topSpeedKmh / 3.6f;
        public float ReverseSpeed => reverseSpeedKmh / 3.6f;
        public float MaxSteer => maxSteer;
        public float SteerAtTopSpeed => steerAtTopSpeed;
        public float SteerRate => steerRate;
        public float AntiRoll => antiRoll;
        public float Downforce => downforce;
        public float TankLitres => tankLitres;
        public float LitresPerKm => litresPerKm;
        public float LitresPerMinuteIdle => litresPerMinuteIdle;
        public float PumpLitresPerSecond => pumpLitresPerSecond;
        public float SafeImpactSpeed => safeImpactSpeed;
        public float DamagePerSpeed => damagePerSpeed;
        public float BreakdownPerKmAtFullDamage => breakdownPerKmAtFullDamage;
        public float BaseBreakdownPerKm => baseBreakdownPerKm;
        public float RepairSeconds => repairSeconds;
        public float RepairPatch => repairPatch;
        public float SeatReach => seatReach;
        public float InputTimeout => inputTimeout;

        public static VanConfig Load()
        {
            var config = Resources.Load<VanConfig>("VanConfig");
            return config != null ? config : CreateInstance<VanConfig>();
        }
    }
}

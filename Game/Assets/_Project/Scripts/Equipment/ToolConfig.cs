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

        [Header("Spray chalk (0.17.6)")]
        [SerializeField, Min(0.5f)] private float chalkReach = 3.5f;
        [Tooltip("Seconds between dots while the button is held (a line is a run of dots).")]
        [SerializeField, Min(0.02f)] private float chalkInterval = 0.06f;
        [SerializeField, Min(0.02f)] private float chalkDotSize = 0.11f;
        [Tooltip("Dots kept per job; the oldest go first.")]
        [SerializeField, Range(50, 2000)] private int chalkMaxDots = 800;
        [Tooltip("Unlit material the marks are drawn with (made by the 0.17.6 step).")]
        [SerializeField] private Material chalkMaterial;

        public float ChalkReach => chalkReach;
        public float ChalkInterval => chalkInterval;
        public float ChalkDotSize => chalkDotSize;
        public int ChalkMaxDots => chalkMaxDots;
        public Material ChalkMaterial => chalkMaterial;

        [Header("Moving blanket (0.18.5)")]
        [Tooltip("Share of an impact's speed a wrapped item feels.")]
        [SerializeField, Range(0.05f, 1f)] private float blanketCushion = 0.4f;
        [SerializeField, Min(0.5f)] private float blanketReach = 2.6f;
        [SerializeField] private Material blanketMaterial;

        public float BlanketCushion => blanketCushion;
        public float BlanketReach => blanketReach;
        public Material BlanketMaterial => blanketMaterial;

        [Header("Portable floodlight (0.20.0)")]
        [SerializeField, Min(10f)] private float floodlightSeconds = 240f;
        [SerializeField, Min(0.1f)] private float floodlightIntensity = 40f;
        [SerializeField, Min(1f)] private float floodlightRange = 22f;
        [SerializeField, Range(10f, 150f)] private float floodlightAngle = 95f;
        [Tooltip("At most this many floodlights cast shadows at once (each is one map in the shadow atlas).")]
        [SerializeField, Range(0, 4)] private int floodlightShadows = 1;

        public float FloodlightSeconds => floodlightSeconds;
        public float FloodlightIntensity => floodlightIntensity;
        public float FloodlightRange => floodlightRange;
        public float FloodlightAngle => floodlightAngle;
        public int FloodlightShadows => floodlightShadows;

        [Header("Lockpick (0.20.1)")]
        [Tooltip("Seconds to pick a shutter's padlock (bolt cutters are quicker, a crowbar quicker still and loud).")]
        [SerializeField, Min(1f)] private float lockpickSeconds = 14f;

        public float LockpickSeconds => lockpickSeconds;

        [Header("Thermal camera (0.20.4)")]
        [SerializeField, Min(5f)] private float thermalSeconds = 60f;
        [Tooltip("Battery seconds regained per second while off.")]
        [SerializeField, Min(0f)] private float thermalRecharge = 0.3f;

        public float ThermalSeconds => thermalSeconds;
        public float ThermalRecharge => thermalRecharge;

        [Header("Drone (0.20.5)")]
        [SerializeField, Min(5f)] private float droneSeconds = 90f;
        [SerializeField, Min(0.5f)] private float droneSpeed = 4.5f;
        [Tooltip("How loud its rotors are to monsters, every 1.5 s.")]
        [SerializeField, Range(0f, 1f)] private float droneNoise = 0.35f;

        public float DroneSeconds => droneSeconds;
        public float DroneSpeed => droneSpeed;
        public float DroneNoise => droneNoise;

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

#if UNITY_EDITOR
        public void EditorSetChalkMaterial(Material material) => chalkMaterial = material;
        public void EditorSetBlanketMaterial(Material material) => blanketMaterial = material;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

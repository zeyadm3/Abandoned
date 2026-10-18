using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Sliding doors and hiding lockers (0.16.0): how doors move and sound, how many start a job shut, how
    /// long the Hunter needs to break one down, and the rules for climbing into a locker.
    /// Lives at Data/Extraction/Resources/DoorConfig (Tools/Abandoned/Content/Create Door And Locker Config
    /// makes it); the defaults below apply until that asset exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Door And Locker Config", fileName = "DoorConfig")]
    public class DoorConfig : ScriptableObject
    {
        public const string ResourcePath = "DoorConfig";

        [Header("Doors")]
        [Tooltip("How close (m) a player must be to a door to slide it.")]
        [SerializeField, Min(1f)] private float reach = 3.2f;
        [Tooltip("Seconds between two pushes on doors by the same player.")]
        [SerializeField, Min(0f)] private float useCooldown = 0.35f;
        [Tooltip("Seconds a door takes to slide open or shut.")]
        [SerializeField, Min(0.05f)] private float slideSeconds = 0.6f;
        [Tooltip("Seconds a door takes to fly open when it is broken down.")]
        [SerializeField, Min(0.05f)] private float breakSeconds = 0.2f;
        [Tooltip("How loud sliding a door is to monsters that listen (1 = heard 40 m away).")]
        [SerializeField, Range(0f, 1f)] private float slideNoise = 0.25f;
        [Tooltip("Chance a store's doors start the job shut.")]
        [SerializeField, Range(0f, 1f)] private float storeShutChance = 0.25f;
        [Tooltip("Chance a service corridor's door starts the job shut.")]
        [SerializeField, Range(0f, 1f)] private float serviceShutChance = 0.5f;

        [Header("The Hunter at a shut door")]
        [Tooltip("It breaks down a door shut between it and where it last saw someone, if the door is this near (m).")]
        [SerializeField, Min(1f)] private float bashNoticeRange = 12f;
        [Tooltip("How close (m) it stands to the door to hit it.")]
        [SerializeField, Min(0.5f)] private float bashReach = 1.9f;
        [Tooltip("Blows before the door gives way.")]
        [SerializeField, Range(1, 10)] private int bashHits = 3;
        [Tooltip("Seconds between blows (the first lands after this too).")]
        [SerializeField, Min(0.2f)] private float bashInterval = 1.3f;
        [Tooltip("How loud each blow is to monsters that listen.")]
        [SerializeField, Range(0f, 1f)] private float bashNoise = 0.8f;
        [Tooltip("The Last Hunter breaks any shut door this near it (m).")]
        [SerializeField, Min(0.5f)] private float lastHunterBreakRadius = 3f;

        [Header("Lockers")]
        [Tooltip("How close (m) a player must be to the front of a locker to climb in.")]
        [SerializeField, Min(0.5f)] private float hideReach = 2.6f;
        [Tooltip("Seconds after climbing out before the same player can hide again.")]
        [SerializeField, Min(0f)] private float hideCooldown = 0.8f;
        [Tooltip("A monster that is after you and sees you climb in from within this distance (m) knows where you are.")]
        [SerializeField, Min(1f)] private float witnessRange = 16f;
        [Tooltip("How far (degrees) each way a hiding player can turn their head.")]
        [SerializeField, Range(10f, 90f)] private float hideLookAngle = 55f;
        [Tooltip("Seconds a locker's door stands open when someone climbs in or out.")]
        [SerializeField, Min(0.1f)] private float lockerDoorSeconds = 0.45f;
        [Tooltip("How loud a locker door is to monsters that listen.")]
        [SerializeField, Range(0f, 1f)] private float lockerNoise = 0.18f;

        public float Reach => reach;
        public float UseCooldown => useCooldown;
        public float SlideSeconds => slideSeconds;
        public float BreakSeconds => breakSeconds;
        public float SlideNoise => slideNoise;
        public float StoreShutChance => storeShutChance;
        public float ServiceShutChance => serviceShutChance;
        public float BashNoticeRange => bashNoticeRange;
        public float BashReach => bashReach;
        public int BashHits => bashHits;
        public float BashInterval => bashInterval;
        public float BashNoise => bashNoise;
        public float LastHunterBreakRadius => lastHunterBreakRadius;
        public float HideReach => hideReach;
        public float HideCooldown => hideCooldown;
        public float WitnessRange => witnessRange;
        public float HideLookAngle => hideLookAngle;
        public float LockerDoorSeconds => lockerDoorSeconds;
        public float LockerNoise => lockerNoise;
        [field: SerializeField, Range(0f, .6f)] public float PeekDistance { get; private set; } = .3f;
        [field: SerializeField, Range(30f, 360f)] public float PeekReturnSpeed { get; private set; } = 160f;

        private static DoorConfig current;

        public static DoorConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<DoorConfig>(ResourcePath);
                if (current == null) current = CreateInstance<DoorConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// How the Iron Factory's machines behave once a breaker brings them back (<see cref="MachineHazard"/>):
    /// each kind's cycle (seconds between strikes, how long the warning lamp shows first, how long a strike
    /// lasts), its damage, and how loud it is to monsters. Lives at Data/Extraction/Resources/MachineHazardConfig
    /// (the Iron Factory build makes it); the defaults below apply until that asset exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Machine Hazard Config", fileName = "MachineHazardConfig")]
    public class MachineHazardConfig : ScriptableObject
    {
        public const string ResourcePath = "MachineHazardConfig";

        public readonly struct Cycle
        {
            public readonly float Period, Warning, Strike, Damage, Noise;
            public Cycle(float period, float warning, float strike, float damage, float noise)
            { Period = period; Warning = warning; Strike = strike; Damage = damage; Noise = noise; }
        }

        [Header("Press")]
        [SerializeField, Min(1f)] private float pressPeriod = 5f;
        [SerializeField, Min(0f)] private float pressWarning = 1.2f;
        [SerializeField, Min(.05f)] private float pressStrike = .35f;
        [SerializeField, Min(0f)] private float pressDamage = 45f;
        [Tooltip("How loud each slam is to monsters that listen (1 = heard 40 m away).")]
        [SerializeField, Range(0f, 1f)] private float pressNoise = .85f;

        [Header("Mill-stand rolls")]
        [SerializeField, Min(.2f)] private float rollerPeriod = 1f;
        [SerializeField, Min(.05f)] private float rollerStrike = .25f;
        [SerializeField, Min(0f)] private float rollerDamage = 10f;
        [SerializeField, Range(0f, 1f)] private float rollerNoise = .3f;

        [Header("Steam")]
        [SerializeField, Min(1f)] private float steamPeriod = 7f;
        [SerializeField, Min(0f)] private float steamWarning = 1.5f;
        [SerializeField, Min(.05f)] private float steamStrike = 1f;
        [SerializeField, Min(0f)] private float steamDamage = 20f;
        [SerializeField, Range(0f, 1f)] private float steamNoise = .5f;

        [Header("Belts and roller tables")]
        [Tooltip("Metres per second a running belt carries whoever stands on it.")]
        [SerializeField, Min(0f)] private float beltSpeed = 1.3f;

        [Header("Everyone")]
        [Tooltip("Speed (m/s) a strike knocks a player away from the machine (the shove cap is 2.2).")]
        [SerializeField, Range(0f, 2.2f)] private float knock = 2.2f;

        [field: SerializeField] public string PressDeathLine { get; private set; } = "Crushed in a press."; // @loc
        [field: SerializeField] public string RollerDeathLine { get; private set; } = "Pulled into the rolls."; // @loc
        [field: SerializeField] public string SteamDeathLine { get; private set; } = "Scalded by steam."; // @loc

        public float BeltSpeed => beltSpeed;
        public float Knock => knock;

        public Cycle For(MachineHazardKind kind) => kind switch
        {
            MachineHazardKind.Press => new Cycle(pressPeriod, pressWarning, pressStrike, pressDamage, pressNoise),
            MachineHazardKind.Rollers => new Cycle(rollerPeriod, 0f, rollerStrike, rollerDamage, rollerNoise),
            MachineHazardKind.Steam => new Cycle(steamPeriod, steamWarning, steamStrike, steamDamage, steamNoise),
            _ => new Cycle(1f, 0f, 0f, 0f, 0f),
        };

        public string DeathLine(MachineHazardKind kind) => kind switch
        {
            MachineHazardKind.Press => PressDeathLine,
            MachineHazardKind.Rollers => RollerDeathLine,
            _ => SteamDeathLine,
        };

        private static MachineHazardConfig cached;

        public static MachineHazardConfig Current
        {
            get
            {
                if (cached != null) return cached;
                cached = Resources.Load<MachineHazardConfig>(ResourcePath);
                if (cached == null) cached = CreateInstance<MachineHazardConfig>();
                return cached;
            }
        }
    }
}

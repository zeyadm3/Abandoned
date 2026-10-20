using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The training job's tunables. Lives at Data/Tutorial/Resources/TutorialConfig (Tools > Abandoned > Maps >
    /// Tutorial House > Build Tutorial Lessons); the defaults below apply until that asset exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Tutorial/Tutorial Config", fileName = "TutorialConfig")]
    public class TutorialConfig : ScriptableObject
    {
        public const string ResourcePath = "TutorialConfig";

        [Header("The job")]
        [Tooltip("What the truck's board asks for. Training pays nothing; the number is there to be read.")]
        [SerializeField, Min(0)] private int quota = 6000;
        [Tooltip("Seconds on the job's clock. Long: nobody should feel rushed while learning.")]
        [SerializeField, Min(60f)] private float windowSeconds = 3600f;
        [Tooltip("The run's seed (the same house every time).")]
        [SerializeField, Min(1)] private int seed = 1214;
        [Tooltip("The house's power is off (the flashlight and breaker lessons need the dark).")]
        [SerializeField] private bool powerOff = true;

        [Header("Safety")]
        [Tooltip("Health never falls below this in training: hits still hurt and show, nobody dies.")]
        [SerializeField, Range(1f, 60f)] private float healthFloor = 12f;
        [Tooltip("Scales what the training monster's hits do.")]
        [SerializeField, Range(0f, 1f)] private float threatDamageScale = 0.4f;

        [Header("The visitor")]
        [Tooltip("The kind of monster the hiding lesson sends in (it should hunt by sound: the lesson is about keeping quiet).")]
        [SerializeField] private Threats.ThreatKind visitorKind = Threats.ThreatKind.BlindOne;
        [Tooltip("Seconds after the hiding lesson starts before the monster heads back out.")]
        [SerializeField, Min(3f)] private float threatLeaveAfter = 14f;
        [Tooltip("Seconds the monster is given to walk out before it is simply taken away.")]
        [SerializeField, Min(5f)] private float threatLeaveTimeout = 30f;
        [Tooltip("Within this many metres of its way out it is gone.")]
        [SerializeField, Min(0.5f)] private float threatExitRadius = 1.8f;

        [Header("Lessons")]
        [Tooltip("Seconds a dispatcher line stays on the radio.")]
        [SerializeField, Min(2f)] private float radioSeconds = 7f;
        [Tooltip("A lesson that only explains something ends after this many seconds when its own value is 0.")]
        [SerializeField, Min(2f)] private float readSeconds = 9f;
        [Tooltip("Personal lessons (look, sprint, crouch) are met by the first player to do them; the crew moves on together.")]
        [SerializeField] private bool anyPlayerCompletes = true;

        [Header("HQ")]
        [Tooltip("A player who never finished training is told about it at the HQ (once per session).")]
        [SerializeField] private bool remindAtHq = true;

        public int Quota => quota;
        public float WindowSeconds => windowSeconds;
        public int Seed => seed;
        public bool PowerOff => powerOff;
        public float HealthFloor => healthFloor;
        public float ThreatDamageScale => threatDamageScale;
        public Threats.ThreatKind VisitorKind => visitorKind;
        public float ThreatLeaveAfter => threatLeaveAfter;
        public float ThreatLeaveTimeout => threatLeaveTimeout;
        public float ThreatExitRadius => threatExitRadius;
        public float RadioSeconds => radioSeconds;
        public float ReadSeconds => readSeconds;
        public bool AnyPlayerCompletes => anyPlayerCompletes;
        public bool RemindAtHq => remindAtHq;

        private static TutorialConfig current;

        public static TutorialConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<TutorialConfig>(ResourcePath);
                if (current == null) current = CreateInstance<TutorialConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

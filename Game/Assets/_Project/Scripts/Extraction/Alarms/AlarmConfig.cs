using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>Store alarms (0.16.5): how many stores are wired, how long and loud the bell rings. Data/Extraction/Resources/AlarmConfig.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Alarm Config", fileName = "AlarmConfig")]
    public class AlarmConfig : ScriptableObject
    {
        public const string ResourcePath = "AlarmConfig";

        [Tooltip("Chance each store is wired this job.")]
        [SerializeField, Range(0f, 1f)] private float wiredChance = 0.35f;
        [Tooltip("Seconds after the job starts before alarms arm (nobody trips one from the car park).")]
        [SerializeField, Min(0f)] private float armDelay = 5f;
        [Tooltip("Seconds a tripped alarm rings (bolt cutters on the box silence it sooner).")]
        [SerializeField, Min(1f)] private float ringSeconds = 30f;
        [Tooltip("How loud the bell is to monsters, every few seconds while it rings (1 = heard 40 m away).")]
        [SerializeField, Range(0f, 1f)] private float ringNoise = 1f;
        [SerializeField, Min(0.5f)] private float ringNoiseInterval = 2.5f;
        [SerializeField, Min(0.5f)] private float reach = 2.4f;

        public float WiredChance => wiredChance;
        public float ArmDelay => armDelay;
        public float RingSeconds => ringSeconds;
        public float RingNoise => ringNoise;
        public float RingNoiseInterval => ringNoiseInterval;
        public float Reach => reach;

        private static AlarmConfig current;

        public static AlarmConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<AlarmConfig>(ResourcePath);
                if (current == null) current = CreateInstance<AlarmConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

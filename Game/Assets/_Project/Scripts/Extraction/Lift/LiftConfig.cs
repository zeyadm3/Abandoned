using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The freight lift (0.16.3): speed, load limit, what an overload costs, how loud it is. Lives at
    /// Data/Extraction/Resources/LiftConfig; code defaults until then.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Lift Config", fileName = "LiftConfig")]
    public class LiftConfig : ScriptableObject
    {
        public const string ResourcePath = "LiftConfig";

        [Tooltip("Seconds for one trip between floors (slow on purpose).")]
        [SerializeField, Min(1f)] private float travelSeconds = 14f;
        [Tooltip("Gameplay weight (kg) it carries safely: players with what they hold, plus loot on the deck.")]
        [SerializeField, Min(50f)] private float loadLimit = 450f;
        [Tooltip("Overloaded, it gives way this far into the trip (0-1) and drops to the bottom.")]
        [SerializeField, Range(0.05f, 0.95f)] private float snapAt = 0.5f;
        [Tooltip("Damage to everyone riding it when it hits the bottom.")]
        [SerializeField, Min(0f)] private float dropDamage = 35f;
        [Tooltip("How close (m) a player must be to a lift button.")]
        [SerializeField, Min(0.5f)] private float reach = 3f;
        [Tooltip("Motor noise to monsters while it runs (1 = heard 40 m away), every few seconds.")]
        [SerializeField, Range(0f, 1f)] private float runNoise = 0.6f;
        [SerializeField, Min(0.5f)] private float runNoiseInterval = 1.5f;

        public float TravelSeconds => travelSeconds;
        public float LoadLimit => loadLimit;
        public float SnapAt => snapAt;
        public float DropDamage => dropDamage;
        public float Reach => reach;
        public float RunNoise => runNoise;
        public float RunNoiseInterval => runNoiseInterval;

        private static LiftConfig current;

        public static LiftConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<LiftConfig>(ResourcePath);
                if (current == null) current = CreateInstance<LiftConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

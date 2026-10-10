using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Seeded route changes (0.16.2): how much of the mall is already down when a job starts. Locked stores
    /// (RunShutters) and shut doors (RunDoors) already change per job; this adds fallen floor and blocked
    /// escalators. Lives at Data/Extraction/Resources/RouteConfig; code defaults until then.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Route Config", fileName = "RouteConfig")]
    public class RouteConfig : ScriptableObject
    {
        public const string ResourcePath = "RouteConfig";

        [Tooltip("Floor sections already fallen when the crew arrives (rolled per job between these).")]
        [SerializeField] private Vector2Int fallen = new(2, 5);
        [Tooltip("Chance one of the escalators is down this job (the service stairs never are).")]
        [SerializeField, Range(0f, 1f)] private float escalatorChance = 0.4f;
        [Tooltip("Fallen sections stand at least this far (m) apart, so one job doesn't lose a whole wing.")]
        [SerializeField, Min(0f)] private float apart = 9f;
        [Tooltip("Seconds after the job starts before they give way (the crew is still at the van).")]
        [SerializeField, Min(0f)] private float delay = 3f;
        [Tooltip("Name prefix of escalator sections.")]
        [SerializeField] private string escalatorPrefix = "Escalator_";

        public Vector2Int Fallen => fallen;
        public float EscalatorChance => escalatorChance;
        public float Apart => apart;
        public float Delay => delay;
        public string EscalatorPrefix => escalatorPrefix;

        private static RouteConfig current;

        public static RouteConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<RouteConfig>(ResourcePath);
                if (current == null) current = CreateInstance<RouteConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

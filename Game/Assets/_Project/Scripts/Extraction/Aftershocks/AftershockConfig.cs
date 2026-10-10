using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>Aftershocks (0.17.4): when tremors come, how much warning they give, what they break. Data/Extraction/Resources/AftershockConfig.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Aftershock Config", fileName = "AftershockConfig")]
    public class AftershockConfig : ScriptableObject
    {
        public const string ResourcePath = "AftershockConfig";

        [Tooltip("Seconds into the job before the first tremor can come.")]
        [SerializeField, Min(0f)] private float firstAfter = 240f;
        [Tooltip("Seconds between tremors (rolled between these).")]
        [SerializeField] private Vector2 gap = new(240f, 420f);
        [Tooltip("Seconds of rumbling warning before the shock.")]
        [SerializeField, Min(1f)] private float warning = 8f;
        [Tooltip("Collapsible sections each shock cracks, and how much of their health it takes (never below the floor).")]
        [SerializeField] private Vector2Int sections = new(4, 8);
        [SerializeField, Range(0f, 1f)] private float damage = 0.35f;
        [SerializeField, Range(0f, 1f)] private float healthFloor = 0.15f;
        [Tooltip("Camera shake strength for everyone inside.")]
        [SerializeField, Min(0f)] private float shake = 3500f;

        public float FirstAfter => firstAfter;
        public Vector2 Gap => gap;
        public float Warning => warning;
        public Vector2Int Sections => sections;
        public float Damage => damage;
        public float HealthFloor => healthFloor;
        public float Shake => shake;

        private static AftershockConfig current;

        public static AftershockConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<AftershockConfig>(ResourcePath);
                if (current == null) current = CreateInstance<AftershockConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

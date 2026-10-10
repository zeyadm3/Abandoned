using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>Vents (0.16.4): crawl speed, how close to a grille you must be, how loud the duct is. Data/Extraction/Resources/VentConfig.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Extraction/Vent Config", fileName = "VentConfig")]
    public class VentConfig : ScriptableObject
    {
        public const string ResourcePath = "VentConfig";

        [Tooltip("Crawl speed (m/s) through a duct; a trip takes the straight distance between grilles at this speed.")]
        [SerializeField, Min(0.2f)] private float crawlSpeed = 1.4f;
        [SerializeField, Min(1f)] private float minSeconds = 4f;
        [SerializeField, Min(0.5f)] private float reach = 1.8f;
        [Tooltip("The duct rattles this loud (to monsters) every few seconds while someone is in it, at its middle.")]
        [SerializeField, Range(0f, 1f)] private float rattleNoise = 0.3f;
        [SerializeField, Min(0.5f)] private float rattleInterval = 2f;

        public float CrawlSpeed => crawlSpeed;
        public float MinSeconds => minSeconds;
        public float Reach => reach;
        public float RattleNoise => rattleNoise;
        public float RattleInterval => rattleInterval;

        private static VentConfig current;

        public static VentConfig Current
        {
            get
            {
                if (current != null) return current;
                current = Resources.Load<VentConfig>(ResourcePath);
                if (current == null) current = CreateInstance<VentConfig>();
                return current;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

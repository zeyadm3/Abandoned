using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// The threat forecast (0.20.6): which monster a job opens with, worked out at the HQ from the contract's
    /// seed exactly as the mall's ThreatDirector will (its catalog-built opening weights and roll), so the
    /// board can say before anyone sets out. Data/Threats/Resources/ThreatForecast points at the catalog.
    /// </summary>
    [CreateAssetMenu(menuName = "Abandoned/Threats/Threat Forecast", fileName = "ThreatForecast")]
    public class ThreatForecast : ScriptableObject
    {
        public const string ResourcePath = "ThreatForecast";

        [SerializeField] private ThreatCatalog catalog;

        private static ThreatForecast current;
        public static ThreatForecast Current => current != null ? current : current = Resources.Load<ThreatForecast>(ResourcePath);

        /// <summary>The opening monster's name for a run seed, or null when it can't be told.</summary>
        public string Opening(int seed)
        {
            if (catalog == null || catalog.Definitions.Length == 0) return null;
            ThreatDefinition[] defs = catalog.Definitions;
            float Weight(ThreatDefinition d) => d == null || d.Prefab == null || d.FinalOnly || d.MinimumDanger != 0 || d.LethalContact || d.Kind == ThreatKind.Collector ? 0f : d.OpeningWeight;
            float total = 0f;
            foreach (ThreatDefinition d in defs) total += Weight(d);
            if (total <= 0f) return null;
            double roll = new System.Random(seed).NextDouble() * total;
            foreach (ThreatDefinition d in defs)
            {
                float w = Weight(d);
                if (w <= 0f) continue;
                roll -= w;
                if (roll <= 0) return d.DisplayName;
            }
            return null;
        }

#if UNITY_EDITOR
        public void EditorSetup(ThreatCatalog threats) => catalog = threats;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => current = null;
    }
}

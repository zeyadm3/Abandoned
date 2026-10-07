using Abandoned.Company;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The truck's floodlights (GDD 13 truck upgrade, M10.1): once the company owns them they light the
    /// lot and the loading bay on every machine, so a dark job still has a bright way out. Local only:
    /// every machine reads the company's replicated upgrades.
    /// </summary>
    public class TruckFloodlights : MonoBehaviour
    {
        [SerializeField] private Light[] lights = System.Array.Empty<Light>();
        [Tooltip("Glowing lamp housings, shown lit only when the lights are on.")]
        [SerializeField] private Renderer[] lamps = System.Array.Empty<Renderer>();

        private bool? shown;

        public bool On => shown ?? false;

        private void Update()
        {
            CompanyService company = CompanyService.Current;
            bool on = CompanyService.RulesApply && company.HasUpgrade(TruckUpgradeKind.Floodlights);
            if (shown == on) return;
            shown = on;
            foreach (Light l in lights) if (l != null) l.enabled = on;
            foreach (Renderer r in lamps) if (r != null) r.enabled = on;
        }

#if UNITY_EDITOR
        public void EditorSetup(Light[] floodLights, Renderer[] lampHousings)
        {
            lights = floodLights;
            lamps = lampHousings;
        }
#endif
    }
}

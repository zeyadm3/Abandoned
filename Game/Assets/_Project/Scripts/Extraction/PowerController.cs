using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The building's power (GDD 14 contracts: "Power: OFF"): on every machine, the level's own lights go
    /// dark when the run's terms say so, and come back for a run with power. Flashlights matter then.
    /// </summary>
    public class PowerController : MonoBehaviour
    {
        [Tooltip("Ambient light with the power off (the building is dark, not black).")]
        [SerializeField] private Color darkAmbient = new(0.05f, 0.05f, 0.06f);

        private readonly List<Light> lights = new();
        private Color litAmbient;
        private bool? powered;

        public bool Powered => powered ?? true;

        private void Start()
        {
            litAmbient = RenderSettings.ambientLight;
            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type != LightType.Directional && l.gameObject.scene == gameObject.scene) lights.Add(l);
        }

        private void Update()
        {
            RunState run = RunState.Current;
            bool on = run == null || !run.IsSpawned || !run.State.PowerOff;
            if (powered == on) return;
            powered = on;
            foreach (Light l in lights) if (l != null) l.enabled = on;
            RenderSettings.ambientLight = on ? litAmbient : darkAmbient;
        }
    }
}

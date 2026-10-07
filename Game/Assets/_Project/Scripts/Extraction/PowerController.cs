using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The building's power (GDD 14 contracts: "Power: OFF"): on every machine, the level's own lights go
    /// dark when the run's terms say so, and come back for a run with power. Flashlights matter then.
    /// Ceiling fixtures (LightFixture) dim their panels too; other non-directional lights just switch.
    /// </summary>
    public class PowerController : MonoBehaviour
    {
        [Tooltip("Ambient light with the power off, as a share of the lit ambient (the building is dark, not black).")]
        [SerializeField, Range(0f, 1f)] private float darkAmbientScale = 0.18f;

        private readonly List<Light> lights = new();
        private readonly List<LightFixture> fixtures = new();
        private Color litSky, litEquator, litGround;
        private bool? powered;

        public bool Powered => powered ?? true;

        private void Start()
        {
            litSky = RenderSettings.ambientSkyColor;
            litEquator = RenderSettings.ambientEquatorColor;
            litGround = RenderSettings.ambientGroundColor;
            foreach (LightFixture f in FindObjectsByType<LightFixture>(FindObjectsSortMode.None))
                if (f.gameObject.scene == gameObject.scene) fixtures.Add(f);
            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type != LightType.Directional && l.gameObject.scene == gameObject.scene && l.GetComponentInParent<LightFixture>() == null)
                    lights.Add(l);
        }

        private void Update()
        {
            RunState run = RunState.Current;
            bool on = run == null || !run.IsSpawned || !run.State.PowerOff;
            if (powered == on) return;
            powered = on;
            foreach (Light l in lights) if (l != null) l.enabled = on;
            foreach (LightFixture f in fixtures) if (f != null) f.SetPowered(on);
            float k = on ? 1f : darkAmbientScale;
            // ambientLight is the sky colour; set all three so a gradient ambient dims evenly.
            RenderSettings.ambientSkyColor = litSky * k;
            RenderSettings.ambientEquatorColor = litEquator * k;
            RenderSettings.ambientGroundColor = litGround * k;
        }

        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            int lit = 0, faulty = 0;
            foreach (LightFixture f in fixtures)
                if (f != null && f.Lit) { lit++; if (f.Faulty) faulty++; }
            GUI.Label(new Rect(Screen.width - 360f, 52f, 350f, 22f),
                $"POWER {(Powered ? "on" : "OFF")}  fixtures lit {lit}/{fixtures.Count} ({faulty} faulty)  fog {(RenderSettings.fog ? RenderSettings.fogDensity.ToString("0.000") : "off")}");
        }
    }
}

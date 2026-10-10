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
        [Tooltip("Night jobs (M9.2): ambient and sun as a share of daylight, on top of the power.")]
        [SerializeField, Range(0f, 1f)] private float nightAmbientScale = 0.35f;
        [SerializeField, Range(0f, 1f)] private float nightSunScale = 0.06f;

        private readonly List<Light> lights = new();
        private readonly List<LightFixture> fixtures = new();
        private Color litSky, litEquator, litGround;
        private float daySun = -1f;
        private bool? powered, night;
        private int shownBreakers;

        public bool Powered => powered ?? true;
        public bool IsNight => night ?? false;

        private void Start()
        {
            litSky = RenderSettings.ambientSkyColor;
            litEquator = RenderSettings.ambientEquatorColor;
            litGround = RenderSettings.ambientGroundColor;
            foreach (LightFixture f in FindObjectsByType<LightFixture>(FindObjectsSortMode.None))
                if (f.gameObject.scene == gameObject.scene) fixtures.Add(f);
            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                // The truck's floodlights run off its own battery, not the building's power.
                if (l.type != LightType.Directional && l.gameObject.scene == gameObject.scene && l.GetComponentInParent<LightFixture>() == null
                    && l.GetComponentInParent<TruckFloodlights>() == null && l.GetComponentInParent<TruckCargo>() == null)
                    lights.Add(l);
        }

        private void Update()
        {
            RunState run = RunState.Current;
            bool on = run == null || !run.IsSpawned || !run.State.PowerOff;
            bool dark = run != null && run.IsSpawned && run.State.Night;
            // 0.16.1: with the mains off, a thrown breaker still lights its own sector's fixtures.
            int breakers = !on && RunBreakers.Current != null ? RunBreakers.Current.RestoredMask : 0;
            if (powered == on && night == dark && breakers == shownBreakers) return;
            if (powered == on && night == dark)
            {
                shownBreakers = breakers;
                ApplyBreakers(on, breakers);
                return;
            }
            shownBreakers = breakers;
            // The mains going, or coming back: everyone hears the breaker (not the day/night change).
            if (powered != on && run != null && run.IsSpawned) Audio.GameAudio.PlayUi(on ? Audio.SoundId.PowerUp : Audio.SoundId.PowerDown, 0.9f);
            powered = on;
            night = dark;
            foreach (Light l in lights) if (l != null) l.enabled = on;
            ApplyBreakers(on, breakers);
            Light sun = RenderSettings.sun;
            if (sun != null)
            {
                if (daySun < 0f) daySun = sun.intensity;
                sun.intensity = daySun * (dark ? nightSunScale : 1f);
            }
            float k = (on ? 1f : darkAmbientScale) * (dark ? nightAmbientScale : 1f);
            // ambientLight is the sky colour; set all three so a gradient ambient dims evenly.
            RenderSettings.ambientSkyColor = litSky * k;
            RenderSettings.ambientEquatorColor = litEquator * k;
            RenderSettings.ambientGroundColor = litGround * k;
        }

        private void ApplyBreakers(bool on, int breakers)
        {
            HorrorConfig sectors = RunHorrorDirector.Current != null ? RunHorrorDirector.Current.Config : null;
            foreach (LightFixture f in fixtures)
                if (f != null) f.SetPowered(on || (sectors != null && (breakers & (1 << sectors.SectorOf(f.transform.position))) != 0));
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            int lit = 0, faulty = 0;
            foreach (LightFixture f in fixtures)
                if (f != null && f.Lit) { lit++; if (f.Faulty) faulty++; }
            GUI.Label(new Rect(Screen.width - 360f, 52f, 350f, 22f),
                $"POWER {(Powered ? "on" : "OFF")}{(IsNight ? "  NIGHT" : "")}  fixtures lit {lit}/{fixtures.Count} ({faulty} faulty)  fog {(RenderSettings.fog ? RenderSettings.fogDensity.ToString("0.000") : "off")}");
        }
#endif
    }
}

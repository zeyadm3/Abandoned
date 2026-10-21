using System.Collections.Generic;
using Abandoned.Structure;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abandoned.Core
{
    /// <summary>
    /// A bounded diffuse-bounce approximation for realtime fixtures. Only nearby visible receivers are sampled;
    /// no baked light survives a blackout or a collapsed floor. Local presentation, never networked.
    /// </summary>
    public sealed class IndirectLighting : MonoBehaviour
    {
        [SerializeField] private IndirectLightingConfig config;
        private readonly List<LightFixture> fixtures = new();
        private readonly List<LightFixture> candidates = new();
        private readonly List<Bounce> bounces = new();
        private float nextChoose;
        private int nextSample;
        private int mask;

        private sealed class Bounce
        {
            public Light Light;
            public LightFixture Source;
            public Vector3 Hit;
            public Color Tint;
            public float RefreshAt;
            public bool Valid;
        }

        private void Start() => Initialize();

        private void Initialize()
        {
            if (config == null || bounces.Count > 0) return;
            mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast", "UI");
            foreach (LightFixture fixture in FindObjectsByType<LightFixture>(FindObjectsSortMode.None))
                if (fixture.gameObject.scene == gameObject.scene && fixture.Lamp != null) fixtures.Add(fixture);
            for (int i = 0; i < config.Lights(3); i++)
            {
                var go = new GameObject("Diffuse bounce " + i);
                go.transform.SetParent(transform, false);
                Light light = go.AddComponent<Light>();
                light.type = LightType.Spot;
                light.spotAngle = 160f;
                light.innerSpotAngle = 100f;
                light.shadows = LightShadows.None;
                light.enabled = false;
                light.bounceIntensity = 0f;
                // Generated lights follow their source. PowerController must not collect and re-enable them.
                go.AddComponent<PowerIndependentLight>();
                bounces.Add(new Bounce { Light = light });
            }
        }

        private void LateUpdate()
        {
            Camera eye = Camera.main;
            if (eye != null) Tick(eye);
        }

        private void Tick(Camera eye)
        {
            if (config == null) return;
            int limit = config.Lights(VideoSettings.Shadows);
            if (Time.unscaledTime >= nextChoose)
            {
                nextChoose = Time.unscaledTime + .35f;
                candidates.Clear();
                foreach (LightFixture fixture in fixtures)
                    if (fixture != null && fixture.Lit && fixture.Lamp.isActiveAndEnabled &&
                        Vector3.Distance(fixture.Lamp.transform.position, eye.transform.position) < config.Distance)
                        candidates.Add(fixture);
                candidates.Sort((a, b) => Score(a, eye).CompareTo(Score(b, eye)));
                // Keep existing assignments so every camera step doesn't retrace all receivers.
                foreach (Bounce bounce in bounces)
                    if (bounce.Source != null && (candidates.IndexOf(bounce.Source) < 0 || candidates.IndexOf(bounce.Source) >= limit))
                    { bounce.Source = null; bounce.Valid = false; }
                for (int i = 0; i < Mathf.Min(limit, candidates.Count); i++)
                {
                    LightFixture fixture = candidates[i];
                    if (bounces.Exists(b => b.Source == fixture)) continue;
                    Bounce free = bounces.Find(b => b.Source == null);
                    if (free == null) break;
                    free.Source = fixture;
                    free.RefreshAt = -1f;
                    free.Valid = false;
                }
            }
            // One receiver per frame, including its wall clearance. Sampling updates when floors disappear.
            if (bounces.Count > 0)
            {
                Bounce sample = bounces[nextSample++ % bounces.Count];
                if (sample.Source != null && Time.unscaledTime >= sample.RefreshAt) Sample(sample);
            }
            foreach (Bounce bounce in bounces) Apply(bounce, limit > 0);
        }

        private static float Score(LightFixture fixture, Camera eye)
        {
            Vector3 delta = fixture.Lamp.transform.position - eye.transform.position;
            return delta.sqrMagnitude * (Vector3.Dot(delta, eye.transform.forward) < 0 ? 1.4f : 1f);
        }

        private void Sample(Bounce bounce)
        {
            bounce.RefreshAt = Time.unscaledTime + config.RefreshSeconds;
            bounce.Valid = false;
            Light source = bounce.Source.Lamp;
            if (source.type != LightType.Spot || source.transform.forward.y > -.5f) return;
            if (!Physics.Raycast(source.transform.position, source.transform.forward, out RaycastHit hit,
                    source.range * .9f, mask, QueryTriggerInteraction.Ignore) || hit.normal.y < .65f || hit.distance < 1f) return;
            Vector3 point = hit.point + hit.normal * .15f;
            float radius = config.Radius;
            // Never send an unshadowed bounce through a nearby wall. Eight rays bound its influence sphere.
            for (int i = 0; i < 8; i++)
            {
                Vector3 dir = Quaternion.Euler(0, i * 45f, 0) * Vector3.forward;
                if (Physics.Raycast(point, dir, out RaycastHit wall, radius, mask, QueryTriggerInteraction.Ignore))
                    radius = Mathf.Min(radius, Mathf.Max(0f, wall.distance - .2f));
            }
            if (radius < .75f) return;
            bounce.Hit = hit.point;
            bounce.Light.transform.SetPositionAndRotation(point, Quaternion.LookRotation(hit.normal));
            bounce.Light.range = radius;
            Renderer surface = hit.collider.GetComponent<Renderer>();
            Material material = surface != null ? surface.sharedMaterial : null;
            Color tint = material != null && material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : Color.gray;
            // Keep scanned albedo from tinting a room as strongly as a saturated painted light.
            bounce.Tint = Color.Lerp(Color.white, tint, .35f);
            bounce.Valid = true;
        }

        private void Apply(Bounce bounce, bool allowed)
        {
            Light source = bounce.Source != null ? bounce.Source.Lamp : null;
            bool on = allowed && bounce.Valid && source != null && source.isActiveAndEnabled && bounce.Source.Lit;
            bounce.Light.enabled = on;
            if (!on) return;
            bounce.Light.color = source.color * bounce.Tint;
            bounce.Light.intensity = LightingMath.Contribution(source, bounce.Hit) * config.Reflectance
                * bounce.Light.range * bounce.Light.range * .65f;
            bounce.Light.GetUniversalAdditionalLightData().renderingLayers = source.GetUniversalAdditionalLightData().renderingLayers;
        }

        private void OnDisable()
        {
            foreach (Bounce bounce in bounces) if (bounce.Light != null) bounce.Light.enabled = false;
        }

#if UNITY_EDITOR
        public void EditorSetup(IndirectLightingConfig settings) => config = settings;

        public void Preview(Camera eye)
        {
            Initialize();
            nextChoose = -1f;
            Tick(eye);
            foreach (Bounce bounce in bounces)
            {
                if (bounce.Source != null) Sample(bounce);
                Apply(bounce, true);
            }
        }
#endif

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            foreach (Bounce bounce in bounces)
            {
                if (!bounce.Valid || bounce.Source == null) continue;
                Gizmos.color = bounce.Light.color;
                Gizmos.DrawLine(bounce.Source.Lamp.transform.position, bounce.Hit);
                Gizmos.DrawWireSphere(bounce.Light.transform.position, bounce.Light.range);
            }
        }

        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            int lit = 0;
            foreach (Bounce bounce in bounces) if (bounce.Light != null && bounce.Light.enabled) lit++;
            GUI.Label(new Rect(Screen.width - 620, 94, 610, 22), $"diffuse bounce {lit}/{bounces.Count} (local receiver approximation)");
        }
    }
}

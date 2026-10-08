using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Power and collapse control both the lamp and every emissive diffuser on its model.</summary>
    public class LightFixture : MonoBehaviour
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly List<LightFixture> all = new();
        private static float disturbedUntil;

        [SerializeField] private Light lamp;
        [SerializeField] private Renderer panel;
        [SerializeField, ColorUsage(false, true)] private Color glow = new(2.2f, 2.1f, 1.9f);
        [SerializeField] private bool faulty;
        [SerializeField] private bool dead;
        [SerializeField] private int seed;

        private readonly List<GlowSurface> surfaces = new();
        private MaterialPropertyBlock block;
        private bool surfacesReady;
        private float baseIntensity = -1f;
        private bool powered = true, attached = true, horrorPower = true, alarm;
        private Color originalColor;
        private float level = -1f;

        private readonly struct GlowSurface
        {
            public readonly Renderer Renderer;
            public readonly int Index;
            public readonly Color Color;
            public GlowSurface(Renderer renderer, int index, Color color)
            { Renderer = renderer; Index = index; Color = color; }
        }

        public static IReadOnlyList<LightFixture> All => all;
        public bool Lit => attached && (alarm || (powered && horrorPower && !dead));
        public bool Faulty => faulty;
        public bool Dead => dead;
        public Light Lamp => lamp;
        public float FullIntensity => baseIntensity >= 0f ? baseIntensity : lamp != null ? lamp.intensity : 0f;
        public static bool IsDisturbed => Time.time < disturbedUntil;
        public static void Disturb(float seconds) => disturbedUntil = Mathf.Max(disturbedUntil, Time.time + seconds);

        private void Awake()
        {
            if (lamp != null) originalColor = lamp.color;
            Apply(Lit ? 1f : 0f);
        }

        private void OnEnable() { if (!all.Contains(this)) all.Add(this); }
        private void OnDisable() => all.Remove(this);

        public void SetHorrorPower(bool on)
        {
            if (horrorPower == on) return;
            horrorPower = on;
            Apply(Lit ? 1f : 0f);
        }

        public void SetAlarm(bool on)
        {
            if (alarm == on) return;
            alarm = on;
            if (lamp != null) lamp.color = on ? new Color(1f, 0.025f, 0.01f) : originalColor;
            level = -1f;
            Apply(Lit ? 1f : 0f);
        }

        public void SetPowered(bool on) { powered = on; Apply(Lit ? 1f : 0f); }
        public void SetAttached(bool on) { attached = on; Apply(Lit ? 1f : 0f); }

        private void Update()
        {
            if (!Lit) return;
            bool disturbed = IsDisturbed;
            if (Core.GameSettings.ReduceFlashing)
            {
                // QA O-01: no strobing. A failing or disturbed light sags and recovers slowly instead.
                float target = disturbed ? 0.5f : faulty ? 0.75f : 1f;
                if (!Mathf.Approximately(level, target)) Apply(Mathf.MoveTowards(Mathf.Max(0f, level), target, Time.deltaTime * 1.5f));
                return;
            }
            if (!faulty && !disturbed) { Apply(1f); return; }

            float clock = Time.time + seed * 0.137f;
            float period = 7f + seed % 9;
            float phase = Mathf.Repeat(clock, period);
            // Failing ballasts sputter in bursts, with a short complete blackout between strikes.
            if (!disturbed && phase > 1.35f) { Apply(1f); return; }
            if (!disturbed && phase > 0.72f && phase < 1.02f) { Apply(0f); return; }
            float n = Mathf.PerlinNoise(clock * (disturbed ? 22f : 17f), seed * 0.37f);
            Apply(n < 0.37f ? 0f : n < 0.52f ? 0.3f : 1f);
        }

        private void GatherSurfaces()
        {
            if (surfacesReady) return;
            surfacesReady = true;
            var renderers = new List<Renderer>(GetComponentsInChildren<Renderer>(true));
            if (panel != null && !renderers.Contains(panel)) renderers.Add(panel);
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    if (material == null || !material.HasProperty(EmissionColor)) continue;
                    Color emission = material.GetColor(EmissionColor);
                    if (!material.IsKeywordEnabled("_EMISSION") || emission.maxColorComponent <= 0.001f) continue;
                    surfaces.Add(new GlowSurface(renderer, i, renderer == panel && materials.Length == 1 ? glow : emission));
                }
            }
        }

        private void Apply(float brightness)
        {
            if (Mathf.Approximately(level, brightness)) return;
            level = brightness;
            if (lamp != null)
            {
                if (baseIntensity < 0f) baseIntensity = lamp.intensity;
                lamp.enabled = brightness > 0f;
                lamp.intensity = baseIntensity * brightness;
            }
            GatherSurfaces();
            block ??= new MaterialPropertyBlock();
            foreach (GlowSurface surface in surfaces)
            {
                if (surface.Renderer == null) continue;
                block.Clear();
                surface.Renderer.GetPropertyBlock(block, surface.Index);
                block.SetColor(EmissionColor, (alarm ? new Color(3f, 0.03f, 0.01f) : surface.Color) * brightness);
                surface.Renderer.SetPropertyBlock(block, surface.Index);
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(Light light, Renderer glowPanel, bool isFaulty, bool isDead, int fixtureSeed)
        {
            lamp = light; panel = glowPanel; faulty = isFaulty; dead = isDead; seed = fixtureSeed;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { all.Clear(); disturbedUntil = 0f; }
    }
}

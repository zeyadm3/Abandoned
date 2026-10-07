using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// A ceiling light: a glowing panel and its light. It's lit only while the building has power
    /// (PowerController) and its ceiling is still up (SectionProp). Faulty fixtures flicker on their
    /// own; every fixture stutters together while the building is disturbed (danger rising).
    /// Cosmetic and local: every machine runs its own copy from replicated power/collapse state.
    /// </summary>
    public class LightFixture : MonoBehaviour
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly List<LightFixture> all = new();
        private static float disturbedUntil;

        [SerializeField] private Light lamp;
        [SerializeField] private Renderer panel;
        [Tooltip("Panel glow when lit (HDR, so bloom picks it up).")]
        [SerializeField, ColorUsage(false, true)] private Color glow = new(2.2f, 2.1f, 1.9f);
        [Tooltip("Flickers on its own: a failing tube.")]
        [SerializeField] private bool faulty;
        [Tooltip("Never lights: a dead tube (the panel stays dark).")]
        [SerializeField] private bool dead;
        [SerializeField] private int seed;

        private MaterialPropertyBlock block;
        private float baseIntensity = -1f;
        private bool powered = true, attached = true;
        private float level = -1f;

        public static IReadOnlyList<LightFixture> All => all;
        public bool Lit => powered && attached && !dead;
        public bool Faulty => faulty;
        public bool Dead => dead;
        public Light Lamp => lamp;
        /// <summary>The light's intensity when steadily lit.</summary>
        public float FullIntensity => baseIntensity >= 0f ? baseIntensity : lamp != null ? lamp.intensity : 0f;

        public static bool IsDisturbed => Time.time < disturbedUntil;

        /// <summary>Every fixture stutters for a while (the building groans: danger went up).</summary>
        public static void Disturb(float seconds) => disturbedUntil = Mathf.Max(disturbedUntil, Time.time + seconds);

        private void Awake() => Apply(Lit ? 1f : 0f);

        private void OnEnable() => all.Add(this);

        private void OnDisable() => all.Remove(this);

        public void SetPowered(bool on)
        {
            powered = on;
            Apply(Lit ? 1f : 0f);
        }

        public void SetAttached(bool on)
        {
            attached = on;
            Apply(Lit ? 1f : 0f);
        }

        private void Update()
        {
            if (!Lit) return;
            bool disturbed = Time.time < disturbedUntil;
            if (!faulty && !disturbed)
            {
                if (level != 1f) Apply(1f);
                return;
            }
            // Coarse steps read as a failing tube; smooth noise reads as a dimmer.
            float t = Time.time * (disturbed ? 14f : 6f) + seed * 7.31f;
            float n = Mathf.PerlinNoise(t, seed * 0.37f);
            float step = n < 0.32f ? 0.05f : n < 0.45f ? 0.55f : 1f;
            if (faulty && !disturbed && Mathf.PerlinNoise(Time.time * 0.2f, seed) > 0.45f) step = 1f; // long steady spells
            Apply(step);
        }

        private void Apply(float l)
        {
            if (Mathf.Approximately(level, l)) return;
            level = l;
            if (lamp != null)
            {
                // Read lazily: the SectionProp beside it may switch it before this Awake runs.
                if (baseIntensity < 0f) baseIntensity = lamp.intensity;
                lamp.enabled = l > 0f;
                lamp.intensity = baseIntensity * l;
            }
            if (panel == null) return;
            block ??= new MaterialPropertyBlock();
            panel.GetPropertyBlock(block);
            block.SetColor(EmissionColor, glow * l);
            panel.SetPropertyBlock(block);
        }

#if UNITY_EDITOR
        public void EditorSetup(Light light, Renderer glowPanel, bool isFaulty, bool isDead, int fixtureSeed)
        {
            lamp = light;
            panel = glowPanel;
            faulty = isFaulty;
            dead = isDead;
            seed = fixtureSeed;
        }
#endif

        // Statics survive Play mode when domain reload is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            all.Clear();
            disturbedUntil = 0f;
        }
    }
}

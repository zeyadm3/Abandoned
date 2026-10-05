using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Synthesised placeholder sounds (no audio assets yet): a distinct impact per material so
    /// glass rings, metal clangs and wood thuds are already recognisable. Replaced by library
    /// sounds in the audio pass.
    /// </summary>
    public static class PlaceholderAudio
    {
        private const int SampleRate = 44100;

        private static readonly Dictionary<SurfaceMaterial, AudioClip> ImpactClips = new();
        private static readonly Dictionary<SurfaceMaterial, AudioClip> FootstepClips = new();

        /// <summary>Last material played and total plays; used by tests and the debug overlay.</summary>
        public static SurfaceMaterial? LastImpactMaterial { get; private set; }
        public static int ImpactCount { get; private set; }

        public static void PlayImpact(SurfaceMaterial material, Vector3 position, float volume01)
        {
            LastImpactMaterial = material;
            ImpactCount++;
            AudioSource.PlayClipAtPoint(GetImpactClip(material), position, Mathf.Clamp01(volume01));
        }

        public static SurfaceMaterial? LastFootstepMaterial { get; private set; }

        public static void PlayFootstep(SurfaceMaterial material, Vector3 position, float volume01)
        {
            LastFootstepMaterial = material;
            AudioSource.PlayClipAtPoint(GetFootstepClip(material), position, Mathf.Clamp01(volume01));
        }

        public static AudioClip GetFootstepClip(SurfaceMaterial material)
        {
            if (FootstepClips.TryGetValue(material, out AudioClip clip) && clip != null) return clip;
            clip = material switch
            {
                SurfaceMaterial.Wood => Thud("Step_Wood", 0.12f, 160f, 0.45f, 45f),
                SurfaceMaterial.Metal => Tones("Step_Metal", 0.2f, 22f, 0.3f, 880f, 1460f),
                SurfaceMaterial.Dirt or SurfaceMaterial.Fabric => Thud("Step_Soft", 0.1f, 0f, 0.35f, 55f),
                SurfaceMaterial.Glass => Tones("Step_Glass", 0.15f, 30f, 0.4f, 3100f, 4400f),
                _ => Thud("Step_Hard", 0.08f, 0f, 0.6f, 70f),
            };
            FootstepClips[material] = clip;
            return clip;
        }

        public static AudioClip GetImpactClip(SurfaceMaterial material)
        {
            if (ImpactClips.TryGetValue(material, out AudioClip clip) && clip != null) return clip;
            clip = material switch
            {
                SurfaceMaterial.Glass => Tones("Impact_Glass", 0.7f, 9f, 0.05f, 2400f, 3710f, 5230f),
                SurfaceMaterial.Metal => Tones("Impact_Metal", 0.9f, 5f, 0.15f, 420f, 1130f, 1870f, 2650f),
                SurfaceMaterial.Wood => Thud("Impact_Wood", 0.18f, 180f, 0.6f, 25f),
                SurfaceMaterial.Stone or SurfaceMaterial.Concrete => Thud("Impact_Stone", 0.25f, 90f, 0.8f, 18f),
                SurfaceMaterial.Paper or SurfaceMaterial.Fabric => Thud("Impact_Soft", 0.1f, 0f, 0.4f, 40f),
                _ => Thud("Impact_Plastic", 0.12f, 600f, 0.5f, 35f),
            };
            ImpactClips[material] = clip;
            return clip;
        }

        /// <summary>Decaying inharmonic partials: bells, glass, metal.</summary>
        private static AudioClip Tones(string name, float seconds, float decay, float noise, params float[] partials)
        {
            var random = new System.Random(name.GetHashCode());
            return Build(name, seconds, t =>
            {
                float s = 0f;
                for (int i = 0; i < partials.Length; i++)
                    s += Mathf.Sin(2f * Mathf.PI * partials[i] * t) * Mathf.Exp(-t * decay * (1f + i * 0.4f)) / (i + 1);
                s += ((float)random.NextDouble() * 2f - 1f) * noise * Mathf.Exp(-t * 60f);
                return s * 0.6f;
            });
        }

        /// <summary>Short noise burst plus an optional low tone: thuds and knocks.</summary>
        private static AudioClip Thud(string name, float seconds, float tone, float noise, float decay)
        {
            var random = new System.Random(name.GetHashCode());
            float smoothed = 0f;
            return Build(name, seconds, t =>
            {
                // One-pole low-pass on the noise so it reads as a thump rather than hiss.
                smoothed += (((float)random.NextDouble() * 2f - 1f) - smoothed) * 0.15f;
                float env = Mathf.Exp(-t * decay);
                return (smoothed * noise * 2f + Mathf.Sin(2f * Mathf.PI * tone * t) * 0.6f) * env * 0.7f;
            });
        }

        private static AudioClip Build(string name, float seconds, System.Func<float, float> sample)
        {
            int count = Mathf.CeilToInt(seconds * SampleRate);
            var data = new float[count];
            for (int i = 0; i < count; i++) data[i] = Mathf.Clamp(sample(i / (float)SampleRate), -1f, 1f);
            AudioClip clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ImpactClips.Clear();
            FootstepClips.Clear();
            LastImpactMaterial = null;
            LastFootstepMaterial = null;
            ImpactCount = 0;
        }
    }
}

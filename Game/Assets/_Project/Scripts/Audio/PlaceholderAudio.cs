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
        private static readonly Dictionary<StructureSound, AudioClip> StructureClips = new();

        public static StructureSound? LastStructureSound { get; private set; }
        public static int StructureSoundCount { get; private set; }

        public static void PlayStructure(StructureSound sound, Vector3 position, float volume01)
        {
            LastStructureSound = sound;
            StructureSoundCount++;
            AudioSource.PlayClipAtPoint(GetStructureClip(sound), position, Mathf.Clamp01(volume01));
        }

        public static AudioClip GetStructureClip(StructureSound sound)
        {
            if (StructureClips.TryGetValue(sound, out AudioClip clip) && clip != null) return clip;
            clip = sound switch
            {
                StructureSound.Creak => Creak("Structure_Creak", 0.9f, 140f, 95f),
                StructureSound.Groan => Creak("Structure_Groan", 1.6f, 70f, 45f),
                StructureSound.Snap => Thud("Structure_Snap", 0.25f, 0f, 1f, 30f),
                _ => Thud("Structure_Crash", 2.5f, 45f, 1f, 1.6f),
            };
            StructureClips[sound] = clip;
            return clip;
        }

        private static AudioClip radioStatic;

        /// <summary>A burst of radio static (danger rising: every handset hisses).</summary>
        public static AudioClip Static()
        {
            if (radioStatic != null) return radioStatic;
            var random = new System.Random(11);
            radioStatic = Build("Radio_Static", 1.2f, t =>
            {
                float env = Mathf.Clamp01(t / 0.05f) * Mathf.Clamp01((1.2f - t) / 0.3f);
                float crackle = random.NextDouble() < 0.02 ? 1f : 0.35f;
                return ((float)random.NextDouble() * 2f - 1f) * crackle * env * 0.35f;
            });
            return radioStatic;
        }

        private static AudioClip click;

        /// <summary>The Blind One's echolocation click: a short dry knock with a hollow ring.</summary>
        public static AudioClip Click()
        {
            if (click != null) return click;
            var random = new System.Random(7);
            click = Build("BlindOne_Click", 0.09f, t =>
            {
                float env = Mathf.Exp(-t * 70f);
                float knock = ((float)random.NextDouble() * 2f - 1f) * 0.6f + Mathf.Sin(2f * Mathf.PI * 1800f * t) * 0.5f;
                return knock * env * 0.8f;
            });
            return click;
        }

        private static AudioClip horn;

        /// <summary>A truck's two-note horn (one blast; the truck repeats it while it waits).</summary>
        public static AudioClip Horn()
        {
            if (horn != null) return horn;
            horn = Build("Truck_Horn", 0.7f, t =>
            {
                float env = Mathf.Clamp01(t / 0.03f) * Mathf.Clamp01((0.7f - t) / 0.08f);
                float a = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 330f * t)), b = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 415f * t));
                return (a + b) * 0.18f * env;
            });
            return horn;
        }

        /// <summary>A sliding, wobbling low tone with grit: timber under strain.</summary>
        private static AudioClip Creak(string name, float seconds, float startHz, float endHz)
        {
            var random = new System.Random(name.GetHashCode());
            float phase = 0f;
            return Build(name, seconds, t =>
            {
                float k = t / seconds;
                float hz = Mathf.Lerp(startHz, endHz, k) * (1f + 0.04f * Mathf.Sin(2f * Mathf.PI * 7f * t));
                phase += hz / SampleRate;
                float saw = (phase % 1f) * 2f - 1f;
                float grit = ((float)random.NextDouble() * 2f - 1f) * 0.25f;
                float env = Mathf.Sin(Mathf.PI * k);
                return (saw * 0.5f + grit) * env * 0.6f;
            });
        }

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
            StructureClips.Clear();
            LastStructureSound = null;
            StructureSoundCount = 0;
            LastImpactMaterial = null;
            LastFootstepMaterial = null;
            ImpactCount = 0;
        }
    }
}

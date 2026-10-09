using Abandoned.Core;

namespace Abandoned.Audio
{
    /// <summary>
    /// Impacts and footsteps by surface material, synthesised. The SoundBank's recordings normally play
    /// instead; these cover a material the bank has no clips for (water always: no pack has a splash).
    /// </summary>
    internal static class SurfacePatches
    {
        public static SynthSamples Impact(SurfaceMaterial material, bool heavy, int take)
        {
            int seed = 900 + (int)material * 7 + take;
            float[] d;
            switch (material)
            {
                case SurfaceMaterial.Glass:
                    d = Synth.Buffer(0.7f);
                    Patch.Layer(d, Synth.Modal(0.6f, 2400f + 300f * take, Patch.Glass, null, heavy ? 7f : 11f), 0.8f);
                    Patch.Layer(d, Synth.Noise(0.01f, seed, 9000f, 4000f, 0.0003f, 300f), 0.5f);
                    if (heavy) Patch.Pings(d, seed, 6, 0.02f, 0.3f, 2600f, 6000f, Patch.Glass, 40f, 0.4f);
                    break;
                case SurfaceMaterial.Metal:
                    d = Synth.Buffer(0.9f);
                    Patch.Layer(d, Synth.Modal(0.9f, (heavy ? 400f : 640f) + 40f * take, Patch.Sheet, null, heavy ? 5f : 8f), 0.8f);
                    Patch.Layer(d, Synth.Thump(0.1f, 220f, 110f, 30f), heavy ? 0.6f : 0.3f);
                    break;
                case SurfaceMaterial.Wood:
                    d = Synth.Buffer(0.3f);
                    Patch.Layer(d, Synth.Thump(0.16f, heavy ? 160f : 190f, 85f, 26f), 0.9f);
                    Patch.Layer(d, Synth.Modal(0.2f, 260f + 30f * take, Patch.Wood, null, 30f), 0.5f);
                    break;
                case SurfaceMaterial.Water:
                    d = Synth.Buffer(0.7f);
                    Patch.Layer(d, Synth.Noise(heavy ? 0.6f : 0.3f, seed, 3500f, 700f, 0.004f, heavy ? 7f : 13f), 0.9f);
                    Patch.Layer(d, Synth.Sweep(0.25f, 380f, 240f, 0.01f, 9f), 0.2f);
                    break;
                case SurfaceMaterial.Paper:
                case SurfaceMaterial.Fabric:
                case SurfaceMaterial.Carpet:
                case SurfaceMaterial.Dirt:
                    d = Synth.Buffer(0.2f);
                    Patch.Layer(d, Synth.Noise(0.12f, seed, 1500f, 400f, 0.003f, 36f), 0.8f);
                    Patch.Layer(d, Synth.Thump(0.1f, 110f, 60f, 34f), heavy ? 0.7f : 0.4f);
                    break;
                case SurfaceMaterial.Plastic:
                    d = Synth.Buffer(0.15f);
                    Patch.Layer(d, Synth.Thump(0.1f, 420f, 250f, 36f), 0.8f);
                    Patch.Layer(d, Synth.Noise(0.05f, seed, 5000f, 1500f, 0.001f, 60f), 0.5f);
                    break;
                default:
                    d = Synth.Buffer(0.25f);
                    Patch.Layer(d, Synth.Thump(0.2f, 120f, 62f, 20f), heavy ? 0.95f : 0.7f);
                    Patch.Layer(d, Synth.Noise(0.12f, seed, 3200f, 700f, 0.001f, 30f), 0.7f);
                    break;
            }
            return new SynthSamples($"Synth_Impact_{material}", Synth.Normalize(d, 0.9f));
        }

        public static SynthSamples Footstep(SurfaceMaterial material, int take)
        {
            int seed = 950 + (int)material * 7 + take;
            float[] d;
            switch (material)
            {
                case SurfaceMaterial.Water:
                    d = Synth.Buffer(0.3f);
                    Patch.Layer(d, Synth.Noise(0.28f, seed, 3000f, 800f, 0.005f, 14f), 0.8f);
                    Patch.Layer(d, Synth.Sweep(0.2f, 410f, 300f, 0.01f, 10f), 0.15f);
                    break;
                case SurfaceMaterial.Metal:
                    d = Synth.Buffer(0.2f);
                    Patch.Layer(d, Synth.Modal(0.18f, 880f + 60f * take, Patch.Sheet, null, 24f), 0.5f);
                    Patch.Layer(d, Synth.Thump(0.07f, 160f, 90f, 40f), 0.7f);
                    break;
                case SurfaceMaterial.Glass:
                    d = Synth.Buffer(0.2f);
                    Patch.Pings(d, seed, 5, 0f, 0.08f, 3000f, 5200f, Patch.Glass, 60f, 0.5f);
                    Patch.Layer(d, Synth.Noise(0.08f, seed, 7000f, 2500f, 0.001f, 40f), 0.5f);
                    break;
                case SurfaceMaterial.Wood:
                    d = Synth.Buffer(0.14f);
                    Patch.Layer(d, Synth.Thump(0.1f, 170f, 95f, 34f), 0.8f);
                    Patch.Layer(d, Synth.Modal(0.1f, 300f + 20f * take, Patch.Wood, null, 45f), 0.3f);
                    break;
                case SurfaceMaterial.Dirt:
                case SurfaceMaterial.Fabric:
                case SurfaceMaterial.Carpet:
                case SurfaceMaterial.Paper:
                    d = Synth.Noise(0.1f, seed, 1100f, 400f, 0.004f, 42f);
                    break;
                default:
                    d = Synth.Buffer(0.1f);
                    Patch.Layer(d, Synth.Thump(0.07f, 150f, 80f, 45f), 0.6f);
                    Patch.Layer(d, Synth.Noise(0.06f, seed, 3800f, 1200f, 0.001f, 60f), 0.7f);
                    break;
            }
            return new SynthSamples($"Synth_Step_{material}", Synth.Normalize(d, 0.8f));
        }
    }
}

using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The building's own sounds (0.14.0): a steel and concrete mall under load, not a timber house. Creak,
    /// groan and snap are layers under the recorded creaks; the collapse, the rubble, the power and the
    /// far-off settling are wholly made here.
    /// </summary>
    internal static class StructurePatches
    {
        /// <summary>
        /// Stick-slip strain: a low buzzing tone that catches and releases in uneven bursts, heard through
        /// the resonance of whatever is bending. The base of every "the structure is working" sound.
        /// </summary>
        public static float[] Strain(float seconds, int seed, float hzFrom, float hzTo, float bandFrom, float bandTo,
            float grit, bool rising = false)
        {
            float[] d = Synth.Buffer(seconds);
            var rng = new Rng(seed);
            var body = new Svf();
            float phase = 0f, wobble = 0f, slip = 0f, slipTarget = 1f;
            int nextSlip = 0;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate, u = i / (float)d.Length;
                if (i >= nextSlip)
                {
                    slipTarget = rng.Range(0.25f, 1f);
                    nextSlip = i + (int)(rng.Range(0.012f, 0.07f) * Synth.Rate);
                }
                slip += (slipTarget - slip) * 0.004f;
                wobble += (rng.Bi() - wobble) * 0.0008f;
                phase += Mathf.Lerp(hzFrom, hzTo, u) * (1f + wobble * 6f) / Synth.Rate;
                if (phase >= 1f) phase -= 1f;
                // A saw that buzzes harder as it slips: friction, not a note.
                float saw = (2f * phase - 1f) * slip + rng.Bi() * grit * slip;
                if ((i & 15) == 0) body.Set(Mathf.Lerp(bandFrom, bandTo, u) * (1f + 0.2f * Mathf.Sin(t * 9f)), 5f, Synth.Rate);
                body.Tick(saw);
                float envelope = rising ? Mathf.Pow(u, 1.5f) * Mathf.Clamp01((seconds - t) / 0.08f) : Synth.Swell(t, seconds);
                d[i] = (body.Band * 0.8f + body.Low * 0.35f) * envelope;
            }
            return d;
        }

        public static float[] Creak(int take)
        {
            float seconds = 1f + 0.2f * take;
            float[] d = Synth.Buffer(seconds);
            Patch.Layer(d, Strain(seconds, 11 + take, 74f - 6f * take, 55f, 430f, 280f, 0.3f), 0.8f);
            // Fixings ticking as they shift.
            Patch.Pings(d, 21 + take, 3, 0.1f, seconds - 0.4f, 900f, 1900f, Patch.Sheet, 40f, 0.22f, 1f);
            return SynthReverb.Apply(d, 0.9f, 0.22f);
        }

        public static float[] Groan(int take)
        {
            float seconds = 2.2f + 0.3f * take;
            float[] d = Synth.Buffer(seconds);
            Patch.Layer(d, Strain(seconds, 31 + take, 44f, 30f, 230f, 140f, 0.25f), 0.8f);
            // Felt as much as heard.
            Patch.Layer(d, Synth.Sweep(seconds, 39f, 31f, 0.4f, 0.6f), 0.5f);
            return SynthReverb.Apply(d, 1.6f, 0.28f);
        }

        public static float[] Snap(int take)
        {
            float[] d = Synth.Buffer(0.6f);
            Patch.Layer(d, Synth.Noise(0.22f, 41 + take, 9000f, 1400f, 0.0005f, 38f), 0.9f);
            // Rebar ringing after it lets go.
            Patch.Layer(d, Synth.Modal(0.5f, 560f + 70f * take, Patch.Sheet, null, 13f), 0.45f, 0.004f);
            Patch.Layer(d, Synth.Thump(0.2f, 190f, 62f, 22f), 0.6f);
            return SynthReverb.Apply(d, 0.7f, 0.28f);
        }

        public static float[] Crash(int take)
        {
            float[] d = Synth.Buffer(3.4f);
            // The slab letting go, tonnes of concrete coming down, and the break itself on top.
            Patch.Layer(d, Synth.Thump(1.3f, 95f, 31f, 3f, 14f), 0.95f);
            Patch.Layer(d, Synth.Noise(3.2f, 51 + take, 2400f, 170f, 0.012f, 1.5f), 0.7f);
            Patch.Layer(d, Synth.Noise(0.5f, 61 + take, 9000f, 900f, 0.001f, 9f), 0.5f);
            Patch.Debris(d, 71 + take, 30, 0.08f, 2.6f, 5200f);
            // Rebar and conduit torn out with it.
            Patch.Pings(d, 81 + take, 5, 0.1f, 1.6f, 210f, 520f, Patch.Sheet, 7f, 0.3f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.6f), 2.1f, 0.33f);
        }

        public static float[] RubbleLand(int take)
        {
            float[] d = Synth.Buffer(1.4f);
            Patch.Layer(d, Synth.Thump(0.6f, 72f, 34f, 6f), 0.9f);
            Patch.Layer(d, Synth.Noise(1f, 91 + take, 2000f, 280f, 0.004f, 5f), 0.6f);
            Patch.Debris(d, 95 + take, 12, 0.05f, 0.9f, 4200f);
            return SynthReverb.Apply(d, 1.2f, 0.3f);
        }

        /// <summary>The whole building going: a long shudder under everything.</summary>
        public static float[] DeepRumble(int take)
        {
            const float seconds = 4f;
            float[] d = Synth.Normalize(Synth.Noise(seconds, 101 + take, 150f, 85f, 0.7f, 0.5f), 0.8f);
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                d[i] *= 0.65f + 0.35f * Mathf.Sin(Synth.Tau * (5.3f + take) * t) * Mathf.Sin(Synth.Tau * 0.7f * t);
            }
            Patch.Layer(d, Synth.Sweep(seconds, 27f, 23f, 0.8f, 0.45f), 0.6f);
            return Synth.Fade(d, 0f, 0.4f);
        }

        public static float[] FixtureFall(int take)
        {
            float[] d = Synth.Buffer(1.5f);
            // The housing hits the floor and the tubes burst.
            Patch.Layer(d, Synth.Modal(1.1f, 230f + 40f * take, Patch.Sheet, null, 6f), 0.7f);
            Patch.Layer(d, Synth.Thump(0.2f, 150f, 70f, 20f), 0.5f);
            Patch.Layer(d, Synth.Noise(0.25f, 111 + take, 9000f, 2500f, 0.001f, 20f, 1500f), 0.5f);
            Patch.Pings(d, 115 + take, 14, 0.02f, 0.5f, 2600f, 6200f, Patch.Glass, 34f, 0.35f);
            return SynthReverb.Apply(d, 1f, 0.25f);
        }

        public static float[] PowerDown()
        {
            float[] d = Synth.Buffer(2.2f);
            // The breaker, then every ballast in the sector winding down, relays dropping out as it goes.
            Patch.Layer(d, Synth.Thump(0.16f, 130f, 52f, 26f), 0.9f);
            Patch.Layer(d, Synth.Noise(0.03f, 121, 8000f, 3000f, 0.0005f, 120f), 0.5f);
            Patch.Layer(d, Synth.LowPass(Synth.Sweep(1.9f, 100f, 34f, 0.01f, 2.4f, 2, 0.7f), 520f), 0.55f, 0.03f);
            Patch.Pings(d, 122, 4, 0.2f, 1.2f, 1800f, 3200f, Patch.Bar, 60f, 0.12f, 1f);
            return SynthReverb.Apply(d, 1.6f, 0.3f);
        }

        public static float[] PowerUp()
        {
            float[] d = Synth.Buffer(1.5f);
            Patch.Layer(d, Synth.Thump(0.16f, 140f, 60f, 26f), 0.8f);
            Patch.Layer(d, Synth.LowPass(Synth.Sweep(0.9f, 38f, 100f, 0.05f, 1.2f, 2), 600f), 0.5f, 0.03f);
            // Starters tinking as the tubes strike.
            Patch.Pings(d, 131, 5, 0.15f, 0.9f, 2400f, 3400f, Patch.Bar, 70f, 0.2f, 1f);
            return SynthReverb.Apply(d, 1f, 0.25f);
        }

        /// <summary>A failing ballast: mains-rate arcing in uneven bursts, and the starter trying again.</summary>
        public static float[] LightBuzz(int take)
        {
            float seconds = 0.38f + 0.1f * take;
            float[] d = Synth.Buffer(seconds);
            var rng = new Rng(141 + take);
            var band = new Svf();
            band.Set(1900f + 300f * take, 2.5f, Synth.Rate);
            float gate = 0f, target = 1f;
            int next = 0;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                if (i >= next)
                {
                    target = rng.Uni() < 0.35f ? 0f : 1f;
                    next = i + (int)(rng.Range(0.012f, 0.06f) * Synth.Rate);
                }
                gate += (target - gate) * 0.02f;
                float buzz = Synth.Wave(3, 100f * t) * 0.6f + rng.Bi() * 0.4f * (0.5f + 0.5f * Synth.Wave(0, 100f * t));
                band.Tick(buzz);
                d[i] = (band.Band + band.High * 0.15f) * gate * Synth.Gate(t, seconds, 0.005f, 0.05f);
            }
            Synth.Normalize(d, 0.7f);
            Patch.Pings(d, 145 + take, 2 + take, 0.02f, seconds - 0.1f, 2600f, 3600f, Patch.Bar, 80f, 0.5f, 1f);
            return d;
        }

        /// <summary>
        /// Something settling a long way off: six different things, never a creak (creaks are warnings).
        /// All of them dulled and washed out by distance.
        /// </summary>
        public static float[] DistantSettle(int take)
        {
            float[] d = Synth.Buffer(2.2f);
            int seed = 151 + take;
            switch (take)
            {
                case 0: // a pipe cooling: ticks further and further apart
                    float[] gaps = { 0f, 0.28f, 0.7f, 1.3f };
                    for (int i = 0; i < gaps.Length; i++)
                        Patch.Layer(d, Synth.Modal(0.4f, 1250f, Patch.Bar, null, 22f), 0.8f - 0.17f * i, gaps[i]);
                    break;
                case 1: // something heavy shifting
                    Patch.Layer(d, Synth.Thump(0.5f, 68f, 34f, 7f), 0.9f);
                    Patch.Layer(d, Synth.Noise(0.6f, seed, 500f, 150f, 0.01f, 6f), 0.5f);
                    break;
                case 2: // water finding its way down
                    Patch.Layer(d, Synth.Sweep(0.09f, 950f, 2100f, 0.002f, 45f), 0.8f);
                    Patch.Layer(d, Synth.Sweep(0.09f, 1100f, 2300f, 0.002f, 45f), 0.5f, 0.37f);
                    break;
                case 3: // glass giving up
                    Patch.Pings(d, seed, 5, 0f, 0.5f, 3000f, 5200f, Patch.Glass, 22f, 0.7f);
                    break;
                case 4: // something rolling until it stops against a wall
                    float[] roll = Synth.Resonant(1.4f, seed, 420f, 300f, 6f, 0.2f, 1.6f);
                    for (int i = 0; i < roll.Length; i++) roll[i] *= 0.6f + 0.4f * Mathf.Sin(Synth.Tau * 7f * i / Synth.Rate);
                    Patch.Layer(d, roll, 0.7f);
                    Patch.Layer(d, Synth.Modal(0.3f, 900f, Patch.Sheet, null, 20f), 0.4f, 1.35f);
                    break;
                default: // a door somewhere
                    Patch.Layer(d, Synth.Thump(0.4f, 100f, 45f, 8f), 0.9f);
                    Patch.Layer(d, Synth.Modal(0.8f, 120f, Patch.Sheet, null, 6f), 0.35f);
                    break;
            }
            return SynthReverb.Apply(Synth.LowPass(d, 2600f), 1.9f, 0.45f);
        }
    }
}

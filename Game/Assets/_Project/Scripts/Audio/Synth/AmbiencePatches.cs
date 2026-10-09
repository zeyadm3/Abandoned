using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The beds under everything (0.14.0): wind, mains hum, the dead air of a big building, ducts, drips,
    /// thunder. Loops are rendered a little too long and folded into themselves, and everything that
    /// repeats inside one (gusts, mains cycles) runs a whole number of times over its length.
    /// </summary>
    internal static class AmbiencePatches
    {
        private const int Rate = Synth.LoopRate;

        /// <summary>Wind, in stereo: the two sides share the gusts but not the air, so it surrounds without moving.</summary>
        public static SynthSamples Wind()
        {
            const float seconds = 14f, fold = 1.5f;
            float[] left = WindChannel(seconds, fold, 801), right = WindChannel(seconds, fold, 802);
            Synth.NormalizePair(left, right, 0.8f);
            return new SynthSamples("Synth_WindLoop", Synth.Interleave(left, right), 2, Rate);
        }

        private static float[] WindChannel(float seconds, float fold, int seed)
        {
            float[] d = Synth.Buffer(seconds + fold, Rate);
            var rng = new Rng(seed);
            var body = new Svf();
            var whistle = new Svf();
            float brown = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float turn = Synth.Tau * (i / (float)Rate) / seconds;
                float gust = Mathf.Clamp01(0.55f + 0.3f * Mathf.Sin(turn * 3f) * Mathf.Sin(turn * 2f + 1f) + 0.15f * Mathf.Sin(turn * 7f + 2f));
                brown = Mathf.Clamp(brown + rng.Bi() * 0.03f, -1f, 1f) * 0.997f;
                if ((i & 31) == 0)
                {
                    body.Set(180f + 420f * gust, 0.9f, Rate);
                    whistle.Set(640f + 380f * gust, 14f, Rate);
                }
                body.Tick(brown * 4f);
                whistle.Tick(rng.Bi());
                // The whistle only shows in the hardest gusts: wind finding a gap in a window frame.
                d[i] = body.Low * (0.35f + 0.65f * gust) + body.Band * 0.5f * gust + whistle.Band * 0.06f * gust * gust * gust;
            }
            return Synth.Loop(d, fold, Rate);
        }

        /// <summary>Fluorescent mains hum: 50 Hz and its harmonics, ballasts buzzing twice a cycle. 200 cycles to the loop.</summary>
        public static float[] Hum()
        {
            const float seconds = 4f, fold = 0.25f;
            float[] d = Synth.Buffer(seconds + fold, Rate);
            var rng = new Rng(811);
            var buzz = new Svf();
            buzz.Set(3800f, 1.2f, Rate);
            for (int i = 0; i < d.Length; i++)
            {
                float w = Synth.Tau * 50f * (i / (float)Rate);
                float tone = Mathf.Sin(w) * 0.5f + Mathf.Sin(2f * w) * 0.4f + Mathf.Sin(3f * w) * 0.14f + Mathf.Sin(6f * w) * 0.07f + Mathf.Sin(12f * w) * 0.03f;
                buzz.Tick(rng.Bi());
                d[i] = tone + buzz.Band * 0.05f * (0.5f + 0.5f * Mathf.Sin(2f * w));
            }
            return Synth.Loop(d, fold, Rate);
        }

        /// <summary>A big dead building's air: a rumble too low to place and the faintest hiss, slowly breathing.</summary>
        public static SynthSamples RoomTone()
        {
            const float seconds = 10f, fold = 1f;
            float[] left = RoomChannel(seconds, fold, 821), right = RoomChannel(seconds, fold, 822);
            Synth.NormalizePair(left, right, 0.6f);
            return new SynthSamples("Synth_RoomToneLoop", Synth.Interleave(left, right), 2, Rate);
        }

        private static float[] RoomChannel(float seconds, float fold, int seed)
        {
            float[] d = Synth.Buffer(seconds + fold, Rate);
            var rng = new Rng(seed);
            float low = 0f, low2 = 0f, air = 0f;
            float lowA = Synth.OnePole(70f, Rate), airA = Synth.OnePole(2600f, Rate);
            for (int i = 0; i < d.Length; i++)
            {
                float n = rng.Bi();
                low += (n - low) * lowA;
                low2 += (low - low2) * lowA;
                air += (n - air) * airA;
                float swell = 0.8f + 0.2f * Mathf.Sin(Synth.Tau * (i / (float)Rate) * 2f / seconds + seed);
                d[i] = (low2 * 14f + (n - air) * 0.015f) * swell;
            }
            return Synth.Loop(d, fold, Rate);
        }

        /// <summary>A ventilation shaft still moving air: duct noise and two tones a hair apart. 558 and 1119 cycles to the loop.</summary>
        public static float[] VentDrone()
        {
            const float seconds = 6f, fold = 0.5f;
            float[] d = Synth.Buffer(seconds + fold, Rate);
            var rng = new Rng(831);
            var duct = new Svf();
            duct.Set(330f, 1.1f, Rate);
            for (int i = 0; i < d.Length; i++)
            {
                float turn = Synth.Tau * (i / (float)Rate);
                duct.Tick(rng.Bi());
                float tone = Mathf.Sin(turn * 93f) * 0.3f + Mathf.Sin(turn * 186.5f) * 0.14f;
                d[i] = (duct.Band * 0.5f + duct.Low * 0.25f + tone) * (0.85f + 0.15f * Mathf.Sin(turn * 5f / seconds));
            }
            return Synth.Loop(d, fold, Rate);
        }

        /// <summary>Drops into standing water, in ones and quick pairs, ringing round a flooded room.</summary>
        public static float[] WaterDrip()
        {
            const float seconds = 8f;
            float[] d = Synth.Buffer(seconds, Rate);
            var rng = new Rng(841);
            float at = 0.3f;
            int n = 0;
            while (at < seconds - 0.5f)
            {
                float hz = rng.Range(820f, 1300f);
                // The bubble's pitch climbs as it closes.
                float[] plink = Synth.Normalize(Synth.Sweep(0.09f, hz, hz * 2.2f, 0.002f, 46f, 0, 1f, Rate), rng.Range(0.35f, 0.9f));
                Synth.Mix(d, plink, at, 1f, Rate);
                float[] splash = Synth.Normalize(Synth.Noise(0.03f, 842 + n++, 5000f, 1500f, 0.001f, 110f, 0f, Rate), 0.15f);
                Synth.Mix(d, splash, at, 1f, Rate);
                at += rng.Uni() < 0.3f ? rng.Range(0.12f, 0.3f) : rng.Range(0.6f, 1.5f);
            }
            return SynthReverb.ApplyLooped(d, 1.5f, 0.45f, 0.3f, 0f, Rate);
        }

        /// <summary>A long way off: the crack already smeared, then the roll.</summary>
        public static float[] Thunder(int take)
        {
            const float seconds = 5f;
            float[] d = Synth.Normalize(Synth.Noise(seconds, 851 + take, 420f, 60f, 0.12f + 0.2f * take, 0.75f), 0.9f);
            var rng = new Rng(855 + take);
            float roll = 1f, target = 1f;
            int next = 0;
            for (int i = 0; i < d.Length; i++)
            {
                if (i >= next)
                {
                    target = rng.Range(0.3f, 1f);
                    next = i + (int)(rng.Range(0.08f, 0.3f) * Synth.Rate);
                }
                roll += (target - roll) * 0.0006f;
                d[i] *= roll;
            }
            Patch.Layer(d, Synth.Noise(0.35f, 859 + take, 1800f, 300f, 0.004f, 9f), take == 0 ? 0.5f : 0.2f, 0.02f);
            return SynthReverb.Apply(d, 3f, 0.4f);
        }
    }
}

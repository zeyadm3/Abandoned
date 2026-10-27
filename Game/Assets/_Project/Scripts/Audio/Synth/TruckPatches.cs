using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>The truck, its doors and horn, the shutters, and the siren (0.14.0).</summary>
    internal static class TruckPatches
    {
        /// <summary>
        /// A diesel: one firing pulse after another, each ringing the block, with exhaust and rumble
        /// around them. The firing rate and the loudness each slide over the length.
        /// </summary>
        public static float[] Engine(float seconds, int seed, float fireFrom, float fireTo, float gainFrom, float gainTo,
            float body = 62f, int rate = Synth.Rate)
        {
            float[] d = Synth.Buffer(seconds, rate);
            var rng = new Rng(seed);
            float phase = 0f, age = 1f, strength = 1f, rumble = 0f, hiss = 0f;
            float rumbleA = Synth.OnePole(140f, rate), hissA = Synth.OnePole(1800f, rate);
            for (int i = 0; i < d.Length; i++)
            {
                float u = i / (float)d.Length;
                phase += Mathf.Lerp(fireFrom, fireTo, u) / rate;
                if (phase >= 1f)
                {
                    phase -= 1f;
                    age = 0f;
                    // No two firings are alike: that unevenness is what makes it an engine and not a tone.
                    strength = 0.8f + 0.2f * rng.Uni();
                }
                float n = rng.Bi();
                rumble += (n - rumble) * rumbleA;
                hiss += (n - hiss) * hissA;
                float pulse = Mathf.Exp(-age * 38f);
                float knock = Mathf.Sin(Synth.Tau * body * age) * pulse * strength;
                float exhaust = hiss * Mathf.Exp(-age * 22f) * 0.6f;
                d[i] = (knock + exhaust + rumble * 1.5f * (0.6f + 0.4f * pulse)) * Mathf.Lerp(gainFrom, gainTo, u);
                age += 1f / rate;
            }
            return d;
        }

        /// <summary>Idling beside you: 22 firings a second, 88 to the loop.</summary>
        public static float[] EngineIdle() =>
            Synth.Loop(Synth.LowPass(Engine(4.5f, 401, 22f, 22f, 1f, 1f), 900f), 0.5f, Synth.Rate);

        /// <summary>The drive: a steadier engine from inside the cab, road noise under it. 30 firings a second, 120 to the loop.</summary>
        public static float[] TravelEngine()
        {
            const int rate = Synth.LoopRate;
            float[] d = Synth.Normalize(Engine(4.4f, 411, 30f, 30f, 1f, 1f, 48f, rate), 0.6f);
            float[] road = Synth.Normalize(Synth.Noise(4.4f, 412, 260f, 260f, 0.001f, 0f, 0f, rate), 0.7f);
            for (int i = 0; i < d.Length; i++)
                d[i] = (d[i] + road[i]) * (0.8f + 0.2f * Mathf.Sin(Synth.Tau * 2f * i / rate));
            return Synth.Loop(Synth.LowPass(d, 500f, 2, rate), 0.4f, rate);
        }

        /// <summary>Arriving: the engine chugs down and dies, and the metal ticks as it cools.</summary>
        public static float[] EngineStop()
        {
            float[] d = Synth.Buffer(2.6f);
            Patch.Layer(d, Engine(1.7f, 421, 24f, 5f, 1f, 0.55f), 0.9f);
            Patch.Layer(d, Synth.Thump(0.3f, 60f, 30f, 9f), 0.6f, 1.7f);
            Synth.LowPass(d, 1400f);
            Patch.Pings(d, 422, 4, 1.9f, 0.6f, 1500f, 2600f, Patch.Bar, 50f, 0.25f, 1f);
            return SynthReverb.Apply(d, 0.8f, 0.2f);
        }

        /// <summary>Leaving: the starter catches, the engine climbs, and it pulls away.</summary>
        public static float[] EngineEscape()
        {
            float[] d = Synth.Buffer(3.2f);
            float[] starter = Synth.Sweep(0.45f, 280f, 340f, 0.02f, 1f, 2);
            for (int i = 0; i < starter.Length; i++) starter[i] *= 0.55f + 0.45f * Mathf.Sin(Synth.Tau * 11f * i / Synth.Rate);
            Patch.Layer(d, Synth.LowPass(starter, 1500f), 0.35f);
            float[] engine = Engine(2.8f, 431, 16f, 58f, 0.7f, 1f);
            Patch.Layer(d, Synth.Fade(engine, 0.15f, 1.2f), 0.9f, 0.4f);
            Patch.Layer(d, Synth.Fade(Synth.Noise(2.6f, 432, 500f, 900f, 0.3f, 0.5f), 0f, 1f), 0.3f, 0.5f);
            return SynthReverb.Apply(Synth.LowPass(d, 1600f), 1.2f, 0.22f);
        }

        /// <summary>Take 0: the truck's own heavy door. Take 1: a fire door somewhere inside.</summary>
        public static float[] DoorSlam(int take)
        {
            float[] d = Synth.Buffer(1.6f);
            if (take == 0)
            {
                Patch.Layer(d, Synth.Thump(0.38f, 135f, 46f, 10f), 0.95f);
                Patch.Layer(d, Synth.Modal(1f, 150f, Patch.Sheet, null, 5f), 0.5f);
                Patch.Layer(d, Synth.Noise(0.2f, 441, 5200f, 500f, 0.001f, 25f), 0.55f);
                Patch.Layer(d, Synth.Modal(0.08f, 1700f, Patch.Bar, null, 70f), 0.3f, 0.05f);
            }
            else
            {
                Patch.Layer(d, Synth.Thump(0.25f, 170f, 70f, 16f), 0.95f);
                Patch.Layer(d, Synth.Modal(0.5f, 260f, Patch.Wood, null, 14f), 0.4f);
                Patch.Layer(d, Synth.Noise(0.12f, 442, 3800f, 700f, 0.001f, 35f), 0.5f);
            }
            return SynthReverb.Apply(d, 1.3f, 0.3f);
        }

        /// <summary>Two air trumpets a minor third apart, each a touch out with itself.</summary>
        public static float[] Horn()
        {
            const float seconds = 0.75f;
            float[] d = Synth.Buffer(seconds);
            var bell = new Svf();
            bell.Set(1150f, 1.3f, Synth.Rate);
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                float reeds = Synth.Wave(2, 311f * t) + Synth.Wave(2, 312.3f * t) * 0.6f + Synth.Wave(2, 370f * t) + Synth.Wave(2, 371.6f * t) * 0.6f;
                bell.Tick(reeds);
                d[i] = (bell.Band * 1.2f + bell.Low * 0.5f) * Synth.Gate(t, seconds, 0.03f, 0.08f);
            }
            return SynthReverb.Apply(Patch.Heavy(d, 1.8f), 1.2f, 0.22f);
        }

        /// <summary>2026 remake: a two-trumpet air horn, a sharp attack and a long fat blast.</summary>
        public static float[] HornAir() => Reeds(1.1f, new[] { 233f, 277f, 234.2f }, 900f, .02f, .15f);

        /// <summary>2026 remake: a three-chime train horn (a minor triad, beating slightly).</summary>
        public static float[] HornTrain() => Reeds(1.4f, new[] { 311f, 370f, 466f, 312.5f }, 1300f, .05f, .25f);

        /// <summary>2026 remake: a ridiculous honk, two short squeaky bursts.</summary>
        public static float[] HornGoose()
        {
            float[] a = Reeds(.18f, new[] { 520f, 523f }, 2200f, .01f, .04f);
            float[] d = Synth.Buffer(.5f);
            Patch.Layer(d, a, 1f);
            Patch.Layer(d, a, .9f, .24f);
            return d;
        }

        /// <summary>2026 remake: a low foghorn-like ship horn.</summary>
        public static float[] HornDeep() => Reeds(1.6f, new[] { 98f, 98.7f, 147f }, 420f, .12f, .4f);

        private static float[] Reeds(float seconds, float[] pitches, float bellHz, float attack, float release)
        {
            float[] d = Synth.Buffer(seconds);
            var bell = new Svf();
            bell.Set(bellHz, 1.3f, Synth.Rate);
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                float reeds = 0f;
                foreach (float p in pitches) reeds += Synth.Wave(2, p * t);
                bell.Tick(reeds / pitches.Length * 2f);
                d[i] = (bell.Band * 1.2f + bell.Low * .5f) * Synth.Gate(t, seconds, attack, release);
            }
            return SynthReverb.Apply(Patch.Heavy(d, 1.8f), 1.2f, .22f);
        }

        /// <summary>Stands in for the recorded latch when the bank has none.</summary>
        public static float[] Lever()
        {
            float[] d = Synth.Buffer(0.5f);
            Patch.Layer(d, Synth.Thump(0.12f, 200f, 90f, 25f), 0.8f);
            Patch.Layer(d, Synth.Modal(0.4f, 420f, Patch.Sheet, null, 16f), 0.5f);
            for (int i = 0; i < 3; i++) Patch.Layer(d, Synth.Modal(0.05f, 1900f, Patch.Bar, null, 90f), 0.3f, 0.06f + 0.04f * i);
            return d;
        }

        /// <summary>A roller shutter going up: slat after slat over the drum, quicker as it gets going, then the stop.</summary>
        public static float[] ShutterOpen()
        {
            float[] d = Synth.Buffer(2.4f);
            var rng = new Rng(451);
            float at = 0.05f, gap = 0.11f;
            while (at < 2.05f)
            {
                Patch.Layer(d, Synth.Modal(0.09f, rng.Range(280f, 360f), Patch.Sheet, null, 26f), rng.Range(0.35f, 0.7f), at);
                gap = Mathf.Max(0.055f, gap * 0.97f);
                at += gap * rng.Range(0.85f, 1.15f);
            }
            Patch.Layer(d, Synth.Resonant(2.1f, 452, 300f, 520f, 1.4f, 0.3f, 0.2f), 0.3f, 0.03f);
            Patch.Layer(d, Synth.Thump(0.2f, 150f, 60f, 18f), 0.7f, 2.1f);
            Patch.Layer(d, Synth.Modal(0.3f, 210f, Patch.Sheet, null, 14f), 0.4f, 2.1f);
            return SynthReverb.Apply(d, 0.9f, 0.22f);
        }

        /// <summary>The same shutter dropped: a short rattle, then all of it hitting the floor.</summary>
        public static float[] ShutterSlam()
        {
            float[] d = Synth.Buffer(1.6f);
            var rng = new Rng(461);
            for (int i = 0; i < 8; i++)
                Patch.Layer(d, Synth.Modal(0.07f, rng.Range(280f, 360f), Patch.Sheet, null, 30f), 0.4f, 0.035f * i);
            Patch.Layer(d, Synth.Thump(0.5f, 120f, 38f, 7f), 0.95f, 0.32f);
            Patch.Layer(d, Synth.Modal(1.2f, 118f, Patch.Sheet, null, 3.6f), 0.6f, 0.32f);
            Patch.Layer(d, Synth.Noise(0.3f, 462, 6000f, 600f, 0.001f, 14f), 0.6f, 0.32f);
            return SynthReverb.Apply(d, 1.6f, 0.33f);
        }

        /// <summary>A hand-cranked wail through a horn speaker: up over a second, sagging back, twice.</summary>
        public static float[] Siren()
        {
            const float seconds = 5f;
            float[] d = Synth.Buffer(seconds);
            var horn = new Svf();
            horn.Set(1100f, 1.6f, Synth.Rate);
            float phase = 0f, upper = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                float hz = 430f + 180f * (0.5f - 0.5f * Mathf.Cos(Synth.Tau * t / 2.5f));
                phase += hz / Synth.Rate;
                if (phase >= 1f) phase -= 1f;
                upper += hz * 1.5f / Synth.Rate;
                if (upper >= 1f) upper -= 1f;
                horn.Tick(2f * phase - 1f + (2f * upper - 1f) * 0.35f);
                d[i] = (horn.Band + horn.Low * 0.4f) * Synth.Gate(t, seconds, 0.5f, 0.9f);
            }
            return SynthReverb.Apply(Synth.Normalize(Synth.Drive(Synth.Normalize(d, 1f), 1.4f), 0.85f), 2.4f, 0.4f);
        }
    }
}

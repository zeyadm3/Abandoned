using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// What the monsters sound like (0.14.0), matching what the threat files say about each one: the Blind
    /// One clicks, the Stalker breathes, the Collector clinks, the Hunter is heavy, the Crawlers skitter,
    /// the Weight winds up before it leans, the Thing rings like glass, the Last Hunter pounds.
    /// None of them ever creaks: a creak must only ever mean the floor (QA B-14).
    /// </summary>
    internal static class ThreatPatches
    {
        /// <summary>An echolocation click: a dry hollow tock, and what comes back off the walls.</summary>
        public static float[] BlindOneClick(int take)
        {
            float[] d = Synth.Buffer(0.35f);
            float[] tock = Synth.Normalize(Synth.Modal(0.09f, 980f + 150f * take, Patch.Hollow, null, 85f, 0.3f), 0.9f);
            Synth.Mix(d, tock);
            Patch.Layer(d, Synth.Noise(0.012f, 201 + take, 9000f, 4000f, 0.0003f, 300f), 0.5f);
            Patch.Layer(d, Synth.LowPass((float[])tock.Clone(), 2400f), 0.22f, 0.085f + 0.012f * take);
            return SynthReverb.Apply(d, 0.5f, 0.18f);
        }

        /// <summary>The trinkets it carries, under the recorded jingle.</summary>
        public static float[] CollectorJingle(int take)
        {
            float[] d = Synth.Buffer(0.5f);
            Patch.Pings(d, 211 + take, 6 + take, 0f, 0.32f, 2300f, 4300f, Patch.Bar, 24f, 0.6f, 1f);
            return SynthReverb.Apply(d, 0.4f, 0.12f);
        }

        /// <summary>A piano's weight on two feet: the part of the tread you feel through the floor.</summary>
        public static float[] HunterStep(int take)
        {
            float[] d = Synth.Buffer(0.8f);
            Patch.Layer(d, Synth.Thump(0.45f, 98f - 6f * take, 38f, 9f), 0.95f);
            Patch.Layer(d, Synth.Noise(0.25f, 221 + take, 950f, 200f, 0.003f, 18f), 0.5f);
            Patch.Layer(d, Synth.Modal(0.5f, 140f, Patch.Sheet, null, 9f), 0.12f);
            return SynthReverb.Apply(d, 0.9f, 0.22f);
        }

        public static float[] HunterLanding()
        {
            float[] d = Synth.Buffer(3f);
            Patch.Layer(d, Synth.Thump(1.5f, 115f, 27f, 2.6f, 10f), 0.95f);
            Patch.Layer(d, Synth.Noise(1.9f, 231, 2800f, 190f, 0.004f, 2.2f), 0.7f);
            Patch.Layer(d, Synth.Noise(0.4f, 232, 9000f, 1200f, 0.001f, 10f), 0.5f);
            Patch.Debris(d, 233, 20, 0.1f, 1.8f, 4800f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.6f), 2f, 0.33f);
        }

        /// <summary>Any monster's blow landing: the swing, then the hit.</summary>
        public static float[] Strike(int take)
        {
            float[] d = Synth.Buffer(0.5f);
            Patch.Layer(d, Synth.Resonant(0.13f, 241 + take, 500f, 2600f, 1.6f, 0.09f, 30f), 0.35f);
            Patch.Layer(d, Synth.Thump(0.24f, 165f - 10f * take, 52f, 17f), 0.95f, 0.1f);
            Patch.Layer(d, Synth.Noise(0.1f, 245 + take, 4200f, 800f, 0.001f, 42f), 0.6f, 0.1f);
            Patch.Layer(d, Synth.Noise(0.16f, 249 + take, 1200f, 300f, 0.002f, 22f), 0.4f, 0.11f);
            return SynthReverb.Apply(d, 0.5f, 0.15f);
        }

        /// <summary>Not a person's scream: two near-unison voices tearing against each other, sagging as they run out.</summary>
        public static float[] Scream(int take)
        {
            float seconds = take == 0 ? 1.7f : 2.3f, baseHz = take == 0 ? 640f : 470f;
            float[] d = Synth.Buffer(seconds);
            var rng = new Rng(251 + take);
            var f1 = new Svf();
            var f2 = new Svf();
            float p1 = 0f, p2 = 0f, rough = 0f, flutter = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate, u = t / seconds;
                // The pitch jumps up as it starts, sags, and breaks downward at the end.
                float contour = 1f + 0.22f * Mathf.Exp(-u * 9f) - 0.25f * u * u + 0.035f * Mathf.Sin(Synth.Tau * (11f + 6f * u) * t);
                flutter += (rng.Bi() - flutter) * 0.002f;
                float hz = baseHz * contour * (1f + flutter * 1.5f);
                p1 += hz / Synth.Rate;
                if (p1 >= 1f) p1 -= 1f;
                p2 += hz * 1.007f / Synth.Rate;
                if (p2 >= 1f) p2 -= 1f;
                rough += (rng.Bi() - rough) * 0.12f;
                float voice = (2f * p1 - 1f + 2f * p2 - 1f) * 0.5f * (0.7f + 0.3f * Mathf.Sin(Synth.Tau * 72f * t)) + rough * 0.5f;
                if ((i & 15) == 0)
                {
                    f1.Set(Mathf.Lerp(1050f, 800f, u), 4f, Synth.Rate);
                    f2.Set(Mathf.Lerp(2900f, 2300f, u), 5f, Synth.Rate);
                }
                f1.Tick(voice);
                f2.Tick(voice);
                d[i] = (f1.Band + f2.Band * 0.7f) * Synth.Gate(t, seconds, 0.035f, 0.5f);
            }
            return SynthReverb.Apply(Patch.Heavy(d, 1.5f), 2.4f, 0.42f);
        }

        /// <summary>Knuckles on a hollow wall: three, or two slow ones.</summary>
        public static float[] Knock(int take)
        {
            float[] d = Synth.Buffer(1.4f);
            float[] times = take == 0 ? new[] { 0f, 0.42f, 0.84f } : new[] { 0f, 0.68f };
            foreach (float at in times)
            {
                Patch.Layer(d, Synth.Thump(0.13f, 190f, 92f, 28f), 0.9f, at);
                Patch.Layer(d, Synth.Modal(0.25f, 235f, Patch.Wood, null, 30f), 0.5f, at);
            }
            return SynthReverb.Apply(d, 1f, 0.3f);
        }

        /// <summary>Words you can't make out: breath through a mouth shape that keeps changing.</summary>
        public static float[] Whisper(int take)
        {
            float seconds = 2.4f + 0.5f * take;
            float[] d = Synth.Buffer(seconds);
            var rng = new Rng(261 + take);
            var a = new Svf();
            var b = new Svf();
            float[] mouthA = { 700f, 400f, 550f, 320f, 620f }, mouthB = { 1200f, 2100f, 1700f, 900f, 2400f };
            float fa = 600f, fb = 1500f, ta = 600f, tb = 1500f, amp = 0f, target = 0f, hiss = 0f;
            float hissA = Synth.OnePole(5200f);
            int next = 0;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                if (i >= next)
                {
                    int shape = rng.Int(mouthA.Length);
                    ta = mouthA[shape];
                    tb = mouthB[shape];
                    // Syllables, with the odd held breath between them.
                    target = rng.Uni() < 0.22f ? 0f : rng.Range(0.5f, 1f);
                    next = i + (int)(rng.Range(0.09f, 0.21f) * Synth.Rate);
                }
                fa += (ta - fa) * 0.002f;
                fb += (tb - fb) * 0.002f;
                amp += (target - amp) * 0.003f;
                float n = rng.Bi();
                if ((i & 15) == 0)
                {
                    a.Set(fa, 6f, Synth.Rate);
                    b.Set(fb, 7f, Synth.Rate);
                }
                a.Tick(n);
                b.Tick(n);
                hiss += (n - hiss) * hissA;
                d[i] = (a.Band + b.Band * 0.8f + (n - hiss) * 0.12f * amp) * amp * Synth.Gate(t, seconds, 0.2f, 0.5f);
            }
            return SynthReverb.Apply(Synth.Normalize(d, 0.8f), 1.3f, 0.3f);
        }

        /// <summary>Something moving in the ceiling void: it scurries, stops, scurries.</summary>
        public static float[] CeilingScuttle(int take)
        {
            float[] d = Synth.Buffer(1.8f);
            var rng = new Rng(271 + take);
            float at = 0.05f, gap = 0.1f;
            int n = 0;
            while (at < 1.5f)
            {
                Patch.Layer(d, Synth.Thump(0.05f, rng.Range(260f, 340f), 140f, 60f), rng.Range(0.4f, 0.9f), at);
                Patch.Layer(d, Synth.Noise(0.02f, 272 + n++, 3000f, 900f, 0.0005f, 140f), 0.3f, at);
                gap = rng.Uni() < 0.14f ? rng.Range(0.18f, 0.32f) : Mathf.Lerp(gap, rng.Range(0.04f, 0.1f), 0.5f);
                at += gap;
            }
            // Whatever it drags along the tiles.
            Patch.Layer(d, Synth.Noise(1.6f, 279 + take, 600f, 300f, 0.3f, 1.2f), 0.18f, 0.05f);
            return SynthReverb.Apply(Synth.LowPass(d, 1900f), 0.6f, 0.2f);
        }

        /// <summary>A throat the size of a doorway: a slow pulse train with its edges torn by noise.</summary>
        public static float[] Roar()
        {
            const float seconds = 3.6f;
            float[] d = Synth.Buffer(seconds);
            var rng = new Rng(281);
            var f1 = new Svf();
            var f2 = new Svf();
            float p = 0f, sub = 0f, rough = 0f, breath = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate, u = t / seconds;
                float hz = Mathf.Lerp(40f, 25f, u) * (1f + 0.06f * Mathf.Sin(Synth.Tau * 5.5f * t));
                p += hz / Synth.Rate;
                if (p >= 1f) p -= 1f;
                sub += hz * 0.5f / Synth.Rate;
                if (sub >= 1f) sub -= 1f;
                rough += (rng.Bi() - rough) * 0.05f;
                breath += (rng.Bi() - breath) * 0.3f;
                float voice = (p < 0.28f ? 1f : -0.4f) * (0.6f + rough * 2.2f) + breath * 0.35f;
                if ((i & 15) == 0)
                {
                    f1.Set(Mathf.Lerp(210f, 130f, u), 2.5f, Synth.Rate);
                    f2.Set(Mathf.Lerp(580f, 390f, u), 3f, Synth.Rate);
                }
                f1.Tick(voice);
                f2.Tick(voice);
                float envelope = Mathf.Clamp01(t / 0.22f) * Mathf.Exp(-Mathf.Max(0f, t - 0.9f) * 0.9f) * Mathf.Clamp01((seconds - t) / 0.3f);
                d[i] = (f1.Band + f2.Band * 0.6f + f1.Low * 0.5f + Mathf.Sin(Synth.Tau * sub) * 0.5f) * envelope;
            }
            return SynthReverb.Apply(Patch.Heavy(d, 2.2f), 3f, 0.4f);
        }

        /// <summary>Thin, dry air through something that isn't a mouth: out, in, out.</summary>
        public static float[] StalkerBreath(int take)
        {
            bool inhale = take == 1;
            float seconds = inhale ? 0.9f : 1.3f;
            float[] d = Synth.Buffer(seconds);
            float low = take == 2 ? 620f : 720f;
            float[] air = inhale
                ? Synth.Resonant(seconds, 291, 650f, 1350f, 2.2f, 0.5f, 6f)
                : Synth.Resonant(seconds, 292 + take, low + 430f, low, 2.2f, 0.35f, 2.5f);
            Patch.Layer(d, air, 0.9f);
            Patch.Layer(d, Synth.Resonant(seconds, 295 + take, inhale ? 2000f : 2700f, inhale ? 2700f : 2000f, 3f, 0.35f, 3f), 0.3f);
            var rng = new Rng(297 + take);
            float rasp = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                rasp += (rng.Bi() - rasp) * 0.01f;
                d[i] *= 0.75f + 0.25f * Mathf.Sin(Synth.Tau * 31f * i / Synth.Rate + rasp * 40f);
            }
            return SynthReverb.Apply(Synth.LowPass(d, 5000f), 0.5f, 0.12f);
        }

        public static float[] StalkerSignature(int take)
        {
            float[] d = Synth.Buffer(1.5f);
            float[] rasp = Synth.Resonant(1.4f, 301 + take, 950f, 480f, 3f, 0.2f, 2f);
            for (int i = 0; i < rasp.Length; i++) rasp[i] *= 0.6f + 0.4f * Mathf.Sin(Synth.Tau * 6.5f * i / Synth.Rate);
            Patch.Layer(d, rasp, 0.8f);
            Patch.Layer(d, Synth.Sweep(1.4f, 215f, 205f, 0.3f, 1.5f), 0.12f);
            // Joints.
            Patch.Pings(d, 305 + take, 3 + take, 0.1f, 1f, 1400f, 2200f, Patch.Hollow, 70f, 0.35f, 1f);
            return SynthReverb.Apply(d, 0.8f, 0.25f);
        }

        /// <summary>The hoard settling: clinks dying away, and something small moving among them.</summary>
        public static float[] CollectorSignature(int take)
        {
            float[] d = Synth.Buffer(1.5f);
            var rng = new Rng(311 + take);
            for (int i = 0; i < 6; i++)
                Patch.Layer(d, Synth.Modal(0.3f, 860f * rng.Range(0.94f, 1.06f), Patch.Coin, null, 9f), 0.8f - 0.11f * i, 0.24f * i);
            Patch.Layer(d, Synth.Noise(1.2f, 315 + take, 2600f, 1400f, 0.1f, 2f, 700f), 0.12f);
            return SynthReverb.Apply(d, 0.6f, 0.18f);
        }

        /// <summary>The wind-up the threat file warns about: it leans, the strain climbs, and its weight comes down.</summary>
        public static float[] WeightSignature(int take)
        {
            float[] d = Synth.Buffer(2.8f);
            Patch.Layer(d, StructurePatches.Strain(2.5f, 321 + take, 27f, 47f, 105f, 270f, 0.25f, rising: true), 0.85f);
            Patch.Layer(d, Synth.Sweep(2.5f, 29f, 43f, 1.6f, 0.2f), 0.5f);
            Patch.Layer(d, Synth.Thump(0.4f, 85f, 30f, 7f), 0.9f, 2.42f);
            Patch.Layer(d, Synth.Noise(0.3f, 325 + take, 1500f, 250f, 0.002f, 12f), 0.4f, 2.42f);
            return SynthReverb.Apply(d, 2.2f, 0.33f);
        }

        /// <summary>A glass bell played backwards over two tones that won't agree: it swells out of nothing and stops.</summary>
        public static float[] ThingSignature(int take)
        {
            float[] d = Synth.Buffer(1.7f);
            float[] bell = Synth.Modal(1.3f, 1244f + 74f * take, Patch.Glass, null, 3.2f);
            System.Array.Reverse(bell);
            Patch.Layer(d, Synth.Fade(bell, 0.01f, 0.004f), 0.7f, 0.1f);
            float[] tones = Synth.Buffer(1.5f);
            for (int i = 0; i < tones.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                tones[i] = (Mathf.Sin(Synth.Tau * 390f * t) + Mathf.Sin(Synth.Tau * 393.75f * t)) * Synth.Swell(t, 1.5f);
            }
            Patch.Layer(d, tones, 0.45f);
            Patch.Layer(d, Synth.Resonant(1.5f, 331 + take, 3000f, 1800f, 4f, 0.6f, 2f), 0.12f);
            return SynthReverb.Apply(d, 1.4f, 0.35f);
        }

        /// <summary>Hundreds of legs in bursts, never a rhythm, and the chittering over them.</summary>
        public static float[] CrawlersSignature(int take)
        {
            float[] d = Synth.Buffer(1.4f);
            var rng = new Rng(341 + take);
            float at = 0f;
            int n = 0;
            while (at < 1.25f)
            {
                float swell = 0.5f + 0.5f * Mathf.Sin(Mathf.PI * at / 1.25f);
                Patch.Layer(d, Synth.Noise(0.006f, 342 + n++, 12000f, 5000f, 0.0002f, 500f, 2500f), rng.Range(0.2f, 0.8f) * swell, at);
                at += rng.Uni() < 0.12f ? rng.Range(0.05f, 0.14f) : rng.Range(0.006f, 0.022f);
            }
            for (int c = 0; c < 6 + take * 2; c++)
            {
                float hz = rng.Range(3200f, 5200f);
                Patch.Layer(d, Synth.Sweep(0.03f, hz, hz * rng.Range(0.7f, 1.3f), 0.003f, 90f), rng.Range(0.1f, 0.25f), rng.Range(0.05f, 1.2f));
            }
            return SynthReverb.Apply(d, 0.5f, 0.15f);
        }

        /// <summary>A heart the size of a room, and a wall of low brass that won't resolve.</summary>
        public static float[] LastHunterSignature(int take)
        {
            float[] d = Synth.Buffer(2.8f);
            Patch.Layer(d, Synth.Thump(0.7f, 52f, 29f, 4.5f, 12f), 0.95f, 0.05f);
            Patch.Layer(d, Synth.Thump(0.7f, 47f, 27f, 4.5f, 12f), 0.75f, 0.55f + 0.1f * take);
            float[] wall = Synth.Buffer(2.6f);
            for (int i = 0; i < wall.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                wall[i] = (Synth.Wave(2, 55f * t) + Synth.Wave(2, 58.3f * t) + 0.7f * Synth.Wave(2, 82.4f * t)) * Synth.Swell(t, 2.6f);
            }
            Patch.Layer(d, Synth.LowPass(wall, 420f), 0.5f, 0.1f);
            Patch.Layer(d, Synth.Noise(2.4f, 351 + take, 700f, 250f, 0.8f, 0.9f), 0.2f, 0.1f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.6f), 2.5f, 0.35f);
        }
    }
}

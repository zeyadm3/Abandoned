using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>Tools, loot and electronics (0.14.0). The last few stand in for recordings the bank normally supplies.</summary>
    internal static class GearPatches
    {
        /// <summary>A sonar ping: a bright note that drops as it leaves, and two echoes coming back duller.</summary>
        public static float[] ScanPing()
        {
            float[] ping = Synth.Buffer(0.9f);
            float phase = 0f;
            for (int i = 0; i < ping.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                phase += Mathf.Lerp(1500f, 1100f, Mathf.Clamp01(t / 0.25f)) / Synth.Rate;
                if (phase >= 1f) phase -= 1f;
                ping[i] = (Mathf.Sin(Synth.Tau * phase) + 0.25f * Mathf.Sin(2f * Synth.Tau * phase)) * Synth.Hit(t, 0.004f, 7f);
            }
            float[] d = Synth.Buffer(1.5f);
            Synth.Mix(d, ping);
            Synth.Mix(d, Synth.LowPass((float[])ping.Clone(), 2500f), 0.32f, 0.3f);
            Synth.Mix(d, Synth.LowPass((float[])ping.Clone(), 1500f), 0.64f, 0.12f);
            return SynthReverb.Apply(Synth.Normalize(d, 0.8f), 1.4f, 0.3f);
        }

        /// <summary>A harsh two-reed alarm warbling nine times a second: built to be heard through walls.</summary>
        public static float[] NoiseMakerShriek()
        {
            const float seconds = 0.9f;
            float[] d = Synth.Buffer(seconds);
            var band = new Svf();
            band.Set(2300f, 1.2f, Synth.Rate);
            float p = 0f, q = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                float hz = 1400f + 600f * Mathf.Sin(Synth.Tau * 9f * t);
                p += hz / Synth.Rate;
                if (p >= 1f) p -= 1f;
                q += hz * 1.5f / Synth.Rate;
                if (q >= 1f) q -= 1f;
                float reed = (p < 0.5f ? 1f : -1f) + (q < 0.5f ? 0.5f : -0.5f);
                band.Tick(reed);
                d[i] = (band.Band + reed * 0.25f) * Synth.Gate(t, seconds, 0.02f, 0.1f);
            }
            return Patch.Heavy(d, 2f);
        }

        /// <summary>A handset's small speaker: hiss with no lows and no top, crackle, a tuning whine. Take 1 ends on the squelch.</summary>
        public static float[] RadioStatic(int take)
        {
            const float seconds = 1.2f;
            float[] d = Synth.Buffer(seconds);
            var rng = new Rng(601 + take);
            float lp = 0f, hp = 0f, flutter = 1f, target = 1f, whine = 0f;
            float lpA = Synth.OnePole(4200f), hpA = Synth.OnePole(700f);
            int next = 0;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                if (i >= next)
                {
                    target = rng.Range(0.35f, 1f);
                    next = i + (int)(rng.Range(0.01f, 0.08f) * Synth.Rate);
                }
                flutter += (target - flutter) * 0.01f;
                lp += (rng.Bi() - lp) * lpA;
                hp += (lp - hp) * hpA;
                float pop = rng.Uni() < 0.0012f ? rng.Bi() * 3f : 0f;
                whine += (2300f + 260f * Mathf.Sin(Synth.Tau * 0.9f * t)) / Synth.Rate;
                if (whine >= 1f) whine -= 1f;
                d[i] = ((lp - hp) * flutter + pop + Mathf.Sin(Synth.Tau * whine) * 0.02f) * Synth.Gate(t, seconds, 0.04f, take == 0 ? 0.3f : 0.06f);
            }
            Synth.Normalize(d, 0.8f);
            if (take == 1) Patch.Layer(d, Synth.Noise(0.01f, 605, 6000f, 2000f, 0.0003f, 300f), 0.6f, seconds - 0.07f);
            return d;
        }

        /// <summary>The goggles charging: a click and a whine climbing out of hearing.</summary>
        public static float[] NightVisionOn()
        {
            float[] d = Synth.Buffer(0.8f);
            Patch.Layer(d, Synth.Modal(0.04f, 2600f, Patch.Bar, null, 110f), 0.5f);
            Patch.Layer(d, Synth.Sweep(0.7f, 900f, 6400f, 0.03f, 2.2f, 0, 0.5f), 0.35f, 0.02f);
            return d;
        }

        public static float[] NightVisionOff()
        {
            float[] d = Synth.Buffer(0.35f);
            Patch.Layer(d, Synth.Modal(0.04f, 2200f, Patch.Bar, null, 110f), 0.5f);
            Patch.Layer(d, Synth.Sweep(0.3f, 5200f, 700f, 0.005f, 9f, 0, 1.5f), 0.3f, 0.01f);
            return d;
        }

        public static float[] DetectorBlip()
        {
            float[] d = Synth.Buffer(0.2f);
            Patch.Layer(d, Synth.Sweep(0.16f, 1850f, 1850f, 0.002f, 26f), 0.8f);
            Patch.Layer(d, Synth.Sweep(0.16f, 3700f, 3700f, 0.002f, 40f), 0.2f);
            return d;
        }

        /// <summary>Whatever it was, it is in pieces now: shards over the material's own impact.</summary>
        public static float[] LootBreak(int take)
        {
            float[] d = Synth.Buffer(0.8f);
            Patch.Pings(d, 611 + take, 16 + 4 * take, 0f, 0.34f, 1900f, 6500f, Patch.Glass, 38f, 0.6f);
            Patch.Layer(d, Synth.Noise(0.2f, 615 + take, 7500f, 1500f, 0.001f, 22f), 0.55f);
            Patch.Debris(d, 619 + take, 8, 0.03f, 0.4f, 6000f);
            return SynthReverb.Apply(d, 0.7f, 0.2f);
        }

        /// <summary>The answering machine: one flat tone and a breath of tape.</summary>
        public static float[] VoicemailBeep()
        {
            float[] d = Synth.Buffer(0.5f);
            float[] tone = Synth.Buffer(0.32f);
            for (int i = 0; i < tone.Length; i++)
            {
                float t = i / (float)Synth.Rate;
                tone[i] = Mathf.Sin(Synth.Tau * 1000f * t) * Synth.Gate(t, 0.32f, 0.01f, 0.03f);
            }
            Patch.Layer(d, tone, 0.7f);
            Patch.Layer(d, Synth.Noise(0.5f, 621, 5000f, 5000f, 0.02f, 2f, 1500f), 0.08f);
            return d;
        }

        // ---- Stand-ins for the bank's recordings ----

        public static float[] Click(float hz)
        {
            float[] d = Synth.Buffer(0.06f);
            Patch.Layer(d, Synth.Modal(0.05f, hz, Patch.Bar, null, 120f), 0.8f);
            Patch.Layer(d, Synth.Noise(0.004f, 631, 9000f, 4000f, 0.0003f, 600f), 0.5f);
            return d;
        }

        public static float[] Rustle(float seconds, float bright, int seed) =>
            Synth.Noise(seconds, seed, bright, bright * 0.45f, 0.02f, 16f, 400f);

        public static float[] Coins()
        {
            float[] d = Synth.Buffer(0.6f);
            Patch.Pings(d, 641, 6, 0f, 0.25f, 2400f, 4600f, Patch.Coin, 20f, 0.6f, 1f);
            return d;
        }

        public static float[] CrowbarHit()
        {
            float[] d = Synth.Buffer(0.9f);
            Patch.Layer(d, Synth.Modal(0.9f, 520f, Patch.Bar, null, 5f), 0.8f);
            Patch.Layer(d, Synth.Thump(0.1f, 220f, 110f, 30f), 0.5f);
            Patch.Layer(d, Synth.Noise(0.01f, 651, 9000f, 3000f, 0.0003f, 300f), 0.5f);
            return d;
        }

        public static float[] JackPlaced()
        {
            float[] d = Synth.Buffer(0.5f);
            Patch.Layer(d, Synth.Thump(0.15f, 160f, 70f, 22f), 0.8f);
            Patch.Layer(d, Synth.Modal(0.3f, 300f, Patch.Sheet, null, 18f), 0.5f);
            for (int i = 0; i < 2; i++) Patch.Layer(d, Synth.Modal(0.05f, 1700f, Patch.Bar, null, 90f), 0.3f, 0.12f + 0.07f * i);
            return d;
        }

        public static float[] BoltCut()
        {
            float[] d = Synth.Buffer(0.4f);
            Patch.Layer(d, Synth.Noise(0.012f, 661, 9000f, 3000f, 0.0003f, 300f), 0.7f);
            Patch.Layer(d, Synth.Noise(0.012f, 662, 9000f, 3000f, 0.0003f, 300f), 0.9f, 0.07f);
            Patch.Layer(d, Synth.Modal(0.25f, 1500f, Patch.Bar, null, 20f), 0.4f, 0.07f);
            return d;
        }
    }
}

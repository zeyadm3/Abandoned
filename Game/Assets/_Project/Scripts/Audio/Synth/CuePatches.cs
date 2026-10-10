using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The 0.24.0 sound cue pass: the sounds players must learn, rebuilt with more physical detail. The floor's
    /// warnings are stick-slip friction (a creak only ever means the floor; nothing else uses this voice), and
    /// each monster gets a signature no other sound shares. Worker thread: no Unity API besides Mathf.
    /// </summary>
    internal static class CuePatches
    {
        // Stick-slip: a run of tiny resonant clicks whose rate slides, the way a loaded joint or slab creaks.
        private static void StickSlip(float[] d, int seed, float seconds, float rateFrom, float rateTo, float hz, float[] ratios, float decay, float level, float at = 0f)
        {
            var rng = new Rng(seed);
            float t = 0f;
            while (t < seconds)
            {
                float u = t / seconds;
                float rate = Mathf.Lerp(rateFrom, rateTo, u) * rng.Range(0.75f, 1.25f);
                float swell = Mathf.Sin(Mathf.PI * u);
                Patch.Layer(d, Synth.Modal(0.05f, hz * rng.Range(0.94f, 1.06f), ratios, null, decay), level * swell * rng.Range(0.5f, 1f), at + t);
                t += 1f / Mathf.Max(5f, rate);
            }
        }

        /// <summary>The floor creaking under weight: a slow friction run with the slab's body under it.</summary>
        public static float[] Creak(int take)
        {
            float seconds = 0.9f + 0.25f * take;
            float[] d = Synth.Buffer(seconds + 0.6f);
            StickSlip(d, 3101 + take * 13, seconds, 38f, 90f + 20f * take, 330f + 40f * take, Patch.Wood, 140f, 0.35f);
            Patch.Layer(d, Synth.Fade(Synth.Resonant(seconds, 3102 + take, 260f, 420f, 7f, 0.15f, 0.8f), 0.1f, 0.2f), 0.35f);
            Patch.Pings(d, 3103 + take, 3, 0.15f, seconds - 0.2f, 1100f, 2100f, Patch.Sheet, 45f, 0.18f, 1f);
            return SynthReverb.Apply(d, 1f, 0.24f);
        }

        /// <summary>The floor groaning: a long low strain, beating against itself, with the slab grinding through it.</summary>
        public static float[] Groan(int take)
        {
            float seconds = 2.3f + 0.3f * take;
            float[] d = Synth.Buffer(seconds + 0.8f);
            Patch.Layer(d, Synth.Fade(Synth.Sweep(seconds, 46f, 38f, 0.5f, 0.4f, 1), 0.4f, 0.6f), 0.55f);
            Patch.Layer(d, Synth.Fade(Synth.Sweep(seconds, 47.5f, 39.2f, 0.5f, 0.4f, 1), 0.4f, 0.6f), 0.45f);
            StickSlip(d, 3111 + take * 7, seconds, 14f, 30f, 120f + 15f * take, Patch.Hollow, 60f, 0.45f, 0.2f);
            Patch.Layer(d, Synth.Fade(Synth.Noise(seconds, 3112 + take, 180f, 90f, 0.3f, 0.5f), 0.3f, 0.6f), 0.4f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.3f), 1.8f, 0.3f);
        }

        /// <summary>Something structural snapping: a dry crack, a crackle running away from it, rebar ringing, a drop.</summary>
        public static float[] Snap(int take)
        {
            float[] d = Synth.Buffer(0.9f);
            Patch.Layer(d, Synth.Noise(0.03f, 3121 + take, 14000f, 6000f, 0.0002f, 140f), 1f);
            Patch.Pings(d, 3122 + take, 12, 0.01f, 0.2f, 2500f, 7500f, Patch.Bar, 110f, 0.4f, 0.7f);
            Patch.Layer(d, Synth.Modal(0.6f, 640f + 60f * take, Patch.Bar, null, 10f), 0.4f, 0.005f);
            Patch.Layer(d, Synth.Thump(0.25f, 210f, 64f, 20f), 0.55f, 0.02f);
            Patch.Debris(d, 3123 + take, 6, 0.08f, 0.5f, 4500f);
            return SynthReverb.Apply(d, 0.8f, 0.3f);
        }

        // ---- Monsters ----

        /// <summary>The Blind One: double tongue clicks, sharp as a sonar ping, then a wet rattle in its throat.</summary>
        public static float[] BlindOneClick(int take)
        {
            float[] d = Synth.Buffer(0.7f);
            var rng = new Rng(3201 + take);
            for (int i = 0; i < 2; i++)
            {
                float at = i * rng.Range(0.07f, 0.11f);
                Patch.Layer(d, Synth.Noise(0.012f, 3202 + take * 3 + i, 9000f, 4000f, 0.0003f, 260f, 2000f), 0.9f, at);
                Patch.Layer(d, Synth.Modal(0.06f, rng.Range(2400f, 3600f), Patch.Hollow, null, 90f), 0.5f, at);
            }
            Patch.Layer(d, Synth.Fade(Synth.Resonant(0.4f, 3203 + take, 600f, 380f, 9f, 0.03f, 5f), 0.02f, 0.1f), 0.35f, 0.2f);
            return SynthReverb.Apply(d, 1.2f, 0.35f);
        }

        /// <summary>The Hunter's footfall: a heavy, slightly dragged step that shakes grit loose.</summary>
        public static float[] HunterStep(int take)
        {
            float[] d = Synth.Buffer(1f);
            Patch.Layer(d, Synth.Thump(0.5f, 95f + take * 5f, 34f, 7f, 18f), 1f);
            Patch.Layer(d, Synth.Noise(0.25f, 3211 + take, 1200f, 300f, 0.01f, 9f), 0.45f, 0.03f);
            Patch.Debris(d, 3212 + take, 8, 0.05f, 0.6f, 3500f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.5f), 1.3f, 0.3f);
        }

        /// <summary>The Stalker: a slow, wet inhale, and a joint popping as it shifts its weight.</summary>
        public static float[] StalkerSignature(int take)
        {
            float[] d = Synth.Buffer(1.8f);
            Patch.Layer(d, Synth.Fade(Synth.Resonant(1.2f, 3221 + take, 700f, 1100f, 4f, 0.5f, 1.2f), 0.4f, 0.4f), 0.55f);
            Patch.Layer(d, Synth.Noise(0.03f, 3222 + take, 6000f, 2000f, 0.0005f, 120f), 0.6f, 1.25f);
            Patch.Layer(d, Synth.Modal(0.08f, 520f, Patch.Wood, null, 60f), 0.4f, 1.25f);
            return SynthReverb.Apply(d, 1.4f, 0.35f);
        }

        /// <summary>The Collector: a pocketful of stolen trinkets clinking, and a quick, pleased chitter.</summary>
        public static float[] CollectorSignature(int take)
        {
            float[] d = Synth.Buffer(1.1f);
            Patch.Pings(d, 3231 + take, 9, 0f, 0.6f, 1800f, 4200f, Patch.Coin, 40f, 0.45f, 1.2f);
            for (int i = 0; i < 5; i++) Patch.Layer(d, Synth.Sweep(0.05f, 1500f + i * 90f, 2300f, 0.003f, 60f), 0.25f, 0.62f + i * 0.055f);
            return SynthReverb.Apply(d, 1f, 0.3f);
        }

        /// <summary>The Weight: something enormous dragging chain over concrete, a deep shudder under it (no creak).</summary>
        public static float[] WeightSignature(int take)
        {
            float[] d = Synth.Buffer(2.4f);
            Patch.Layer(d, Synth.Fade(Synth.Noise(2.2f, 3241 + take, 700f, 300f, 0.3f, 0.6f), 0.3f, 0.5f), 0.55f);
            for (int i = 0; i < 9; i++) Patch.Layer(d, Synth.Modal(0.15f, 380f + (i % 3) * 70f, Patch.Bar, null, 25f), 0.3f, 0.2f + i * 0.2f);
            Patch.Layer(d, Synth.Fade(Synth.Sweep(2.2f, 31f, 27f, 0.4f, 0.3f, 1), 0.4f, 0.5f), 0.6f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.4f), 2f, 0.33f);
        }

        /// <summary>The Thing: a glassy ring that swells instead of fading, like a sound played backwards.</summary>
        public static float[] ThingSignature(int take)
        {
            float[] ring = Synth.Modal(1.4f, 880f + take * 55f, Patch.Glass, null, 3f);
            float[] d = Synth.Buffer(1.6f);
            for (int i = 0; i < ring.Length && i < d.Length; i++) d[i] = ring[ring.Length - 1 - i];
            Patch.Layer(d, Synth.Modal(0.3f, 1760f + take * 110f, Patch.Glass, null, 12f), 0.4f, 1.35f);
            return SynthReverb.Apply(d, 2f, 0.45f);
        }

        /// <summary>The Crawlers: dozens of tiny hard feet skittering over hard floor, in bursts.</summary>
        public static float[] CrawlersSignature(int take)
        {
            float[] d = Synth.Buffer(1.2f);
            var rng = new Rng(3251 + take);
            for (int burst = 0; burst < 3; burst++)
            {
                float start = burst * rng.Range(0.3f, 0.4f);
                for (int i = 0; i < 14; i++)
                    Patch.Layer(d, Synth.Noise(0.008f, 3252 + take * 50 + burst * 20 + i, 8000f, 5000f, 0.0002f, 300f, 2500f), rng.Range(0.2f, 0.55f), start + i * rng.Range(0.012f, 0.022f));
            }
            return SynthReverb.Apply(d, 0.8f, 0.25f);
        }

        /// <summary>The Last Hunter: a slow double pound, like a heart the size of a room.</summary>
        public static float[] LastHunterSignature(int take)
        {
            float[] d = Synth.Buffer(2.2f);
            Patch.Layer(d, Synth.Thump(0.6f, 62f, 28f, 5f, 12f), 1f);
            Patch.Layer(d, Synth.Thump(0.6f, 58f, 26f, 5f, 12f), 0.8f, 0.32f);
            Patch.Layer(d, Synth.Fade(Synth.Noise(1.8f, 3261 + take, 140f, 60f, 0.05f, 1.2f), 0.05f, 0.6f), 0.4f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.7f), 2.2f, 0.3f);
        }
    }
}

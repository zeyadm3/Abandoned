namespace Abandoned.Audio
{
    /// <summary>
    /// The Iron Factory's machines: a motor dragging a line back into motion, a press bottoming out, a steam
    /// valve venting. None of these is a creak: a creak only ever means the floor. Worker thread: no Unity API.
    /// </summary>
    internal static class FactoryPatches
    {
        /// <summary>A big motor winding up from a growl to a whine, belts and gears clattering in.</summary>
        public static float[] MachineStart()
        {
            float[] d = Synth.Buffer(2.4f);
            Patch.Layer(d, Synth.Fade(Synth.Resonant(2.3f, 2101, 45f, 260f, 2.5f, 0.4f, 1.2f), 0.3f, 0.6f), 0.8f);
            Patch.Layer(d, Synth.Fade(Synth.Noise(2.2f, 2102, 300f, 1400f, 0.5f, 1.4f, 120f), 0.4f, 0.5f), 0.35f, 0.1f);
            Patch.Debris(d, 2103, 10, 0.3f, 1.8f, 2600f);
            return SynthReverb.Apply(d, 1.4f, 0.3f);
        }

        /// <summary>Hundreds of tonnes meeting the die: a flat iron bang, the floor's boom, and the ring after.</summary>
        public static float[] PressSlam(int take)
        {
            float[] d = Synth.Buffer(1.8f);
            Patch.Layer(d, Synth.Noise(0.06f, 2111 + take, 6000f, 1800f, 0.001f, 45f), 0.9f);
            Patch.Layer(d, Synth.Thump(0.9f, 70f, 34f, 5f), 1f);
            Patch.Layer(d, Synth.Modal(1.2f, 165f + take * 9f, Patch.Hollow, null, 4f), 0.4f);
            Patch.Debris(d, 2112 + take, 6, 0.04f, 0.5f, 3000f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.5f), 1.8f, 0.35f);
        }

        /// <summary>Molten iron moving in its channel: a deep roar that swells, a thin hiss over it, and slow heavy
        /// plops as bubbles break. A seamless loop.</summary>
        public static float[] LavaRumble()
        {
            const int rate = Synth.LoopRate;
            const float seconds = 8f, fold = .5f;
            float[] d = Synth.Buffer(seconds + fold, rate);
            var rng = new Rng(2131);
            var roar = new Svf();
            roar.Set(70f, .9f, rate);
            var hiss = new Svf();
            hiss.Set(2400f, .7f, rate);
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)rate;
                roar.Tick(rng.Bi());
                hiss.Tick(rng.Bi());
                float swell = .8f + .2f * UnityEngine.Mathf.Sin(Synth.Tau * t * 2f / seconds);
                d[i] = (roar.Low * 3f + hiss.Band * .05f) * swell;
            }
            var pops = new Rng(2132);
            for (int k = 0; k < 14; k++)
            {
                float at = pops.Range(0f, seconds), hz = pops.Range(60f, 140f);
                Synth.MixWrapped(d, Synth.Thump(.35f, hz, hz * .5f, 12f, 30f, rate), at, pops.Range(.15f, .4f), rate);
            }
            return Synth.Loop(d, fold, rate);
        }

        /// <summary>A valve letting go: a sharp, wide hiss with a low roar under it, tailing off.</summary>
        public static float[] SteamBlast(int take)
        {
            float[] d = Synth.Buffer(1.8f);
            Patch.Layer(d, Synth.Fade(Synth.Noise(1.7f, 2121 + take, 9500f, 6000f, 0.02f, 0.9f, 900f), 0.02f, 0.6f), 0.75f);
            Patch.Layer(d, Synth.Fade(Synth.Noise(1.6f, 2122 + take, 420f, 220f, 0.05f, 1.1f), 0.05f, 0.5f), 0.5f);
            return SynthReverb.Apply(d, 1.1f, 0.25f);
        }
    }
}

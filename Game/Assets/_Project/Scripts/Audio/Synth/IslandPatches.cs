namespace Abandoned.Audio
{
    /// <summary>
    /// The island prison's own sounds: a distant foghorn across the bay, surf breaking on the rocks below the
    /// cliffs, and the lighthouse lamp's motor turning. None of them is a creak: a creak only ever means the floor.
    /// Worker thread: no Unity API beyond Mathf.
    /// </summary>
    internal static class IslandPatches
    {
        /// <summary>A diaphone a long way off: a low reedy blast that drops at the end ("bee-ohh"), smeared by fog.</summary>
        public static float[] Foghorn(int take)
        {
            float[] d = Synth.Buffer(5.2f);
            float hz = 98f + take * 6f;
            Patch.Layer(d, Synth.Fade(Synth.Sweep(4.2f, hz, hz, 0.35f, 99f, 2), 0.35f, 0.4f), 0.45f);
            Patch.Layer(d, Synth.Fade(Synth.Sweep(4.2f, hz * 1.5f, hz * 1.5f, 0.35f, 99f, 3), 0.35f, 0.4f), 0.18f);
            // The grunt at the end, a fifth lower.
            Patch.Layer(d, Synth.Fade(Synth.Sweep(1.1f, hz * .75f, hz * .7f, 0.05f, 99f, 2), 0.05f, 0.6f), 0.5f, 3.9f);
            Patch.Layer(d, Synth.Fade(Synth.Resonant(4.6f, 4101 + take, hz * 2f, hz * 1.6f, 3f, 0.4f, 99f), 0.4f, 0.8f), 0.25f);
            // Far away: the fog takes the top off and the bay smears it out.
            return SynthReverb.Apply(Synth.LowPass(d, 900f), 3.2f, 0.55f);
        }

        /// <summary>Waves breaking on rocks below the cliffs: a slow swell of low roar, a hiss as each wave
        /// spreads, and the drag of water back off the stones. A seamless loop.</summary>
        public static float[] Surf()
        {
            const int rate = Synth.LoopRate;
            const float seconds = 12f, fold = .6f;
            float[] d = Synth.Buffer(seconds + fold, rate);
            var rng = new Rng(4111);
            var roar = new Svf();
            roar.Set(180f, .8f, rate);
            var hiss = new Svf();
            hiss.Set(2600f, .6f, rate);
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)rate;
                roar.Tick(rng.Bi());
                hiss.Tick(rng.Bi());
                // Three waves a loop, each a fast rise and a long fall.
                float phase = t * 3f / seconds;
                float p = phase - UnityEngine.Mathf.Floor(phase);
                float wave = p < .15f ? p / .15f : UnityEngine.Mathf.Pow(1f - (p - .15f) / .85f, 1.6f);
                d[i] = roar.Low * (1.1f + 1.6f * wave) + hiss.Band * (.05f + .4f * wave * wave);
            }
            return Synth.Loop(d, fold, rate);
        }

        /// <summary>The lamp room: an old motor turning the lens on its mercury bath, a low hum with a slow beat.</summary>
        public static float[] LighthouseMotor()
        {
            const int rate = Synth.LoopRate;
            const float seconds = 4f, fold = .3f;
            float[] d = Synth.Buffer(seconds + fold, rate);
            var rng = new Rng(4121);
            var grind = new Svf();
            grind.Set(900f, 2.5f, rate);
            float a = 0f, b = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)rate;
                a += 60f / rate;
                b += 120.6f / rate;
                grind.Tick(rng.Bi());
                float beat = .85f + .15f * UnityEngine.Mathf.Sin(Synth.Tau * t * 2f / seconds);
                d[i] = (Synth.Wave(0, a) * .5f + Synth.Wave(1, b) * .2f + grind.Band * .08f) * beat;
            }
            return Synth.Loop(d, fold, rate);
        }
    }
}

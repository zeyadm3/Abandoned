namespace Abandoned.Audio
{
    /// <summary>
    /// The salvager's own body (0.14.0). Low health is the heartbeat alone (user, 2026-10-09): the soft
    /// breath here is only the fear breath above the critical threshold, and nothing here is a gasp.
    /// </summary>
    internal static class PlayerPatches
    {
        /// <summary>Lub-dub, all below 260 Hz: felt in the chest, never mistaken for a sound in the room.</summary>
        public static float[] Heartbeat()
        {
            float[] d = Synth.Buffer(0.42f);
            Patch.Layer(d, Synth.Thump(0.2f, 64f, 40f, 24f, 40f), 1f);
            Patch.Layer(d, Synth.Thump(0.2f, 80f, 47f, 32f, 40f), 0.62f, 0.17f);
            Patch.Layer(d, Synth.Noise(0.1f, 501, 180f, 90f, 0.004f, 30f), 0.25f);
            Patch.Layer(d, Synth.Noise(0.1f, 502, 200f, 100f, 0.004f, 36f), 0.16f, 0.17f);
            return Synth.Fade(Synth.LowPass(d, 260f), 0f, 0.02f);
        }

        /// <summary>One quiet breath out through an open mouth.</summary>
        public static float[] Breath()
        {
            float[] d = Synth.Buffer(1.6f);
            Patch.Layer(d, Synth.Resonant(1.6f, 511, 950f, 620f, 1.3f, 0.6f, 2.4f), 0.8f);
            Patch.Layer(d, Synth.Resonant(1.6f, 512, 2300f, 1700f, 2f, 0.6f, 3f), 0.25f);
            return Synth.Fade(Synth.LowPass(d, 3200f), 0.05f, 0.2f);
        }

        /// <summary>A blow landing on a body in a coverall: blunt, no voice.</summary>
        public static float[] Hurt(int take)
        {
            float[] d = Synth.Buffer(0.4f);
            Patch.Layer(d, Synth.Thump(0.2f, 150f - 10f * take, 58f, 20f), 0.95f);
            Patch.Layer(d, Synth.Noise(0.09f, 521 + take, 2600f, 600f, 0.001f, 45f), 0.6f);
            Patch.Layer(d, Synth.Noise(0.16f, 525 + take, 5000f, 1500f, 0.01f, 18f, 1500f), 0.2f);
            return d;
        }

        /// <summary>Understated: deaths stay in the world, they don't get a fanfare.</summary>
        public static float[] Death()
        {
            float[] d = Synth.Buffer(2f);
            Patch.Layer(d, Synth.Sweep(1.3f, 112f, 36f, 0.01f, 2.2f), 0.7f);
            Patch.Layer(d, Synth.Thump(0.4f, 90f, 40f, 8f), 0.8f);
            Patch.Layer(d, Synth.Noise(1.2f, 531, 800f, 120f, 0.05f, 2.5f), 0.3f);
            return SynthReverb.Apply(d, 1.8f, 0.4f);
        }

        /// <summary>Both boots, a moment apart, and the gear settling.</summary>
        public static float[] Land(int take)
        {
            float[] d = Synth.Buffer(0.35f);
            Patch.Layer(d, Synth.Thump(0.12f, 125f - 8f * take, 55f, 30f), 0.9f);
            Patch.Layer(d, Synth.Thump(0.12f, 118f, 52f, 30f), 0.7f, 0.022f);
            Patch.Layer(d, Synth.Noise(0.14f, 541 + take, 3000f, 900f, 0.003f, 26f, 600f), 0.3f);
            Patch.Pings(d, 545 + take, 1, 0.01f, 0.03f, 2000f, 3000f, Patch.Bar, 70f, 0.12f, 1f);
            return d;
        }

        /// <summary>Knocked off your feet: the body, then the bounce.</summary>
        public static float[] BodyFall(int take)
        {
            float[] d = Synth.Buffer(0.8f);
            Patch.Layer(d, Synth.Thump(0.32f, 96f - 6f * take, 42f, 12f), 0.95f);
            Patch.Layer(d, Synth.Thump(0.3f, 88f, 40f, 14f), 0.5f, 0.17f);
            Patch.Layer(d, Synth.Noise(0.3f, 551 + take, 2200f, 500f, 0.005f, 12f, 300f), 0.35f);
            return SynthReverb.Apply(d, 0.4f, 0.12f);
        }
    }
}

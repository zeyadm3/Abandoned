namespace Abandoned.Audio
{
    /// <summary>Crew and comfort (0.19+): ping, the replacement worker, ghosts, the break room.</summary>
    internal static class CrewPatches
    {
        /// <summary>A short two-note radio blip for a ping.</summary>
        public static float[] Ping()
        {
            float[] d = Synth.Buffer(0.3f);
            Patch.Layer(d, Synth.Sweep(0.08f, 1320f, 1320f, 0.003f, 25f), 0.5f);
            Patch.Layer(d, Synth.Sweep(0.12f, 1760f, 1760f, 0.003f, 18f), 0.5f, 0.08f);
            return d;
        }

        /// <summary>Three soft knuckle raps on wood, airy and a little too even.</summary>
        public static float[] GhostKnock(int take)
        {
            float[] d = Synth.Buffer(1.2f);
            for (int i = 0; i < 3; i++)
                Patch.Layer(d, Synth.Modal(0.25f, 230f + take * 12f, Patch.Wood, null, 22f), 0.7f, i * 0.26f);
            Patch.Layer(d, Synth.Fade(Synth.Resonant(1.1f, 1901 + take, 500f, 300f, 4f, 0.3f, 1.2f), 0.2f, 0.4f), 0.15f);
            return SynthReverb.Apply(d, 1.6f, 0.45f);
        }

        /// <summary>A breath of air past something small.</summary>
        public static float[] GhostWhoosh(int take)
        {
            float[] d = Synth.Buffer(0.7f);
            Patch.Layer(d, Synth.Fade(Synth.Resonant(0.65f, 1902 + take, 400f, 1400f, 2f, 0.2f, 3f), 0.1f, 0.2f), 0.6f);
            return SynthReverb.Apply(d, 1.2f, 0.4f);
        }
    }
}

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

        /// <summary>A little rising arpeggio for a score in the break room.</summary>
        public static float[] ToyScore()
        {
            float[] d = Synth.Buffer(0.6f);
            float[] notes = { 523f, 659f, 784f, 1047f };
            for (int i = 0; i < notes.Length; i++) Patch.Layer(d, Synth.Sweep(0.18f, notes[i], notes[i], 0.004f, 12f, 1), 0.4f, i * 0.07f);
            return d;
        }

        /// <summary>A shoulder into a padded coverall: a soft thump and a rustle.</summary>
        public static float[] Shove(int take)
        {
            float[] d = Synth.Buffer(0.35f);
            Patch.Layer(d, Synth.Thump(0.2f, 120f + take * 10f, 70f, 20f), 0.6f);
            Patch.Layer(d, Synth.Noise(0.25f, 2101 + take, 2500f, 1200f, 0.01f, 12f), 0.3f);
            return d;
        }

        /// <summary>A monster taking a blow: a startled, throaty hiss and a thud.</summary>
        public static float[] Flinch(int take)
        {
            float[] d = Synth.Buffer(0.8f);
            Patch.Layer(d, Synth.Thump(0.25f, 110f, 60f, 16f), 0.6f);
            Patch.Layer(d, Synth.Fade(Synth.Resonant(0.7f, 2201 + take, 700f, 300f, 5f, 0.01f, 5f), 0.01f, 0.2f), 0.8f);
            return SynthReverb.Apply(d, 1f, 0.25f);
        }
    }
}

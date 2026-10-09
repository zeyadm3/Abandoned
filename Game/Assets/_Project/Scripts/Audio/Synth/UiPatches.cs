namespace Abandoned.Audio
{
    /// <summary>
    /// Interface sounds (0.14.0). The buttons keep Kenney's recordings; these are the sounds that pack has
    /// nothing for (hover, toast, chat), and stand-ins for the buttons should the bank lose its clips.
    /// </summary>
    internal static class UiPatches
    {
        /// <summary>The pointer crossing a button: barely there.</summary>
        public static float[] Hover() => Synth.Modal(0.05f, 1800f, new[] { 1f, 2.4f }, null, 110f);

        /// <summary>A note sliding in from the side of the screen: two soft tines, a fifth apart.</summary>
        public static float[] Toast()
        {
            float[] d = Synth.Buffer(0.6f);
            Patch.Layer(d, Synth.Pluck(0.5f, 880f, 9f, 0.5f, 4), 0.7f);
            Patch.Layer(d, Synth.Pluck(0.5f, 1320f, 10f, 0.5f, 4), 0.6f, 0.07f);
            return SynthReverb.Apply(d, 0.8f, 0.25f);
        }

        public static float[] ChatBlip() => Synth.Pluck(0.14f, 1175f, 22f, 0.5f, 3);

        public static float[] Click()
        {
            float[] d = Synth.Buffer(0.08f);
            Patch.Layer(d, Synth.Modal(0.07f, 1200f, new[] { 1f, 2.1f, 3.3f }, null, 80f), 0.8f);
            Patch.Layer(d, Synth.Noise(0.004f, 701, 9000f, 4000f, 0.0003f, 600f), 0.3f);
            return d;
        }

        /// <summary>Two notes: rising for yes, falling for back.</summary>
        public static float[] TwoNote(float first, float second)
        {
            float[] d = Synth.Buffer(0.4f);
            Patch.Layer(d, Synth.Pluck(0.3f, first, 12f, 0.5f, 4), 0.7f);
            Patch.Layer(d, Synth.Pluck(0.3f, second, 12f, 0.5f, 4), 0.7f, 0.08f);
            return d;
        }

        public static float[] Error()
        {
            float[] d = Synth.Buffer(0.3f);
            for (int i = 0; i < 2; i++)
                Patch.Layer(d, Synth.LowPass(Synth.Sweep(0.1f, 150f, 140f, 0.004f, 14f, 3), 900f), 0.8f, 0.13f * i);
            return d;
        }

        /// <summary>A short breath of air: rising to open, falling to close.</summary>
        public static float[] Whoosh(bool opening) => opening
            ? Synth.Resonant(0.16f, 711, 500f, 2200f, 1.5f, 0.1f, 20f)
            : Synth.Resonant(0.16f, 712, 2200f, 500f, 1.5f, 0.02f, 16f);
    }
}

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
    }
}

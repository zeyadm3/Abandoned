namespace Abandoned.Audio
{
    /// <summary>
    /// The building's rooms (0.16.1+): breakers, the lift, vents, alarms, the vault drill. Switches, motors
    /// and sheet metal, never a creak: a creak must only ever mean the floor.
    /// </summary>
    internal static class RoomPatches
    {
        /// <summary>A big breaker slammed home: the throw, the contactor's clack, then the mains hum swelling up.</summary>
        public static float[] Breaker(int take)
        {
            float[] d = Synth.Buffer(2.2f);
            var rng = new Rng(1611 + take * 17);
            Patch.Layer(d, Synth.Thump(0.35f, rng.Range(170f, 200f), 70f, 14f), 0.9f);
            Patch.Layer(d, Synth.Modal(0.4f, rng.Range(260f, 300f), Patch.Sheet, null, 12f), 0.55f);
            Patch.Layer(d, Synth.Modal(0.08f, 1800f, Patch.Bar, null, 70f), 0.4f, 0.05f);
            // 50 Hz mains and its buzz rising as the circuits take the load.
            float[] hum = Synth.Sweep(1.9f, 50f, 50f, 0.5f, 0.6f, 1);
            Patch.Layer(d, Synth.Fade(hum, 0.4f, 0.5f), 0.45f, 0.25f);
            Patch.Layer(d, Synth.Fade(Synth.Resonant(1.8f, 1612 + take, 100f, 100f, 6f, 0.4f, 0.8f), 0.4f, 0.5f), 0.3f, 0.3f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.3f), 1.2f, 0.25f);
        }
    }
}

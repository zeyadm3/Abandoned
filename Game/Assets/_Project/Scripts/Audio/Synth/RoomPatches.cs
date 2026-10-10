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

        /// <summary>The lift's motor winding up: a contactor clack, then a low geared whine that settles to a drone.</summary>
        public static float[] LiftStart()
        {
            float[] d = Synth.Buffer(3f);
            Patch.Layer(d, Synth.Modal(0.1f, 1500f, Patch.Bar, null, 60f), 0.5f);
            Patch.Layer(d, Synth.Fade(Synth.Sweep(2.9f, 40f, 92f, 0.6f, 0.3f, 1, 0.5f), 0.3f, 0.6f), 0.7f, 0.08f);
            Patch.Layer(d, Synth.Fade(Synth.Sweep(2.9f, 180f, 410f, 0.6f, 0.3f, 0, 0.5f), 0.4f, 0.6f), 0.25f, 0.1f);
            Patch.Layer(d, Synth.Fade(Synth.Noise(2.9f, 1631, 600f, 900f, 0.4f, 0.3f), 0.4f, 0.6f), 0.3f, 0.1f);
            return SynthReverb.Apply(d, 1.2f, 0.2f);
        }

        /// <summary>The lift arriving: the motor dying away and the brake shoes biting.</summary>
        public static float[] LiftArrive(int take)
        {
            float[] d = Synth.Buffer(1.6f);
            Patch.Layer(d, Synth.Fade(Synth.Sweep(1f, 90f, 35f, 0.01f, 3f, 1), 0f, 0.3f), 0.6f);
            Patch.Layer(d, Synth.Thump(0.4f, 150f + take * 10f, 60f, 12f), 0.85f, 0.5f);
            Patch.Layer(d, Synth.Modal(0.6f, 210f, Patch.Sheet, null, 9f), 0.45f, 0.5f);
            return SynthReverb.Apply(d, 1f, 0.25f);
        }

        /// <summary>A lift cable letting go: the whip-crack, the shriek of the guides, the cage dropping away.</summary>
        public static float[] LiftSnap()
        {
            float[] d = Synth.Buffer(2.4f);
            Patch.Layer(d, Synth.Noise(0.15f, 1641, 9000f, 2000f, 0.001f, 30f), 0.95f);
            Patch.Layer(d, Synth.Modal(1.2f, 410f, Patch.Bar, null, 4f), 0.6f, 0.02f);
            Patch.Layer(d, Synth.Fade(Synth.Sweep(1.6f, 2300f, 900f, 0.02f, 1.2f, 2), 0.02f, 0.4f), 0.35f, 0.15f);
            Patch.Pings(d, 1642, 6, 0.1f, 0.8f, 900f, 2600f, Patch.Bar, 40f, 0.3f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.4f), 1.6f, 0.3f);
        }
    }
}

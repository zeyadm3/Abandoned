namespace Abandoned.Audio
{
    /// <summary>Gear from 0.20 on: the lockpick, thermal camera, drone and the truck's winch.</summary>
    internal static class GearPatches2
    {
        /// <summary>Picks working in a padlock: tiny metal ticks and a scratch.</summary>
        public static float[] Lockpick(int take)
        {
            float[] d = Synth.Buffer(1.2f);
            Patch.Pings(d, 2001 + take, 10, 0f, 1.1f, 2800f, 5200f, Patch.Bar, 120f, 0.35f, 1f);
            Patch.Layer(d, Synth.Fade(Synth.Noise(0.8f, 2002 + take, 7000f, 6000f, 0.05f, 2f, 2500f), 0.05f, 0.2f), 0.12f, 0.2f);
            return d;
        }

        /// <summary>A charger's soft electronic tick.</summary>
        public static float[] ChargeTick()
        {
            float[] d = Synth.Buffer(0.15f);
            Patch.Layer(d, Synth.Sweep(0.06f, 2200f, 2600f, 0.002f, 50f), 0.4f);
            return d;
        }

        /// <summary>A winch clutch dropping in: a clank and the cable snapping taut.</summary>
        public static float[] WinchStart()
        {
            float[] d = Synth.Buffer(0.9f);
            Patch.Layer(d, Synth.Thump(0.3f, 180f, 70f, 14f), 0.8f);
            Patch.Layer(d, Synth.Modal(0.5f, 330f, Patch.Bar, null, 10f), 0.4f);
            Patch.Layer(d, Synth.Sweep(0.3f, 600f, 1400f, 0.005f, 8f, 2), 0.25f, 0.15f);
            return SynthReverb.Apply(d, 0.9f, 0.2f);
        }

        /// <summary>The drum turning: a geared motor and the ratchet's clicks.</summary>
        public static float[] WinchRun(int take)
        {
            float[] d = Synth.Buffer(1f);
            Patch.Layer(d, Synth.Fade(Synth.Sweep(1f, 95f + take * 4f, 95f, 0.05f, 0.1f, 2), 0.05f, 0.05f), 0.5f);
            for (int i = 0; i < 8; i++) Patch.Layer(d, Synth.Modal(0.05f, 2100f, Patch.Bar, null, 90f), 0.25f, i * 0.12f);
            return d;
        }
    }
}

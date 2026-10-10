using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Sliding doors and steel lockers (0.16.0). Rollers, thuds and sheet metal, never a creak: a creak
    /// must only ever mean the floor.
    /// </summary>
    internal static class DoorPatches
    {
        /// <summary>
        /// A heavy leaf rolling on its track, then the stop: soft against the buffer when it opens, harder
        /// with the latch dropping when it shuts.
        /// </summary>
        public static float[] Slide(int take, bool shut)
        {
            float roll = shut ? 0.5f : 0.55f;
            float[] d = Synth.Buffer(roll + 0.7f);
            int seed = 1601 + take * 7 + (shut ? 100 : 0);
            // The rollers: a low rumble and the track's faint ring.
            Patch.Layer(d, Synth.Fade(Synth.Noise(roll, seed, 420f, 260f, 0.05f, 0.4f), 0.04f, 0.08f), 0.55f);
            Patch.Layer(d, Synth.Fade(Synth.Resonant(roll, seed + 1, 620f, 480f, 3f, 0.08f, 0.6f), 0.05f, 0.1f), 0.3f);
            // The stop.
            Patch.Layer(d, Synth.Thump(0.3f, shut ? 150f : 120f, 55f, shut ? 12f : 16f), shut ? 0.95f : 0.6f, roll);
            Patch.Layer(d, Synth.Modal(0.5f, shut ? 190f : 240f, Patch.Sheet, null, shut ? 9f : 14f), shut ? 0.5f : 0.3f, roll);
            if (shut) Patch.Layer(d, Synth.Modal(0.06f, 1500f, Patch.Bar, null, 80f), 0.3f, roll + 0.04f);
            return SynthReverb.Apply(d, 0.9f, 0.2f);
        }

        /// <summary>Something very heavy hitting the shut door: the blow, the sheet booming, the leaves jumping on their track.</summary>
        public static float[] Bash(int take)
        {
            float[] d = Synth.Buffer(1.5f);
            var rng = new Rng(1621 + take * 13);
            Patch.Layer(d, Synth.Thump(0.45f, rng.Range(110f, 135f), 40f, 8f), 0.95f);
            Patch.Layer(d, Synth.Modal(1f, rng.Range(96f, 120f), Patch.Sheet, null, 4.5f), 0.6f);
            Patch.Layer(d, Synth.Noise(0.22f, 1622 + take, 4200f, 500f, 0.001f, 22f), 0.5f);
            for (int i = 0; i < 4; i++)
                Patch.Layer(d, Synth.Modal(0.08f, rng.Range(300f, 420f), Patch.Sheet, null, 28f), rng.Range(0.2f, 0.4f), 0.07f + 0.045f * i);
            return SynthReverb.Apply(Patch.Heavy(d, 1.5f), 1.5f, 0.3f);
        }

        /// <summary>The door giving way: the last blow, the leaves torn off their track and thrown aside, fixings scattering.</summary>
        public static float[] Break()
        {
            float[] d = Synth.Buffer(2.4f);
            Patch.Layer(d, Synth.Thump(0.6f, 125f, 34f, 6f), 0.95f);
            Patch.Layer(d, Synth.Modal(1.6f, 88f, Patch.Sheet, null, 3f), 0.7f);
            Patch.Layer(d, Synth.Noise(0.5f, 1641, 6500f, 700f, 0.001f, 9f), 0.65f);
            Patch.Layer(d, Synth.Thump(0.4f, 140f, 50f, 10f), 0.7f, 0.42f);
            Patch.Layer(d, Synth.Modal(0.9f, 150f, Patch.Sheet, null, 6f), 0.5f, 0.42f);
            Patch.Pings(d, 1642, 7, 0.3f, 1.2f, 1400f, 3200f, Patch.Bar, 45f, 0.3f);
            Patch.Debris(d, 1643, 10, 0.2f, 1.4f, 3800f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.6f), 1.8f, 0.33f);
        }

        /// <summary>A thin steel locker door: the latch and the sheet's small ring; shutting adds the clap of the door on its frame.</summary>
        public static float[] Locker(int take, bool shut)
        {
            float[] d = Synth.Buffer(0.6f);
            var rng = new Rng(1661 + take * 5 + (shut ? 50 : 0));
            Patch.Layer(d, Synth.Modal(0.05f, rng.Range(1700f, 2100f), Patch.Bar, null, 90f), 0.5f);
            if (shut)
            {
                Patch.Layer(d, Synth.Thump(0.12f, 230f, 110f, 30f), 0.7f, 0.03f);
                Patch.Layer(d, Synth.Modal(0.4f, rng.Range(330f, 390f), Patch.Sheet, null, 16f), 0.6f, 0.03f);
                Patch.Layer(d, Synth.Noise(0.06f, 1662 + take, 5000f, 1200f, 0.001f, 50f), 0.4f, 0.03f);
            }
            else
            {
                Patch.Layer(d, Synth.Modal(0.3f, rng.Range(420f, 500f), Patch.Sheet, null, 22f), 0.4f, 0.04f);
                Patch.Layer(d, Synth.Noise(0.12f, 1663 + take, 2600f, 900f, 0.01f, 24f), 0.25f, 0.05f);
            }
            return d;
        }
    }
}

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
    }
}

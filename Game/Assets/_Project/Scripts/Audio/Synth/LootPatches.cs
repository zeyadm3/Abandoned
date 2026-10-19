namespace Abandoned.Audio
{
    /// <summary>Loot and money (0.18+): quirks, carts, sets and the HQ catalogue.</summary>
    internal static class LootPatches
    {
        /// <summary>Loose parts shaken about inside something.</summary>
        public static float[] Rattle(int take)
        {
            float[] d = Synth.Buffer(0.35f);
            Patch.Pings(d, 1801 + take, 7, 0f, 0.25f, 900f, 2600f, Patch.Bar, 60f, 0.5f, 1f);
            Patch.Debris(d, 1802 + take, 5, 0f, 0.25f, 4000f);
            return d;
        }

        /// <summary>A drip landing in a small puddle.</summary>
        public static float[] Drip(int take)
        {
            float[] d = Synth.Buffer(0.3f);
            Patch.Layer(d, Synth.Sweep(0.08f, 900f + take * 120f, 1900f, 0.002f, 40f), 0.5f);
            Patch.Layer(d, Synth.Noise(0.04f, 1803 + take, 6000f, 3000f, 0.001f, 80f), 0.15f);
            return d;
        }

        /// <summary>An old bell telephone or alarm clock going off: a bright double ring.</summary>
        public static float[] Ring(int take)
        {
            float[] d = Synth.Buffer(1f);
            for (int i = 0; i < 16; i++)
                Patch.Layer(d, Synth.Modal(0.12f, 1650f + (i % 2) * 40f, Patch.Bar, null, 30f), 0.45f, i * 0.025f + (i >= 8 ? 0.3f : 0f));
            return d;
        }

        /// <summary>Hot iron or fresh slag hissing and sizzling when handled.</summary>
        public static float[] Sizzle(int take)
        {
            float[] d = Synth.Buffer(0.45f);
            Patch.Layer(d, Synth.Noise(0.4f, 1820 + take, 4500f, 2500f, 0.01f, 15f), 0.6f);
            Patch.Pings(d, 1821 + take, 6, 0.05f, 0.35f, 2200f, 4800f, Patch.Bar, 90f, 0.2f, 0.4f);
            return d;
        }

        /// <summary>Liquid sloshing inside a heavy container or metal drum.</summary>
        public static float[] Slosh(int take)
        {
            float[] d = Synth.Buffer(0.4f);
            Patch.Layer(d, Synth.Sweep(0.25f, 140f + take * 30f, 90f, 0.05f, 12f), 0.7f);
            Patch.Layer(d, Synth.Noise(0.3f, 1830 + take, 1200f, 900f, 0.02f, 25f), 0.45f);
            return d;
        }
    }
}

namespace Abandoned.Audio
{
    /// <summary>
    /// Structure and hazard gear (0.17+): the sledgehammer, crash mat, live water, chalk, glow sticks and
    /// flares. None of these is a creak: a creak only ever means the floor.
    /// </summary>
    internal static class HazardPatches
    {
        /// <summary>A sledgehammer blow into concrete: the crack of the head, the slab's dull boom, grit.</summary>
        public static float[] SledgeHit(int take)
        {
            float[] d = Synth.Buffer(1.1f);
            var rng = new Rng(1701 + take * 13);
            Patch.Layer(d, Synth.Noise(0.08f, 1702 + take, 7000f, 2500f, 0.001f, 40f), 0.8f);
            Patch.Layer(d, Synth.Thump(0.5f, rng.Range(110f, 130f), 45f, 9f), 0.95f);
            Patch.Layer(d, Synth.Modal(0.4f, rng.Range(300f, 360f), Patch.Hollow, null, 12f), 0.35f);
            Patch.Debris(d, 1703 + take, 8, 0.05f, 0.6f, 3500f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.3f), 1.1f, 0.25f);
        }

        /// <summary>A heavy swing through air.</summary>
        public static float[] SledgeSwing(int take)
        {
            float[] d = Synth.Buffer(0.45f);
            Patch.Layer(d, Synth.Resonant(0.4f, 1704 + take, 300f, 900f, 2f, 0.15f, 6f), 0.6f);
            return d;
        }

        /// <summary>Stressed glass: dry ticks running into a long spidering crackle.</summary>
        public static float[] GlassCrack(int take)
        {
            float[] d = Synth.Buffer(1.4f);
            Patch.Pings(d, 1711 + take, 14, 0f, 1.1f, 2600f, 6200f, Patch.Glass, 70f, 0.45f, 0.8f);
            Patch.Layer(d, Synth.Fade(Synth.Noise(1.1f, 1712 + take, 9000f, 5000f, 0.3f, 2f, 2500f), 0.2f, 0.3f), 0.25f, 0.15f);
            return SynthReverb.Apply(d, 0.9f, 0.2f);
        }

        /// <summary>A glass floor letting go: the crack through, then a shower of panes.</summary>
        public static float[] GlassFail(int take)
        {
            float[] d = Synth.Buffer(2f);
            Patch.Layer(d, Synth.Noise(0.12f, 1713 + take, 11000f, 4000f, 0.001f, 30f), 0.9f);
            Patch.Layer(d, Synth.Modal(0.8f, 1800f, Patch.Glass, null, 6f), 0.5f);
            Patch.Pings(d, 1714 + take, 30, 0.05f, 1.4f, 2000f, 7000f, Patch.Glass, 50f, 0.5f);
            return SynthReverb.Apply(d, 1.2f, 0.25f);
        }

        /// <summary>Floorboards splitting along the grain: a dry rip and a knock (not a creak).</summary>
        public static float[] TimberCrack(int take)
        {
            float[] d = Synth.Buffer(1f);
            Patch.Layer(d, Synth.Noise(0.35f, 1715 + take, 4200f, 1500f, 0.005f, 9f, 400f), 0.7f);
            Patch.Layer(d, Synth.Modal(0.4f, 210f + take * 15f, Patch.Wood, null, 14f), 0.6f, 0.02f);
            Patch.Layer(d, Synth.Modal(0.3f, 330f, Patch.Wood, null, 20f), 0.4f, 0.25f);
            return SynthReverb.Apply(d, 0.8f, 0.2f);
        }

        /// <summary>Boards and joists breaking through.</summary>
        public static float[] TimberFail(int take)
        {
            float[] d = Synth.Buffer(1.8f);
            Patch.Layer(d, Synth.Noise(0.5f, 1716 + take, 5000f, 1200f, 0.002f, 6f, 300f), 0.8f);
            Patch.Layer(d, Synth.Thump(0.5f, 140f, 55f, 8f), 0.9f, 0.05f);
            for (int i = 0; i < 4; i++) Patch.Layer(d, Synth.Modal(0.3f, 180f + i * 40f, Patch.Wood, null, 16f), 0.5f, 0.1f + i * 0.18f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.3f), 1.1f, 0.25f);
        }

        /// <summary>Ceramic tiles lifting off their bed: sharp pops, one after another.</summary>
        public static float[] TileCrack(int take)
        {
            float[] d = Synth.Buffer(1.1f);
            var rng = new Rng(1717 + take);
            for (int i = 0; i < 5; i++)
            {
                float at = i * rng.Range(0.12f, 0.22f);
                Patch.Layer(d, Synth.Noise(0.05f, 1718 + take * 7 + i, 8000f, 3500f, 0.001f, 60f), rng.Range(0.4f, 0.8f), at);
                Patch.Layer(d, Synth.Modal(0.12f, rng.Range(1400f, 2200f), Patch.Hollow, null, 40f), 0.3f, at);
            }
            return SynthReverb.Apply(d, 0.7f, 0.2f);
        }

        /// <summary>A tiled slab buckling: the bed cracking under a run of popping tiles.</summary>
        public static float[] TileFail(int take)
        {
            float[] d = Synth.Buffer(1.8f);
            Patch.Layer(d, Synth.Thump(0.5f, 130f, 50f, 8f), 0.9f);
            Patch.Debris(d, 1719 + take, 16, 0.02f, 1.2f, 6000f);
            Patch.Pings(d, 1720 + take, 10, 0f, 1f, 1300f, 2400f, Patch.Hollow, 35f, 0.35f);
            return SynthReverb.Apply(Patch.Heavy(d, 1.2f), 1f, 0.25f);
        }
    }
}

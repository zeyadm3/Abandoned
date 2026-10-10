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
    }
}

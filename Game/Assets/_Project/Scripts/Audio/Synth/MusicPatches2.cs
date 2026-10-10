namespace Abandoned.Audio
{
    /// <summary>
    /// The 0.24.1 music tracks, in code like the first ones (nothing to license): a menu theme of its own, and
    /// longer HQ and drive tracks with a second section, so a wait at the HQ doesn't loop every half minute.
    /// Jobs still have no music: only the song of whatever is coming for you.
    /// </summary>
    internal static class MusicPatches2
    {
        // D minor: Dm, B♭, F, C.
        private static readonly float[][] MenuChords =
        {
            new[] { 146.83f, 174.61f, 220f }, new[] { 116.54f, 146.83f, 174.61f },
            new[] { 130.81f, 174.61f, 220f }, new[] { 130.81f, 164.81f, 196f },
        };

        /// <summary>The menu: 12 bars at 45 bpm (64 s). Bowed chords, a slow bell tune, tape hiss: the company's waiting room.</summary>
        public static SynthSamples Menu()
        {
            const float bar = 16f / 3f;
            const int bars = 12;
            var score = new MusicScore(bars * bar, loop: true);
            for (int b = 0; b < bars; b++)
            {
                float start = b * bar;
                float[] c = MenuChords[b % 4];
                for (int n = 0; n < 3; n++) score.Add(MusicVoices.Bowed(c[n], bar + 1.2f, 0.18f + n * 0.05f, 1.2f, 1.4f), start - 0.6f, 0.07f, (n - 1) * 0.45f);
                score.Add(MusicVoices.Bass(c[0] * 0.5f, bar), start, 0.26f);
                if (b >= 4)
                {
                    float[] tune = { 587.33f, 523.25f, 440f, 466.16f };
                    score.Add(MusicVoices.Box(tune[b % 4], 3.5f), start + 1.33f, 0.1f, 0.2f);
                    if (b >= 8) score.Add(MusicVoices.Box(tune[(b + 2) % 4] * 0.75f, 3f), start + 3.3f, 0.07f, -0.25f);
                }
            }
            score.Add(MusicVoices.Tape(bars * bar, 2401), 0f, 0.035f);
            return score.Finish("Synth_MusicMenu2", 3f, 0.38f);
        }

        // A minor, then a B section: Dm, Am, E, Am.
        private static readonly float[][] HqA = { new[] { 220f, 261.63f, 329.63f }, new[] { 220f, 261.63f, 349.23f }, new[] { 196f, 261.63f, 329.63f }, new[] { 196f, 246.94f, 293.66f } };
        private static readonly float[][] HqB = { new[] { 146.83f, 174.61f, 220f }, new[] { 220f, 261.63f, 329.63f }, new[] { 164.81f, 207.65f, 246.94f }, new[] { 220f, 261.63f, 329.63f } };

        /// <summary>The HQ: 16 bars at 60 bpm (64 s), A then B; keys picking the chords, a bowed line in B.</summary>
        public static SynthSamples Hq()
        {
            const float bar = 4f;
            const int bars = 16;
            int[] arp = { 0, 1, 2, 1, 0, 2, 1, 2 };
            var score = new MusicScore(bars * bar, loop: true);
            for (int b = 0; b < bars; b++)
            {
                float start = b * bar;
                bool partB = b >= 8;
                float[] c = (partB ? HqB : HqA)[b % 4];
                for (int n = 0; n < 3; n++) score.Add(MusicVoices.Pad(c[n], bar + 1.4f), start - 0.4f, 0.1f, (n - 1) * 0.5f);
                for (int e = 0; e < 8; e++)
                {
                    if (!partB && (e == 1 || e == 4)) continue;
                    score.Add(MusicVoices.Keys(c[arp[e]] * 2f, 1.5f), start + e * 0.5f, 0.15f, e % 2 == 0 ? -0.3f : 0.3f);
                }
                score.Add(MusicVoices.Bass(c[0] * 0.5f, 2.2f), start, 0.32f);
                score.Add(MusicVoices.Bass(c[0] * 0.5f, 1.8f), start + 2f, 0.22f);
                if (partB) score.Add(MusicVoices.Bowed(c[2] * 2f, 3.6f, 4.5f, 0.6f, 0.8f), start + 0.2f, 0.06f, 0.1f);
            }
            score.Add(MusicVoices.Tape(bars * bar, 2402), 0f, 0.03f);
            return score.Finish("Synth_MusicHq2", 2.4f, 0.3f);
        }

        /// <summary>The drive: 16 bars at 96 bpm (40 s). A road groove: kick, a soft snare, a bass ostinato under muted keys.</summary>
        public static SynthSamples Drive()
        {
            const float beat = 60f / 96f, bar = 4f * beat;
            const int bars = 16;
            float[] roots = { 110f, 110f, 87.31f, 98f };
            var score = new MusicScore(bars * bar, loop: true);
            for (int b = 0; b < bars; b++)
            {
                float start = b * bar;
                float root = roots[(b / 2) % 4];
                for (int k = 0; k < 4; k++)
                {
                    float at = start + k * beat;
                    if (k % 2 == 0) score.Add(MusicVoices.Kick(), at, 0.32f);
                    else if (b >= 4) score.Add(MusicVoices.Snare(2410 + b * 4 + k), at, 0.1f, 0.1f);
                    score.Add(MusicVoices.Tick(2420 + b * 4 + k), at + beat * 0.5f, 0.05f, -0.2f);
                    score.Add(MusicVoices.Bass(k == 3 ? root * 1.5f : root, beat * 0.9f), at, 0.28f);
                }
                if (b >= 8) score.Add(MusicVoices.Keys(root * 4f, bar), start, 0.1f, 0.3f);
            }
            score.Add(MusicVoices.Air(bars * bar, 2430, 900f), 0f, 0.04f);
            return score.Finish("Synth_MusicDrive2", 1.6f, 0.22f);
        }
    }
}

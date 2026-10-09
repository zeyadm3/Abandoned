using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The game's own music (0.14.0), made in code: nothing needs licensing. A slow, tired A-minor loop for
    /// the HQ and menus, a muted pulse for the drive, a clock for the truck's departure, and short stings.
    /// Exploration has no music of its own: only the song of whatever is coming for you (ThreatThemePatches).
    /// </summary>
    internal static class MusicPatches
    {
        // A minor, F, C, G: the upper voices move as little as possible from chord to chord.
        private static readonly float[][] Chords =
        {
            new[] { 220f, 261.63f, 329.63f },
            new[] { 220f, 261.63f, 349.23f },
            new[] { 196f, 261.63f, 329.63f },
            new[] { 196f, 246.94f, 293.66f },
        };
        private static readonly float[] Roots = { 110f, 87.31f, 130.81f, 98f };
        private static readonly int[] Arp = { 0, 1, 2, 1, 0, 2, 1, 2 };

        /// <summary>The HQ theme: 8 bars at 60 bpm (32 s). The second pass is fuller and brings a tune.</summary>
        public static SynthSamples Hq()
        {
            const float bar = 4f;
            const int bars = 8;
            var score = new MusicScore(bars * bar, loop: true);
            for (int b = 0; b < bars; b++)
            {
                float start = b * bar;
                int c = b % 4;
                bool second = b >= 4;
                // Each chord fades in as the last one fades out.
                for (int n = 0; n < 3; n++) score.Add(MusicVoices.Pad(Chords[c][n], bar + 1.4f), start - 0.4f, 0.11f, (n - 1) * 0.5f);
                for (int e = 0; e < 8; e++)
                {
                    if (!second && (e == 1 || e == 4 || e == 7)) continue;
                    float hz = Chords[c][Arp[e]] * 2f * (second && e == 7 ? 1.5f : 1f);
                    score.Add(MusicVoices.Keys(hz, 1.6f), start + e * 0.5f, second ? 0.2f : 0.15f, e % 2 == 0 ? -0.3f : 0.3f);
                }
                score.Add(MusicVoices.Bass(Roots[c], 2.2f), start, 0.34f);
                score.Add(MusicVoices.Bass(Roots[c], 1.8f), start + 2f, 0.24f);
            }
            // A music box someone left running, only audible the second time round.
            (float at, float hz, float length)[] tune =
            {
                (18f, 659.25f, 2.5f), (20f, 523.25f, 2.5f), (22f, 440f, 2.5f),
                (24f, 392f, 2.5f), (26f, 659.25f, 2.5f), (28f, 587.33f, 3.5f),
            };
            foreach (var note in tune) score.Add(MusicVoices.Box(note.hz, note.length), note.at, 0.12f, 0.15f);
            score.Add(MusicVoices.Tape(bars * bar, 1001), 0f, 0.03f);
            return score.Finish("Synth_MusicHq", 2.4f, 0.3f);
        }

        /// <summary>The drive: 6 bars at 90 bpm (16 s). A muted bass pulse that leaves room for the engine and the crew.</summary>
        public static SynthSamples Drive()
        {
            const float beat = 2f / 3f, bar = 4f * beat;
            int[] order = { 0, 0, 1, 1, 2, 3 };
            var score = new MusicScore(order.Length * bar, loop: true);
            for (int b = 0; b < order.Length; b++)
            {
                float start = b * bar;
                int c = order[b];
                for (int n = 0; n < 3; n++) score.Add(MusicVoices.Pad(Chords[c][n], bar + 1f, 0.6f, 1f), start - 0.3f, 0.08f, (n - 1) * 0.6f);
                for (int e = 0; e < 8; e++)
                    score.Add(MusicVoices.Bass(Roots[c], 0.3f), start + e * beat * 0.5f, e % 4 == 0 ? 0.3f : 0.17f);
                // One note off the beat, like a thought you don't finish.
                score.Add(MusicVoices.Keys(Chords[c][b % 3] * 2f, 1.4f), start + beat * 1.5f, 0.14f, b % 2 == 0 ? 0.4f : -0.4f);
                score.Add(MusicVoices.Keys(Chords[c][(b + 2) % 3] * 2f, 1.2f), start + beat * 3.5f, 0.1f, b % 2 == 0 ? -0.4f : 0.4f);
            }
            return score.Finish("Synth_MusicDrive", 1.6f, 0.25f, 0.8f);
        }

        /// <summary>The departure: 16 beats at 120 bpm (8 s). A clock, and a chord that keeps leaning in.</summary>
        public static SynthSamples Departure()
        {
            const float beat = 0.5f;
            var score = new MusicScore(16 * beat, loop: true);
            for (int b = 0; b < 16; b++)
            {
                float at = b * beat;
                score.Add(MusicVoices.Tick(1011 + b % 2, b % 2 == 0 ? 9000f : 5000f), at, b % 2 == 0 ? 0.22f : 0.14f, b % 2 == 0 ? -0.2f : 0.2f);
                score.Add(MusicVoices.Kick(55f, 12f), at, b % 4 == 0 ? 0.5f : 0.3f);
                score.Add(MusicVoices.Keys(b % 4 == 3 ? 329.63f : 220f, 0.45f), at, 0.2f);
            }
            for (int half = 0; half < 2; half++)
            {
                // A and B flat together: it never settles.
                score.Add(MusicVoices.Pad(110f, 4.6f, 2.5f, 1f), half * 4f, 0.09f, -0.5f);
                score.Add(MusicVoices.Pad(116.54f, 4.6f, 2.5f, 1f), half * 4f, 0.07f, 0.5f);
            }
            return score.Finish("Synth_MusicDeparture", 1.2f, 0.2f, 0.8f);
        }

        public static SynthSamples Sting(SoundId id)
        {
            switch (id)
            {
                case SoundId.StingPayday: return Rise("Synth_StingPayday", new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.11f);
                case SoundId.StingLevelUp: return Rise("Synth_StingLevelUp", new[] { 392f, 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f }, 0.08f);
                case SoundId.StingQuotaMissed: return Missed();
                default: return Low();
            }
        }

        // Rising tines that ring together at the top.
        private static SynthSamples Rise(string name, float[] notes, float gap)
        {
            var score = new MusicScore(1.8f, loop: false);
            for (int i = 0; i < notes.Length; i++)
            {
                float pan = Mathf.Lerp(-0.5f, 0.5f, i / (float)(notes.Length - 1));
                score.Add(MusicVoices.Box(notes[i], 1.7f - i * gap), i * gap, 0.3f, pan);
                score.Add(MusicVoices.Keys(notes[i] * 0.5f, 1.2f), i * gap, 0.14f, -pan);
            }
            return score.Finish(name, 1.6f, 0.3f);
        }

        // Sad trombone: four notes sliding down, the last one wobbling (the comedy half of horror-comedy).
        private static SynthSamples Missed()
        {
            float[] notes = { 311.13f, 293.66f, 277.18f, 261.63f };
            var score = new MusicScore(2.7f, loop: false);
            for (int i = 0; i < 4; i++)
            {
                bool last = i == 3;
                float[] note = MusicVoices.Bowed(notes[i] * 1.03f, last ? 1.2f : 0.43f, last ? 6f : 0f, 0.05f, last ? 0.5f : 0.08f, notes[i] * (last ? 0.97f : 1f), 6);
                score.Add(Synth.LowPass(note, 1400f, 2, Synth.LoopRate), i * 0.45f, 0.3f);
            }
            return score.Finish("Synth_StingQuotaMissed", 1f, 0.2f);
        }

        // A low swell that doesn't resolve.
        private static SynthSamples Low()
        {
            var score = new MusicScore(3f, loop: false);
            score.Add(MusicVoices.Pad(55f, 3f, 0.6f, 2f), 0f, 0.3f, -0.4f);
            score.Add(MusicVoices.Pad(58.27f, 3f, 0.6f, 2f), 0f, 0.24f, 0.4f);
            score.Add(Synth.Fade(MusicVoices.Air(3f, 1021, 400f), 0.6f, 2f, Synth.LoopRate), 0f, 0.1f);
            return score.Finish("Synth_StingDeath", 2f, 0.35f);
        }
    }
}

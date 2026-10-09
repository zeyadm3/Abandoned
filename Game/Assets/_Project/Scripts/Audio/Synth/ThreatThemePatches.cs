using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Each monster's own song (2026-10-08, re-voiced for 0.14.0), in ThreatKind order. Short seamless
    /// loops: notes wrap round the end, and every held tone runs a whole number of cycles. The Blind One
    /// ticks over a drone, the Stalker is a bowed tremolo over a heartbeat, the Collector a tired music
    /// box, the Hunter war drums, the Weight a dirge, the Thing two clashing tones and glass, the Crawlers
    /// skittering, the Last Hunter all of it at once and faster.
    /// </summary>
    internal static class ThreatThemePatches
    {
        private const int Rate = Synth.LoopRate;

        public static SynthSamples Render(int kind) => kind switch
        {
            0 => BlindOne(),
            1 => Stalker(),
            2 => Collector(),
            3 => Hunter(),
            4 => Weight(),
            5 => Thing(),
            6 => Crawlers(),
            _ => LastHunter(),
        };

        // 80 bpm: a beating low drone, its click on every quarter of the beat, one wrong note a bar.
        private static SynthSamples BlindOne()
        {
            const float beat = 0.75f;
            var score = new MusicScore(16 * beat, loop: true);
            int n = score.Length;
            score.Add(MusicVoices.Drone(n, 55f), 0f, 0.3f, -0.3f);
            score.Add(MusicVoices.Drone(n, 55.6f), 0f, 0.24f, 0.3f);
            score.Add(MusicVoices.Breathe(MusicVoices.Drone(n, 110.3f), 3, 0.8f), 0f, 0.1f);
            for (int s = 0; s < 64; s++)
            {
                bool onBeat = s % 4 == 0;
                float[] tock = Synth.Modal(0.08f, onBeat ? 1100f : 1500f, Patch.Hollow, null, 90f, 0.3f, Rate);
                score.Add(tock, s * beat / 4f, onBeat ? 0.3f : 0.1f, s % 2 == 0 ? -0.4f : 0.4f);
            }
            for (int bar = 0; bar < 4; bar++)
            {
                score.Add(MusicVoices.Box(1760f, 2f), bar * beat * 4f, 0.1f, 0.5f);
                score.Add(MusicVoices.Box(1661.2f, 2f), bar * beat * 4f + 0.02f, 0.07f, -0.5f);
            }
            return score.Finish("Synth_ThemeBlindOne", 1.8f, 0.3f);
        }

        // 70 bpm: a minor second, bowed and trembling, swelling once a bar over a heartbeat that isn't yours.
        private static SynthSamples Stalker()
        {
            const float beat = 60f / 70f;
            var score = new MusicScore(16 * beat, loop: true);
            for (int bar = 0; bar < 4; bar++)
            {
                float start = bar * beat * 4f, length = beat * 4f + 0.6f;
                score.Add(MusicVoices.Bowed(110f, length, 11f, 1.6f, 1.6f), start, 0.1f, -0.5f);
                score.Add(MusicVoices.Bowed(116.54f, length, 11.6f, 1.6f, 1.6f), start, 0.09f, 0.5f);
                score.Add(MusicVoices.Bowed(164.81f, length, 10.4f, 1.9f, 1.6f, 0f, 5), start, 0.05f);
            }
            for (int b = 0; b < 16; b++)
            {
                score.Add(MusicVoices.Kick(38f, 7f), b * beat, 0.5f);
                score.Add(MusicVoices.Kick(38f, 8f), b * beat + 0.22f, 0.3f);
            }
            score.Add(MusicVoices.Air(16 * beat, 1101, 900f, 200f), 0f, 0.03f);
            return score.Finish("Synth_ThemeStalker", 2.2f, 0.35f);
        }

        // 100 bpm: a music box with a tired spring. Every tine a little off, some notes late, clockwork under it.
        private static SynthSamples Collector()
        {
            const float beat = 0.6f;
            float[] tune =
            {
                880f, 1046.5f, 1318.5f, 987.8f, 880f, 659.3f, 784f, 698.5f,
                880f, 1046.5f, 1318.5f, 1568f, 1396.9f, 1318.5f, 1046.5f, 987.8f,
            };
            var score = new MusicScore(tune.Length * beat, loop: true);
            var rng = new Rng(1201);
            for (int b = 0; b < tune.Length; b++)
            {
                score.Add(MusicVoices.Box(tune[b] * rng.Range(0.99f, 1.012f), 1.4f), b * beat + rng.Range(0f, 0.03f), 0.28f, rng.Range(-0.5f, 0.5f));
                if (b % 4 == 0) score.Add(MusicVoices.Box(tune[b] * 0.5f, 1.8f), b * beat, 0.16f);
                score.Add(MusicVoices.Tick(1202 + b % 3), b * beat, 0.1f, -0.6f);
                score.Add(MusicVoices.Tick(1205 + b % 3, 5000f), b * beat + beat * 0.5f, 0.06f, 0.6f);
            }
            int n = score.Length;
            score.Add(MusicVoices.Drone(n, 110f), 0f, 0.07f, -0.3f);
            score.Add(MusicVoices.Drone(n, 109.2f), 0f, 0.06f, 0.3f);
            return score.Finish("Synth_ThemeCollector", 1.6f, 0.3f);
        }

        // 120 bpm: war drums, low brass leaning into each bar, a crash on every bar line.
        private static SynthSamples Hunter()
        {
            const float beat = 0.5f, eighth = beat / 2f;
            bool[] kicks = { true, false, false, true, true, false, true, false };
            bool[] toms = { false, false, true, false, false, false, false, true };
            var score = new MusicScore(16 * beat, loop: true);
            for (int s = 0; s < 32; s++)
            {
                float at = s * eighth;
                bool firstHalf = s % 16 < 8;
                if (kicks[s % 8]) score.Add(MusicVoices.Kick(42f), at, 0.6f);
                if (toms[s % 8]) score.Add(MusicVoices.Tom(firstHalf ? 85f : 72f), at, 0.4f, firstHalf ? -0.4f : 0.4f);
            }
            for (int bar = 0; bar < 4; bar++)
            {
                float start = bar * beat * 4f;
                score.Add(Synth.LowPass(MusicVoices.Bowed(73.42f, 2.3f, 0f, 1.2f, 0.6f), 600f, 2, Rate), start, 0.16f, -0.3f);
                score.Add(Synth.LowPass(MusicVoices.Bowed(110f, 2.3f, 0f, 1.4f, 0.6f, 0f, 6), 700f, 2, Rate), start, 0.08f, 0.3f);
                score.Add(Synth.Normalize(Synth.Noise(0.9f, 1301 + bar, 9000f, 3000f, 0.002f, 4f, 2000f, Rate), 1f), start, 0.12f);
            }
            return score.Finish("Synth_ThemeHunter", 1.4f, 0.25f);
        }

        // 40 bpm: a dirge near the floor of hearing, a slow boom every other beat, the building complaining under it.
        private static SynthSamples Weight()
        {
            const float beat = 1.5f;
            var score = new MusicScore(8 * beat, loop: true);
            int n = score.Length;
            score.Add(MusicVoices.Breathe(MusicVoices.Drone(n, 36.71f), 4, 0.6f), 0f, 0.42f);
            score.Add(MusicVoices.Breathe(MusicVoices.Drone(n, 73.42f, 3, 1.4f), 4, 0.6f), 0f, 0.16f);
            for (int pair = 0; pair < 4; pair++)
            {
                float start = pair * beat * 2f;
                score.Add(Synth.Thump(1.6f, 60f, 30f, 2.5f, 20f, Rate), start, 0.6f);
                float[] grind = MusicVoices.FromEffects(StructurePatches.Strain(2.6f, 1401 + pair, 26f, 34f, 100f, 180f, 0.3f));
                score.Add(Synth.Normalize(grind, 1f), start + 0.3f, 0.2f, pair % 2 == 0 ? -0.5f : 0.5f);
            }
            return score.Finish("Synth_ThemeWeight", 2.6f, 0.35f);
        }

        // 90 bpm: two tones a few hertz apart beating for the whole loop, a glass note a beat, a bell arriving backwards.
        private static SynthSamples Thing()
        {
            const float beat = 2f / 3f;
            float[] glass = { 1244.5f, 1318.5f, 1396.9f, 1174.7f };
            var score = new MusicScore(16 * beat, loop: true);
            int n = score.Length;
            score.Add(MusicVoices.Drone(n, 390f), 0f, 0.13f, -0.6f);
            score.Add(MusicVoices.Drone(n, 393.75f), 0f, 0.13f, 0.6f);
            for (int b = 0; b < 16; b++)
            {
                float hz = glass[(b * 7 + 3) % glass.Length];
                score.Add(Synth.Modal(1.6f, hz, Patch.Glass, null, 2.6f, 0.5f, Rate), b * beat, 0.09f, b % 2 == 0 ? -0.4f : 0.4f);
                score.Add(MusicVoices.Kick(49f, 6f), b * beat, 0.35f);
            }
            for (int bar = 0; bar < 4; bar++)
            {
                float[] bell = Synth.Modal(1.2f, glass[bar] * 0.5f, Patch.Glass, null, 3f, 0.5f, Rate);
                System.Array.Reverse(bell);
                score.Add(Synth.Fade(bell, 0.01f, 0.004f, Rate), (bar + 1) * beat * 4f - 1.2f, 0.12f);
            }
            return score.Finish("Synth_ThemeThing", 2.2f, 0.4f);
        }

        // 140 bpm: sixteenths with random accents and doubles (too many legs to count), a thin squeal, a low drone.
        private static SynthSamples Crawlers()
        {
            const float step = 60f / 140f / 4f;
            var score = new MusicScore(64 * step, loop: true);
            var rng = new Rng(1601);
            for (int s = 0; s < 64; s++)
            {
                float accent = rng.Uni() > 0.4f ? 1f : 0.3f;
                score.Add(MusicVoices.Tick(1602 + s % 5, 10000f), s * step, 0.3f * accent, rng.Range(-0.8f, 0.8f));
                if (rng.Uni() > 0.5f) score.Add(MusicVoices.Tick(1610 + s % 5, 7000f), s * step + step * 0.5f, 0.16f, rng.Range(-0.8f, 0.8f));
                if (s % 16 == 0) score.Add(MusicVoices.Kick(50f, 10f), s * step, 0.35f);
            }
            int n = score.Length;
            score.Add(MusicVoices.Breathe(MusicVoices.Drone(n, 2600f), 6, 0.9f), 0f, 0.035f, 0.5f);
            score.Add(MusicVoices.Drone(n, 82.41f, 6, 1.6f), 0f, 0.12f, -0.2f);
            return score.Finish("Synth_ThemeCrawlers", 1f, 0.2f);
        }

        // 150 bpm: a kick on every beat, a snare on two and four, a siren climbing through every bar, a wall of low brass.
        private static SynthSamples LastHunter()
        {
            const float beat = 0.4f;
            var score = new MusicScore(16 * beat, loop: true);
            for (int b = 0; b < 16; b++)
            {
                float at = b * beat;
                score.Add(MusicVoices.Kick(45f, 8f), at, 0.6f);
                if (b % 2 == 1) score.Add(MusicVoices.Snare(1701 + b % 4), at, 0.35f, 0.2f);
                if (b % 4 == 3) score.Add(MusicVoices.Tom(80f), at + beat * 0.5f, 0.35f, -0.4f);
            }
            for (int bar = 0; bar < 4; bar++)
            {
                float[] siren = MusicVoices.Bowed(220f, beat * 4f, 0f, 0.2f, 0.1f, 440f, 6);
                score.Add(Synth.LowPass(siren, 1800f, 2, Rate), bar * beat * 4f, 0.06f, bar % 2 == 0 ? -0.6f : 0.6f);
            }
            int n = score.Length;
            score.Add(MusicVoices.Drone(n, 55f, 8, 1.3f), 0f, 0.13f, -0.4f);
            score.Add(MusicVoices.Drone(n, 58.27f, 8, 1.3f), 0f, 0.11f, 0.4f);
            score.Add(MusicVoices.Drone(n, 82.41f, 6, 1.4f), 0f, 0.08f);
            return score.Finish("Synth_ThemeLastHunter", 1.2f, 0.22f);
        }
    }
}

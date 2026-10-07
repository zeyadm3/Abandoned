using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The game's music, made in code (M10.8; no CC0 track fits and nothing needs licensing): a slow,
    /// tired A-minor loop for the HQ and menus (pad, plucked arpeggio, a soft bass), and short stings
    /// for payday, a missed quota, a death and a level-up. Mono, 22 kHz, generated once on first use.
    /// </summary>
    public static class MusicSynth
    {
        private const int SampleRate = 22050;
        private const float Tau = 2f * Mathf.PI;

        // A minor - F - C - G, as chord tones (Hz), root first.
        private static readonly float[][] Chords =
        {
            new[] { 220.00f, 261.63f, 329.63f },
            new[] { 174.61f, 220.00f, 261.63f },
            new[] { 130.81f, 196.00f, 329.63f },
            new[] { 196.00f, 246.94f, 293.66f },
        };
        private static readonly int[] Arp = { 0, 1, 2, 1, 0, 2, 1, 2 };

        /// <summary>The HQ theme: 8 bars at 60 bpm (32 s), seamless when looped.</summary>
        public static AudioClip HqTheme()
        {
            const float beat = 1f, bar = 4f * beat;
            int bars = 8;
            float length = bars * bar;
            var random = new System.Random(11);
            float crackle = 0f;
            return Make("Music_HQ", length, true, t =>
            {
                int b = Mathf.FloorToInt(t / bar) % bars;
                float inBar = t - Mathf.Floor(t / bar) * bar;
                float[] now = Chords[b % 4], before = Chords[(b + 3) % 4];
                // Pad: the chord fades in over a bar's first second while the last one fades out.
                float rise = Mathf.Clamp01(inBar / 1.2f);
                float pad = Chord(now, t) * rise + Chord(before, t) * (1f - rise);
                pad *= 0.16f * (0.85f + 0.15f * Mathf.Sin(Tau * t / 8f));
                // Arpeggio in eighths, up an octave; quieter on the first pass, a fifth higher at times on the second.
                float eighth = beat / 2f;
                int step = Mathf.FloorToInt(t / eighth);
                float since = t - step * eighth;
                float note = now[Arp[step % Arp.Length]] * 2f * (b >= 4 && step % 8 == 7 ? 1.5f : 1f);
                float arp = Pluck(note, since, 4.5f) * (b < 4 ? 0.13f : 0.18f);
                // Bass on beats one and three.
                float half = 2f * beat;
                float sinceBass = inBar - Mathf.Floor(inBar / half) * half;
                float bass = Mathf.Sin(Tau * now[0] / 2f * t) * Mathf.Exp(-sinceBass * 1.8f) * 0.22f;
                // A little room noise, like an old tape.
                crackle = crackle * 0.97f + ((float)random.NextDouble() * 2f - 1f) * 0.03f;
                return Soft(pad + arp + bass + crackle * 0.05f);
            });
        }

        /// <summary>A muted pulse over the drive, leaving room for the engine and crew's voices.</summary>
        public static AudioClip TravelTheme() => Make("Music_Drive", 16f, true, t =>
        {
            float[] chord = Chords[Mathf.FloorToInt(t / 4f) % Chords.Length];
            float step = t % 0.75f;
            float bass = Mathf.Sin(Tau * chord[0] * 0.5f * t) * Mathf.Exp(-step * 5f) * 0.2f;
            float pad = Chord(chord, t) * 0.09f;
            float note = Pluck(chord[Mathf.FloorToInt(t / 0.75f) % 3], step, 5f) * 0.12f;
            return Soft(bass + pad + note);
        });

        /// <summary>A short repeating clock pulse only while the truck is preparing to leave.</summary>
        public static AudioClip DepartureTheme() => Make("Music_Departure", 8f, true, t =>
        {
            float step = t % 0.5f;
            int beat = Mathf.FloorToInt(t * 2f);
            float bass = Mathf.Sin(Tau * 55f * t) * Mathf.Exp(-step * 7f) * 0.22f;
            float note = Pluck(beat % 4 == 3 ? 329.63f : 220f, step, 9f) * 0.15f;
            float tick = Mathf.Sin(Tau * 1600f * step) * Mathf.Exp(-step * 90f) * 0.055f;
            return Soft(bass + note + tick);
        });

        public static AudioClip Sting(MusicSting sting) => sting switch
        {
            MusicSting.Payday => Arpeggio("Sting_Payday", new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.11f, 1.8f),
            MusicSting.LevelUp => Arpeggio("Sting_LevelUp", new[] { 392f, 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f }, 0.08f, 1.8f),
            MusicSting.QuotaMissed => Slide("Sting_Missed"),
            _ => Drone("Sting_Death"),
        };

        // Rising plucks that ring together at the end.
        private static AudioClip Arpeggio(string name, float[] notes, float gap, float seconds) =>
            Make(name, seconds, false, t =>
            {
                float v = 0f;
                for (int i = 0; i < notes.Length; i++)
                {
                    float since = t - i * gap;
                    if (since >= 0f) v += Pluck(notes[i], since, 2.2f) * 0.22f;
                }
                return Soft(v * Fade(t, seconds));
            });

        // Sad trombone: four notes sliding down, the last one wobbling (the comedy half of horror-comedy).
        private static AudioClip Slide(string name)
        {
            float[] notes = { 311.13f, 293.66f, 277.18f, 261.63f };
            float phase = 0f, seconds = 2.6f;
            return Make(name, seconds, false, t =>
            {
                int i = Mathf.Min(3, Mathf.FloorToInt(t / 0.45f));
                float f = notes[i] * (i == 3 ? 1f + 0.012f * Mathf.Sin(Tau * 6f * (t - 1.35f)) : 1f);
                phase += Tau * f / SampleRate;
                float tone = Mathf.Sin(phase) + 0.5f * Mathf.Sin(2f * phase) + 0.3f * Mathf.Sin(3f * phase);
                float since = i < 3 ? t - i * 0.45f : t - 1.35f;
                float env = Mathf.Clamp01(since / 0.05f) * (i < 3 ? Mathf.Exp(-since * 1.5f) : 1f);
                return Soft(tone * env * 0.18f * Fade(t, seconds));
            });
        }

        // A low, dissonant swell under a breath of noise.
        private static AudioClip Drone(string name)
        {
            var random = new System.Random(5);
            float noise = 0f, seconds = 3f;
            return Make(name, seconds, false, t =>
            {
                noise = noise * 0.98f + ((float)random.NextDouble() * 2f - 1f) * 0.02f;
                float swell = Mathf.Clamp01(t / 0.6f) * Mathf.Exp(-Mathf.Max(0f, t - 0.6f) * 0.9f);
                float tone = Mathf.Sin(Tau * 55f * t) + 0.8f * Mathf.Sin(Tau * 58.27f * t) + 0.4f * Mathf.Sin(Tau * 110f * t);
                return Soft((tone * 0.2f + noise * 1.5f) * swell);
            });
        }

        private static float Chord(float[] notes, float t)
        {
            float v = 0f;
            foreach (float f in notes) v += Mathf.Sin(Tau * f * t) + 0.35f * Mathf.Sin(Tau * f * 1.003f * t);
            return v / notes.Length;
        }

        private static float Pluck(float f, float since, float decay) =>
            since < 0f ? 0f : (Mathf.Sin(Tau * f * since) + 0.3f * Mathf.Sin(Tau * 2f * f * since) * Mathf.Exp(-since * 6f)) * Mathf.Exp(-since * decay);

        private static float Fade(float t, float seconds) => Mathf.Clamp01((seconds - t) / 0.3f);

        private static float Soft(float x) => x / (1f + Mathf.Abs(x));

        private static AudioClip Make(string name, float seconds, bool loop, System.Func<float, float> sample)
        {
            int count = Mathf.CeilToInt(seconds * SampleRate);
            var data = new float[count];
            for (int i = 0; i < count; i++) data[i] = sample(i / (float)SampleRate);
            if (loop)
            {
                // The loop is whole bars, so the seam only needs a few milliseconds of smoothing.
                int edge = SampleRate / 50;
                for (int i = 0; i < edge; i++)
                {
                    float k = i / (float)edge;
                    data[i] *= k;
                    data[count - 1 - i] *= k;
                }
            }
            AudioClip clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}

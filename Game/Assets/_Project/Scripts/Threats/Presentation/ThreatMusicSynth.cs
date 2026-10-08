using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// Each monster's own song (2026-10-08), made in code like the HQ theme: short seamless loops (every tone
    /// is snapped to a whole number of cycles per loop), mono 22 kHz, built once per kind on first use.
    /// The Blind One ticks over a drone, the Stalker is a bowed tremolo over a heartbeat, the Collector a
    /// detuned music box, the Hunter war drums, the Weight a sub-bass dirge, the Thing two clashing tones and
    /// glass, the Crawlers skittering percussion, the Last Hunter all of it at once and faster.
    /// </summary>
    public static class ThreatMusicSynth
    {
        private const int Rate = 22050;
        private const float Tau = 2f * Mathf.PI;

        private static readonly Dictionary<ThreatKind, AudioClip> Clips = new();

        public static bool Has(ThreatKind kind) => Clips.TryGetValue(kind, out AudioClip c) && c != null;

        public static AudioClip Theme(ThreatKind kind)
        {
            if (Clips.TryGetValue(kind, out AudioClip clip) && clip != null) return clip;
            clip = kind switch
            {
                ThreatKind.Stalker => Stalker(),
                ThreatKind.Collector => Collector(),
                ThreatKind.Hunter => Hunter(),
                ThreatKind.Weight => Weight(),
                ThreatKind.Thing => Thing(),
                ThreatKind.Crawlers => Crawlers(),
                ThreatKind.LastHunter => LastHunter(),
                _ => BlindOne(),
            };
            Clips[kind] = clip;
            return clip;
        }

        // 80 bpm: a beating low drone, clicks on every quarter beat, a dissonant ping each bar.
        private static AudioClip BlindOne()
        {
            const float beat = 0.75f, length = 16 * beat;
            return Make("Theme_BlindOne", length, (i, t) =>
            {
                float drone = 0.32f * Sin(Q(55f, length), t) + 0.24f * Sin(Q(55.6f, length), t)
                              + 0.12f * Sin(Q(110.3f, length), t) * (0.5f + 0.5f * Sin(Q(0.25f, length), t));
                float step = beat / 4f, ts = Mathf.Repeat(t, step);
                int s = Mathf.FloorToInt(t / step);
                float click = Noise(i) * Mathf.Exp(-ts * 180f) * (s % 4 == 0 ? 0.5f : 0.2f);
                float bar = Mathf.Repeat(t, beat * 4f);
                float ping = (0.18f * Sin(Q(1760f, length), t) + 0.1f * Sin(Q(1661f, length), t)) * Mathf.Exp(-bar * 2.5f);
                return drone + click + ping;
            });
        }

        // 70 bpm: a bowed minor-second cluster trembling fast, swelling every bar, over a heartbeat.
        private static AudioClip Stalker()
        {
            const float beat = 60f / 70f, length = 16 * beat;
            float air = 0f;
            return Make("Theme_Stalker", length, (i, t) =>
            {
                float cluster = 0.18f * Saw(Q(110f, length), t) + 0.16f * Saw(Q(116.5f, length), t) + 0.1f * Saw(Q(164.8f, length), t);
                float tremolo = 0.55f + 0.45f * Sin(Q(11f, length), t);
                float swell = Mathf.Sin(Mathf.PI * Mathf.Repeat(t / (beat * 4f), 1f));
                float tb = Mathf.Repeat(t, beat);
                float heart = Kick(tb, 38f, 7f) * 0.55f + Kick(tb - 0.22f, 38f, 8f) * 0.35f;
                air += (Noise(i) - air) * 0.04f;
                return cluster * tremolo * (0.35f + 0.65f * swell * swell) + heart + air * 0.25f;
            });
        }

        // 100 bpm: a wobbling, out-of-tune music box tune in A minor with clockwork ticking.
        private static AudioClip Collector()
        {
            const float beat = 0.6f, length = 16 * beat;
            float[] tune = { 880f, 1046.5f, 1318.5f, 987.8f, 880f, 659.3f, 784f, 698.5f, 880f, 1046.5f, 1318.5f, 1568f, 1396.9f, 1318.5f, 1046.5f, 987.8f };
            return Make("Theme_Collector", length, (i, t) =>
            {
                int b = Mathf.FloorToInt(t / beat) % tune.Length;
                float tb = Mathf.Repeat(t, beat);
                float wobble = 1f + 0.012f * Sin(Q(0.3125f, length), t);
                float note = Box(tune[b] * wobble, tb) + 0.6f * Box(tune[(b + tune.Length - 1) % tune.Length] * wobble, tb + beat);
                float half = Mathf.Repeat(t, beat / 2f);
                float tick = Noise(i) * Mathf.Exp(-half * 300f) * 0.1f;
                float drone = 0.08f * Sin(Q(110f, length), t) + 0.06f * Sin(Q(109.2f, length), t);
                return note * 0.35f + tick + drone;
            });
        }

        // 120 bpm: war drums (kicks and toms), a low brass drone that swells each bar, a crash per bar.
        private static AudioClip Hunter()
        {
            const float beat = 0.5f, length = 16 * beat, eighth = beat / 2f;
            bool[] kicks = { true, false, false, true, true, false, true, false };
            bool[] toms = { false, false, true, false, false, false, false, true };
            float hiss = 0f;
            return Make("Theme_Hunter", length, (i, t) =>
            {
                int s = Mathf.FloorToInt(t / eighth) % 8;
                float ts = Mathf.Repeat(t, eighth), bar = Mathf.Repeat(t, eighth * 8f);
                float drums = (kicks[s] ? Kick(ts, 42f, 9f) * 0.8f : 0f) + (toms[s] ? Kick(ts, 85f, 10f) * 0.5f : 0f);
                float brass = (0.16f * Saw(Q(73.4f, length), t) + 0.08f * Saw(Q(110f, length), t)) * (0.4f + 0.6f * Mathf.Sin(Mathf.PI * bar / (eighth * 8f)));
                hiss += (Noise(i) - hiss) * 0.5f;
                return drums + brass + hiss * Mathf.Exp(-bar * 3f) * 0.18f;
            });
        }

        // 40 bpm: a sub-bass dirge, a slow boom every other beat and grinding, groaning noise.
        private static AudioClip Weight()
        {
            const float beat = 1.5f, length = 8 * beat;
            float grind = 0f;
            return Make("Theme_Weight", length, (i, t) =>
            {
                float pair = Mathf.Repeat(t, beat * 2f);
                float sub = (0.45f * Sin(Q(36.7f, length), t) + 0.2f * Sin(Q(73.4f, length), t)) * (0.4f + 0.6f * Mathf.Sin(Mathf.PI * pair / (beat * 2f)));
                grind += (Noise(i) - grind) * 0.02f;
                float groan = grind * 2.2f * (0.5f + 0.5f * Sin(Q(1f / 3f, length), t));
                return sub + Kick(pair, 30f, 2.5f) * 0.7f + groan;
            });
        }

        // 90 bpm: two tones a few hertz apart beating against each other, a glass note per beat, a low pulse.
        private static AudioClip Thing()
        {
            const float beat = 60f / 90f, length = 16 * beat;
            float[] glass = { 1244.5f, 1318.5f, 1396.9f, 1174.7f };
            return Make("Theme_Thing", length, (i, t) =>
            {
                float clash = 0.18f * Sin(Q(390f, length), t) + 0.18f * Sin(Q(393.75f, length), t);
                int b = Mathf.FloorToInt(t / beat);
                float tb = Mathf.Repeat(t, beat), f = glass[Mathf.Abs(b * 7 + 3) % glass.Length];
                float ring = (Mathf.Sin(Tau * f * tb) + 0.7f * Mathf.Sin(Tau * (f + 7f) * tb)) * Mathf.Exp(-tb * 1.5f) * 0.1f;
                float pulse = Sin(Q(49f, length), t) * Mathf.Exp(-tb * 5f) * 0.4f;
                return clash + ring + pulse;
            });
        }

        // 140 bpm: skittering sixteenth ticks with random accents and doubles, a thin squeal, a low drone.
        private static AudioClip Crawlers()
        {
            const float step = 60f / 140f / 4f, length = 64 * step;
            return Make("Theme_Crawlers", length, (i, t) =>
            {
                int s = Mathf.FloorToInt(t / step);
                float ts = Mathf.Repeat(t, step);
                float accent = Noise(s * 13 + 7) > 0.2f ? 1f : 0.3f;
                float ticks = Noise(i) * (Mathf.Exp(-ts * 220f) * accent + (Noise(s * 5 + 1) > 0.5f ? Mathf.Exp(-Mathf.Max(0f, ts - 0.05f) * 260f) * (ts > 0.05f ? 0.6f : 0f) : 0f)) * 0.4f;
                float squeal = 0.05f * Sin(Q(2600f, length), t) * (0.5f + 0.5f * Sin(Q(0.875f, length), t));
                return ticks + squeal + 0.1f * Saw(Q(82.4f, length), t);
            });
        }

        // 150 bpm: kick every beat, snare on two and four, a rising siren each bar, a dissonant brass wall.
        private static AudioClip LastHunter()
        {
            const float beat = 0.4f, length = 16 * beat;
            float phase = 0f, hiss = 0f;
            return Make("Theme_LastHunter", length, (i, t) =>
            {
                int b = Mathf.FloorToInt(t / beat);
                float tb = Mathf.Repeat(t, beat), bar = Mathf.Repeat(t, beat * 4f);
                hiss += (Noise(i) - hiss) * 0.6f;
                float drums = Kick(tb, 45f, 8f) * 0.8f + (b % 2 == 1 ? (hiss * 0.5f + Mathf.Sin(Tau * 180f * tb) * 0.3f) * Mathf.Exp(-tb * 14f) : 0f);
                if (bar < 1f / Rate) phase = 0f;
                phase += Mathf.Lerp(220f, 440f, bar / (beat * 4f)) / Rate;
                float siren = (Mathf.Repeat(phase, 1f) * 2f - 1f) * 0.08f;
                float wall = 0.15f * Saw(Q(55f, length), t) + 0.12f * Saw(Q(58.3f, length), t) + 0.1f * Saw(Q(82.4f, length), t);
                return drums + siren + wall;
            });
        }

        // ---- Helpers ----

        private static AudioClip Make(string name, float length, Func<int, float, float> sample)
        {
            int count = Mathf.RoundToInt(length * Rate);
            var data = new float[count];
            float peak = 0.0001f;
            for (int i = 0; i < count; i++)
            {
                data[i] = sample(i, i / (float)Rate);
                peak = Mathf.Max(peak, Mathf.Abs(data[i]));
            }
            float gain = 0.8f / peak;
            for (int i = 0; i < count; i++) data[i] *= gain;
            AudioClip clip = AudioClip.Create(name, count, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // A tone with a whole number of cycles in the loop, so the loop point never clicks.
        private static float Q(float hz, float length) => Mathf.Max(1f, Mathf.Round(hz * length)) / length;

        private static float Sin(float hz, float t) => Mathf.Sin(Tau * hz * t);

        // Band-limited saw: the first six harmonics.
        private static float Saw(float hz, float t)
        {
            float v = 0f;
            for (int k = 1; k <= 6; k++) v += Mathf.Sin(Tau * hz * k * t) / k;
            return v * 0.6f;
        }

        // A drum: a sine sweeping down onto its base pitch, dying away.
        private static float Kick(float t, float baseHz, float decay)
        {
            if (t < 0f) return 0f;
            float phase = baseHz * t + baseHz * 2f * (1f - Mathf.Exp(-t * 25f)) / 25f;
            return Mathf.Sin(Tau * phase) * Mathf.Exp(-t * decay);
        }

        // A plucked music box tine.
        private static float Box(float hz, float t) =>
            t < 0f ? 0f : (Mathf.Sin(Tau * hz * t) + 0.3f * Mathf.Sin(Tau * 2f * hz * t) + 0.15f * Mathf.Sin(Tau * 3.01f * hz * t)) * Mathf.Exp(-t * 4f);

        // Stateless white noise in [-1, 1] (the same every build).
        private static float Noise(int i)
        {
            unchecked
            {
                uint x = (uint)i * 747796405u + 2891336453u;
                x = ((x >> (int)((x >> 28) + 4u)) ^ x) * 277803737u;
                x = (x >> 22) ^ x;
                return x / 4294967295f * 2f - 1f;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetClips() => Clips.Clear();
    }
}

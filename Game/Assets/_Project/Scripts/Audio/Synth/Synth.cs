using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Offline sound-building blocks (0.14.0): every synthesised sound in the game is layered from these.
    /// Pure functions over float buffers, safe on a worker thread (nothing from Unity but Mathf's maths).
    /// A patch renders a few layers, levels each with <see cref="Normalize"/>, mixes them and adds a room.
    /// </summary>
    public static class Synth
    {
        public const int Rate = 44100;
        /// <summary>Long beds and music: less memory, and nothing above 16 kHz worth keeping.</summary>
        public const int LoopRate = 32000;
        public const float Tau = 2f * Mathf.PI;

        public static int Samples(float seconds, int rate = Rate) => Mathf.Max(1, Mathf.RoundToInt(seconds * rate));

        public static float[] Buffer(float seconds, int rate = Rate) => new float[Samples(seconds, rate)];

        /// <summary>Coefficient of a one-pole low-pass: y += (x - y) * a.</summary>
        public static float OnePole(float cutoffHz, int rate = Rate) => 1f - Mathf.Exp(-Tau * cutoffHz / rate);

        public static float SoftClip(float x) => x / (1f + Mathf.Abs(x));

        /// <summary>A strike: a linear rise over the attack, then an exponential fall.</summary>
        public static float Hit(float t, float attack, float decay) =>
            t < 0f ? 0f : t < attack ? t / attack : Mathf.Exp(-(t - attack) * decay);

        /// <summary>Held sound: fades in, holds, fades out by the end.</summary>
        public static float Gate(float t, float seconds, float fadeIn, float fadeOut) =>
            Mathf.Clamp01(t / fadeIn) * Mathf.Clamp01((seconds - t) / fadeOut);

        /// <summary>One slow rise and fall across the whole length.</summary>
        public static float Swell(float t, float seconds) => t <= 0f || t >= seconds ? 0f : Mathf.Sin(Mathf.PI * t / seconds);

        /// <summary>0 sine, 1 triangle, 2 saw, 3 square; phase in cycles.</summary>
        public static float Wave(int wave, float phase)
        {
            float p = phase - Mathf.Floor(phase);
            switch (wave)
            {
                case 1: return 4f * Mathf.Abs(p - 0.5f) - 1f;
                case 2: return 2f * p - 1f;
                case 3: return p < 0.5f ? 1f : -1f;
                default: return Mathf.Sin(Tau * p);
            }
        }

        // ---- Generators (each returns a new buffer) ----

        /// <summary>Noise through a low-pass that slides between two cutoffs: thuds, rumble, hiss, debris.</summary>
        public static float[] Noise(float seconds, int seed, float lowFrom, float lowTo, float attack, float decay,
            float highPass = 0f, int rate = Rate)
        {
            float[] d = Buffer(seconds, rate);
            var rng = new Rng(seed);
            float lp = 0f, lp2 = 0f, hp = 0f, a = OnePole(lowFrom, rate);
            float hpA = highPass > 0f ? OnePole(highPass, rate) : 0f;
            for (int i = 0; i < d.Length; i++)
            {
                if ((i & 31) == 0) a = OnePole(Mathf.Lerp(lowFrom, lowTo, i / (float)d.Length), rate);
                lp += (rng.Bi() - lp) * a;
                lp2 += (lp - lp2) * a;
                float x = lp2;
                if (hpA > 0f)
                {
                    hp += (x - hp) * hpA;
                    x -= hp;
                }
                d[i] = x * Hit(i / (float)rate, attack, decay);
            }
            return d;
        }

        /// <summary>Noise through a resonant band that slides: wind, breath, whooshes, voices without words.</summary>
        public static float[] Resonant(float seconds, int seed, float hzFrom, float hzTo, float q, float attack, float decay,
            int rate = Rate)
        {
            float[] d = Buffer(seconds, rate);
            var rng = new Rng(seed);
            var filter = new Svf();
            for (int i = 0; i < d.Length; i++)
            {
                if ((i & 15) == 0) filter.Set(Mathf.Lerp(hzFrom, hzTo, i / (float)d.Length), q, rate);
                filter.Tick(rng.Bi());
                d[i] = filter.Band * Hit(i / (float)rate, attack, decay);
            }
            return d;
        }

        /// <summary>
        /// A struck object: decaying partials at the given ratios of a base pitch. The ratios make the
        /// material (near-harmonic rings like a bell, wide and clashing like sheet metal), and higher
        /// partials die sooner by <paramref name="spread"/>.
        /// </summary>
        public static float[] Modal(float seconds, float baseHz, float[] ratios, float[] gains, float decay,
            float spread = 0.5f, int rate = Rate)
        {
            float[] d = Buffer(seconds, rate);
            for (int p = 0; p < ratios.Length; p++)
            {
                float hz = baseHz * ratios[p];
                if (hz >= rate * 0.45f) continue;
                float step = Tau * hz / rate, phase = 0f, env = 1f;
                float envStep = Mathf.Exp(-decay * (1f + spread * p) / rate);
                float gain = gains != null && p < gains.Length ? gains[p] : 1f / (p + 1);
                for (int i = 0; i < d.Length; i++)
                {
                    d[i] += Mathf.Sin(phase) * env * gain;
                    phase += step;
                    if (phase > Tau) phase -= Tau;
                    env *= envStep;
                }
            }
            return Fade(d, 0.001f, 0f, rate);
        }

        /// <summary>A pitched voice sliding from one frequency to another.</summary>
        public static float[] Sweep(float seconds, float hzFrom, float hzTo, float attack, float decay, int wave = 0,
            float curve = 1f, int rate = Rate)
        {
            float[] d = Buffer(seconds, rate);
            float phase = 0f;
            bool straight = Mathf.Approximately(curve, 1f);
            for (int i = 0; i < d.Length; i++)
            {
                float u = i / (float)d.Length;
                phase += Mathf.Lerp(hzFrom, hzTo, straight ? u : Mathf.Pow(u, curve)) / rate;
                if (phase >= 1f) phase -= 1f;
                d[i] = Wave(wave, phase) * Hit(i / (float)rate, attack, decay);
            }
            return d;
        }

        /// <summary>A drum or a body blow: a sine that drops fast from a knock to a boom.</summary>
        public static float[] Thump(float seconds, float hzFrom, float hzTo, float decay, float drop = 30f, int rate = Rate)
        {
            float[] d = Buffer(seconds, rate);
            float phase = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)rate;
                phase += (hzTo + (hzFrom - hzTo) * Mathf.Exp(-drop * t)) / rate;
                if (phase >= 1f) phase -= 1f;
                d[i] = Mathf.Sin(Tau * phase) * Hit(t, 0.002f, decay);
            }
            return d;
        }

        /// <summary>A plucked string or struck tine at an exact pitch; upper harmonics die first.</summary>
        public static float[] Pluck(float seconds, float hz, float decay, float brightness = 0.6f, int harmonics = 6,
            int rate = Rate)
        {
            float[] d = Buffer(seconds, rate);
            float amp = 1f;
            for (int k = 1; k <= harmonics; k++)
            {
                // Stiff strings and tines run slightly sharp in their upper partials.
                float f = hz * k * Mathf.Sqrt(1f + 0.0005f * k * k);
                if (f >= rate * 0.45f) break;
                float step = Tau * f / rate, phase = 0f, env = 1f;
                float envStep = Mathf.Exp(-decay * (1f + 0.7f * (k - 1)) / rate);
                float gain = amp / k;
                for (int i = 0; i < d.Length; i++)
                {
                    d[i] += Mathf.Sin(phase) * env * gain;
                    phase += step;
                    if (phase > Tau) phase -= Tau;
                    env *= envStep;
                }
                amp *= brightness;
            }
            return Fade(d, 0.002f, 0f, rate);
        }

        // ---- Buffer operations (in place unless they say otherwise; all return the buffer for chaining) ----

        /// <summary>Adds <paramref name="src"/> into <paramref name="dst"/> starting at a time; whatever runs past the end is dropped.</summary>
        public static float[] Mix(float[] dst, float[] src, float atSeconds = 0f, float gain = 1f, int rate = Rate)
        {
            int offset = Mathf.RoundToInt(atSeconds * rate);
            for (int i = 0; i < src.Length; i++)
            {
                int j = offset + i;
                if (j < 0) continue;
                if (j >= dst.Length) break;
                dst[j] += src[i] * gain;
            }
            return dst;
        }

        /// <summary>Scales so the loudest sample sits at <paramref name="peak"/>.</summary>
        public static float[] Normalize(float[] d, float peak = 0.9f)
        {
            float max = 1e-6f;
            for (int i = 0; i < d.Length; i++) max = Mathf.Max(max, Mathf.Abs(d[i]));
            float gain = peak / max;
            for (int i = 0; i < d.Length; i++) d[i] *= gain;
            return d;
        }

        public static float[] Fade(float[] d, float inSeconds, float outSeconds, int rate = Rate)
        {
            int rise = Mathf.Min(d.Length, Mathf.RoundToInt(inSeconds * rate));
            for (int i = 0; i < rise; i++) d[i] *= i / (float)rise;
            int fall = Mathf.Min(d.Length, Mathf.RoundToInt(outSeconds * rate));
            for (int i = 0; i < fall; i++) d[d.Length - 1 - i] *= i / (float)fall;
            return d;
        }

        public static float[] LowPass(float[] d, float hz, int poles = 2, int rate = Rate)
        {
            float a = OnePole(hz, rate);
            for (int pole = 0; pole < poles; pole++)
            {
                float y = 0f;
                for (int i = 0; i < d.Length; i++)
                {
                    y += (d[i] - y) * a;
                    d[i] = y;
                }
            }
            return d;
        }

        /// <summary>Pushes into soft clipping: grit and weight. Normalize afterwards.</summary>
        public static float[] Drive(float[] d, float amount)
        {
            for (int i = 0; i < d.Length; i++) d[i] = SoftClip(d[i] * amount);
            return d;
        }

        /// <summary>A copy played faster or slower (pitch and length change together).</summary>
        public static float[] Resample(float[] src, float speed)
        {
            int n = Mathf.Max(1, Mathf.FloorToInt(src.Length / Mathf.Max(0.05f, speed)));
            var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float p = i * speed;
                int a = Mathf.Min(src.Length - 1, (int)p), b = Mathf.Min(src.Length - 1, a + 1);
                float f = p - a;
                d[i] = src[a] * (1f - f) + src[b] * f;
            }
            return d;
        }

        /// <summary>
        /// Makes a seamless loop from a buffer rendered a little too long: the surplus tail is folded
        /// into the head. Returns a new, shorter buffer.
        /// </summary>
        public static float[] Loop(float[] raw, float fadeSeconds, int rate = LoopRate)
        {
            int fade = Mathf.Clamp(Samples(fadeSeconds, rate), 1, raw.Length / 2);
            int count = raw.Length - fade;
            var d = new float[count];
            System.Array.Copy(raw, d, count);
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade * Mathf.PI * 0.5f;
                // Equal power: the two ends are unrelated noise, so a straight crossfade would dip.
                d[i] = raw[i] * Mathf.Sin(k) + raw[count + i] * Mathf.Cos(k);
            }
            return d;
        }

        /// <summary>Like <see cref="Mix"/>, but what runs past the end comes back in at the start: for loops.</summary>
        public static float[] MixWrapped(float[] dst, float[] src, float atSeconds, float gain = 1f, int rate = LoopRate)
        {
            int offset = Mathf.RoundToInt(atSeconds * rate) % dst.Length;
            if (offset < 0) offset += dst.Length;
            int n = Mathf.Min(src.Length, dst.Length);
            for (int i = 0; i < n; i++)
            {
                int j = offset + i;
                if (j >= dst.Length) j -= dst.Length;
                dst[j] += src[i] * gain;
            }
            return dst;
        }

        /// <summary>Scales two channels by the same amount, so the loudest sample of either sits at <paramref name="peak"/>.</summary>
        public static void NormalizePair(float[] left, float[] right, float peak = 0.9f)
        {
            float max = 1e-6f;
            for (int i = 0; i < left.Length; i++) max = Mathf.Max(max, Mathf.Abs(left[i]));
            for (int i = 0; i < right.Length; i++) max = Mathf.Max(max, Mathf.Abs(right[i]));
            float gain = peak / max;
            for (int i = 0; i < left.Length; i++) left[i] *= gain;
            for (int i = 0; i < right.Length; i++) right[i] *= gain;
        }

        public static float[] Interleave(float[] left, float[] right)
        {
            int n = Mathf.Min(left.Length, right.Length);
            var d = new float[n * 2];
            for (int i = 0; i < n; i++)
            {
                d[i * 2] = left[i];
                d[i * 2 + 1] = right[i];
            }
            return d;
        }
    }
}

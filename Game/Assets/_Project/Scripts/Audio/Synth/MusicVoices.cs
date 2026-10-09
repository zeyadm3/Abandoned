using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>The instruments the music is played on (0.14.0): each call renders one note or one hit.</summary>
    internal static class MusicVoices
    {
        private const int Rate = Synth.LoopRate;
        private static readonly float[] BoxGains = { 1f, 0.45f, 0.2f, 0.08f };

        /// <summary>A tired electric piano.</summary>
        public static float[] Keys(float hz, float seconds) => Synth.Pluck(seconds, hz, 2.4f, 0.55f, 6, Rate);

        public static float[] Bass(float hz, float seconds) => Synth.Pluck(seconds, hz, 2.8f, 0.45f, 4, Rate);

        /// <summary>A music-box tine: a bar's clashing partials, so it is never quite in tune with anything.</summary>
        public static float[] Box(float hz, float seconds) => Synth.Modal(seconds, hz, Patch.Bar, BoxGains, 3.5f, 0.6f, Rate);

        /// <summary>Strings from another room: two voices a few cents apart, fading in and out.</summary>
        public static float[] Pad(float hz, float seconds, float attack = 0.9f, float release = 1.2f)
        {
            float[] d = Synth.Buffer(seconds, Rate);
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate, w = Synth.Tau * hz * t, w2 = w * 1.004f;
                float v = Mathf.Sin(w) + 0.5f * Mathf.Sin(2f * w) + 0.22f * Mathf.Sin(3f * w) + 0.1f * Mathf.Sin(4f * w)
                          + Mathf.Sin(w2) + 0.4f * Mathf.Sin(2f * w2);
                d[i] = v * Synth.Gate(t, seconds, attack, release);
            }
            return d;
        }

        /// <summary>A bowed or blown note: a saw's harmonics, trembling if asked, sliding if asked.</summary>
        public static float[] Bowed(float hz, float seconds, float tremoloHz = 0f, float attack = 0.3f, float release = 0.4f,
            float slideTo = 0f, int harmonics = 8)
        {
            float[] d = Synth.Buffer(seconds, Rate);
            float phase = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Rate;
                float f = slideTo > 0f ? Mathf.Lerp(hz, slideTo, t / seconds) : hz;
                phase += f / Rate;
                if (phase >= 1f) phase -= 1f;
                float v = 0f;
                for (int k = 1; k <= harmonics; k++) v += Mathf.Sin(Synth.Tau * k * phase) / k;
                float tremble = tremoloHz > 0f ? 0.55f + 0.45f * Mathf.Sin(Synth.Tau * tremoloHz * t) : 1f;
                d[i] = v * tremble * Synth.Gate(t, seconds, attack, release);
            }
            return d;
        }

        /// <summary>
        /// A held tone that lasts the whole loop. Its pitch is nudged so a whole number of cycles fits the
        /// buffer exactly: the loop point can't click.
        /// </summary>
        public static float[] Drone(int samples, float hz, int harmonics = 1, float tilt = 1f)
        {
            var d = new float[samples];
            double cycles = System.Math.Max(1.0, System.Math.Round(hz * samples / (double)Rate));
            for (int k = 1; k <= harmonics; k++)
            {
                if (cycles * k * Rate / samples > Rate * 0.45) break;
                float gain = 1f / Mathf.Pow(k, tilt);
                double step = 2.0 * System.Math.PI * cycles * k / samples;
                for (int i = 0; i < samples; i++) d[i] += (float)System.Math.Sin(step * i) * gain;
            }
            return d;
        }

        /// <summary>Makes a full-length part rise and fall a whole number of times over the loop.</summary>
        public static float[] Breathe(float[] d, int cycles, float depth)
        {
            for (int i = 0; i < d.Length; i++)
                d[i] *= 1f - depth + depth * (0.5f - 0.5f * Mathf.Cos(Synth.Tau * cycles * i / d.Length));
            return d;
        }

        public static float[] Kick(float hz = 45f, float decay = 9f) => Synth.Thump(0.45f, hz * 2.8f, hz, decay, 30f, Rate);

        public static float[] Tom(float hz) => Synth.Thump(0.3f, hz * 2f, hz, 10f, 30f, Rate);

        public static float[] Snare(int seed)
        {
            float[] d = Synth.Buffer(0.22f, Rate);
            Synth.Mix(d, Synth.Normalize(Synth.Noise(0.2f, seed, 6000f, 2500f, 0.001f, 22f, 0f, Rate), 0.8f), 0f, 1f, Rate);
            Synth.Mix(d, Synth.Normalize(Synth.Thump(0.12f, 220f, 180f, 30f, 30f, Rate), 0.5f), 0f, 1f, Rate);
            return d;
        }

        public static float[] Tick(int seed, float bright = 8000f) =>
            Synth.Normalize(Synth.Noise(0.02f, seed, bright, bright * 0.4f, 0.0003f, 200f, 1500f, Rate), 1f);

        /// <summary>Air, hiss or dust for the whole loop (plain noise has no seam to hide).</summary>
        public static float[] Air(float seconds, int seed, float lowPass, float highPass = 0f) =>
            Synth.Normalize(Synth.Noise(seconds, seed, lowPass, lowPass, 0.001f, 0f, highPass, Rate), 1f);

        /// <summary>An old tape: hiss and the odd pop.</summary>
        public static float[] Tape(float seconds, int seed)
        {
            float[] d = Air(seconds, seed, 3500f, 800f);
            var rng = new Rng(seed + 1);
            for (int i = 0; i < d.Length; i++)
                if (rng.Uni() < 0.00002f) d[i] += rng.Bi() * 3f;
            return d;
        }

        /// <summary>A sound rendered at the effects rate, brought to the music's rate at the same pitch.</summary>
        public static float[] FromEffects(float[] rendered) => Synth.Resample(rendered, Synth.Rate / (float)Rate);
    }
}

using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// A state-variable filter for offline synthesis: one <see cref="Tick"/> gives the low-, band- and
    /// high-passed signal at once, and the cutoff can be moved every sample. Good up to about a sixth of
    /// the sample rate; brighter filtering uses the one-pole helpers in <see cref="Synth"/>.
    /// </summary>
    public struct Svf
    {
        public float Low, Band, High;
        private float f, damp;

        public void Set(float cutoffHz, float q, int rate)
        {
            float fc = Mathf.Clamp(cutoffHz, 10f, rate * 0.16f);
            f = 2f * Mathf.Sin(Mathf.PI * fc / rate);
            // This filter form rings out of control when the damping is too high for the cutoff.
            damp = Mathf.Min(1f / Mathf.Max(0.5f, q), 2f / f - f * 0.5f - 0.05f);
        }

        public void Tick(float x)
        {
            Low += f * Band;
            High = x - Low - damp * Band;
            Band += f * High;
        }
    }
}

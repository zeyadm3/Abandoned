using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>The voice formulas, kept pure so they're unit-tested and shared by playback, UI and noise.</summary>
    public static class VoiceMath
    {
        /// <summary>
        /// Proximity gain: full inside <paramref name="minDistance"/>, then inverse-distance (like real
        /// speech) blended to exactly 0 at <paramref name="maxDistance"/>, so nobody is heard across the map.
        /// </summary>
        public static float DistanceGain(float distance, float minDistance, float maxDistance)
        {
            if (distance <= minDistance) return 1f;
            if (distance >= maxDistance) return 0f;
            float inverse = minDistance / distance;
            float fade = 1f - (distance - minDistance) / (maxDistance - minDistance);
            return inverse * Mathf.Sqrt(fade);
        }

        /// <summary>RMS level of a block of samples (0..1).</summary>
        public static float Rms(float[] samples, int count)
        {
            if (count <= 0) return 0f;
            double sum = 0;
            for (int i = 0; i < count; i++) sum += samples[i] * samples[i];
            return Mathf.Clamp01((float)System.Math.Sqrt(sum / count));
        }

        /// <summary>Level in a byte for the wire (0..255).</summary>
        public static byte ToByte(float level) => (byte)Mathf.RoundToInt(Mathf.Clamp01(level) * 255f);

        public static float FromByte(byte level) => level / 255f;
    }
}

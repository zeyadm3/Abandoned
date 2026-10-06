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

        /// <summary>No filtering: the low-pass is wide open.</summary>
        public const float OpenCutoff = 22000f;

        /// <summary>Volume left after <paramref name="walls"/> walls (capped at <paramref name="maxWalls"/>).</summary>
        public static float OcclusionGain(int walls, float perWall, int maxWalls) =>
            Mathf.Pow(Mathf.Clamp01(perWall), Mathf.Clamp(walls, 0, maxWalls));

        /// <summary>Low-pass cutoff through <paramref name="walls"/> walls: open, then the one-wall cutoff, then lower.</summary>
        public static float OcclusionCutoff(int walls, float oneWallCutoff, int maxWalls)
        {
            if (walls <= 0) return OpenCutoff;
            return oneWallCutoff * Mathf.Pow(0.6f, Mathf.Min(walls, maxWalls) - 1);
        }

        /// <summary>
        /// How loud a voice is to monsters: nothing at a whisper, rising linearly to
        /// <paramref name="shoutLoudness"/> at a shout; the radio adds its own squawk on top.
        /// </summary>
        public static float NoiseLoudness(float level, bool radio, VoiceConfig c)
        {
            float voice = level <= c.WhisperLevel ? 0f
                : Mathf.Clamp01((level - c.WhisperLevel) / (c.ShoutLevel - c.WhisperLevel)) * c.ShoutLoudness;
            return Mathf.Clamp01(voice + (radio ? c.RadioLoudness : 0f));
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

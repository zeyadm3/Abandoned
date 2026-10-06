using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// G.711 mu-law: 16-bit samples squeezed into 8 bits with more precision near silence, where
    /// speech lives. Halves bandwidth for the raw-microphone backend with no codec library.
    /// </summary>
    public static class MuLaw
    {
        private const int Bias = 0x84, Clip = 32635;

        public static byte Encode(float sample)
        {
            int pcm = (int)(Mathf.Clamp(sample, -1f, 1f) * 32767f);
            int sign = pcm < 0 ? 0x80 : 0;
            if (sign != 0) pcm = -pcm;
            if (pcm > Clip) pcm = Clip;
            pcm += Bias;
            int exponent = 7;
            for (int mask = 0x4000; (pcm & mask) == 0 && exponent > 0; mask >>= 1) exponent--;
            int mantissa = (pcm >> (exponent + 3)) & 0x0F;
            return (byte)~(sign | (exponent << 4) | mantissa);
        }

        public static float Decode(byte value)
        {
            int u = ~value & 0xFF;
            int exponent = (u >> 4) & 0x07;
            int sample = ((((u & 0x0F) << 3) + Bias) << exponent) - Bias;
            return ((u & 0x80) != 0 ? -sample : sample) / 32768f;
        }
    }
}

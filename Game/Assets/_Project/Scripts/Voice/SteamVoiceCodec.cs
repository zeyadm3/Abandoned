using System.IO;
using Steamworks;

namespace Abandoned.Voice
{
    /// <summary>Steam's voice codec. Every Steam client can decode anyone's packets; needs Steam running.</summary>
    public sealed class SteamVoiceCodec : IVoiceCodec
    {
        private readonly MemoryStream output = new();

        public VoiceCodecId Id => VoiceCodecId.Steam;
        public int SampleRate => SteamClient.IsValid ? (int)SteamUser.SampleRate : 0;

        public int Decode(byte[] data, int length, float[] pcm)
        {
            if (!SteamClient.IsValid) return 0;
            output.SetLength(0);
            using var input = new MemoryStream(data, 0, length, false);
            int bytes = SteamUser.DecompressVoice(input, length, output);
            // 16-bit little-endian mono PCM.
            byte[] raw = output.GetBuffer();
            int n = System.Math.Min(bytes / 2, pcm.Length);
            for (int i = 0; i < n; i++) pcm[i] = (short)(raw[2 * i] | (raw[2 * i + 1] << 8)) / 32768f;
            return n;
        }
    }
}

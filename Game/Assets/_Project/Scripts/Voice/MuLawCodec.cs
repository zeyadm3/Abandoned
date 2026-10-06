namespace Abandoned.Voice
{
    public sealed class MuLawCodec : IVoiceCodec
    {
        /// <summary>Speech band only; 16 kB/s per speaker is fine on a LAN, where this backend is used.</summary>
        public const int Rate = 16000;
        /// <summary>20 ms: smaller packets would mostly be header (and give jumpy levels at high frame rates).</summary>
        public const int MinPacketSamples = Rate / 50;

        public VoiceCodecId Id => VoiceCodecId.MuLaw;
        public int SampleRate => Rate;

        public int Decode(byte[] data, int length, float[] pcm)
        {
            int n = System.Math.Min(length, pcm.Length);
            for (int i = 0; i < n; i++) pcm[i] = MuLaw.Decode(data[i]);
            return n;
        }

        public static int Encode(float[] samples, int count, byte[] into)
        {
            int n = System.Math.Min(count, into.Length);
            for (int i = 0; i < n; i++) into[i] = MuLaw.Encode(samples[i]);
            return n;
        }
    }
}

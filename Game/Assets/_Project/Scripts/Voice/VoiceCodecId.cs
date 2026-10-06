namespace Abandoned.Voice
{
    /// <summary>How a voice packet is encoded; sent with every packet so any receiver picks the right decoder.</summary>
    public enum VoiceCodecId : byte
    {
        /// <summary>8-bit G.711 mu-law at <see cref="MuLawCodec.Rate"/> (raw-microphone backend, local testing).</summary>
        MuLaw = 0,
        /// <summary>Steam's own compressed voice (Steam backend; decoding needs Steam running).</summary>
        Steam = 1,
    }
}

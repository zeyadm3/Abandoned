namespace Abandoned.Voice
{
    /// <summary>Turns one received voice packet back into mono PCM.</summary>
    public interface IVoiceCodec
    {
        VoiceCodecId Id { get; }
        int SampleRate { get; }

        /// <summary>Decodes <paramref name="length"/> bytes into <paramref name="pcm"/>; returns samples written.</summary>
        int Decode(byte[] data, int length, float[] pcm);
    }
}

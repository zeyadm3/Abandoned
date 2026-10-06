namespace Abandoned.Voice
{
    /// <summary>The local player's microphone, already encoded into network packets.</summary>
    public interface IVoiceCapture : System.IDisposable
    {
        string Name { get; }
        /// <summary>Empty when it works; otherwise why the player can't talk (no microphone, Steam off).</summary>
        string Problem { get; }
        VoiceCodecId Codec { get; }
        /// <summary>While false nothing is captured (push-to-talk released), and stale audio is dropped.</summary>
        bool Recording { get; set; }

        /// <summary>Copies one packet's worth of new audio into <paramref name="packet"/>; returns bytes (0 = nothing yet).</summary>
        int ReadPacket(byte[] packet);
    }
}

namespace Abandoned.Voice
{
    /// <summary>No microphone (batch mode, headless tests): the player can listen but not talk.</summary>
    public sealed class SilentVoiceCapture : IVoiceCapture
    {
        public SilentVoiceCapture(string problem) => Problem = problem;

        public string Name => "None";
        public string Problem { get; }
        public VoiceCodecId Codec => VoiceCodecId.MuLaw;
        public bool Recording { get; set; }
        public int ReadPacket(byte[] packet) => 0;
        public void Dispose() { }
    }
}

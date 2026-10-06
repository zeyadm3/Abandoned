using System.IO;
using Steamworks;

namespace Abandoned.Voice
{
    /// <summary>
    /// Steam's voice recording (the player's Steam voice settings and mic). Packets are Steam-compressed
    /// and must be sent whole; one read per frame keeps them small.
    /// </summary>
    public sealed class SteamVoiceCapture : IVoiceCapture
    {
        private readonly MemoryStream stream = new();

        public string Name => "Steam voice";
        public string Problem => SteamClient.IsValid ? string.Empty : "Steam isn't running, so Steam voice is off.";
        public VoiceCodecId Codec => VoiceCodecId.Steam;

        public SteamVoiceCapture()
        {
            if (SteamClient.IsValid) SteamUser.SampleRate = SteamUser.OptimalSampleRate;
        }

        public bool Recording
        {
            get => SteamClient.IsValid && SteamUser.VoiceRecord;
            set
            {
                if (SteamClient.IsValid) SteamUser.VoiceRecord = value;
            }
        }

        public int ReadPacket(byte[] packet)
        {
            if (!SteamClient.IsValid || !SteamUser.HasVoiceData) return 0;
            stream.SetLength(0);
            int bytes = SteamUser.ReadVoiceData(stream);
            // A packet bigger than one datagram can't be split (it's one compressed unit); drop it.
            if (bytes <= 0 || bytes > packet.Length) return 0;
            System.Array.Copy(stream.GetBuffer(), packet, bytes);
            return bytes;
        }

        public void Dispose()
        {
            if (SteamClient.IsValid) SteamUser.VoiceRecord = false;
        }
    }
}

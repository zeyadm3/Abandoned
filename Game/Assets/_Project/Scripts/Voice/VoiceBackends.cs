using System;
using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// Picks the voice backend: Steam voice in Steam sessions, the raw microphone over Unity Transport,
    /// nothing in batch mode. Tests and the nettest swap in a tone through <see cref="CaptureOverride"/>.
    /// This is the seam a bought voice SDK (Dissonance) would replace.
    /// </summary>
    public static class VoiceBackends
    {
        private static readonly MuLawCodec MuLawDecoder = new();
        private static SteamVoiceCodec steamDecoder;

        /// <summary>Set: every new capture comes from here (tests, -fakevoice).</summary>
        public static Func<IVoiceCapture> CaptureOverride { get; set; }

        public static IVoiceCapture CreateCapture(bool steamSession)
        {
            if (CaptureOverride != null) return CaptureOverride();
            if (Application.isBatchMode) return new SilentVoiceCapture("No microphone in batch mode.");
            return steamSession ? new SteamVoiceCapture() : new MicrophoneVoiceCapture();
        }

        public static IVoiceCodec Codec(VoiceCodecId id) => id switch
        {
            VoiceCodecId.MuLaw => MuLawDecoder,
            VoiceCodecId.Steam => steamDecoder ??= new SteamVoiceCodec(),
            _ => null,
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => CaptureOverride = null;
    }
}

namespace Abandoned.Voice
{
    public enum VoiceMode
    {
        /// <summary>Talk while V is held (default: nobody broadcasts their room by accident).</summary>
        PushToTalk,
        /// <summary>Talk whenever the mic hears speech above the threshold.</summary>
        OpenMic,
    }
}

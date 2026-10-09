namespace Abandoned.Audio
{
    /// <summary>A cancellable use of a pooled voice; its ownership expires when that voice is reused.</summary>
    public readonly struct SoundPlayback
    {
        private readonly AudioVoice voice;
        private readonly uint generation;

        internal SoundPlayback(AudioVoice voice)
        {
            this.voice = voice;
            generation = voice.Generation;
        }

        public bool IsPlaying => OwnsVoice && voice.Source.isPlaying;

        private bool OwnsVoice => voice != null && voice.Source != null && voice.Generation == generation;

        public void Stop()
        {
            if (OwnsVoice) voice.Source.Stop();
        }
    }
}

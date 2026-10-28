using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// A sound that keeps playing (wind, hum, an engine): made by <see cref="GameAudio.Loop"/> as a child
    /// of whoever owns it, and levelled every frame by the pool. The owner only sets <see cref="Level"/>;
    /// the bus gain, the cue's volume and starting or pausing the source are handled here. The clip may
    /// arrive a moment late (synthesised loops are rendered in the background); until then it is silent.
    /// </summary>
    public sealed class SoundLoop
    {
        private readonly AudioSource source;
        private readonly SoundId id;
        private readonly SoundInfo info;
        private float cueVolume = 1f;
        private bool hasClip, paused, started;

        /// <summary>Wanted loudness, 0..1, before the bus gain.</summary>
        public float Level { get; set; }
        public float Pitch { get; set; } = 1f;
        public AudioSource Source => source;
        /// <summary>Start somewhere in the recording, so neighbouring copies of one bed don't play in unison.</summary>
        public bool RandomStart { get; set; }

        internal SoundLoop(SoundId id, Transform parent, string name, bool spatial)
        {
            this.id = id;
            info = SoundCatalog.Info(id);
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            source = go.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
            source.dopplerLevel = 0f;
            source.priority = info.Priority;
            source.spatialBlend = spatial ? 1f : 0f;
            SpatialAudio.Apply(source, spatial);
            source.bypassListenerEffects = !spatial;
            source.bypassReverbZones = !spatial;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = info.MinDistance;
            source.maxDistance = info.MaxDistance;
        }

        /// <summary>Changes how far a positioned loop carries.</summary>
        public void SetRange(float minDistance, float maxDistance, AudioRolloffMode rolloff = AudioRolloffMode.Linear)
        {
            if (source == null) return;
            source.rolloffMode = rolloff;
            source.minDistance = minDistance;
            source.maxDistance = Mathf.Max(minDistance + 0.1f, maxDistance);
        }

        /// <summary>False once its object is gone; the pool then forgets it.</summary>
        internal bool Tick()
        {
            if (source == null) return false;
            if (!hasClip) FindClip();
            source.pitch = Pitch;
            float volume = Mathf.Clamp01(Level * cueVolume * AudioMix.Gain(info.Bus));
            source.volume = volume;
            if (!hasClip || !source.isActiveAndEnabled) return true;
            if (volume > 0.0005f)
            {
                if (paused) source.UnPause();
                else if (!source.isPlaying)
                {
                    source.Play();
                    // The clip only arrives here (it may still have been rendering when the owner made the loop).
                    if (!started && RandomStart && source.clip.length > 0f)
                        source.time = Random.Range(0f, source.clip.length * 0.9f);
                    started = true;
                }
                paused = false;
            }
            else if (source.isPlaying)
            {
                source.Pause();
                paused = true;
            }
            return true;
        }

        private void FindClip()
        {
            SoundBank bank = GameAudio.Bank;
            SoundCue cue = bank != null ? bank.Get(id) : null;
            cueVolume = cue != null ? cue.Volume : info.Volume;
            AudioClip clip;
            if (cue != null && cue.HasClips && (info.Mode != SoundMode.Synth || bank.Revision >= SoundCatalog.Revision)) clip = cue.Pick();
            else if (!SynthLibrary.TryGet(id, 0, out clip)) return; // still being rendered
            source.clip = clip;
            hasClip = true;
        }
    }
}

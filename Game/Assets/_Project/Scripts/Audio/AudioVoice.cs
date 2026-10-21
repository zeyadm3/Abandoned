using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// One reusable sound source of the pool: an AudioSource with a low-pass for walls and distance.
    /// The pool keeps its volume at level x bus gain every frame.
    /// </summary>
    public sealed class AudioVoice
    {
        public const float OpenCutoff = 22000f;

        public readonly AudioSource Source;
        private readonly AudioLowPassFilter filter;

        public AudioBus Bus { get; private set; }
        /// <summary>Loudness before the bus gain.</summary>
        public float Level { get; private set; }
        public float Started { get; private set; }
        public float Cutoff { get; private set; } = OpenCutoff;
        internal uint Generation { get; private set; }

        public AudioVoice(Transform parent, int index)
        {
            var go = new GameObject($"Source{index}");
            go.transform.SetParent(parent, false);
            Source = go.AddComponent<AudioSource>();
            Source.playOnAwake = false;
            Source.rolloffMode = AudioRolloffMode.Linear;
            Source.dopplerLevel = 0f;
            filter = go.AddComponent<AudioLowPassFilter>();
            filter.lowpassResonanceQ = 1f;
            filter.enabled = false;
        }

        public void Play(AudioClip clip, in VoiceRequest request)
        {
            Generation++;
            Bus = request.Bus;
            Level = request.Volume * request.OcclusionGain;
            Started = Time.unscaledTime;
            Cutoff = request.Spatial ? request.Cutoff : OpenCutoff;
            Source.transform.position = request.Position;
            Source.clip = clip;
            Source.pitch = request.Pitch;
            Source.spatialBlend = request.Spatial ? 1f : 0f;
            SpatialAudio.Apply(Source, request.Spatial);
            // 2D sounds are in the player's head or the UI: the listener's room and the low-health muffle are for the world.
            Source.bypassListenerEffects = !request.Spatial;
            Source.bypassReverbZones = !request.Spatial;
            Source.minDistance = request.MinDistance;
            Source.maxDistance = Mathf.Max(request.MinDistance + 0.1f, request.MaxDistance);
            Source.priority = request.Priority;
            // The filter costs DSP time, so it only runs for sounds that need dulling.
            filter.enabled = Cutoff < OpenCutoff - 500f;
            if (filter.enabled) filter.cutoffFrequency = Cutoff;
            Refresh();
            if (request.Delay > 0f) Source.PlayDelayed(request.Delay);
            else Source.Play();
        }

        public void Refresh() => Source.volume = Mathf.Clamp01(Level * AudioMix.Gain(Bus));
    }
}

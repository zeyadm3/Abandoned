using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>Everything the pool needs to start one sound.</summary>
    public struct VoiceRequest
    {
        public Vector3 Position;
        /// <summary>Loudness before the bus gain (cue volume x the caller's volume).</summary>
        public float Volume;
        public float Pitch, MinDistance, MaxDistance;
        public bool Spatial;
        public int Priority;
        public AudioBus Bus;
        /// <summary>From <see cref="AudioOcclusion"/>: 1 and an open cutoff when nothing is in the way.</summary>
        public float OcclusionGain, Cutoff;
        /// <summary>Seconds before it starts (something falling that hasn't landed yet).</summary>
        public float Delay;
    }
}

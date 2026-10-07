using System;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// One sound: a few variations picked at random, with a pitch spread so repeats don't sound
    /// mechanical, and how far it carries. No clips = the synthesised placeholder plays instead.
    /// </summary>
    [Serializable]
    public class SoundCue
    {
        [SerializeField] private AudioClip[] clips = Array.Empty<AudioClip>();
        [SerializeField, Range(0f, 2f)] private float volume = 1f;
        [SerializeField] private Vector2 pitch = new(0.94f, 1.06f);
        [Tooltip("Full volume within this distance (m).")]
        [SerializeField] private float minDistance = 2f;
        [Tooltip("Silent beyond this distance (m).")]
        [SerializeField] private float maxDistance = 40f;

        public bool HasClips => clips != null && clips.Length > 0;
        public float Volume => volume;
        public float MinDistance => minDistance;
        public float MaxDistance => maxDistance;

        public AudioClip Pick() => HasClips ? clips[UnityEngine.Random.Range(0, clips.Length)] : null;

        public float PickPitch() => UnityEngine.Random.Range(pitch.x, pitch.y);

#if UNITY_EDITOR
        public SoundCue() { }

        public SoundCue(AudioClip[] clips, float volume, Vector2 pitch, float minDistance, float maxDistance)
        {
            this.clips = clips;
            this.volume = volume;
            this.pitch = pitch;
            this.minDistance = minDistance;
            this.maxDistance = maxDistance;
        }

        public void EditorSetClips(AudioClip[] newClips) => clips = newClips;
#endif
    }
}

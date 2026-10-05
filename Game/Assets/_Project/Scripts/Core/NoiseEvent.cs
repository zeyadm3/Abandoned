using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>A sound in the world that threats can hear. Loudness 0–1 maps to a hearing radius.</summary>
    public readonly struct NoiseEvent
    {
        public readonly Vector3 Position;
        public readonly float Loudness;
        public readonly NoiseSource Source;
        public readonly float Time;

        public NoiseEvent(Vector3 position, float loudness, NoiseSource source, float time)
        {
            Position = position;
            Loudness = Mathf.Clamp01(loudness);
            Source = source;
            Time = time;
        }
    }
}

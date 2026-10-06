using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Velocity estimated from successive positions, for bodies moved by setting their transform.
    /// Lightly smoothed so one uneven frame doesn't spike it; a jump faster than
    /// <see cref="TeleportSpeed"/> is a teleport (unpocket, snap to the host's pose), not motion, and
    /// resets it instead of reading as a 100 m/s hit.
    /// </summary>
    public sealed class TrackedVelocity
    {
        /// <summary>Faster than any thrown or falling item gets in a level; anything above is a teleport.</summary>
        public const float TeleportSpeed = 40f;
        private const float SmoothingTime = 0.03f;

        private Vector3 lastPosition;
        private bool primed;

        public Vector3 Value { get; private set; }

        public void Reset()
        {
            primed = false;
            Value = Vector3.zero;
        }

        public void Sample(Vector3 position, float dt)
        {
            if (!float.IsFinite(position.x) || !float.IsFinite(position.y) || !float.IsFinite(position.z)) return;
            if (!primed || dt <= 0f)
            {
                if (!primed) Value = Vector3.zero;
                lastPosition = position;
                primed = true;
                return;
            }
            Vector3 raw = (position - lastPosition) / dt;
            lastPosition = position;
            if (raw.sqrMagnitude > TeleportSpeed * TeleportSpeed)
            {
                Value = Vector3.zero;
                return;
            }
            Value = Vector3.Lerp(Value, raw, 1f - Mathf.Exp(-dt / SmoothingTime));
        }
    }
}

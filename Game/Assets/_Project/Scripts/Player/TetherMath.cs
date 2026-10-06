using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Keeps a player near an anchor on the ground plane (their handle on a shared carry): inside the
    /// slack they move freely; past it, moving further away is cancelled; past the pull distance they're
    /// also dragged back toward it. Pure, so it's testable without a scene.
    /// </summary>
    public static class TetherMath
    {
        /// <param name="offset">Player position minus anchor; only x/z are used.</param>
        /// <param name="velocity">Horizontal velocity the player wants this step.</param>
        public static Vector3 Constrain(Vector3 offset, Vector3 velocity, float slack, float pullStart, float pullSpeed)
        {
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance <= slack || distance < 1e-4f) return velocity;
            Vector3 away = offset / distance;
            float outward = Vector3.Dot(velocity, away);
            if (outward > 0f) velocity -= away * outward;
            // Ramp the pull in, so standing right at the limit doesn't jitter.
            if (distance > pullStart) velocity -= away * Mathf.Min(pullSpeed, (distance - pullStart) * pullSpeed * 2f);
            return velocity;
        }
    }
}

using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Something players can ride (0.16.3, the freight lift): it moves itself early in the frame and keeps
    /// how far it went, and a player's motor standing on it adds that to its own move. Owner-authoritative
    /// movement stays owner-side: each machine moves its own player with its own copy of the platform.
    /// </summary>
    [DefaultExecutionOrder(-60)]
    public class MovingPlatform : MonoBehaviour
    {
        private Vector3 last;
        private bool started;

        /// <summary>How far it moved this frame.</summary>
        public Vector3 FrameDelta { get; private set; }

        /// <summary>Set by the owner of the motion (FreightLift) before this component's LateUpdate-free bookkeeping.</summary>
        public void MoveTo(Vector3 position)
        {
            if (!started)
            {
                started = true;
                last = position;
            }
            FrameDelta = position - last;
            last = position;
            transform.position = position;
        }

        /// <summary>A belt (Iron Factory): carries its riders this far this frame without moving itself.</summary>
        public void Drift(Vector3 frameDelta) => FrameDelta = frameDelta;

        /// <summary>The platform under a point (feet), if any.</summary>
        public static MovingPlatform Under(Vector3 feet, float reach = 0.45f)
        {
            // Not players or loot: the ray starts inside the rider's own body, and a crate between the feet
            // and the deck would otherwise hide the platform and leave the rider behind.
            int ground = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            if (!Physics.Raycast(feet + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 0.2f + reach, ground, QueryTriggerInteraction.Ignore))
                return null;
            return hit.collider.GetComponentInParent<MovingPlatform>();
        }
    }
}

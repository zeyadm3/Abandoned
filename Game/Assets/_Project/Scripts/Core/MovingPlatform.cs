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
        private Collider[] carryColliders;
        private bool[] restoreCollisions;

        /// <summary>How far it moved this frame.</summary>
        public Vector3 FrameDelta { get; private set; }

        /// <summary>Set by the owner of the motion (FreightLift) before this component's LateUpdate-free bookkeeping.</summary>
        public void MoveTo(Vector3 position)
        {
            if (!started)
            {
                started = true;
                last = transform.position;
            }
            FrameDelta = position - last;
            last = position;
            transform.position = position;
            // Player sweeps and support rays run before the next physics tick, including at the final stop.
            if (FrameDelta != Vector3.zero) Physics.SyncTransforms();
        }

        /// <summary>A belt (Iron Factory): carries its riders this far this frame without moving itself.</summary>
        public void Drift(Vector3 frameDelta) => FrameDelta = frameDelta;

        public void Carry(CharacterController rider)
        {
            // The cab's floor and roof move with its passengers. A delayed height update must not
            // sweep the rider into either; external walls and ceilings still block the displacement.
            if(carryColliders==null)
            {
                carryColliders=GetComponentsInChildren<Collider>(true);
                restoreCollisions=new bool[carryColliders.Length];
            }
            for(int i=0;i<carryColliders.Length;i++)
            {
                Collider part=carryColliders[i];
                restoreCollisions[i]=part!=null && part!=rider && part.enabled && !part.isTrigger && part.gameObject.activeInHierarchy && !Physics.GetIgnoreCollision(rider,part);
                if(restoreCollisions[i])Physics.IgnoreCollision(rider,part,true);
            }
            try { rider.Move(FrameDelta); }
            finally
            {
                for(int i=0;i<carryColliders.Length;i++)
                    if(restoreCollisions[i] && carryColliders[i]!=null)Physics.IgnoreCollision(rider,carryColliders[i],false);
            }
        }

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

using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// What other machines need to know about a player beyond the synced transform: enough for
    /// footsteps, structural load and a lying body. Written by the owner only.
    /// </summary>
    public struct PlayerNetState : INetworkSerializeByMemcpy
    {
        private const byte GroundedBit = 1, SprintingBit = 2, CrouchingBit = 4, RagdolledBit = 8, RestingBit = 16;
        // Body position is snapped to this grid so a lying body doesn't resend every frame for millimetres.
        private const float BodyGrid = 0.02f;

        public byte Flags;
        public Vector3 BodyPosition;

        public bool Grounded => (Flags & GroundedBit) != 0;
        public bool Sprinting => (Flags & SprintingBit) != 0;
        public bool Crouching => (Flags & CrouchingBit) != 0;
        public bool Ragdolled => (Flags & RagdolledBit) != 0;
        public bool BodyResting => (Flags & RestingBit) != 0;

        public static PlayerNetState From(bool grounded, bool sprinting, bool crouching, bool ragdolled, bool resting, Vector3 body)
        {
            byte flags = 0;
            if (grounded) flags |= GroundedBit;
            if (sprinting) flags |= SprintingBit;
            if (crouching) flags |= CrouchingBit;
            if (ragdolled) flags |= RagdolledBit;
            if (resting) flags |= RestingBit;
            // Only a ragdoll's body position means anything; keep it zero otherwise so it never resends.
            Vector3 snapped = ragdolled
                ? new Vector3(Snap(body.x), Snap(body.y), Snap(body.z))
                : Vector3.zero;
            return new PlayerNetState { Flags = flags, BodyPosition = snapped };
        }

        private static float Snap(float v) => Mathf.Round(v / BodyGrid) * BodyGrid;
    }
}

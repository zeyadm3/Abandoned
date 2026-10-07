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

        // Look pitch in 2° steps: others see a flashlight beam where the owner points it.
        private const float PitchStep = 2f;

        public byte Flags;
        public sbyte PitchSteps;
        public Vector3 BodyPosition;

        public float Pitch => PitchSteps * PitchStep;

        public bool Grounded => (Flags & GroundedBit) != 0;
        public bool Sprinting => (Flags & SprintingBit) != 0;
        public bool Crouching => (Flags & CrouchingBit) != 0;
        public bool Ragdolled => (Flags & RagdolledBit) != 0;
        public bool BodyResting => (Flags & RestingBit) != 0;

        public static PlayerNetState From(bool grounded, bool sprinting, bool crouching, bool ragdolled, bool resting, Vector3 body, float pitch = 0f)
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
            sbyte steps = (sbyte)Mathf.Clamp(Mathf.RoundToInt(pitch / PitchStep), sbyte.MinValue, sbyte.MaxValue);
            return new PlayerNetState { Flags = flags, PitchSteps = steps, BodyPosition = snapped };
        }

        private static float Snap(float v) => Mathf.Round(v / BodyGrid) * BodyGrid;
    }
}

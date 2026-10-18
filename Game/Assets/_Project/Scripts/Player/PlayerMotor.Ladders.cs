using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Climbing fixed ladders (Iron Factory). Hands must be empty (pockets are fine): pick something up and
    /// you let go. Facing the ladder, forward climbs and back descends; jump lets go. Climbing is never a
    /// fall, so stepping off at the bottom or grabbing the ladder mid-drop doesn't hurt.
    /// </summary>
    public partial class PlayerMotor
    {
        private const float LadderGrabInput = .3f;
        private const float LadderLetGoSeconds = .4f;

        private Ladder ladder;
        private float ladderCooldownUntil = float.NegativeInfinity;

        public bool IsClimbing => ladder != null;

        // Runs after the normal horizontal and vertical update: on a ladder, input climbs instead.
        private void UpdateLadder(PlayerInputFrame input, float now)
        {
            Ladder at = now >= ladderCooldownUntil ? Ladder.At(transform.position) : null;
            bool handsFree = carrier == null || carrier.Held == null || carrier.Held.IsPocketed;
            if (at == null || !handsFree)
            {
                ladder = null;
                return;
            }
            if (input.JumpPressed)
            {
                // Only cancel our own climb, never a jump just started from the floor beside the ladder.
                if (ladder != null) verticalVelocity = 0f;
                ladder = null;
                ladderCooldownUntil = now + LadderLetGoSeconds;
                return;
            }

            Vector3 wish = transform.right * input.Move.x + transform.forward * input.Move.y;
            wish.y = 0f;
            float toward = Vector3.Dot(Vector3.ClampMagnitude(wish, 1f), at.Inward);
            // Standing beside it does nothing: walk into it, or drop past it from above, to take hold.
            if (ladder == null && IsGrounded && toward < LadderGrabInput) return;
            float climb = toward * config.LadderClimbSpeed;
            if (IsGrounded && climb < 0f)
            {
                ladder = null;
                return;
            }

            ladder = at;
            stuckToGround = false;
            if (transform.position.y >= at.TopY - .05f && climb > 0f)
            {
                // Over the top: step forward onto the walkway.
                verticalVelocity = 0f;
                horizontalVelocity = at.Inward * config.WalkSpeed * .6f;
                return;
            }
            verticalVelocity = climb;
            horizontalVelocity = Vector3.zero;
        }
    }
}

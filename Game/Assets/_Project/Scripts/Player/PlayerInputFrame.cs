using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// One frame of player intent. Movement and look only ever see this struct, never the
    /// Input System, so the same simulation can run from local input, a replay or the network.
    /// </summary>
    public readonly struct PlayerInputFrame
    {
        public readonly Vector2 Move;
        public readonly Vector2 Look;
        public readonly bool SprintHeld;
        public readonly bool CrouchHeld;
        public readonly bool CrouchPressed;
        public readonly bool JumpPressed;
        public readonly bool UsePressed;
        public readonly bool PausePressed;

        public PlayerInputFrame(Vector2 move, Vector2 look, bool sprintHeld, bool crouchHeld, bool crouchPressed,
            bool jumpPressed, bool usePressed, bool pausePressed)
        {
            Move = move;
            Look = look;
            SprintHeld = sprintHeld;
            CrouchHeld = crouchHeld;
            CrouchPressed = crouchPressed;
            JumpPressed = jumpPressed;
            UsePressed = usePressed;
            PausePressed = pausePressed;
        }
    }
}

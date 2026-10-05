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
        public readonly bool UseHeld;
        public readonly bool InteractPressed;
        public readonly bool DropPressed;
        public readonly bool InventoryHeld;
        public readonly bool PausePressed;
        public readonly bool DebugRagdollPressed;

        public PlayerInputFrame(Vector2 move, Vector2 look, bool sprintHeld, bool crouchHeld, bool crouchPressed,
            bool jumpPressed, bool usePressed, bool useHeld, bool interactPressed, bool dropPressed,
            bool inventoryHeld, bool pausePressed, bool debugRagdollPressed = false)
        {
            Move = move;
            Look = look;
            SprintHeld = sprintHeld;
            CrouchHeld = crouchHeld;
            CrouchPressed = crouchPressed;
            JumpPressed = jumpPressed;
            UsePressed = usePressed;
            UseHeld = useHeld;
            InteractPressed = interactPressed;
            DropPressed = dropPressed;
            InventoryHeld = inventoryHeld;
            PausePressed = pausePressed;
            DebugRagdollPressed = debugRagdollPressed;
        }
    }
}

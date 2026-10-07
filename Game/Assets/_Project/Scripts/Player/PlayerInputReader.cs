using Abandoned.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Player
{
    /// <summary>
    /// The only player component that touches the Input System. Samples the Gameplay map once per
    /// frame into <see cref="Current"/>. In multiplayer only the owning client enables this.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Sample before PlayerLook and PlayerMotor read the frame.
    public class PlayerInputReader : MonoBehaviour
    {
        private AbandonedInput input;
        private InputAction ragdollDebugAction;

        public PlayerInputFrame Current { get; private set; }

        /// <summary>This reader's actions, with the player's rebinding applied.</summary>
        public InputActionAsset Actions => input?.asset;

        /// <summary>When set, used instead of the devices (tests, replays, bots).</summary>
        public PlayerInputFrame? Override { get; set; }

        private void Awake()
        {
            input = new AbandonedInput();
            // Debug-only action; looked up by name so this doesn't depend on wrapper regeneration order.
            ragdollDebugAction = input.asset.FindAction("Debug/ToggleRagdoll", true);
            InputBindings.Apply(input.asset);
            InputBindings.Changed += Rebind;
        }

        private void Rebind() => InputBindings.Apply(input.asset);

        private void OnEnable()
        {
            input.Gameplay.Enable();
            ragdollDebugAction.Enable();
        }

        private void OnDisable()
        {
            input.Gameplay.Disable();
            ragdollDebugAction.Disable();
            Current = default;
        }

        private void OnDestroy()
        {
            InputBindings.Changed -= Rebind;
            input?.Dispose();
        }

        private void Update()
        {
            if (Override.HasValue)
            {
                Current = Override.Value;
                return;
            }
            AbandonedInput.GameplayActions g = input.Gameplay;
            Current = new PlayerInputFrame(
                g.Move.ReadValue<Vector2>(),
                g.Look.ReadValue<Vector2>(),
                g.Sprint.IsPressed(),
                g.Crouch.IsPressed(),
                g.Crouch.WasPressedThisFrame(),
                g.Jump.WasPressedThisFrame(),
                g.Use.WasPressedThisFrame(),
                g.Use.IsPressed(),
                g.Interact.WasPressedThisFrame(),
                g.Drop.WasPressedThisFrame(),
                g.Inventory.IsPressed(),
                g.Pause.WasPressedThisFrame(),
                ragdollDebugAction.WasPressedThisFrame(),
                g.PushToTalk.IsPressed(),
                g.Radio.IsPressed(),
                g.HandSlot1.WasPressedThisFrame(),
                g.HandSlot2.WasPressedThisFrame(),
                g.Flashlight.WasPressedThisFrame());
        }
    }
}

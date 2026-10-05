using Abandoned.Core;
using UnityEngine;

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

        public PlayerInputFrame Current { get; private set; }

        private void Awake() => input = new AbandonedInput();

        private void OnEnable() => input.Gameplay.Enable();

        private void OnDisable()
        {
            input.Gameplay.Disable();
            Current = default;
        }

        private void OnDestroy() => input?.Dispose();

        private void Update()
        {
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
                g.Pause.WasPressedThisFrame());
        }
    }
}

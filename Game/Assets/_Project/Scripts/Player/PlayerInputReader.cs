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
        private InputAction ragdollDebugAction, giveGearAction, pingAction;
        private InputAction[] emoteActions;
        private InputAction padLook;

        public PlayerInputFrame Current { get; private set; }

        /// <summary>This reader's actions, with the player's rebinding applied.</summary>
        public InputActionAsset Actions => input?.asset;

        /// <summary>0.19.0: the ping key this frame (read by the ping marker, outside the frame struct).</summary>
        public bool PingPressed => isActiveAndEnabled && !Override.HasValue && pingAction != null && pingAction.WasPressedThisFrame();

        // 0.19.8: a stick is a rate, the mouse a distance: turn the stick into the mouse movement that would
        // turn the view as fast (full tilt = GameSettings.PadLookSpeed "pixels" a second).
        private Vector2 PadLook()
        {
            if (padLook == null) return Vector2.zero;
            Vector2 stick = padLook.ReadValue<Vector2>();
            // A gentle curve: fine aim near the centre, full speed at the edge.
            stick = stick.normalized * Mathf.Pow(stick.magnitude, 1.6f);
            return stick * GameSettings.PadLookSpeed * Time.deltaTime;
        }

        /// <summary>0.19.5: the emote pressed this frame (1 point, 2 wave, 3 panic), or 0.</summary>
        public int EmotePressed
        {
            get
            {
                if (!isActiveAndEnabled || Override.HasValue || emoteActions == null) return 0;
                for (int i = 0; i < emoteActions.Length; i++)
                    if (emoteActions[i] != null && emoteActions[i].WasPressedThisFrame()) return i + 1;
                return 0;
            }
        }

        /// <summary>When set, used instead of the devices (tests, replays, bots).</summary>
        public PlayerInputFrame? Override { get; set; }

        private void Awake()
        {
            input = new AbandonedInput();
            // Debug-only action; looked up by name so this doesn't depend on wrapper regeneration order.
            ragdollDebugAction = input.asset.FindAction("Debug/ToggleRagdoll", true);
            giveGearAction = input.asset.FindAction("Gameplay/GiveGear", true);
            pingAction = input.asset.FindAction("Gameplay/Ping", false); // 0.19.0
            padLook = input.asset.FindAction("Gameplay/LookPad", false); // 0.19.8: the right stick
            emoteActions = new[] // 0.19.5: point, wave, panic
            {
                input.asset.FindAction("Gameplay/EmotePoint", false), input.asset.FindAction("Gameplay/EmoteWave", false),
                input.asset.FindAction("Gameplay/EmotePanic", false),
            };
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
                g.Look.ReadValue<Vector2>() + PadLook(),
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
                ragdollDebugAction.WasPressedThisFrame() && DevTools.Enabled,
                g.PushToTalk.IsPressed(),
                g.Radio.IsPressed(),
                g.HandSlot1.WasPressedThisFrame(),
                g.HandSlot2.WasPressedThisFrame(),
                g.Flashlight.WasPressedThisFrame(),
                g.Rotate.IsPressed(),
                g.Drop.IsPressed(),
                g.Drop.WasReleasedThisFrame(),
                Mouse.current != null ? Mathf.Sign(Mouse.current.scroll.ReadValue().y) * (Mathf.Abs(Mouse.current.scroll.ReadValue().y) > 0.01f ? 1f : 0f) : 0f,
                giveGearAction.WasPressedThisFrame());
        }
    }
}

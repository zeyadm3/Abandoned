using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// What a player is holding and how it follows them. The held object is pulled toward a hold
    /// point in front of the camera by an acceleration spring (not parented), so it collides,
    /// wobbles and lags with weight. Physics runs on the carrier's machine; value/damage stay on the host.
    /// Remote copies (another machine's player) only mirror what they hold, for weight and load.
    /// </summary>
    [RequireComponent(typeof(PlayerInventory))]
    public partial class PlayerCarrier : MonoBehaviour
    {
        [SerializeField] private CarryConfig config;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private PlayerRagdoll ragdoll;

        private const float HintDuration = 2f;

        private CharacterController controller;
        private Quaternion holdRotationOffset;
        private float holdTime;
        private float dragNoiseTimer;
        private Vector3 lastDragPosition;
        private Quaternion placementRotation;

        public CarryConfig Config => config;
        public PlayerInventory Inventory { get; private set; }
        public CharacterController Controller => controller;
        public Grabbable Held { get; private set; }
        public Vector3 EyePosition => cameraRoot.position;
        public Vector3 EyeForward => cameraRoot.forward;
        public string Hint { get; private set; }
        public float HintTime { get; private set; } = float.NegativeInfinity;

        /// <summary>False on another machine's copy of a player: it mirrors holds but never drives or drops them.</summary>
        public bool IsLocal { get; private set; } = true;

        /// <summary>Everything this player carries, in kg; also what they add to structural load.</summary>
        public float CarriedWeight => (Held != null ? Held.WeightOnHolder : 0f) + Inventory.TotalWeight;

        public bool IsDragging => Held != null && Held.IsDragged;

        /// <summary>Holding a carry point of a shared Heavy/Huge item (lifted or still waiting for the crew).</summary>
        public bool IsSharing => Held != null && Held.Shared != null;

        public bool IsRagdolled => ragdoll != null && ragdoll.IsRagdolled;
        public bool IsPlacing { get; private set; }
        public bool PlacementBlocked { get; private set; }
        public bool PlacementReady => IsPlacing && Held != null && Held.HasPhysicsAuthority
            && PlacementRules.Ready(Held, EyePosition, config);
        public bool CanRotate => Held != null && !IsSharing && !IsDragging && !IsRagdolled;
        public bool CanSprint => !IsPlacing && (Held == null || Held.CarryClass < CarryClass.TwoHand)
            && !IsSharing && !IsDragging;

        public void RotateHeld(Vector2 mouse)
        {
            if (!CanRotate || IsPlacing || !IsLocal) return;
            Vector2 angles = Vector2.ClampMagnitude(mouse * config.RotateSensitivity, 10f);
            holdRotationOffset = Quaternion.Euler(-angles.y, angles.x, 0f) * holdRotationOffset;
        }

        public void BeginPlacement()
        {
            if (!CanRotate || !IsLocal) return;
            IsPlacing = true;
            placementRotation = Held.Body.rotation;
            PlacementBlocked = !PlacementRules.TryTarget(Held, EyePosition, config, out _);
        }

        public void CancelPlacement()
        {
            IsPlacing = false;
            PlacementBlocked = false;
        }

        /// <summary>
        /// May drag Heavy items alone (GDD 7.2: the hand trolley). Equipment sets it; true where there is
        /// no company (dev levels, tests).
        /// </summary>
        public bool SoloDragAllowed { get; set; } = true;

        /// <summary>A flatbed trolley in hand (M10.2): Heavy and Huge items can be dragged short-handed, Huge ones slowly.</summary>
        public bool FlatbedDrag { get; set; }
        public bool CanSoloDrag(CarryClass carryClass) =>
            (SoloDragAllowed || FlatbedDrag) && config.CanSoloDrag(carryClass) || FlatbedDrag && carryClass == CarryClass.Huge;

        /// <summary>Velocity a dropped object inherits, so dropping on the run doesn't stop it dead.</summary>
        public Vector3 DropVelocity => motor != null ? motor.MovementVelocity : Vector3.zero;

        public bool HintVisible => Time.time - HintTime < HintDuration;

        public Vector3 HoldPoint
        {
            get
            {
                if (IsSharing)
                {
                    int handle = Held.Shared.IndexOf(this);
                    if (handle >= 0) return Held.Shared.TargetFor(handle);
                }
                if (IsDragging)
                {
                    // Floor level in front of the player, flat (dragging doesn't follow the look pitch).
                    Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                    return transform.position + flatForward * config.DragHoldDistance
                        + Vector3.up * (Held.GetBounds().extents.y + 0.05f);
                }

                bool twoHand = Held != null && Held.CarryClass == CarryClass.TwoHand;
                float distance = twoHand ? config.TwoHandHoldDistance : config.OneHandHoldDistance;
                float drop = twoHand ? config.TwoHandHoldDrop : config.OneHandHoldDrop;
                Vector3 point = cameraRoot.position + cameraRoot.forward * distance - Vector3.up * drop;

                // Looking down would put the point inside our own capsule; push it out in front.
                Vector3 flat = new(point.x - transform.position.x, 0f, point.z - transform.position.z);
                if (flat.magnitude < config.MinHoldRadius)
                {
                    Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                    point += forward * (config.MinHoldRadius - Vector3.Dot(flat, forward));
                }
                return point;
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            Inventory = GetComponent<PlayerInventory>();
            // Never knock ourselves over with the thing we're holding.
            ragdoll.HitFilter = body => Held == null || body != Held.Body;
        }

        private void OnEnable() => ragdoll.Started += OnRagdollStarted;

        /// <summary>Networking: this is another machine's player.</summary>
        public void MakeRemote() => IsLocal = false;

        private void OnRagdollStarted()
        {
            CancelPlacement();
            // The owner's machine decides; a remote copy dropping too would race it.
            if (Held != null && IsLocal) InteractionService.Handler.RequestDrop(this);
        }

        private void Update()
        {
            // Threats live on the host, so the host makes the scraping noise for every dragging player.
            if (IsDragging && GameAuthority.IsHost) EmitDragNoise();
            if (motor == null) return;
            float weight = CarriedWeight;
            motor.SpeedMultiplier = IsDragging ? config.DragSpeedMultiplier : config.SpeedMultiplierFor(weight);
            motor.StaminaDrainMultiplier = config.StaminaDrainMultiplierFor(weight);
            ApplySharedLimits();
        }

        /// <summary>
        /// A shared carry ties the player to the item: nobody outruns the slowest carrier, and walking
        /// away from your handle is blocked (or you get pulled along). Only the owner's motor moves them.
        /// </summary>
        private void ApplySharedLimits()
        {
            int point = IsSharing && IsLocal ? Held.Shared.IndexOf(this) : -1;
            if (point < 0)
            {
                motor.MaxSpeed = float.PositiveInfinity;
                motor.ClearTether();
                return;
            }
            SharedCarryable shared = Held.Shared;
            SharedCarryConfig c = shared.Config;
            motor.MaxSpeed = shared.CarrierMaxSpeed;
            motor.SetTether(shared.AnchorFor(point), c.TetherSlack, c.TetherPullStart, c.TetherPullSpeed);
        }

        /// <summary>Top speed (m/s) this player could carry at right now: gait (crouch/walk/sprint) times their load slowdown.</summary>
        public float CarrySpeedCapability(bool allowSprint)
        {
            if (motor == null) return float.PositiveInfinity;
            PlayerMovementConfig m = motor.Config;
            float gait = motor.IsCrouching ? m.CrouchSpeed : allowSprint && motor.IsSprinting ? m.SprintSpeed : m.WalkSpeed;
            return gait * config.SpeedMultiplierFor(CarriedWeight);
        }

        /// <summary>Top speed (m/s) dragging something heavy alone: gait times the drag slowdown.</summary>
        public float DragSpeedCapability(CarryClass carryClass = CarryClass.Heavy)
        {
            if (motor == null) return float.PositiveInfinity;
            PlayerMovementConfig m = motor.Config;
            float gait = motor.IsCrouching ? m.CrouchSpeed : motor.IsSprinting ? m.SprintSpeed : m.WalkSpeed;
            return gait * config.DragSpeedMultiplier * (carryClass == CarryClass.Huge ? config.HugeDragSpeedScale : 1f);
        }

        private void FixedUpdate()
        {
            if (Held == null) return;

            if (Held.IsPocketed || !Held.IsHeldBy(this))
            {
                Held = null;
                CancelPlacement();
                return;
            }
            // A release can be in flight when a knockdown interrupts input. Retry until the host has
            // mirrored it, and never keep driving a held object from the fallen player's old eye pose.
            if (IsLocal && IsRagdolled)
            {
                CancelPlacement();
                InteractionService.Handler.RequestDrop(this);
                return;
            }
            // Shared carries are simulated by the item itself (on the host) from every carrier's target.
            if (Held.Shared != null) return;
            // Remote copies mirror; a client waiting for ownership has nothing to push yet.
            if (!IsLocal || !Held.HasPhysicsAuthority) return;

            Rigidbody body = Held.Body;
            if (IsPlacing)
            {
                LowerForPlacement(body);
                return;
            }
            Vector3 toTarget = HoldPoint - body.worldCenterOfMass;
            holdTime += Time.fixedDeltaTime;
            if (holdTime > config.BreakGraceTime && toTarget.magnitude > config.BreakDistance)
            {
                // Snagged on a door frame or wall: let go rather than tunnelling or dragging the player.
                InteractionService.Handler.RequestDrop(this);
                return;
            }

            float springScale = Held.IsDragged
                ? config.DragSpringScale
                : Mathf.Lerp(1f, config.HeavySpringScale, config.WeightFraction(Held.Weight));
            Vector3 accel = toTarget * (config.Spring * springScale) - body.linearVelocity * config.Damping;
            // Dragging slides it along the floor: gravity stays on and we never lift it.
            if (Held.IsDragged) accel.y = Mathf.Min(0f, accel.y);
            body.AddForce(Vector3.ClampMagnitude(accel, config.MaxHoldAcceleration), ForceMode.Acceleration);

            Quaternion target = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * holdRotationOffset;
            (target * Quaternion.Inverse(body.rotation)).ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            if (float.IsFinite(axis.x))
                body.angularVelocity = Vector3.ClampMagnitude(axis * (angle * Mathf.Deg2Rad * config.RotationSpring * springScale), config.MaxRotationSpeed);
        }

        private void LowerForPlacement(Rigidbody body)
        {
            Vector3 fromFeet = body.worldCenterOfMass - transform.position;
            fromFeet.y = 0f;
            if (fromFeet.magnitude > config.BreakDistance
                || Vector3.Distance(EyePosition, body.worldCenterOfMass) > config.Reach + config.ReachTolerance)
            {
                CancelPlacement();
                ShowHint("Moved too far from the item");
                InteractionService.Handler.RequestDrop(this);
                return;
            }
            PlacementBlocked = !PlacementRules.TryTarget(Held, EyePosition, config, out Vector3 target);
            // An invalid surface cancels the descent, rather than releasing into a hole or obstacle.
            Vector3 desired = PlacementBlocked ? Vector3.zero
                : Vector3.ClampMagnitude((target - body.worldCenterOfMass) * 5f, config.PlaceLowerSpeed);
            body.AddForce((desired - body.linearVelocity) * 25f, ForceMode.Acceleration);
            body.linearVelocity = Vector3.ClampMagnitude(body.linearVelocity, config.PlaceLowerSpeed);
            (placementRotation * Quaternion.Inverse(body.rotation)).ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            if (float.IsFinite(axis.x)) body.angularVelocity = Vector3.ClampMagnitude(
                axis * (angle * Mathf.Deg2Rad * 4f), config.PlaceReleaseAngularSpeed);
        }

        private void EmitDragNoise()
        {
            // Speed from movement, not the body's velocity: the host's copy of a client-dragged item is kinematic.
            Vector3 position = Held.Body.worldCenterOfMass;
            float speed = Time.deltaTime > 0f ? Vector3.Distance(position, lastDragPosition) / Time.deltaTime : 0f;
            lastDragPosition = position;
            dragNoiseTimer += Time.deltaTime;
            if (dragNoiseTimer < config.DragNoiseInterval || speed < config.DragNoiseMinSpeed) return;
            dragNoiseTimer = 0f;
            // Scraping something heavy across the floor is loud; heavier is louder.
            float loudness = Mathf.Clamp01(Held.Weight / config.DragNoiseFullWeight) * config.DragNoiseMax;
            NoiseSystem.Emit(position, loudness, NoiseSource.LootDrag);
        }

        private void OnDisable()
        {
            CancelPlacement();
            ragdoll.Started -= OnRagdollStarted;
            if (Held != null) ApplyRelease(DropVelocity);
        }
    }
}

using System;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// CharacterController movement: walk, sprint, crouch, jump and gravity, driven only by a
    /// <see cref="PlayerInputFrame"/>. The pivot is at the feet.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private PlayerMovementConfig config;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerStamina stamina;
        [Tooltip("Pitch pivot at eye height; moved down when crouching.")]
        [SerializeField] private Transform cameraRoot;

        private const float SprintMinInput = 0.1f;
        private const float StandCheckSkin = 0.05f;

        private readonly Collider[] standBlockers = new Collider[8];
        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float lastGroundedTime = float.NegativeInfinity;
        private float lastJumpPressedTime = float.NegativeInfinity;
        private bool crouchToggled;
        private float speedMultiplier = 1f;
        // Simulation clock advanced only by Simulate(dt), so results depend on inputs and steps, not wall time.
        private float clock;
        private bool airborne;
        private bool stuckToGround;
        private float airPeakY;
        private bool tethered;
        private Vector3 tetherAnchor;
        private float tetherSlack, tetherPullStart, tetherPullSpeed;

        public PlayerMovementConfig Config => config;
        public Vector3 Velocity => horizontalVelocity + Vector3.up * verticalVelocity;
        public float HorizontalSpeed => horizontalVelocity.magnitude;

        /// <summary>
        /// Velocity of the body itself, without the artificial downward push used to hug the ground.
        /// Use this for anything the player hands momentum to (drops, throws).
        /// </summary>
        public Vector3 MovementVelocity => horizontalVelocity + Vector3.up * (IsGrounded ? 0f : verticalVelocity);
        public bool IsGrounded { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }
        public float CurrentHeight => controller.height;

        /// <summary>0–1 scale on move speed; the carry system lowers it for heavy loot.</summary>
        public float SpeedMultiplier
        {
            get => speedMultiplier;
            set => speedMultiplier = Mathf.Clamp01(value);
        }

        /// <summary>Hard cap on move speed (m/s); a shared carry holds everyone to the slowest carrier.</summary>
        public float MaxSpeed { get; set; } = float.PositiveInfinity;

        public bool IsTethered => tethered;

        /// <summary>Keep the player within <paramref name="slack"/> m of <paramref name="anchor"/> on the ground plane (see <see cref="TetherMath"/>).</summary>
        public void SetTether(Vector3 anchor, float slack, float pullStart, float pullSpeed)
        {
            tethered = true;
            tetherAnchor = anchor;
            tetherSlack = slack;
            tetherPullStart = pullStart;
            tetherPullSpeed = pullSpeed;
        }

        public void ClearTether() => tethered = false;

        /// <summary>Raised on touchdown: fall height (peak to landing, m) and downward impact speed (m/s).</summary>
        public event Action<float, float> Landed;

        /// <summary>Scales stamina drain from sprinting; the carry system raises it for heavy loot.</summary>
        public float StaminaDrainMultiplier { get; set; } = 1f;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.radius = config.Radius;
            controller.slopeLimit = config.SlopeLimit;
            controller.stepOffset = config.StepOffset;
            SetHeight(config.StandingHeight);
        }

        private void OnEnable()
        {
            // Coming back from a ragdoll: don't resume the old run or fall.
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            stuckToGround = false;
            airborne = false;
        }

        private void Update() => Simulate(inputReader.Current, Time.deltaTime);

        public void Simulate(PlayerInputFrame input, float dt)
        {
            clock += dt;
            float now = clock;
            IsGrounded = controller.isGrounded;
            if (IsGrounded) lastGroundedTime = now;
            if (input.JumpPressed) lastJumpPressedTime = now;

            UpdateCrouch(input, dt);
            UpdateHorizontal(input, dt);
            UpdateVertical(now, dt);
            stamina.Tick(dt);

            Vector3 before = transform.position;
            float fallSpeed = -verticalVelocity;
            CollisionFlags flags = controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * dt);
            TrackFall(before.y, fallSpeed);

            // Feed back what actually happened so speed doesn't build up against walls or ceilings.
            // Computed from our own dt rather than controller.velocity, which uses Time.deltaTime.
            // Only ever slows us: a depenetration push (something spawned or landed inside us) divided
            // by a tiny frame time would otherwise fling the player at hundreds of m/s.
            if (dt > 0f)
            {
                Vector3 actual = (transform.position - before) / dt;
                horizontalVelocity = Vector3.ClampMagnitude(new Vector3(actual.x, 0f, actual.z), horizontalVelocity.magnitude);
            }
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;

            // Report the state after this step's move, not before it.
            IsGrounded = controller.isGrounded;
        }

        /// <summary>
        /// Remote copies of other players don't simulate; their owner's grounded/sprint/crouch state is
        /// replicated and pushed in here so footsteps and structural load read the same properties.
        /// </summary>
        public void ApplyRemoteState(bool grounded, bool sprinting, bool crouching)
        {
            IsGrounded = grounded;
            IsSprinting = sprinting;
            IsCrouching = crouching;
            if (controller != null) SetHeight(crouching ? config.CrouchHeight : config.StandingHeight);
        }

        /// <summary>A remote player's landing reported by its owner (host side): same listeners as a local landing.</summary>
        public void RaiseRemoteLanding(float fallHeight, float impactSpeed) => Landed?.Invoke(fallHeight, impactSpeed);

        /// <summary>Forget any fall in progress (after teleports, respawns, getting up from a ragdoll).</summary>
        public void ResetFallTracking() => airborne = false;

        private void TrackFall(float yBefore, float fallSpeed)
        {
            float y = transform.position.y;
            if (!controller.isGrounded)
            {
                airPeakY = airborne ? Mathf.Max(airPeakY, y) : Mathf.Max(yBefore, y);
                airborne = true;
                return;
            }
            if (!airborne) return;
            airborne = false;
            Landed?.Invoke(Mathf.Max(0f, airPeakY - y), Mathf.Max(0f, fallSpeed));
        }

        private void UpdateHorizontal(PlayerInputFrame input, float dt)
        {
            Vector3 wish = transform.right * input.Move.x + transform.forward * input.Move.y;
            wish = Vector3.ClampMagnitude(wish, 1f);
            bool hasInput = wish.sqrMagnitude > SprintMinInput * SprintMinInput;

            IsSprinting = input.SprintHeld && hasInput && !IsCrouching && stamina.CanSprint;
            float speed = IsCrouching ? config.CrouchSpeed : IsSprinting ? config.SprintSpeed : config.WalkSpeed;
            Vector3 target = wish * Mathf.Min(speed * speedMultiplier, MaxSpeed);

            float rate = hasInput ? config.GroundAcceleration : config.GroundDeceleration;
            if (!IsGrounded) rate *= config.AirControl;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, rate * dt);
            if (tethered)
                horizontalVelocity = TetherMath.Constrain(transform.position - tetherAnchor, horizontalVelocity,
                    tetherSlack, tetherPullStart, tetherPullSpeed);

            if (IsSprinting && IsGrounded)
                stamina.Drain(config.SprintDrainPerSecond * StaminaDrainMultiplier * dt);
        }

        private void UpdateVertical(float now, float dt)
        {
            bool coyote = now - lastGroundedTime <= config.CoyoteTime;
            bool buffered = now - lastJumpPressedTime <= config.JumpBufferTime;
            if (coyote && buffered && !IsCrouching && stamina.TryConsume(config.JumpStaminaCost))
            {
                verticalVelocity = Mathf.Sqrt(2f * config.Gravity * config.JumpHeight);
                // Consume both windows so one press can't trigger a second jump.
                lastGroundedTime = float.NegativeInfinity;
                lastJumpPressedTime = float.NegativeInfinity;
                IsGrounded = false;
                stuckToGround = false;
                return;
            }

            if (IsGrounded && verticalVelocity <= 0f)
            {
                verticalVelocity = -config.GroundStickSpeed;
                stuckToGround = true;
                return;
            }

            if (stuckToGround)
            {
                // Walked off an edge: the ground-stick push isn't real speed, so start the fall from rest.
                stuckToGround = false;
                verticalVelocity = 0f;
            }

            float gravity = config.Gravity * (verticalVelocity < 0f ? config.FallGravityMultiplier : 1f);
            verticalVelocity = Mathf.Max(verticalVelocity - gravity * dt, -config.MaxFallSpeed);
        }

        private void UpdateCrouch(PlayerInputFrame input, float dt)
        {
            if (input.CrouchPressed) crouchToggled = !crouchToggled;
            bool wantsCrouch = config.CrouchIsToggle ? crouchToggled : input.CrouchHeld;

            if (wantsCrouch) IsCrouching = true;
            else if (IsCrouching && CanStand()) IsCrouching = false;
            else if (IsCrouching && config.CrouchIsToggle) crouchToggled = true; // blocked: stay crouched

            float targetHeight = IsCrouching ? config.CrouchHeight : config.StandingHeight;
            SetHeight(Mathf.MoveTowards(controller.height, targetHeight, config.CrouchTransitionSpeed * dt));
        }

        private bool CanStand()
        {
            float radius = controller.radius - StandCheckSkin;
            Vector3 bottom = transform.position + Vector3.up * (controller.height - radius);
            Vector3 top = transform.position + Vector3.up * (config.StandingHeight - radius);
            if (top.y <= bottom.y) return true;

            int hits = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, standBlockers, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits; i++)
            {
                Collider c = standBlockers[i];
                // Loose physics objects (including what we're holding) get pushed aside, not stood under.
                if (c == controller || (c.attachedRigidbody != null && !c.attachedRigidbody.isKinematic)) continue;
                return false;
            }
            return true;
        }

        private void SetHeight(float height)
        {
            controller.height = height;
            controller.center = Vector3.up * (height / 2f);

            if (cameraRoot == null) return;
            float t = Mathf.InverseLerp(config.CrouchHeight, config.StandingHeight, height);
            Vector3 eye = cameraRoot.localPosition;
            eye.y = Mathf.Lerp(config.CrouchEyeHeight, config.StandingEyeHeight, t);
            cameraRoot.localPosition = eye;
        }
    }
}

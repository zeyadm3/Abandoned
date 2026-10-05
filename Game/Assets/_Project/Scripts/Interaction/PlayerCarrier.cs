using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// What a player is holding and how it follows them. The held object is pulled toward a hold
    /// point in front of the camera by an acceleration spring (not parented), so it collides,
    /// wobbles and lags with weight. Physics runs on the carrier's machine; value/damage stay on the host.
    /// </summary>
    [RequireComponent(typeof(PlayerInventory))]
    public class PlayerCarrier : MonoBehaviour
    {
        [SerializeField] private CarryConfig config;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private PlayerRagdoll ragdoll;

        private const float HintDuration = 2f;

        private CharacterController controller;
        private Quaternion holdRotationOffset;
        private float holdTime;

        public CarryConfig Config => config;
        public PlayerInventory Inventory { get; private set; }
        public CharacterController Controller => controller;
        public Grabbable Held { get; private set; }
        public Vector3 EyePosition => cameraRoot.position;
        public Vector3 EyeForward => cameraRoot.forward;
        public string Hint { get; private set; }
        public float HintTime { get; private set; } = float.NegativeInfinity;

        /// <summary>Everything this player carries, in kg; also what they add to structural load.</summary>
        public float CarriedWeight => (Held != null && !Held.IsDragged ? Held.Weight : 0f) + Inventory.TotalWeight;

        public bool IsDragging => Held != null && Held.IsDragged;

        /// <summary>Velocity a dropped object inherits, so dropping on the run doesn't stop it dead.</summary>
        public Vector3 DropVelocity => motor != null ? motor.MovementVelocity : Vector3.zero;

        public bool HintVisible => Time.time - HintTime < HintDuration;

        public Vector3 HoldPoint
        {
            get
            {
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

        private void OnRagdollStarted()
        {
            if (Held != null) InteractionService.Handler.RequestDrop(this);
        }

        private void Update()
        {
            if (motor == null) return;
            float weight = CarriedWeight;
            motor.SpeedMultiplier = IsDragging ? config.DragSpeedMultiplier : config.SpeedMultiplierFor(weight);
            if (IsDragging) EmitDragNoise();
            motor.StaminaDrainMultiplier = config.StaminaDrainMultiplierFor(weight);
        }

        private void FixedUpdate()
        {
            if (Held == null) return;

            if (Held.IsPocketed || Held.Holder != this)
            {
                Held = null;
                return;
            }

            Rigidbody body = Held.Body;
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
                body.angularVelocity = axis * (angle * Mathf.Deg2Rad * config.RotationSpring * springScale);
        }

        private float dragNoiseTimer;

        private void EmitDragNoise()
        {
            dragNoiseTimer += Time.deltaTime;
            if (dragNoiseTimer < config.DragNoiseInterval || Held.Body.linearVelocity.sqrMagnitude < 0.04f) return;
            dragNoiseTimer = 0f;
            // Scraping something heavy across the floor is loud; heavier is louder.
            Abandoned.Core.NoiseSystem.Emit(Held.Body.worldCenterOfMass, Mathf.Clamp01(Held.Weight / 500f) * 0.8f,
                Abandoned.Core.NoiseSource.LootDrag);
        }

        public void ShowHint(string message)
        {
            Hint = message;
            HintTime = Time.time;
        }

        // Apply* methods change state and are called only by an IInteractionHandler after validation.

        internal void ApplyHold(Grabbable target)
        {
            bool drag = target.CarryClass > config.HeaviestSoloClass && config.CanSoloDrag(target.CarryClass);
            Held = target;
            holdTime = 0f;
            // Keep the object's current facing relative to the player, so it doesn't snap-rotate.
            holdRotationOffset = Quaternion.Inverse(Quaternion.Euler(0f, transform.eulerAngles.y, 0f)) * target.Body.rotation;
            target.BeginHold(this, drag);
        }

        internal void ApplyRelease(Vector3 velocity)
        {
            Grabbable item = Held;
            Held = null;
            item.EndHold(velocity);
        }

        internal void ApplyPocket(Grabbable target)
        {
            target.Pocket();
            Inventory.Add(target);
        }

        internal void ApplyUnpocketLast()
        {
            Grabbable item = Inventory.RemoveLast();
            float distance = config.OneHandHoldDistance;
            // Don't spawn it inside a wall the player is facing.
            if (Physics.Raycast(cameraRoot.position, cameraRoot.forward, out RaycastHit hit, distance,
                    ~0, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0.2f, hit.distance - 0.25f);
            Vector3 position = cameraRoot.position + cameraRoot.forward * distance;
            item.Unpocket(position, Quaternion.Euler(0f, transform.eulerAngles.y, 0f), DropVelocity);
        }

        private void OnDisable()
        {
            ragdoll.Started -= OnRagdollStarted;
            if (Held != null) ApplyRelease(DropVelocity);
        }
    }
}

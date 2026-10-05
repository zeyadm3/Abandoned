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

        private const float HintDuration = 2f;

        private CharacterController controller;
        private Quaternion holdRotationOffset;

        public CarryConfig Config => config;
        public PlayerInventory Inventory { get; private set; }
        public CharacterController Controller => controller;
        public Grabbable Held { get; private set; }
        public Vector3 EyePosition => cameraRoot.position;
        public Vector3 EyeForward => cameraRoot.forward;
        public string Hint { get; private set; }
        public float HintTime { get; private set; } = float.NegativeInfinity;

        /// <summary>Everything this player carries, in kg; also what they add to structural load.</summary>
        public float CarriedWeight => (Held != null ? Held.Weight : 0f) + Inventory.TotalWeight;

        /// <summary>Velocity a dropped object inherits, so dropping on the run doesn't stop it dead.</summary>
        public Vector3 DropVelocity => motor != null ? motor.MovementVelocity : Vector3.zero;

        public bool HintVisible => Time.time - HintTime < HintDuration;

        public Vector3 HoldPoint
        {
            get
            {
                bool twoHand = Held != null && Held.CarryClass == CarryClass.TwoHand;
                float distance = twoHand ? config.TwoHandHoldDistance : config.OneHandHoldDistance;
                float drop = twoHand ? config.TwoHandHoldDrop : config.OneHandHoldDrop;
                return cameraRoot.position + cameraRoot.forward * distance - Vector3.up * drop;
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            Inventory = GetComponent<PlayerInventory>();
        }

        private void Update()
        {
            if (motor == null) return;
            float weight = CarriedWeight;
            motor.SpeedMultiplier = config.SpeedMultiplierFor(weight);
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
            if (toTarget.magnitude > config.BreakDistance)
            {
                // Snagged on a door frame or wall: let go rather than tunnelling or dragging the player.
                InteractionService.Handler.RequestDrop(this);
                return;
            }

            float springScale = Mathf.Lerp(1f, config.HeavySpringScale, config.WeightFraction(Held.Weight));
            Vector3 accel = toTarget * (config.Spring * springScale) - body.linearVelocity * config.Damping;
            body.AddForce(Vector3.ClampMagnitude(accel, config.MaxHoldAcceleration), ForceMode.Acceleration);

            Quaternion target = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * holdRotationOffset;
            (target * Quaternion.Inverse(body.rotation)).ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            if (float.IsFinite(axis.x))
                body.angularVelocity = axis * (angle * Mathf.Deg2Rad * config.RotationSpring * springScale);
        }

        public void ShowHint(string message)
        {
            Hint = message;
            HintTime = Time.time;
        }

        // Apply* methods change state and are called only by an IInteractionHandler after validation.

        internal void ApplyHold(Grabbable target)
        {
            Held = target;
            // Keep the object's current facing relative to the player, so it doesn't snap-rotate.
            holdRotationOffset = Quaternion.Inverse(Quaternion.Euler(0f, transform.eulerAngles.y, 0f)) * target.Body.rotation;
            target.BeginHold(this);
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
            if (Held != null) ApplyRelease(DropVelocity);
        }
    }
}

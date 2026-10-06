using System;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// A physics object players can pick up, carry, throw or pocket. Carry properties come from an
    /// <see cref="ICarryable"/> on the same object (loot, tools). State changes are applied only by
    /// an <see cref="IInteractionHandler"/> after validation, never directly by player input.
    /// Only the machine with physics authority simulates the body; everywhere else it's kinematic
    /// and follows the network (CLAUDE.md: the carrier owns a carried item's physics, the host the rest).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        private Rigidbody body;
        private ICarryable carryable;
        private Collider[] colliders;
        private Renderer[] renderers;
        private RigidbodyInterpolation restingInterpolation;
        private CollisionDetectionMode dynamicDetection;

        public Rigidbody Body => body;
        public PlayerCarrier Holder { get; private set; }
        public bool IsPocketed { get; private set; }
        /// <summary>Whose pockets it's in (null when not pocketed).</summary>
        public PlayerCarrier PocketHolder { get; private set; }
        /// <summary>Held, but dragged along the floor (Heavy items solo): still rests its weight on the structure.</summary>
        public bool IsDragged { get; private set; }
        public bool IsAvailable => Holder == null && !IsPocketed;
        /// <summary>This machine simulates the body. True offline; networking hands it to the owner.</summary>
        public bool HasPhysicsAuthority { get; private set; } = true;
        public string DisplayName => carryable?.DisplayName ?? name;
        public CarryClass CarryClass => carryable?.CarryClass ?? CarryClass.OneHand;
        public float Weight => carryable?.GameplayWeight ?? body.mass;

        /// <summary>Raised when the object leaves a holder or pocket (dropped, thrown, auto-dropped).</summary>
        public event Action<Grabbable> Released;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            carryable = GetComponent<ICarryable>();
            colliders = GetComponentsInChildren<Collider>();
            renderers = GetComponentsInChildren<Renderer>();
            restingInterpolation = body.interpolation;
            dynamicDetection = body.collisionDetectionMode;
        }

        /// <summary>World-space bounds of the object's colliders, used for reach checks.</summary>
        public Bounds GetBounds()
        {
            if (colliders.Length == 0) return new Bounds(transform.position, Vector3.zero);
            Bounds bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++) bounds.Encapsulate(colliders[i].bounds);
            return bounds;
        }

        /// <summary>
        /// Moves the body to where the transform already is. An interpolated body keeps its old pose
        /// until a physics step runs, and in frames without one, interpolation writes that old pose
        /// back over the transform: a spawned item set in place after Instantiate snaps back to where
        /// it was created (the prefab's origin).
        /// </summary>
        public void SnapBodyToTransform()
        {
            body.position = transform.position;
            body.rotation = transform.rotation;
        }

        public void SetPhysicsAuthority(bool authority)
        {
            HasPhysicsAuthority = authority;
            ApplyBodyMode();
        }

        internal void BeginHold(PlayerCarrier holder, bool drag)
        {
            Holder = holder;
            IsDragged = drag;
            body.useGravity = drag;
            ApplyBodyMode();
            SetIgnoreCollisions(holder.Controller, true);
        }

        internal void EndHold(Vector3 velocity)
        {
            if (Holder != null) SetIgnoreCollisions(Holder.Controller, false);
            Holder = null;
            IsDragged = false;
            body.useGravity = true;
            ApplyBodyMode();
            SetVelocity(velocity);
            Released?.Invoke(this);
        }

        internal void Pocket(PlayerCarrier holder)
        {
            IsPocketed = true;
            PocketHolder = holder;
            ApplyBodyMode();
            SetVisible(false);
        }

        internal void Unpocket(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            IsPocketed = false;
            PocketHolder = null;
            transform.SetPositionAndRotation(position, rotation);
            SnapBodyToTransform();
            ApplyBodyMode();
            SetVelocity(velocity);
            SetVisible(true);
            Released?.Invoke(this);
        }

        private void ApplyBodyMode()
        {
            bool kinematic = IsPocketed || !HasPhysicsAuthority;
            // Kinematic bodies only support discrete/speculative detection; switch before toggling.
            if (kinematic) body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.isKinematic = kinematic;
            if (!kinematic) body.collisionDetectionMode = dynamicDetection;
            body.detectCollisions = !IsPocketed;
            // A non-authority copy is placed by the network every frame; Rigidbody interpolation would fight it.
            body.interpolation = !HasPhysicsAuthority ? RigidbodyInterpolation.None
                : Holder != null ? RigidbodyInterpolation.Interpolate
                : restingInterpolation;
        }

        private void SetVelocity(Vector3 velocity)
        {
            if (!body.isKinematic) body.linearVelocity = velocity;
        }

        private void SetVisible(bool visible)
        {
            foreach (Renderer r in renderers) r.enabled = visible;
        }

        private void SetIgnoreCollisions(Collider other, bool ignore)
        {
            if (other == null) return;
            foreach (Collider c in colliders) Physics.IgnoreCollision(c, other, ignore);
        }

        private void OnDestroy()
        {
            // Shattered or despawned while carried: nobody may keep a reference to a dead item.
            if (Holder != null) Holder.ForgetHeld(this);
            if (IsPocketed && PocketHolder != null) PocketHolder.Inventory.Remove(this);
        }
    }
}

using System;
using Abandoned.Core;
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
    public class Grabbable : MonoBehaviour, IVelocitySource
    {
        private Rigidbody body;
        private ICarryable carryable;
        private Collider[] colliders;
        private Renderer[] renderers;
        private RigidbodyInterpolation restingInterpolation;
        private CollisionDetectionMode dynamicDetection;
        private readonly TrackedVelocity followedVelocity = new();
        private bool draggedByHolder;

        public Rigidbody Body => body;
        public PlayerCarrier Holder { get; private set; }
        public bool IsPocketed { get; private set; }
        /// <summary>Whose pockets it's in (null when not pocketed).</summary>
        public PlayerCarrier PocketHolder { get; private set; }
        /// <summary>Carried together through carry points (Heavy/Huge loot), or null for single-holder items.</summary>
        public SharedCarryable Shared { get; private set; }
        /// <summary>Held, but dragged along the floor (Heavy items solo, or a shared carry short of its crew): still rests its weight on the structure.</summary>
        public bool IsDragged => Shared != null ? Shared.IsDragged : draggedByHolder;
        /// <summary>Held up by a full crew of carriers: its weight is on them, not on the floor.</summary>
        public bool IsLifted => Shared != null && Shared.IsLifted;
        public bool IsAvailable => Holder == null && !IsPocketed && (Shared == null || Shared.HasFreePoint);
        /// <summary>Gameplay kg one holder carries: their share of a lifted shared item, nothing for a dragged one.</summary>
        public float WeightOnHolder => Shared != null ? Shared.SharePerCarrier : IsDragged ? 0f : Weight;
        /// <summary>This machine simulates the body. True offline; networking hands it to the owner.</summary>
        public bool HasPhysicsAuthority { get; private set; } = true;
        private bool externallyHeld;
        public string DisplayName => carryable?.DisplayName ?? name;
        public CarryClass CarryClass => carryable?.CarryClass ?? CarryClass.OneHand;
        public float Weight => carryable?.GameplayWeight ?? body.mass;

        /// <summary>
        /// Real motion on this machine: the body's own velocity where it's simulated, else estimated
        /// from how the network moves the kinematic copy (whose Rigidbody velocity stays zero), so a
        /// host-thrown safe still knocks a client's player down on the client.
        /// </summary>
        public Vector3 Velocity => body.isKinematic ? followedVelocity.Value : body.linearVelocity;

        /// <summary>Raised when the object leaves a holder or pocket (dropped, thrown, auto-dropped).</summary>
        public event Action<Grabbable> Released;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            carryable = GetComponent<ICarryable>();
            Shared = GetComponent<SharedCarryable>();
            colliders = GetComponentsInChildren<Collider>();
            renderers = GetComponentsInChildren<Renderer>();
            restingInterpolation = body.interpolation;
            dynamicDetection = body.collisionDetectionMode;
        }

        public bool IsHeldBy(PlayerCarrier carrier) =>
            carrier != null && (Holder == carrier || (Shared != null && Shared.IsCarriedBy(carrier)));

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

        /// <summary>
        /// Something that isn't a player (the Collector) moves this body by hand: kinematic while it
        /// does. Releasing restores whatever mode the item's hold and authority call for.
        /// </summary>
        public void SetExternallyHeld(bool held)
        {
            externallyHeld = held;
            ApplyBodyMode();
        }

        public void SetPhysicsAuthority(bool authority)
        {
            HasPhysicsAuthority = authority;
            ApplyBodyMode();
        }

        private void LateUpdate()
        {
            // After NetworkTransform has placed the copy this frame. Dynamic bodies report their own velocity.
            if (body.isKinematic && !IsPocketed) followedVelocity.Sample(transform.position, Time.deltaTime);
            else followedVelocity.Reset();
        }

        internal void BeginHold(PlayerCarrier holder, bool drag)
        {
            Holder = holder;
            draggedByHolder = drag;
            body.useGravity = drag;
            ApplyBodyMode();
            SetIgnoreCollisions(holder.Controller, true);
        }

        internal void EndHold(Vector3 velocity)
        {
            if (Holder != null) SetIgnoreCollisions(Holder.Controller, false);
            Holder = null;
            draggedByHolder = false;
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

        /// <summary>A shared carry gained or lost a carrier: gravity comes back when nobody holds it up.</summary>
        internal void SharedCarryChanged()
        {
            if (body == null) return;
            if (!Shared.IsLifted) body.useGravity = true;
            ApplyBodyMode();
            if (Shared.CarrierCount == 0) Released?.Invoke(this);
        }

        private void ApplyBodyMode()
        {
            bool kinematic = IsPocketed || !HasPhysicsAuthority || externallyHeld;
            // Kinematic bodies only support discrete/speculative detection; switch before toggling.
            if (kinematic) body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.isKinematic = kinematic;
            if (!kinematic) body.collisionDetectionMode = dynamicDetection;
            body.detectCollisions = !IsPocketed;
            // A non-authority copy is placed by the network every frame; Rigidbody interpolation would fight it.
            body.interpolation = !HasPhysicsAuthority ? RigidbodyInterpolation.None
                : Holder != null || (Shared != null && Shared.CarrierCount > 0) ? RigidbodyInterpolation.Interpolate
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

        internal void SetIgnoreCollisions(Collider other, bool ignore)
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

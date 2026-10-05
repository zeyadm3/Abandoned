using System;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// A physics object players can pick up, carry, throw or pocket. Carry properties come from an
    /// <see cref="ICarryable"/> on the same object (loot, tools). State changes are applied only by
    /// an <see cref="IInteractionHandler"/> after validation, never directly by player input.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        private Rigidbody body;
        private ICarryable carryable;
        private Collider[] colliders;
        private Renderer[] renderers;
        private RigidbodyInterpolation restingInterpolation;

        public Rigidbody Body => body;
        public PlayerCarrier Holder { get; private set; }
        public bool IsPocketed { get; private set; }
        public bool IsAvailable => Holder == null && !IsPocketed;
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
        }

        /// <summary>World-space bounds of the object's colliders, used for reach checks.</summary>
        public Bounds GetBounds()
        {
            if (colliders.Length == 0) return new Bounds(transform.position, Vector3.zero);
            Bounds bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++) bounds.Encapsulate(colliders[i].bounds);
            return bounds;
        }

        internal void BeginHold(PlayerCarrier holder)
        {
            Holder = holder;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            SetIgnoreCollisions(holder.Controller, true);
        }

        internal void EndHold(Vector3 velocity)
        {
            if (Holder != null) SetIgnoreCollisions(Holder.Controller, false);
            Holder = null;
            body.useGravity = true;
            body.interpolation = restingInterpolation;
            body.linearVelocity = velocity;
            Released?.Invoke(this);
        }

        internal void Pocket()
        {
            IsPocketed = true;
            body.isKinematic = true;
            body.detectCollisions = false;
            SetVisible(false);
        }

        internal void Unpocket(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            IsPocketed = false;
            transform.SetPositionAndRotation(position, rotation);
            body.isKinematic = false;
            body.detectCollisions = true;
            body.linearVelocity = velocity;
            SetVisible(true);
            Released?.Invoke(this);
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
    }
}

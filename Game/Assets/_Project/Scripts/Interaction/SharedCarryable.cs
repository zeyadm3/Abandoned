using System;
using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// A Heavy/Huge item several players carry together (GDD 15). Each carrier holds one carry point;
    /// the item lifts only with a full crew (otherwise it's dragged along the floor), moves at the
    /// slowest carrier's speed and sways when they disagree. Membership is applied here on every
    /// machine (validated by an <see cref="IInteractionHandler"/>, mirrored by networking); the forces
    /// run only where the body is simulated, which for a shared carry is always the host (CLAUDE.md).
    /// Each carrier's "desired hold point" is their feet + the grip offset taken when they grabbed;
    /// networking feeds remote carriers' own fresher targets in through <see cref="SetInputTarget"/>.
    /// </summary>
    [RequireComponent(typeof(Grabbable))]
    public class SharedCarryable : MonoBehaviour
    {
        public const int MaxPoints = 4;

        [SerializeField] private SharedCarryConfig config;

        private readonly PlayerCarrier[] carriers = new PlayerCarrier[MaxPoints];
        private readonly Vector3[] grips = new Vector3[MaxPoints];
        private readonly Vector3[] inputTargets = new Vector3[MaxPoints];
        private readonly float[] inputTimes = { float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity };
        private Grabbable grabbable;
        private Vector3[] localPoints;
        private float[] pointHeights;

        public SharedCarryConfig Config => config;
        public Grabbable Grabbable => grabbable;
        public int PointCount => localPoints.Length;
        public int RequiredCarriers { get; private set; }
        public int CarrierCount { get; private set; }
        public bool IsLifted => CarrierCount > 0 && CarrierCount >= RequiredCarriers;
        /// <summary>Held, but by too few people: it stays on the floor and rests its own weight.</summary>
        public bool IsDragged => CarrierCount > 0 && !IsLifted;
        public bool HasFreePoint => CarrierCount < PointCount;
        public int MissingCarriers => Mathf.Max(0, RequiredCarriers - CarrierCount);
        /// <summary>Gameplay kg each carrier takes while lifted (their load and slowdown); 0 while dragged.</summary>
        public float SharePerCarrier => IsLifted ? grabbable.Weight / CarrierCount : 0f;
        /// <summary>F1: last step's per-point pull (target - point) and the item's commanded acceleration.</summary>
        public Vector3[] LastPull { get; } = new Vector3[MaxPoints];
        public Vector3 LastAcceleration { get; internal set; }

        public event Action<SharedCarryable> CarriersChanged;

        private void Awake()
        {
            grabbable = GetComponent<Grabbable>();
            var spec = GetComponent<ISharedCarrySpec>();
            CarryClass carryClass = GetComponent<ICarryable>()?.CarryClass ?? CarryClass.Heavy;
            Vector3 size = spec?.CarrySize ?? Vector3.one;
            int required = config.RequiredFor(carryClass, spec?.RequiredCarriersOverride ?? 0);
            IReadOnlyList<Vector3> authored = spec?.AuthoredCarryPoints;
            List<Vector3> points = authored is { Count: > 0 }
                ? new List<Vector3>(authored)
                : CarryPointLayout.Generate(size, config.PointCountFor(carryClass, required));
            if (points.Count > MaxPoints) points.RemoveRange(MaxPoints, points.Count - MaxPoints);
            localPoints = points.ToArray();
            RequiredCarriers = Mathf.Min(required, localPoints.Length);
            pointHeights = new float[localPoints.Length];
            // The grip height is the point's height above the item's bottom, so a lifted item clears the floor by LiftClearance.
            for (int i = 0; i < localPoints.Length; i++) pointHeights[i] = localPoints[i].y + size.y * 0.5f;
        }

        public PlayerCarrier CarrierAt(int index) => carriers[index];
        public Vector3 GripAt(int index) => grips[index];
        public Vector3 PointWorld(int index) => transform.TransformPoint(localPoints[index]);

        public int IndexOf(PlayerCarrier carrier)
        {
            if (carrier == null) return -1;
            for (int i = 0; i < localPoints.Length; i++) if (carriers[i] == carrier) return i;
            return -1;
        }

        public bool IsCarriedBy(PlayerCarrier carrier) => IndexOf(carrier) >= 0;

        /// <summary>The free point closest to a player's feet (horizontally), or -1.</summary>
        public int NearestFreePoint(Vector3 from)
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < localPoints.Length; i++)
            {
                if (carriers[i] != null) continue;
                Vector3 d = PointWorld(i) - from;
                d.y = 0f;
                if (d.sqrMagnitude >= bestDistance) continue;
                bestDistance = d.sqrMagnitude;
                best = i;
            }
            return best;
        }

        /// <summary>Horizontal offset from the carrier's feet to the point, clamped to a comfortable arm's length.</summary>
        public Vector3 GripFor(int index, PlayerCarrier carrier)
        {
            Vector3 flat = PointWorld(index) - carrier.transform.position;
            flat.y = 0f;
            float distance = flat.magnitude;
            Vector3 direction = distance > 1e-3f ? flat / distance : Vector3.ProjectOnPlane(carrier.transform.forward, Vector3.up).normalized;
            return direction * Mathf.Clamp(distance, config.MinGripDistance, config.MaxGripDistance);
        }

        /// <summary>Where the carrier wants their point, from where this machine sees them.</summary>
        public Vector3 ComputedTarget(int index)
        {
            Vector3 feet = carriers[index].transform.position;
            return new Vector3(feet.x + grips[index].x, feet.y + pointHeights[index] + config.LiftClearance, feet.z + grips[index].z);
        }

        /// <summary>The carrier's own reported target while fresh (remote carriers), else the computed one.</summary>
        public Vector3 TargetFor(int index) =>
            Time.time - inputTimes[index] <= config.TargetTimeout ? inputTargets[index] : ComputedTarget(index);

        public void SetInputTarget(int index, Vector3 target)
        {
            inputTargets[index] = target;
            inputTimes[index] = Time.time;
        }

        /// <summary>Where the carrier of a point should stand: the point minus their grip (horizontal only matters).</summary>
        public Vector3 AnchorFor(int index) => PointWorld(index) - grips[index];

        /// <summary>Slowest carrier's top speed (m/s) at their own gait and load: the whole crew's limit.</summary>
        public float GroupMaxSpeed
        {
            get
            {
                float slowest = float.PositiveInfinity;
                for (int i = 0; i < localPoints.Length; i++)
                    if (carriers[i] != null) slowest = Mathf.Min(slowest, carriers[i].CarrySpeedCapability(config.AllowSprint));
                return slowest;
            }
        }

        /// <summary>True when one player may drag it alone (Heavy with solo drag on, standing in for the trolley).</summary>
        public bool CanBeDraggedUnderCrewed
        {
            get
            {
                for (int i = 0; i < localPoints.Length; i++)
                    if (carriers[i] != null) return carriers[i].Config.CanSoloDrag(grabbable.CarryClass);
                return false;
            }
        }

        /// <summary>Speed cap for each carrier: the crew's when lifted; a creep for an under-crewed item nobody may drag alone.</summary>
        public float CarrierMaxSpeed => IsLifted ? GroupMaxSpeed
            : CanBeDraggedUnderCrewed ? float.PositiveInfinity : config.CreepSpeed;

        // ---- Membership (only after an IInteractionHandler validated it, or mirroring the host) ----

        internal void Grab(int index, PlayerCarrier carrier) => SetCarrier(index, carrier, GripFor(index, carrier));

        internal void SetCarrier(int index, PlayerCarrier carrier, Vector3 grip)
        {
            if (carriers[index] == carrier)
            {
                grips[index] = grip;
                return;
            }
            if (carriers[index] != null) Detach(index);
            if (carrier != null)
            {
                int previous = IndexOf(carrier);
                if (previous >= 0) Detach(previous);
                // A stale mirror may still show them holding something else; the host says otherwise.
                if (carrier.Held != null && carrier.Held != grabbable) carrier.ApplyRelease(Vector3.zero);
                carriers[index] = carrier;
                grips[index] = grip;
                inputTimes[index] = float.NegativeInfinity;
                CarrierCount++;
                grabbable.SetIgnoreCollisions(carrier.Controller, true);
                carrier.AttachShared(grabbable);
            }
            Changed();
        }

        internal void RemoveCarrier(PlayerCarrier carrier)
        {
            int index = IndexOf(carrier);
            if (index < 0) return;
            Detach(index);
            Changed();
        }

        public void ReleaseAll()
        {
            bool any = false;
            for (int i = 0; i < carriers.Length; i++)
            {
                if (carriers[i] is null) continue;
                Detach(i);
                any = true;
            }
            if (any) Changed();
        }

        private void Detach(int index)
        {
            PlayerCarrier carrier = carriers[index];
            carriers[index] = null;
            CarrierCount--;
            LastPull[index] = Vector3.zero;
            // The player object may already be destroyed (left the session).
            if (carrier == null) return;
            grabbable.SetIgnoreCollisions(carrier.Controller, false);
            carrier.DetachShared(grabbable);
        }

        private void Changed()
        {
            grabbable.SharedCarryChanged();
            CarriersChanged?.Invoke(this);
        }

        private void FixedUpdate()
        {
            if (CarrierCount == 0 || !grabbable.HasPhysicsAuthority || grabbable.IsPocketed) return;
            SharedCarryForces.Step(this, grabbable.Body, Time.fixedDeltaTime, Time.time);
        }

        private void OnDisable() => ReleaseAll();

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !DebugView.Visible || localPoints == null) return;
            for (int i = 0; i < localPoints.Length; i++)
            {
                Vector3 point = PointWorld(i);
                Gizmos.color = carriers[i] != null ? Color.yellow : Color.green;
                Gizmos.DrawWireSphere(point, 0.1f);
                if (carriers[i] == null) continue;
                Gizmos.DrawLine(carriers[i].transform.position + Vector3.up, point);
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(TargetFor(i), 0.06f);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(point, LastPull[i]);
            }
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(grabbable.Body.worldCenterOfMass, LastAcceleration * 0.05f);
        }
    }
}

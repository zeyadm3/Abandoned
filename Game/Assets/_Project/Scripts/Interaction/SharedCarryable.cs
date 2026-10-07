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
    /// Each carrier's "desired hold point" is their feet + the grip offset taken when they grabbed,
    /// kept in the item's yaw frame so it turns with the item (a carrier at one end stays at that end
    /// through a corner); networking feeds remote carriers' own fresher targets in through <see cref="SetInputTarget"/>.
    /// </summary>
    [RequireComponent(typeof(Grabbable))]
    public class SharedCarryable : MonoBehaviour
    {
        public const int MaxPoints = 4;

        [SerializeField] private SharedCarryConfig config;

        private readonly PlayerCarrier[] carriers = new PlayerCarrier[MaxPoints];
        // Feet -> handle, in the item's yaw frame (see GripWorld).
        private readonly Vector3[] grips = new Vector3[MaxPoints];
        private readonly Vector3[] inputTargets = new Vector3[MaxPoints];
        private readonly float[] inputTimes = { float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity };
        private Grabbable grabbable;
        private Vector3[] localPoints;
        private float[] pointHeights;
        private Vector3 nudgeOrigin;
        private bool hasNudgeOrigin;

        public SharedCarryConfig Config => config;
        public Grabbable Grabbable => grabbable;
        public int PointCount => localPoints.Length;
        public int RequiredCarriers { get; private set; }
        public int CarrierCount { get; private set; }
        public bool IsLifted => CarrierCount > 0 && CarrierCount >= RequiredCarriers;
        /// <summary>Held, but by too few people: it stays on the floor and rests its own weight.</summary>
        public bool IsDragged => CarrierCount > 0 && !IsLifted;
        /// <summary>Held by too few people and too big to drag alone: it only moves within a small nudge budget.</summary>
        public bool IsNudgeOnly => IsDragged && !CanBeDraggedUnderCrewed;
        /// <summary>Host: where the current nudge budget is centred (where the under-crewed hold began).</summary>
        public Vector3 NudgeOrigin => nudgeOrigin;
        /// <summary>Horizontal metres of the nudge budget used so far (F1).</summary>
        public float NudgeUsed => hasNudgeOrigin ? Vector3.ProjectOnPlane(transform.position - nudgeOrigin, Vector3.up).magnitude : 0f;
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
        /// <summary>The grip in the item's yaw frame (what's replicated).</summary>
        public Vector3 GripAt(int index) => grips[index];
        /// <summary>The grip turned to the item's current heading.</summary>
        public Vector3 GripWorld(int index) => Yaw * grips[index];

        /// <summary>The item's heading on the floor plane (tilt ignored, so a swaying load doesn't swing its carriers).</summary>
        public Quaternion Yaw => YawOf(transform.rotation);

        public static Quaternion YawOf(Quaternion rotation)
        {
            Vector3 forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up);
            // Tipped onto its front or back: its up axis shows which way it lies.
            if (forward.sqrMagnitude < 1e-4f) forward = Vector3.ProjectOnPlane(rotation * Vector3.down, Vector3.up);
            return forward.sqrMagnitude < 1e-8f ? Quaternion.identity : Quaternion.LookRotation(forward, Vector3.up);
        }
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

        /// <summary>
        /// Horizontal offset from the carrier's feet to the point, clamped to a comfortable arm's length,
        /// in the item's yaw frame: a world-fixed offset would jam long items at corners.
        /// </summary>
        public Vector3 GripFor(int index, PlayerCarrier carrier)
        {
            Vector3 flat = PointWorld(index) - carrier.transform.position;
            flat.y = 0f;
            float distance = flat.magnitude;
            Vector3 direction = distance > 1e-3f ? flat / distance : Vector3.ProjectOnPlane(carrier.transform.forward, Vector3.up).normalized;
            return Quaternion.Inverse(Yaw) * (direction * Mathf.Clamp(distance, config.MinGripDistance, config.MaxGripDistance));
        }

        /// <summary>Where the carrier wants their point, from where this machine sees them.</summary>
        public Vector3 ComputedTarget(int index)
        {
            Vector3 feet = carriers[index].transform.position;
            Vector3 grip = GripWorld(index);
            return new Vector3(feet.x + grip.x, feet.y + pointHeights[index] + config.LiftClearance, feet.z + grip.z);
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
        public Vector3 AnchorFor(int index)
        {
            // A client sees the host-simulated item late; leading it by its motion keeps a high-ping
            // carrier from being held back by where the item was (AnchorLead, set by the network side).
            Vector3 lead = grabbable.Velocity * AnchorLead;
            lead.y = 0f;
            return PointWorld(index) - GripWorld(index) + lead;
        }

        /// <summary>Seconds the anchor leads the item's copy by (0 where the item is simulated).</summary>
        public float AnchorLead { get; set; }

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
                    if (carriers[i] != null) return carriers[i].CanSoloDrag(grabbable.CarryClass);
                return false;
            }
        }

        /// <summary>Speed cap for each carrier: the crew's when lifted; a creep for an under-crewed item nobody may drag alone.</summary>
        public float CarrierMaxSpeed => IsLifted ? GroupMaxSpeed
            : CanBeDraggedUnderCrewed ? DragMaxSpeed : config.CreepSpeed;

        // A solo drag at the dragger's own (slowed) pace; also caps the host-simulated item, so a forged
        // target can't whip a 300 kg rack around.
        private float DragMaxSpeed
        {
            get
            {
                float slowest = float.PositiveInfinity;
                for (int i = 0; i < localPoints.Length; i++)
                    if (carriers[i] != null) slowest = Mathf.Min(slowest, carriers[i].DragSpeedCapability());
                return slowest;
            }
        }

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
            UpdateNudgeOrigin();
            grabbable.SharedCarryChanged();
            CarriersChanged?.Invoke(this);
        }

        /// <summary>
        /// The nudge budget is tied to the item, not to a grab: letting go and grabbing again doesn't
        /// refill it, or a solo player could inch a statue to the truck. It starts afresh only after a
        /// full crew lifted it or it ended up well away from the budget (fell through a floor).
        /// </summary>
        private void UpdateNudgeOrigin()
        {
            if (IsLifted) hasNudgeOrigin = false;
            if (!IsNudgeOnly) return;
            Vector3 here = transform.position;
            if (hasNudgeOrigin && Vector3.Distance(here, nudgeOrigin) <= config.NudgeRadius + config.NudgeResetDistance) return;
            nudgeOrigin = here;
            hasNudgeOrigin = true;
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
            if (!IsNudgeOnly || !hasNudgeOrigin) return;
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.matrix = Matrix4x4.TRS(nudgeOrigin, Quaternion.identity, new Vector3(1f, 0.01f, 1f));
            Gizmos.DrawWireSphere(Vector3.zero, config.NudgeRadius);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}

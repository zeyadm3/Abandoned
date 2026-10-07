using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// Conservative support/clearance checks for a physical set-down. The object still travels through
    /// the physics world; these checks never move it or exempt it from impact damage.
    /// </summary>
    public static class PlacementRules
    {
        private static readonly RaycastHit[] Hits = new RaycastHit[64];
        private static readonly Collider[] Overlaps = new Collider[64];

        public static bool TryTarget(Grabbable item, Vector3 eye, CarryConfig config, out Vector3 centre)
        {
            centre = item.Body.worldCenterOfMass;
            Bounds bounds = item.GetBounds();
            if (!Support(item, bounds, config.Reach, out RaycastHit hit)) return false;
            centre.y = hit.point.y + (centre.y - bounds.min.y) + config.PlaceClearance;
            if (Vector3.Distance(eye, centre) > config.Reach + config.ReachTolerance) return false;
            Bounds destination = bounds;
            destination.center += centre - item.Body.worldCenterOfMass;
            return Clear(item, destination, config.PlaceClearance);
        }

        public static bool Ready(Grabbable item, Vector3 eye, CarryConfig config)
        {
            return TryTarget(item, eye, config, out Vector3 target)
                && Vector3.Distance(item.Body.worldCenterOfMass, target) <= config.PlaceTolerance
                && item.Velocity.magnitude <= config.PlaceReleaseSpeed
                && item.Body.angularVelocity.magnitude <= config.PlaceReleaseAngularSpeed;
        }

        /// <summary>Host: a delayed client pose must be nearby, supported, clear, and reachable without crossing geometry.</summary>
        public static bool PlausibleRelease(Grabbable item, Vector3 eye, CarryConfig config, Pose pose)
        {
            if (!Finite(pose.position) || !Finite(pose.rotation)
                || Vector3.Distance(eye, pose.position) > config.Reach + config.ReachTolerance) return false;
            // Rotation should already have arrived through the owning carrier's NetworkTransform.
            if (Quaternion.Angle(item.Body.rotation, pose.rotation) > 2f) return false;
            Bounds actual = item.GetBounds();
            Vector3 delta = pose.position - item.Body.position;
            Bounds reported = actual;
            reported.center += delta;
            if (!Clear(item, reported, config.PlaceClearance) || !Support(item, reported, config.PlaceTolerance * 2f, out RaycastHit support)) return false;
            if (reported.min.y - support.point.y > config.PlaceTolerance + config.PlaceClearance) return false;
            float distance = delta.magnitude;
            if (distance <= 0.0001f) return true;
            // AABB is intentionally conservative for rotated loot. A rejected handoff keeps the item held.
            Vector3 extents = Inset(actual.extents, config.PlaceClearance);
            int count = Physics.BoxCastNonAlloc(actual.center, extents, delta / distance, Hits,
                Quaternion.identity, distance, ~0, QueryTriggerInteraction.Ignore);
            if (count == Hits.Length) return false;
            for (int i = 0; i < count; i++)
                if (!Ignored(item, Hits[i].collider)) return false;
            return true;
        }

        private static bool Support(Grabbable item, Bounds bounds, float distance, out RaycastHit support)
        {
            support = default;
            Vector3 origin = new(bounds.center.x, bounds.min.y + 0.04f, bounds.center.z);
            int count = Physics.RaycastNonAlloc(origin, Vector3.down, Hits, distance + 0.04f, ~0, QueryTriggerInteraction.Ignore);
            if (count == Hits.Length) return false;
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = Hits[i];
                if (Ignored(item, hit.collider) || hit.distance >= nearest) continue;
                nearest = hit.distance;
                support = hit;
            }
            // Do not set something down against the side of a shelf or on a steep slope.
            return nearest < float.PositiveInfinity && support.normal.y >= 0.8f;
        }

        private static bool Clear(Grabbable item, Bounds bounds, float inset)
        {
            int count = Physics.OverlapBoxNonAlloc(bounds.center, Inset(bounds.extents, inset), Overlaps,
                Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
            if (count == Overlaps.Length) return false;
            for (int i = 0; i < count; i++)
                if (!Ignored(item, Overlaps[i])) return false;
            return true;
        }

        private static bool Ignored(Grabbable item, Collider collider) => collider == null
            || collider.attachedRigidbody == item.Body
            || item.Holder != null && collider == item.Holder.Controller
            || collider.gameObject.layer == Abandoned.Core.GameLayers.DebrisLayer;

        private static Vector3 Inset(Vector3 extents, float amount) => new(
            Mathf.Max(0.01f, extents.x - amount), Mathf.Max(0.01f, extents.y - amount), Mathf.Max(0.01f, extents.z - amount));
        public static bool Finite(Vector3 value) => float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        private static bool Finite(Quaternion value)
        {
            float norm = value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w;
            return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z) && float.IsFinite(value.w)
                && norm >= 0.99f && norm <= 1.01f;
        }
    }
}

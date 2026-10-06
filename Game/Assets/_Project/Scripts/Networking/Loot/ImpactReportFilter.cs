using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Host-side gate for impacts a carrier reports: only the current (or just-replaced) owner may
    /// report, no faster than the interval, at a believable spot, and the speed is clamped. Pure, so the
    /// rules are EditMode-testable. One per item.
    /// A just-replaced owner's reports were in flight when the host took the item back; once the host's
    /// own physics has hit something since then, it has covered that contact, so late reports would
    /// count the same hit twice.
    /// </summary>
    public sealed class ImpactReportFilter
    {
        private readonly LootNetConfig config;
        private float lastAccepted = float.NegativeInfinity;
        private ulong previousOwner = ulong.MaxValue;
        private float ownerChangedAt = float.NegativeInfinity;
        private float hostImpactAt = float.NegativeInfinity;

        public ImpactReportFilter(LootNetConfig config) => this.config = config;

        /// <summary>Remember who owned the item before, so their in-flight reports still land.</summary>
        public void OwnerChanged(ulong previous, float now)
        {
            previousOwner = previous;
            ownerChangedAt = now;
        }

        /// <summary>The host's own simulation of the item hit something.</summary>
        public void HostImpact(float now) => hostImpactAt = now;

        /// <summary>Returns the speed to apply, or false when the report is rejected.</summary>
        public bool TryAccept(ulong sender, ulong currentOwner, float now, float speed, Vector3 point, Bounds itemBounds, out float acceptedSpeed)
        {
            acceptedSpeed = 0f;
            bool owner = sender == currentOwner;
            bool justReleased = sender == previousOwner && now - ownerChangedAt <= config.ReportGraceTime
                && hostImpactAt < ownerChangedAt;
            if (!owner && !justReleased) return false;
            if (now - lastAccepted < config.ImpactReportInterval) return false;
            if (!float.IsFinite(speed) || speed <= 0f || !IsFinite(point)) return false;
            float maxDistance = config.MaxImpactPointDistance;
            if (itemBounds.SqrDistance(point) > maxDistance * maxDistance) return false;

            lastAccepted = now;
            acceptedSpeed = Mathf.Min(speed, config.MaxReportedImpactSpeed);
            return true;
        }

        private static bool IsFinite(Vector3 v) => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
    }
}

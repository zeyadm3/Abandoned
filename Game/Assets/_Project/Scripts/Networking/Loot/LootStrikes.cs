using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// A client-carried item hitting other loot. On the carrier's machine every other item is a
    /// kinematic copy the host simulates, so it acts like a wall there and never judges the hit; on the
    /// host the carried copy is moved by its transform, so the contact has no relative speed. The hit
    /// really happened only on the carrier's machine: the carrier names the struck item in its impact
    /// report, and the host gives that item the same impact and a push.
    /// </summary>
    public static class LootStrikes
    {
        public const ulong None = 0; // NGO object ids start at 1

        /// <summary>Carrier: the struck item if this machine only follows it (else <see cref="None"/>).</summary>
        public static ulong StruckId(Rigidbody other) =>
            other != null && other.TryGetComponent(out NetworkLoot loot) && loot.IsSpawned && !loot.IsOwner
                ? loot.NetworkObjectId
                : None;

        /// <summary>Host: the struck item takes the reporter's (already filtered and clamped) impact.</summary>
        public static bool ApplyStruck(NetworkLoot reporter, ulong struckId, float speed, Vector3 point, Vector3 normal)
        {
            if (struckId == None || struckId == reporter.NetworkObjectId) return false;
            if (!reporter.NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(struckId, out NetworkObject no)
                || no == null || !no.TryGetComponent(out NetworkLoot struck) || !struck.IsSpawned) return false;
            // Only loot the host simulates: a carried item's own carrier reports its hits itself.
            if (!struck.IsOwner || struck.Hold.Mode != LootHoldMode.Free || struck.Item.IsShattered) return false;
            float maxDistance = reporter.Config.MaxImpactPointDistance;
            if (struck.Grabbable.GetBounds().SqrDistance(point) > maxDistance * maxDistance) return false;

            Rigidbody body = struck.Grabbable.Body;
            if (!body.isKinematic)
                body.AddForce(PushDirection(body, point, normal) * speed * PushTransfer(reporter, body), ForceMode.VelocityChange);
            struck.LastStruckSpeed = speed;
            // Sound first: a shatter despawns the item. Nobody heard this one yet, not even the carrier.
            struck.ServerReplayImpactEverywhere(speed, point);
            struck.Item.ApplyImpact(speed, point);
            struck.Item.EmitImpactNoise(speed, point);
            return true;
        }

        /// <summary>
        /// The carried item is held by a hand, so it pushes a light item along at its own speed but
        /// barely shifts a heavy one (Rigidbody masses, which are clamped but keep the order).
        /// </summary>
        public static float PushTransfer(NetworkLoot reporter, Rigidbody struck) =>
            reporter.Config.StruckPushTransfer * Mathf.Min(1f, reporter.Grabbable.Body.mass / Mathf.Max(0.01f, struck.mass));

        // The carrier's normal points from the struck item toward the carried one; don't trust its sign.
        private static Vector3 PushDirection(Rigidbody struck, Vector3 point, Vector3 normal)
        {
            Vector3 away = struck.worldCenterOfMass - point;
            Vector3 dir = float.IsFinite(normal.sqrMagnitude) && normal.sqrMagnitude > 1e-6f ? -normal.normalized : away.normalized;
            return Vector3.Dot(dir, away) < 0f ? -dir : dir;
        }
    }
}

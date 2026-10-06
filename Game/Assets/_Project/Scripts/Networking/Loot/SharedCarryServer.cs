using Abandoned.Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// The host's side of shared carrying: validate a grab with the same <see cref="PickupRules"/> as
    /// solo play (against the host's view of the player), keep the item's physics on the host
    /// (CLAUDE.md: shared carries are host-simulated from the carriers' input), apply, and judge the
    /// hold targets clients send.
    /// </summary>
    public static class SharedCarryServer
    {
        public static bool TryGrab(NetworkSharedCarry net, PlayerCarrier carrier, out string reason)
        {
            SharedCarryable shared = net.Shared;
            if (!PickupRules.CanGrabPoint(carrier, shared, out int point, out reason)) return false;
            if (net.Loot.Hold.Mode != LootHoldMode.Free) { reason = "Someone else has it"; return false; }
            if (PlayerObjectId(carrier) == 0) { reason = "Not in the session"; return false; }

            if (net.Loot.OwnerClientId != NetworkManager.ServerClientId) net.Loot.NetworkObject.RemoveOwnership();
            net.Loot.SyncPhysicsAuthority();
            // The item publishes the new crew itself (CarriersChanged -> state).
            shared.Grab(point, carrier);
            return true;
        }

        public static bool TryLetGo(NetworkSharedCarry net, PlayerCarrier carrier)
        {
            if (carrier == null || !net.Shared.IsCarriedBy(carrier)) return false;
            carrier.ApplyRelease(Vector3.zero);
            return true;
        }

        /// <summary>
        /// A remote carrier's own hold target is fresher than the host's interpolated view of them, but
        /// it's only believed within <see cref="SharedCarryConfig.MaxTargetDeviation"/> of what the host
        /// computes, so a modified client can't fling a piano across the map.
        /// </summary>
        public static bool TryClampTarget(SharedCarryable shared, int point, Vector3 reported, out Vector3 target)
        {
            target = default;
            if (!float.IsFinite(reported.x) || !float.IsFinite(reported.y) || !float.IsFinite(reported.z)) return false;
            Vector3 expected = shared.ComputedTarget(point);
            target = expected + Vector3.ClampMagnitude(reported - expected, shared.Config.MaxTargetDeviation);
            return true;
        }

        /// <summary>
        /// Host: carriers who went down or ended up far from their handle (fell off a ledge with it,
        /// got pushed away) let go. Returns the carrier released, or null.
        /// </summary>
        public static PlayerCarrier CheckGrip(SharedCarryable shared, int point)
        {
            PlayerCarrier carrier = shared.CarrierAt(point);
            if (carrier == null) return null;
            bool lost = carrier.IsRagdolled
                || Vector3.Distance(shared.PointWorld(point), shared.ComputedTarget(point)) > shared.Config.BreakDistance;
            if (!lost) return null;
            carrier.ApplyRelease(Vector3.zero);
            return carrier;
        }

        public static ulong PlayerObjectId(PlayerCarrier carrier) =>
            carrier != null && carrier.TryGetComponent(out NetworkObject player) && player.IsSpawned ? player.NetworkObjectId : 0;
    }
}

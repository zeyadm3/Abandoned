using Abandoned.Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// The host's side of every loot request, for its own player and for clients alike: validate with
    /// the same <see cref="PickupRules"/> as single-player (against the host's view of the player),
    /// apply, publish the hold state and move physics ownership. Also mirrors that state on clients.
    /// </summary>
    public static class LootServerActions
    {
        public static bool TryPickup(NetworkLoot loot, PlayerCarrier carrier, out string reason)
        {
            Grabbable item = loot.Grabbable;
            if (!PickupRules.CanPickUp(carrier, item, out reason)) return false;

            if (item.CarryClass == CarryClass.Pocket)
            {
                // Pocketed items have no physics, so the host keeps them.
                carrier.ApplyPocket(item);
                loot.ServerSetHold(LootHoldMode.Pocketed, carrier);
                return true;
            }

            carrier.ApplyHold(item);
            loot.ServerSetHold(LootHoldMode.Held, carrier);
            // CLAUDE.md: the single carrier owns the item's physics while carrying it.
            ulong owner = OwnerOf(carrier);
            if (loot.OwnerClientId != owner) loot.NetworkObject.ChangeOwnership(owner);
            loot.SyncPhysicsAuthority();
            return true;
        }

        /// <summary>Drop or throw. <paramref name="reported"/> is the carrier's own view of the item, used when believable.</summary>
        public static bool TryRelease(NetworkLoot loot, PlayerCarrier carrier, Vector3 velocity, bool isThrow, Pose? reported)
        {
            Grabbable item = loot.Grabbable;
            if (carrier == null || item.Holder != carrier) return false;

            bool remote = OwnerOf(carrier) != NetworkManager.ServerClientId;
            float max = MaxReleaseSpeed(loot.Config, carrier, item, isThrow, remote);
            Vector3 clamped = IsFinite(velocity) ? Vector3.ClampMagnitude(velocity, max) : Vector3.zero;

            // Settled and flying loot is host-simulated: take the physics back first, then let go.
            if (loot.OwnerClientId != NetworkManager.ServerClientId) loot.NetworkObject.RemoveOwnership();
            loot.SyncPhysicsAuthority();
            if (remote && reported.HasValue && IsFinite(reported.Value.position)
                && Vector3.Distance(reported.Value.position, carrier.EyePosition) <= loot.Config.MaxReleaseDistance)
            {
                // The host's copy trails the carrier by the interpolation delay; start from where they really let go.
                Pose pose = reported.Value;
                item.transform.SetPositionAndRotation(pose.position, pose.rotation);
                item.Body.position = pose.position;
                item.Body.rotation = pose.rotation;
            }

            carrier.ApplyRelease(clamped);
            loot.LastReleaseSpeed = clamped.magnitude;
            loot.ServerSetHold(LootHoldMode.Free, null);
            if (remote) loot.ServerTeleport();
            return true;
        }

        public static bool TryUnpocket(NetworkLoot loot, PlayerCarrier carrier, Vector3 aim, Vector3 inheritedVelocity, out string reason)
        {
            Grabbable item = loot.Grabbable;
            if (carrier == null || !item.IsPocketed || item.PocketHolder != carrier)
            {
                reason = "That's not in your pockets";
                return false;
            }
            if (!PickupRules.CanDropFromPocket(carrier, out reason)) return false;

            bool remote = OwnerOf(carrier) != NetworkManager.ServerClientId;
            float max = remote ? loot.Config.MaxInheritedSpeed : inheritedVelocity.magnitude;
            Vector3 velocity = IsFinite(inheritedVelocity) ? Vector3.ClampMagnitude(inheritedVelocity, max) : Vector3.zero;
            // The host's own view of the eyes; only the look direction comes from the client.
            Pose pose = carrier.UnpocketPose(IsFinite(aim) ? aim : Vector3.zero);
            carrier.ApplyUnpocket(item, pose.position, pose.rotation, velocity);
            loot.ServerSetHold(LootHoldMode.Free, null);
            loot.ServerTeleport();
            return true;
        }

        /// <summary>Full-charge throw plus what the player's movement may add (host's own player: its real velocity).</summary>
        public static float MaxReleaseSpeed(LootNetConfig config, PlayerCarrier carrier, Grabbable item, bool isThrow, bool remote)
        {
            float inherited = remote ? config.MaxInheritedSpeed : carrier.DropVelocity.magnitude;
            float thrown = isThrow && !item.IsDragged ? carrier.Config.ThrowSpeedFor(1f, item.Weight) : 0f;
            return thrown + inherited;
        }

        /// <summary>Host: the holder left the session; the item drops where they last were.</summary>
        public static void FreeOrphan(NetworkLoot loot)
        {
            Grabbable item = loot.Grabbable;
            if (loot.OwnerClientId != NetworkManager.ServerClientId) loot.NetworkObject.RemoveOwnership();
            loot.SyncPhysicsAuthority();
            if (item.IsPocketed) item.Unpocket(loot.LastHolderPosition + Vector3.up, item.transform.rotation, Vector3.zero);
            else item.EndHold(Vector3.zero);
            loot.ServerSetHold(LootHoldMode.Free, null);
            loot.ServerTeleport();
        }

        /// <summary>Client: make this machine's copies match the host's hold state.</summary>
        public static void Mirror(Grabbable item, LootHoldMode mode, PlayerCarrier target)
        {
            if (mode != LootHoldMode.Held && item.Holder != null) item.Holder.ReleaseItem(item, CurrentVelocity(item));
            if (mode != LootHoldMode.Pocketed && item.IsPocketed) UnpocketInPlace(item);

            if (mode == LootHoldMode.Held && item.Holder != target)
            {
                if (item.Holder != null) item.Holder.ReleaseItem(item, Vector3.zero);
                target.ApplyHold(item);
            }
            else if (mode == LootHoldMode.Pocketed && (!item.IsPocketed || item.PocketHolder != target))
            {
                if (item.IsPocketed) UnpocketInPlace(item);
                target.ApplyPocket(item);
            }
        }

        /// <summary>Lets go of the item on this machine only (it left the session).</summary>
        public static void ReleaseLocally(Grabbable item)
        {
            if (item == null) return;
            if (item.Holder != null) item.Holder.ReleaseItem(item, Vector3.zero);
            if (item.IsPocketed) UnpocketInPlace(item);
        }

        public static ulong OwnerOf(PlayerCarrier carrier) =>
            carrier != null && carrier.TryGetComponent(out NetworkObject player) && player.IsSpawned
                ? player.OwnerClientId
                : NetworkManager.ServerClientId;

        // Position comes from the network on this machine; only reveal it where it is.
        private static void UnpocketInPlace(Grabbable item)
        {
            Transform t = item.transform;
            if (item.PocketHolder != null) item.PocketHolder.ApplyUnpocket(item, t.position, t.rotation, Vector3.zero);
            else item.Unpocket(t.position, t.rotation, Vector3.zero);
        }

        private static Vector3 CurrentVelocity(Grabbable item) => item.Body.isKinematic ? Vector3.zero : item.Body.linearVelocity;

        private static bool IsFinite(Vector3 v) => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
    }
}

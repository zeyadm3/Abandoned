using Abandoned.Core;
using Abandoned.Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    public static partial class LootServerActions
    {
        /// <summary>Host: switch a shared pocket, stowing its old loot and physically presenting the new one.</summary>
        public static bool SelectPocket(PlayerCarrier carrier, int slot)
        {
            if (carrier == null || slot < 0 || slot >= carrier.Inventory.Capacity) return false;
            Grabbable held = carrier.Held;
            if (held != null && !carrier.Inventory.Contains(held))
            {
                if (held.CarryClass >= CarryClass.TwoHand || carrier.Inventory.AtSlot(slot) != null) return false;
                carrier.Inventory.Select(slot);
                return true;
            }
            if (held != null && carrier.Inventory.SlotOf(held) == slot) return true;
            if (held != null)
            {
                int previous = carrier.Inventory.SlotOf(held);
                NetworkLoot net = NetworkLoot.Of(held);
                if (net != null && net.OwnerClientId != NetworkManager.ServerClientId) net.NetworkObject.RemoveOwnership();
                carrier.ApplyRelease(Vector3.zero);
                carrier.ApplyPocket(held, previous);
                if (net != null) { net.SyncPhysicsAuthority(); net.ServerSetHold(LootHoldMode.Pocketed, carrier); }
            }
            carrier.Inventory.Select(slot);
            Grabbable item = carrier.Inventory.AtSlot(slot);
            if (item == null) return true;
            Pose pose = carrier.UnpocketPose(carrier.EyeForward);
            carrier.ApplyUnpocket(item, pose.position, pose.rotation, Vector3.zero);
            carrier.Inventory.Add(item, slot);
            carrier.ApplyHold(item);
            NetworkLoot loot = NetworkLoot.Of(item);
            if (loot != null)
            {
                loot.ServerSetHold(LootHoldMode.PocketHeld, carrier);
                ulong owner = OwnerOf(carrier);
                if (loot.OwnerClientId != owner) loot.NetworkObject.ChangeOwnership(owner);
                loot.SyncPhysicsAuthority();
            }
            return true;
        }
    }
}

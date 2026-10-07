using Abandoned.Interaction;
using Unity.Netcode;

namespace Abandoned.Networking
{
    /// <summary>
    /// Routes interaction requests for networked loot: on the host they're validated and applied at
    /// once (<see cref="LootServerActions"/>); on a client they become RPCs the host validates. Items
    /// that aren't spawned network objects (offline, test rigs) go to the single-player handler, so
    /// solo play behaves exactly as before. Stateless: one instance serves every session in the process.
    /// </summary>
    public sealed class NetworkInteractionHandler : IInteractionHandler
    {
        private readonly LocalInteractionHandler local = new();

        public void RequestPickup(PlayerCarrier carrier, Grabbable target)
        {
            if (target != null && target.Shared != null) { RequestGrab(carrier, target); return; }
            NetworkLoot loot = NetworkLoot.Of(target);
            if (loot == null) { PickUpOffline(carrier, target); return; }

            string reason;
            if (loot.IsServer)
            {
                if (!LootServerActions.TryPickup(loot, carrier, out reason)) carrier.ShowHint(reason);
                return;
            }
            // Same rules on the client first for instant feedback; the host checks again with its own view.
            if (!PickupRules.CanPickUp(carrier, target, out reason)) { carrier.ShowHint(reason); return; }
            loot.ClientRequestPickup();
        }

        /// <summary>Shared items: the host picks the free carry point nearest the player.</summary>
        private void RequestGrab(PlayerCarrier carrier, Grabbable target)
        {
            NetworkSharedCarry net = NetworkSharedCarry.Of(target);
            if (net == null) { PickUpOffline(carrier, target); return; }

            string reason;
            if (net.IsServer)
            {
                if (!SharedCarryServer.TryGrab(net, carrier, out reason)) carrier.ShowHint(reason);
                return;
            }
            if (!PickupRules.CanGrabPoint(carrier, target.Shared, out _, out reason)) { carrier.ShowHint(reason); return; }
            net.ClientRequestGrab();
        }

        // Not a spawned network item: single-player rules. But a network item that hasn't spawned here
        // yet (a client just joined) must wait for the host, or this machine would simulate it alone.
        private void PickUpOffline(PlayerCarrier carrier, Grabbable target)
        {
            if (target != null && target.TryGetComponent(out NetworkObject no) && !no.IsSpawned &&
                NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                carrier.ShowHint("Not ready yet");
                return;
            }
            local.RequestPickup(carrier, target);
        }

        public void RequestDrop(PlayerCarrier carrier) => Release(carrier, carrier.DropVelocity, isThrow: false);

        public void RequestThrow(PlayerCarrier carrier, UnityEngine.Vector3 velocity) => Release(carrier, velocity, isThrow: true);

        private void Release(PlayerCarrier carrier, UnityEngine.Vector3 velocity, bool isThrow)
        {
            if (carrier.Held == null) return;
            if (carrier.IsSharing)
            {
                // Throwing a handle is just letting go of it.
                NetworkSharedCarry net = NetworkSharedCarry.Of(carrier.Held);
                if (net == null) local.RequestDrop(carrier);
                else if (net.IsServer) SharedCarryServer.TryLetGo(net, carrier);
                else net.ClientRequestLetGo();
                return;
            }
            NetworkLoot loot = NetworkLoot.Of(carrier.Held);
            if (loot == null)
            {
                if (isThrow) local.RequestThrow(carrier, velocity);
                else local.RequestDrop(carrier);
                return;
            }
            if (loot.IsServer) LootServerActions.TryRelease(loot, carrier, velocity, isThrow, null);
            else loot.ClientRequestRelease(velocity, isThrow);
        }

        public void RequestDropFromPocket(PlayerCarrier carrier)
        {
            if (!PickupRules.CanDropFromPocket(carrier, out string reason)) { carrier.ShowHint(reason); return; }
            Grabbable last = carrier.Inventory.Items[^1];
            NetworkLoot loot = NetworkLoot.Of(last);
            if (loot == null) { local.RequestDropFromPocket(carrier); return; }

            if (!loot.IsServer) { loot.ClientRequestUnpocket(carrier.EyeForward, carrier.DropVelocity); return; }
            if (!LootServerActions.TryUnpocket(loot, carrier, carrier.EyeForward, carrier.DropVelocity, out reason))
                carrier.ShowHint(reason);
        }

        public bool RequestPlace(PlayerCarrier carrier)
        {
            if (carrier == null || !carrier.PlacementReady) return false;
            NetworkLoot loot = NetworkLoot.Of(carrier.Held);
            if (loot == null) return local.RequestPlace(carrier);
            if (!loot.IsServer) return loot.ClientRequestPlace();
            return LootServerActions.TryPlace(loot, carrier, carrier.Held.Body.linearVelocity,
                carrier.Held.Body.angularVelocity, null);
        }
    }
}

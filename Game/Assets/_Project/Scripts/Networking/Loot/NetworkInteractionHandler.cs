using Abandoned.Interaction;

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
            NetworkLoot loot = NetworkLoot.Of(target);
            if (loot == null) { local.RequestPickup(carrier, target); return; }

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

        public void RequestDrop(PlayerCarrier carrier) => Release(carrier, carrier.DropVelocity, isThrow: false);

        public void RequestThrow(PlayerCarrier carrier, UnityEngine.Vector3 velocity) => Release(carrier, velocity, isThrow: true);

        private void Release(PlayerCarrier carrier, UnityEngine.Vector3 velocity, bool isThrow)
        {
            if (carrier.Held == null) return;
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
    }
}

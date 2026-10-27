using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// Truck cosmetics (2026 remake): buying them with company money at the HQ's truck garage and fitting them.
    /// The host owns the decision (money, the crew's spending rule, the company level); every machine sees what the
    /// company owns and what is fitted through <see cref="CompanyNetState.TruckCosmetics"/> and <see cref="CompanyNetState.TruckLook"/>.
    /// </summary>
    public partial class CompanyService
    {
        public TruckCosmeticCatalog TruckCosmetics => TruckCosmeticCatalog.Load();

        /// <summary>Every machine: the company owns this catalog entry (stock fittings are always owned).</summary>
        public bool OwnsTruckCosmetic(int index) => index >= 0 && index < TruckCosmeticCatalog.Max && (State.TruckCosmetics & (1UL << index)) != 0;

        /// <summary>Every machine: what is fitted in a slot (catalog index; -1 = nothing).</summary>
        public int FittedTruckCosmetic(TruckCosmeticSlot slot) => State.Fitted(slot, TruckCosmetics);

        /// <summary>The garage's last answer ("" while none), for its status line.</summary>
        public string TruckCosmeticMessage { get; private set; } = "";

        /// <summary>Anyone at the garage: buy a fitting (it is fitted at once).</summary>
        public void RequestBuyTruckCosmetic(int index)
        {
            if (IsServer) BuyTruckCosmetic(index);
            else if (!RefuseLocally(CrewRule.Spend)) BuyTruckCosmeticRpc(index);
        }

        /// <summary>Anyone at the garage: fit an owned fitting (index -1: back to the slot's stock).</summary>
        public void RequestFitTruckCosmetic(TruckCosmeticSlot slot, int index)
        {
            if (IsServer) FitTruckCosmetic(slot, index);
            else if (!RefuseLocally(CrewRule.Spend)) FitTruckCosmeticRpc(slot, index);
        }

        [Rpc(SendTo.Server)]
        private void BuyTruckCosmeticRpc(int index, RpcParams rpcParams = default)
        {
            if (Allows(rpcParams.Receive.SenderClientId, CrewRule.Spend)) BuyTruckCosmetic(index);
        }

        [Rpc(SendTo.Server)]
        private void FitTruckCosmeticRpc(TruckCosmeticSlot slot, int index, RpcParams rpcParams = default)
        {
            if (Allows(rpcParams.Receive.SenderClientId, CrewRule.Spend)) FitTruckCosmetic(slot, index);
        }

        private void BuyTruckCosmetic(int index)
        {
            TruckCosmeticDefinition d = TruckCosmetics != null ? TruckCosmetics.At(index) : null;
            if (d == null || d.Stock || save.Owns(d.Key) || d.UnlockLevel > save.level || save.money < d.Price) return;
            save.money -= d.Price;
            save.Unlock(d.Key);
            Fit(d);
            Saved();
            Debug.Log($"[Company] Truck cosmetic: {d.DisplayName} for ${d.Price:N0}; ${save.money:N0} left.");
        }

        private void FitTruckCosmetic(TruckCosmeticSlot slot, int index)
        {
            TruckCosmeticCatalog catalog = TruckCosmetics;
            if (catalog == null) return;
            save.truckLook ??= new System.Collections.Generic.List<string>();
            if (index < 0)
            {
                save.truckLook.RemoveAll(e => e.StartsWith(slot + ":"));
                Saved();
                return;
            }
            TruckCosmeticDefinition d = catalog.At(index);
            if (d == null || d.Slot != slot || !(d.Stock || save.Owns(d.Key))) return;
            Fit(d);
            Saved();
        }

        private void Fit(TruckCosmeticDefinition d)
        {
            save.truckLook ??= new System.Collections.Generic.List<string>();
            save.truckLook.RemoveAll(e => e.StartsWith(d.Slot + ":"));
            save.truckLook.Add(d.Slot + ":" + d.Id);
        }
    }
}

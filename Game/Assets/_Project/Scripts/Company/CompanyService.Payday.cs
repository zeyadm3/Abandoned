using System;
using System.Collections.Generic;
using Abandoned.Contracts;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Extraction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>Payday after a job, the gear shop and company stock.</summary>
    public partial class CompanyService
    {
        // ---- Payday ----

        private void OnDeparted(RunResults results)
        {
            if (!IsServer || !active.Value.IsValid) return;
            RunOutcome o = CompanyLedger.Apply(save, config, results.Haul, results.Quota, results.Wiped ? 0f : active.Value.PayoutBonus);
            save.pendingQuota = 0;
            save.pendingBonus = 0f;
            save.pendingLeftOnPurpose = false;
            Saved();
            lastOutcome.Value = OutcomeNet.Of(o, save.runs + save.bankruptcies * 1000);
            Debug.Log($"[Company] Run {save.runs}: payout ${o.Payout:N0}, penalty ${o.Penalty:N0}, +{o.Xp} xp, money ${save.money:N0}" +
                      (o.Bankrupt ? " - BANKRUPT, a new company starts" : ""));
        }

        /// <summary>Host: spend (the shop, 6.4). Saves at once.</summary>
        public bool TrySpend(string itemId, int price)
        {
            if (!IsServer || !CompanyLedger.TryBuy(save, itemId, price)) return false;
            Saved();
            return true;
        }

        /// <summary>Host: a wardrobe purchase (0.12.5). Money only: a coverall isn't company stock.</summary>
        public bool TryCharge(int price, string what)
        {
            if (!IsServer || save == null || price < 0 || save.money < price) return false;
            save.money -= price;
            Saved();
            Debug.Log($"[Company] Wardrobe: {what} for ${price:N0}; ${save.money:N0} left.");
            return true;
        }

        /// <summary>Anyone at the shop: buy one of a catalog item (the host checks level and money).</summary>
        public void RequestBuy(int index)
        {
            if (IsServer) Buy(index);
            else if (!RefuseLocally(CrewRule.Spend)) BuyRpc(index);
        }

        [Rpc(SendTo.Server)]
        private void BuyRpc(int index, RpcParams rpcParams = default)
        {
            if (Allows(rpcParams.Receive.SenderClientId, CrewRule.Spend)) Buy(index);
        }

        private void Buy(int index)
        {
            EquipmentDefinition d = equipment != null ? equipment.At(index) : null;
            if (d == null || d.UnlockLevel > save.level) return;
            if (TrySpend(d.Id, d.Price)) Debug.Log($"[Company] Bought {d.DisplayName} for ${d.Price:N0}; ${save.money:N0} left.");
        }

        /// <summary>Host: a consumable was used up (medkit, planks, noise maker).</summary>
        public bool Consume(int index)
        {
            EquipmentDefinition d = equipment != null ? equipment.At(index) : null;
            if (!IsServer || d == null || !save.Remove(d.Id)) return false;
            Saved();
            return true;
        }
    }
}

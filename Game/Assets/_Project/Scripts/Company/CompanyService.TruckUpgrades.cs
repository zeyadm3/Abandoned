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
    /// <summary>Truck upgrades (M10.1, GDD 13): what the company owns and buying the next tier.</summary>
    public partial class CompanyService
    {
        // ---- Truck upgrades (M10.1, GDD 13) ----

        public TruckUpgradeCatalog TruckUpgrades => truckUpgrades;
        public CompanyMessages Messages => messages;

        /// <summary>Every machine: the company owns this catalog entry.</summary>
        public bool OwnsUpgrade(int index) => index >= 0 && index < TruckUpgradeCatalog.MaxUpgrades && (State.TruckUpgrades & (1 << index)) != 0;

        /// <summary>Every machine: the best owned tier's amount of a kind, or <paramref name="none"/> without one.</summary>
        public float UpgradeAmount(TruckUpgradeKind kind, float none)
        {
            TruckUpgradeDefinition best = BestOwned(kind);
            return best != null ? best.Amount : none;
        }

        public bool HasUpgrade(TruckUpgradeKind kind) => BestOwned(kind) != null;

        private TruckUpgradeDefinition BestOwned(TruckUpgradeKind kind)
        {
            TruckUpgradeDefinition best = null;
            if (truckUpgrades == null) return null;
            for (int i = 0; i < truckUpgrades.Items.Count; i++)
            {
                TruckUpgradeDefinition d = truckUpgrades.Items[i];
                if (d != null && d.Kind == kind && OwnsUpgrade(i) && (best == null || d.Tier > best.Tier)) best = d;
            }
            return best;
        }

        /// <summary>Every machine: can the company buy this upgrade now (level, the tier below, not owned yet)? Money aside.</summary>
        public bool UpgradeAvailable(int index)
        {
            TruckUpgradeDefinition d = truckUpgrades != null ? truckUpgrades.At(index) : null;
            if (d == null || OwnsUpgrade(index) || d.UnlockLevel > State.Level) return false;
            TruckUpgradeDefinition previous = truckUpgrades.Previous(d);
            return previous == null || OwnsUpgrade(truckUpgrades.IndexOf(previous));
        }

        /// <summary>Anyone at the shop: buy a truck upgrade (the host checks level, tier and money).</summary>
        public void RequestBuyUpgrade(int index)
        {
            if (IsServer) BuyUpgrade(index);
            else if (!RefuseLocally(CrewRule.Spend)) BuyUpgradeRpc(index);
        }

        [Rpc(SendTo.Server)]
        private void BuyUpgradeRpc(int index, RpcParams rpcParams = default)
        {
            if (Allows(rpcParams.Receive.SenderClientId, CrewRule.Spend)) BuyUpgrade(index);
        }

        private void BuyUpgrade(int index)
        {
            TruckUpgradeDefinition d = truckUpgrades != null ? truckUpgrades.At(index) : null;
            if (d == null || save.Owns(d.Id) || d.UnlockLevel > save.level || d.Price < 0 || save.money < d.Price) return;
            TruckUpgradeDefinition previous = truckUpgrades.Previous(d);
            if (previous != null && !save.Owns(previous.Id)) return;
            save.money -= d.Price;
            save.Unlock(d.Id);
            Saved();
            Debug.Log($"[Company] Truck upgrade: {d.DisplayName} for ${d.Price:N0}; ${save.money:N0} left.");
        }
    }
}

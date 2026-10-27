using System;
using Unity.Collections;
using Unity.Netcode;

namespace Abandoned.Company
{
    /// <summary>The company as every player sees it (the save itself stays on the host).</summary>
    public struct CompanyNetState : INetworkSerializable, IEquatable<CompanyNetState>
    {
        public int Money, Xp, Level, MissedQuotas, Runs, Bankruptcies, BestHaul;
        /// <summary>Owned truck upgrades (M10.1): bit i = the upgrade catalog's entry i.</summary>
        public int TruckUpgrades;
        /// <summary>The company's name (QA B-24: the host can rename it).</summary>
        public Unity.Collections.FixedString64Bytes Name;
        /// <summary>2026 remake: owned truck cosmetics (bit i = TruckCosmeticCatalog entry i).</summary>
        public ulong TruckCosmetics;
        /// <summary>2026 remake: the fitted truck cosmetic per slot, as catalog index + 1 in byte (slot) (0 = the slot's stock).</summary>
        public ulong TruckLook;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Money);
            s.SerializeValue(ref Xp);
            s.SerializeValue(ref Level);
            s.SerializeValue(ref MissedQuotas);
            s.SerializeValue(ref Runs);
            s.SerializeValue(ref Bankruptcies);
            s.SerializeValue(ref BestHaul);
            s.SerializeValue(ref TruckUpgrades);
            s.SerializeValue(ref Name);
            s.SerializeValue(ref TruckCosmetics);
            s.SerializeValue(ref TruckLook);
        }

        public static CompanyNetState Of(CompanySave s, TruckUpgradeCatalog upgrades = null) => new()
        {
            Money = s.money, Xp = s.xp, Level = s.level, MissedQuotas = s.missedQuotas, Runs = s.runs,
            Bankruptcies = s.bankruptcies, BestHaul = s.bestHaul, TruckUpgrades = MaskOf(s, upgrades),
            Name = NameOf(s.companyName),
            TruckCosmetics = CosmeticsOf(s, TruckCosmeticCatalog.Load()),
            TruckLook = LookOf(s, TruckCosmeticCatalog.Load()),
        };

        private static ulong CosmeticsOf(CompanySave s, TruckCosmeticCatalog catalog)
        {
            ulong mask = 0;
            if (catalog == null) return mask;
            for (int i = 0; i < catalog.Items.Count && i < TruckCosmeticCatalog.Max; i++)
                if (catalog.Items[i] != null && (catalog.Items[i].Stock || s.Owns(catalog.Items[i].Key))) mask |= 1UL << i;
            return mask;
        }

        private static ulong LookOf(CompanySave s, TruckCosmeticCatalog catalog)
        {
            ulong look = 0;
            if (catalog == null || s.truckLook == null) return look;
            foreach (string entry in s.truckLook)
            {
                int colon = entry.IndexOf(':');
                if (colon <= 0 || !System.Enum.TryParse(entry.Substring(0, colon), out TruckCosmeticSlot slot)) continue;
                string id = entry.Substring(colon + 1);
                for (int i = 0; i < catalog.Items.Count && i < 255; i++)
                    if (catalog.Items[i] != null && catalog.Items[i].Id == id && catalog.Items[i].Slot == slot)
                        look |= (ulong)(i + 1) << ((int)slot * 8);
            }
            return look;
        }

        /// <summary>The catalog index fitted in a slot (the slot's stock when none is chosen; -1 = nothing).</summary>
        public int Fitted(TruckCosmeticSlot slot, TruckCosmeticCatalog catalog)
        {
            int v = (int)((TruckLook >> ((int)slot * 8)) & 0xFF);
            return v > 0 ? v - 1 : catalog != null ? catalog.StockOf(slot) : -1;
        }

        // The wire holds 61 UTF-8 bytes; a name is cleaned to fit when it's set, this is the backstop.
        private static Unity.Collections.FixedString64Bytes NameOf(string name)
        {
            var fs = new Unity.Collections.FixedString64Bytes();
            fs.CopyFromTruncated(name ?? "");
            return fs;
        }

        private static int MaskOf(CompanySave s, TruckUpgradeCatalog upgrades)
        {
            int mask = 0;
            if (upgrades == null) return mask;
            for (int i = 0; i < upgrades.Items.Count && i < TruckUpgradeCatalog.MaxUpgrades; i++)
                if (upgrades.Items[i] != null && s.Owns(upgrades.Items[i].Id)) mask |= 1 << i;
            return mask;
        }

        public bool Equals(CompanyNetState o) => Money == o.Money && Xp == o.Xp && Level == o.Level && MissedQuotas == o.MissedQuotas
                                                 && Runs == o.Runs && Bankruptcies == o.Bankruptcies && BestHaul == o.BestHaul && TruckUpgrades == o.TruckUpgrades
                                                 && Name.Equals(o.Name) && TruckCosmetics == o.TruckCosmetics && TruckLook == o.TruckLook;
    }
}

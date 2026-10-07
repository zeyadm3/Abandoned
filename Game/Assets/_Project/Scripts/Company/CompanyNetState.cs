using System;
using Unity.Netcode;

namespace Abandoned.Company
{
    /// <summary>The company as every player sees it (the save itself stays on the host).</summary>
    public struct CompanyNetState : INetworkSerializable, IEquatable<CompanyNetState>
    {
        public int Money, Xp, Level, MissedQuotas, Runs, Bankruptcies, BestHaul;
        /// <summary>Owned truck upgrades (M10.1): bit i = the upgrade catalog's entry i.</summary>
        public int TruckUpgrades;

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
        }

        public static CompanyNetState Of(CompanySave s, TruckUpgradeCatalog upgrades = null) => new()
        {
            Money = s.money, Xp = s.xp, Level = s.level, MissedQuotas = s.missedQuotas, Runs = s.runs,
            Bankruptcies = s.bankruptcies, BestHaul = s.bestHaul, TruckUpgrades = MaskOf(s, upgrades),
        };

        private static int MaskOf(CompanySave s, TruckUpgradeCatalog upgrades)
        {
            int mask = 0;
            if (upgrades == null) return mask;
            for (int i = 0; i < upgrades.Items.Count && i < TruckUpgradeCatalog.MaxUpgrades; i++)
                if (upgrades.Items[i] != null && s.Owns(upgrades.Items[i].Id)) mask |= 1 << i;
            return mask;
        }

        public bool Equals(CompanyNetState o) => Money == o.Money && Xp == o.Xp && Level == o.Level && MissedQuotas == o.MissedQuotas
                                                 && Runs == o.Runs && Bankruptcies == o.Bankruptcies && BestHaul == o.BestHaul && TruckUpgrades == o.TruckUpgrades;
    }
}

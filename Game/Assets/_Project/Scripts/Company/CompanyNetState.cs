using System;
using Unity.Netcode;

namespace Abandoned.Company
{
    /// <summary>The company as every player sees it (the save itself stays on the host).</summary>
    public struct CompanyNetState : INetworkSerializable, IEquatable<CompanyNetState>
    {
        public int Money, Xp, Level, MissedQuotas, Runs, Bankruptcies, BestHaul;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Money);
            s.SerializeValue(ref Xp);
            s.SerializeValue(ref Level);
            s.SerializeValue(ref MissedQuotas);
            s.SerializeValue(ref Runs);
            s.SerializeValue(ref Bankruptcies);
            s.SerializeValue(ref BestHaul);
        }

        public static CompanyNetState Of(CompanySave s) => new()
        {
            Money = s.money, Xp = s.xp, Level = s.level, MissedQuotas = s.missedQuotas, Runs = s.runs,
            Bankruptcies = s.bankruptcies, BestHaul = s.bestHaul,
        };

        public bool Equals(CompanyNetState o) => Money == o.Money && Xp == o.Xp && Level == o.Level && MissedQuotas == o.MissedQuotas
                                                 && Runs == o.Runs && Bankruptcies == o.Bankruptcies && BestHaul == o.BestHaul;
    }
}

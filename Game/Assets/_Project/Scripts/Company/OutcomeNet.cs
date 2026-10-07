using System;
using Unity.Netcode;

namespace Abandoned.Company
{
    /// <summary>The last run's effect on the company, for the appraisal on every machine.</summary>
    public struct OutcomeNet : INetworkSerializable, IEquatable<OutcomeNet>
    {
        public int Run, Payout, Penalty, Xp, NewLevel, MissedInARow;
        public bool QuotaMet, LevelledUp, Bankrupt;

        public static OutcomeNet Of(RunOutcome o, int run) => new()
        {
            Run = run, Payout = o.Payout, Penalty = o.Penalty, Xp = o.Xp, NewLevel = o.NewLevel, MissedInARow = o.MissedInARow,
            QuotaMet = o.QuotaMet, LevelledUp = o.LevelledUp, Bankrupt = o.Bankrupt,
        };

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Run);
            s.SerializeValue(ref Payout);
            s.SerializeValue(ref Penalty);
            s.SerializeValue(ref Xp);
            s.SerializeValue(ref NewLevel);
            s.SerializeValue(ref MissedInARow);
            s.SerializeValue(ref QuotaMet);
            s.SerializeValue(ref LevelledUp);
            s.SerializeValue(ref Bankrupt);
        }

        public bool Equals(OutcomeNet o) => Run == o.Run && Payout == o.Payout && Penalty == o.Penalty && Xp == o.Xp && Bankrupt == o.Bankrupt;
    }
}

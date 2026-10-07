namespace Abandoned.Company
{
    /// <summary>What one run did to the company (shown on the appraisal, applied to the save).</summary>
    public readonly struct RunOutcome
    {
        public readonly int Haul, Quota, Payout, Penalty, Xp, NewLevel, MissedInARow;
        public readonly bool QuotaMet, LevelledUp, Bankrupt;

        public RunOutcome(int haul, int quota, int payout, int penalty, int xp, int newLevel, bool levelledUp, int missed, bool bankrupt)
        {
            Haul = haul;
            Quota = quota;
            Payout = payout;
            Penalty = penalty;
            Xp = xp;
            NewLevel = newLevel;
            LevelledUp = levelledUp;
            MissedInARow = missed;
            QuotaMet = haul >= quota;
            Bankrupt = bankrupt;
        }

        public int Net => Payout - Penalty;
    }
}

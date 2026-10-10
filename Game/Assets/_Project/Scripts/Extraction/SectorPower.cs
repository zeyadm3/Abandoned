namespace Abandoned.Extraction
{
    /// <summary>
    /// Does a power sector have power right now (0.16.1+): its breaker is thrown, or the mains are on and the
    /// danger hasn't cut it. Read on every machine (the lift, the security desk, live water).
    /// </summary>
    public static class SectorPower
    {
        public static bool Live(int sector)
        {
            RunState run = RunState.Current;
            if (run == null || !run.IsSpawned) return true;
            if (RunBreakers.Current != null && RunBreakers.Current.Restored(sector)) return true;
            if (run.State.PowerOff) return false;
            RunHorrorDirector horror = RunHorrorDirector.Current;
            return horror == null || horror.Config == null || horror.Config.SectorPowered(sector, run.State.Danger);
        }
    }
}

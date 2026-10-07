namespace Abandoned.Threats
{
    public enum StalkerState : byte { Follow, Frozen, Rush }

    /// <summary>
    /// The Stalker's rule, pure: while its target watches it, it freezes and the dread resets; while the
    /// target is alone and looking away, dread builds; enough of it and it rushes (it still freezes if
    /// they turn round in time). Company makes it lose interest: dread fades when they're not alone.
    /// </summary>
    public sealed class StalkerBrain
    {
        /// <summary>The tuning in use (swappable: danger or tests may hand it another).</summary>
        public StalkerConfig Config { get; set; }

        public StalkerState State { get; private set; } = StalkerState.Follow;
        public float Dread { get; private set; }

        public StalkerBrain(StalkerConfig config) => Config = config;

        public void Tick(bool watched, bool isolated, float dt)
        {
            if (watched)
            {
                Dread = 0f;
                State = StalkerState.Frozen;
                return;
            }
            Dread = isolated ? Dread + dt : System.Math.Max(0f, Dread - dt * 0.5f);
            State = Dread >= Config.RushAfter ? StalkerState.Rush : StalkerState.Follow;
        }

        public void Reset()
        {
            Dread = 0f;
            State = StalkerState.Follow;
        }
    }
}

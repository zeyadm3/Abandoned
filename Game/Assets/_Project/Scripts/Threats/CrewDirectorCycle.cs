namespace Abandoned.Threats
{
    /// <summary>
    /// Build up until the most stressed player peaks, hold it a few seconds, let the fight end, then a quiet
    /// stretch, and again. Plain C# fed the highest stress each tick, so it can be tested without a scene.
    /// </summary>
    public sealed class CrewDirectorCycle
    {
        private readonly System.Random random;
        private float timer;

        public CrewDirectorPhase Phase { get; private set; } = CrewDirectorPhase.BuildUp;
        /// <summary>Seconds spent in the current phase.</summary>
        public float InPhase { get; private set; }
        /// <summary>Seconds left in a timed phase (sustain, relax); 0 otherwise.</summary>
        public float Remaining => Phase == CrewDirectorPhase.SustainPeak || Phase == CrewDirectorPhase.Relax ? timer : 0f;
        /// <summary>Fade and relax: no new monsters, and idle ones keep away from the crew.</summary>
        public bool Quiet => Phase == CrewDirectorPhase.PeakFade || Phase == CrewDirectorPhase.Relax;

        public CrewDirectorCycle(int seed) => random = new System.Random(seed ^ 0x4C3D);

        public void Tick(float dt, float highest, CrewDirectorConfig config)
        {
            InPhase += dt;
            switch (Phase)
            {
                case CrewDirectorPhase.BuildUp:
                    if (highest >= config.Peak && InPhase >= config.MinBuildUp)
                        Enter(CrewDirectorPhase.SustainPeak, CrewDirectorConfig.Roll(config.SustainSeconds, random));
                    break;
                case CrewDirectorPhase.SustainPeak:
                    timer -= dt;
                    if (timer <= 0f) Enter(CrewDirectorPhase.PeakFade, 0f);
                    break;
                case CrewDirectorPhase.PeakFade:
                    // Capped: a monster loitering near someone must not hold the quiet open forever.
                    if (highest <= config.FadeEndsBelow || InPhase >= config.MaxFade)
                        Enter(CrewDirectorPhase.Relax, CrewDirectorConfig.Roll(config.RelaxSeconds, random));
                    break;
                case CrewDirectorPhase.Relax:
                    timer -= dt;
                    if (timer <= 0f) Enter(CrewDirectorPhase.BuildUp, 0f);
                    break;
            }
        }

        private void Enter(CrewDirectorPhase phase, float seconds)
        {
            Phase = phase;
            InPhase = 0f;
            timer = seconds;
        }
    }
}

namespace Abandoned.Onboarding
{
    /// <summary>How far this player is through being hired (2026 remake). Append only: it is saved as a number.</summary>
    public enum OnboardingStage
    {
        New,
        Interviewed,
        Trained,
        Hired,
    }
}

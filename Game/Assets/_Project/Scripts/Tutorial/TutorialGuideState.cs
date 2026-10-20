namespace Abandoned.Tutorial
{
    /// <summary>
    /// Host: what the guide remembers within one lesson, so a bar fills from where the crew started rather than
    /// from nothing (the farthest anyone was from the target, the farthest the item was from where it goes).
    /// Forgotten when the lesson or its target changes.
    /// </summary>
    public sealed class TutorialGuideState
    {
        public string TargetId = "";
        public float ApproachFrom = -1f;
        public float CarryFrom = -1f;

        public void Reset()
        {
            TargetId = "";
            ApproachFrom = CarryFrom = -1f;
        }

        /// <summary>A new target starts its bars again.</summary>
        public void Aim(string targetId)
        {
            if (targetId == TargetId) return;
            TargetId = targetId;
            ApproachFrom = CarryFrom = -1f;
        }
    }
}

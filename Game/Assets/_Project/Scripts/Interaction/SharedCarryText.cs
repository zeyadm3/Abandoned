namespace Abandoned.Interaction
{
    /// <summary>HUD wording for shared carries, kept pure so it's testable and the same on every screen.</summary>
    public static class SharedCarryText
    {
        public static string Status(int carriers, int required, bool canDragAlone)
        {
            if (carriers >= required) return $"Carrying {carriers}/{required} - lifted";
            int missing = required - carriers;
            string more = $"needs {missing} more {(missing == 1 ? "person" : "people")}";
            string drag = canDragAlone ? " (dragging)" : carriers > 0 ? " (barely budges)" : "";
            return $"Carrying {carriers}/{required} - {more}{drag}";
        }

        /// <summary>What aiming at a shared item says before you grab it.</summary>
        public static string Crew(int carriers, int required) =>
            carriers == 0 ? $"needs {required} people" : $"{carriers}/{required} holding";

        public static string Of(SharedCarryable shared) =>
            Status(shared.CarrierCount, shared.RequiredCarriers, shared.CanBeDraggedUnderCrewed);
    }
}

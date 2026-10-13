using Abandoned.Core;
namespace Abandoned.Interaction
{
    /// <summary>HUD wording for shared carries, kept pure so it's testable and the same on every screen.</summary>
    public static class SharedCarryText
    {
        public static string Status(int carriers, int required, bool canDragAlone)
        {
            if (carriers >= required) return Loc.T("Carrying {0}/{1} - lifted", carriers, required);
            int missing = required - carriers;
            string line = Loc.T("Carrying {0}/{1} - {2}", carriers, required, Loc.N(missing, "needs {0} more person", "needs {0} more people"));
            return canDragAlone ? Loc.T("{0} (dragging)", line) : carriers > 0 ? Loc.T("{0} (barely budges)", line) : line;
        }

        /// <summary>What aiming at a shared item says before you grab it.</summary>
        public static string Crew(int carriers, int required) =>
            carriers == 0 ? Loc.N(required, "needs {0} person", "needs {0} people") : Loc.T("{0}/{1} holding", carriers, required);

        public static string Of(SharedCarryable shared) =>
            Status(shared.CarrierCount, shared.RequiredCarriers, shared.CanBeDraggedUnderCrewed);
    }
}

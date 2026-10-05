namespace Abandoned.Interaction
{
    /// <summary>
    /// The host's validation for interaction requests. Pure rules with no side effects, so the
    /// same checks run for local play and for client requests arriving over the network.
    /// </summary>
    public static class PickupRules
    {
        public static bool CanPickUp(PlayerCarrier carrier, Grabbable target, out string reason)
        {
            reason = null;
            if (carrier == null || target == null) { reason = "Nothing to pick up"; return false; }
            if (!target.IsAvailable) { reason = "Someone else has it"; return false; }

            CarryConfig config = carrier.Config;
            float maxReach = config.Reach + config.ReachTolerance;
            if (target.GetBounds().SqrDistance(carrier.EyePosition) > maxReach * maxReach)
            {
                reason = "Too far away";
                return false;
            }

            if (target.CarryClass > config.HeaviestSoloClass)
            {
                reason = target.CarryClass == CarryClass.Huge
                    ? "Too heavy — needs 3–4 people or a trolley and ramp"
                    : "Too heavy — needs 2 people or a hand trolley";
                return false;
            }

            if (target.CarryClass == CarryClass.Pocket)
            {
                if (carrier.Inventory.HasSpace) return true;
                reason = "Pockets full";
                return false;
            }

            if (carrier.Held != null) { reason = "Hands full"; return false; }
            return true;
        }

        public static bool CanDropFromPocket(PlayerCarrier carrier, out string reason)
        {
            reason = null;
            if (carrier.Inventory.Count == 0) { reason = "Pockets empty"; return false; }
            if (carrier.Held != null) { reason = "Hands full"; return false; }
            return true;
        }
    }
}

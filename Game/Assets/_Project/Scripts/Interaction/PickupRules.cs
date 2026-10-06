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
            // Team items are only ever held by their carry points (CanGrabPoint), never picked up whole.
            if (target.Shared != null) { reason = "Grab one of its handles"; return false; }
            // A pickup that reaches the host after the player was knocked down would leave them holding it while down.
            if (carrier.IsRagdolled) { reason = "You're down"; return false; }
            if (!target.IsAvailable) { reason = "Someone else has it"; return false; }

            CarryConfig config = carrier.Config;
            float maxReach = config.Reach + config.ReachTolerance;
            if (target.GetBounds().SqrDistance(carrier.EyePosition) > maxReach * maxReach)
            {
                reason = "Too far away";
                return false;
            }

            if (target.CarryClass > config.HeaviestSoloClass && !config.CanSoloDrag(target.CarryClass))
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

        /// <summary>A free carry point of a shared item within reach; <paramref name="point"/> is the one nearest the player.</summary>
        public static bool CanGrabPoint(PlayerCarrier carrier, SharedCarryable target, out int point, out string reason)
        {
            point = -1;
            reason = null;
            if (carrier == null || target == null) { reason = "Nothing to pick up"; return false; }
            Grabbable item = target.Grabbable;
            if (item.IsPocketed || item.Holder != null) { reason = "Someone else has it"; return false; }
            if (target.IsCarriedBy(carrier)) { reason = "You're already holding it"; return false; }
            if (carrier.Held != null) { reason = "Hands full"; return false; }
            if (carrier.IsRagdolled) { reason = "You're down"; return false; }

            point = target.NearestFreePoint(carrier.transform.position);
            if (point < 0) { reason = "No free handhold"; return false; }
            CarryConfig config = carrier.Config;
            float maxReach = config.Reach + config.ReachTolerance;
            if ((target.PointWorld(point) - carrier.EyePosition).sqrMagnitude > maxReach * maxReach)
            {
                point = -1;
                reason = "Too far away";
                return false;
            }
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

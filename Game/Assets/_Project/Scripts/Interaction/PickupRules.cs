using Abandoned.Core;

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
            if (carrier == null || target == null) { reason = Loc.Mark("Nothing to pick up"); return false; }
            // Team items are only ever held by their carry points (CanGrabPoint), never picked up whole.
            if (target.Shared != null) { reason = Loc.Mark("Grab one of its handles"); return false; }
            // A pickup that reaches the host after the player was knocked down would leave them holding it while down.
            if (carrier.IsRagdolled) { reason = Loc.Mark("You're down"); return false; }
            if (!target.IsAvailable) { reason = Loc.Mark("Someone else has it"); return false; }

            CarryConfig config = carrier.Config;
            float maxReach = config.Reach + config.ReachTolerance;
            if (target.GetBounds().SqrDistance(carrier.EyePosition) > maxReach * maxReach)
            {
                reason = Loc.Mark("Too far away");
                return false;
            }

            if (target.CarryClass > config.HeaviestSoloClass && !carrier.CanSoloDrag(target.CarryClass))
            {
                reason = target.CarryClass == CarryClass.Huge
                    ? Loc.Mark("Too heavy — needs 3–4 people or a trolley and ramp")
                    : Loc.Mark("Too heavy — needs 2 people or a hand trolley");
                return false;
            }

            if (target.CarryClass == CarryClass.Pocket)
            {
                if (carrier.Inventory.HasSpace) return true;
                reason = Loc.Mark("Pockets full");
                return false;
            }

            if (carrier.Held != null) { reason = Loc.Mark("Hands full"); return false; }
            return true;
        }

        /// <summary>A free carry point of a shared item within reach; <paramref name="point"/> is the one nearest the player.</summary>
        public static bool CanGrabPoint(PlayerCarrier carrier, SharedCarryable target, out int point, out string reason)
        {
            point = -1;
            reason = null;
            if (carrier == null || target == null) { reason = Loc.Mark("Nothing to pick up"); return false; }
            Grabbable item = target.Grabbable;
            if (item.IsPocketed || item.Holder != null) { reason = Loc.Mark("Someone else has it"); return false; }
            if (target.IsCarriedBy(carrier)) { reason = Loc.Mark("You're already holding it"); return false; }
            if (carrier.Held != null) { reason = Loc.Mark("Hands full"); return false; }
            if (carrier.IsRagdolled) { reason = Loc.Mark("You're down"); return false; }

            point = target.NearestFreePoint(carrier.transform.position);
            if (point < 0) { reason = Loc.Mark("No free handhold"); return false; }
            CarryConfig config = carrier.Config;
            float maxReach = config.Reach + config.ReachTolerance;
            if ((target.PointWorld(point) - carrier.EyePosition).sqrMagnitude > maxReach * maxReach)
            {
                point = -1;
                reason = Loc.Mark("Too far away");
                return false;
            }
            return true;
        }

        public static bool CanDropFromPocket(PlayerCarrier carrier, out string reason)
        {
            reason = null;
            if (carrier.Inventory.Count == 0) { reason = Loc.Mark("Pockets empty"); return false; }
            if (carrier.Held != null) { reason = Loc.Mark("Hands full"); return false; }
            return true;
        }
    }
}

using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// Single-player handler: this machine is the host, so validate and apply immediately.
    /// </summary>
    public class LocalInteractionHandler : IInteractionHandler
    {
        public void RequestPickup(PlayerCarrier carrier, Grabbable target)
        {
            if (target != null && target.Shared != null)
            {
                if (PickupRules.CanGrabPoint(carrier, target.Shared, out int point, out string why)) target.Shared.Grab(point, carrier);
                else carrier.ShowHint(why);
                return;
            }
            if (!PickupRules.CanPickUp(carrier, target, out string reason))
            {
                carrier.ShowHint(reason);
                return;
            }

            if (target.CarryClass == CarryClass.Pocket) carrier.ApplyPocket(target);
            else carrier.ApplyHold(target);
        }

        public void RequestDrop(PlayerCarrier carrier)
        {
            if (carrier.Held != null) carrier.ApplyRelease(carrier.DropVelocity);
        }

        public void RequestThrow(PlayerCarrier carrier, Vector3 velocity)
        {
            if (carrier.Held == null) return;
            // Nobody throws a piano: letting go of a handle is all a throw does.
            if (carrier.IsSharing) { carrier.ApplyRelease(Vector3.zero); return; }
            // Clamp to what the rules allow at full charge, so a modified client can't fling loot.
            float max = carrier.Config.ThrowSpeedFor(1f, carrier.Held.Weight) + carrier.DropVelocity.magnitude;
            carrier.ApplyRelease(Vector3.ClampMagnitude(velocity, max));
        }

        public void RequestDropFromPocket(PlayerCarrier carrier)
        {
            if (!PickupRules.CanDropFromPocket(carrier, out string reason))
            {
                carrier.ShowHint(reason);
                return;
            }
            carrier.ApplyUnpocketLast();
        }

        public bool RequestPlace(PlayerCarrier carrier)
        {
            if (carrier == null || !carrier.PlacementReady) return false;
            Grabbable item = carrier.Held;
            Vector3 velocity = item.Body.linearVelocity;
            Vector3 spin = item.Body.angularVelocity;
            carrier.ApplyRelease(velocity);
            item.Body.angularVelocity = spin;
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>Who holds which carry point (only after a handler validated it, or mirroring the host), and the nudge budget.</summary>
    public partial class SharedCarryable
    {
        // ---- Membership (only after an IInteractionHandler validated it, or mirroring the host) ----

        internal void Grab(int index, PlayerCarrier carrier) => SetCarrier(index, carrier, GripFor(index, carrier));

        internal void SetCarrier(int index, PlayerCarrier carrier, Vector3 grip)
        {
            if (carriers[index] == carrier)
            {
                grips[index] = grip;
                return;
            }
            if (carriers[index] != null) Detach(index);
            if (carrier != null)
            {
                int previous = IndexOf(carrier);
                if (previous >= 0) Detach(previous);
                // A stale mirror may still show them holding something else; the host says otherwise.
                if (carrier.Held != null && carrier.Held != grabbable) carrier.ApplyRelease(Vector3.zero);
                carriers[index] = carrier;
                grips[index] = grip;
                inputTimes[index] = float.NegativeInfinity;
                CarrierCount++;
                grabbable.SetIgnoreCollisions(carrier.Controller, true);
                carrier.AttachShared(grabbable);
            }
            Changed();
        }

        internal void RemoveCarrier(PlayerCarrier carrier)
        {
            int index = IndexOf(carrier);
            if (index < 0) return;
            Detach(index);
            Changed();
        }

        public void ReleaseAll()
        {
            bool any = false;
            for (int i = 0; i < carriers.Length; i++)
            {
                if (carriers[i] is null) continue;
                Detach(i);
                any = true;
            }
            if (any) Changed();
        }

        private void Detach(int index)
        {
            PlayerCarrier carrier = carriers[index];
            carriers[index] = null;
            CarrierCount--;
            LastPull[index] = Vector3.zero;
            // The player object may already be destroyed (left the session).
            if (carrier == null) return;
            grabbable.SetIgnoreCollisions(carrier.Controller, false);
            carrier.DetachShared(grabbable);
        }

        private void Changed()
        {
            UpdateNudgeOrigin();
            grabbable.SharedCarryChanged();
            CarriersChanged?.Invoke(this);
        }

        /// <summary>
        /// The nudge budget is tied to the item, not to a grab: letting go and grabbing again doesn't
        /// refill it, or a solo player could inch a statue to the truck. It starts afresh only after a
        /// full crew lifted it or it ended up well away from the budget (fell through a floor).
        /// </summary>
        private void UpdateNudgeOrigin()
        {
            if (IsLifted) hasNudgeOrigin = false;
            if (!IsNudgeOnly) return;
            Vector3 here = transform.position;
            if (hasNudgeOrigin && Vector3.Distance(here, nudgeOrigin) <= config.NudgeRadius + config.NudgeResetDistance) return;
            nudgeOrigin = here;
            hasNudgeOrigin = true;
        }
    }
}

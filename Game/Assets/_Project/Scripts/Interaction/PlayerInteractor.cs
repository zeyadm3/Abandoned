using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// Turns player input into interaction requests: aims at grabbables, E to pick up, hold left
    /// mouse to charge a throw, right mouse to drop, Tab + right mouse to drop the last pocket item.
    /// Runs only for the local player; requests go through <see cref="InteractionService"/>.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerLook look;

        private readonly RaycastHit[] hits = new RaycastHit[8];
        private bool charging;
        private bool suppressUseUntilRelease;

        public Grabbable Target { get; private set; }

        /// <summary>A non-loot thing in reach to press E on (truck ignition...), when no loot is targeted.</summary>
        public IUsable UseTarget { get; private set; }

        /// <summary>0–1 throw charge while left mouse is held with something in hand.</summary>
        public float Charge { get; private set; }

        private void Update() => Tick(inputReader.Current, Time.deltaTime);

        private void OnDisable() => ResetCharge();

        private void ResetCharge()
        {
            Target = null;
            UseTarget = null;
            charging = false;
            Charge = 0f;
        }

        public void Tick(PlayerInputFrame input, float dt)
        {
            if (look != null && look.isActiveAndEnabled)
            {
                // Paused (cursor free): ignore gameplay clicks. The click that re-captures the cursor
                // must not also start a throw, so ignore Use until it's released.
                if (!look.CursorCaptured) { ResetCharge(); return; }
                if (look.CaptureFrame == Time.frameCount) suppressUseUntilRelease = true;
            }
            if (!input.UseHeld) suppressUseUntilRelease = false;
            bool useHeld = input.UseHeld && !suppressUseUntilRelease;

            UseTarget = null;
            Target = carrier.Held == null ? FindTarget() : null;
            IInteractionHandler handler = InteractionService.Handler;

            if (input.InteractPressed && carrier.Held == null && Target != null)
                handler.RequestPickup(carrier, Target);
            else if (input.InteractPressed && UseTarget != null && UseTarget.UsePrompt(gameObject) != null)
                UseTarget.Use(gameObject);

            if (carrier.Held == null)
            {
                charging = false;
                Charge = 0f;
                if (input.InventoryHeld && input.DropPressed) handler.RequestDropFromPocket(carrier);
                return;
            }

            if (input.DropPressed)
            {
                charging = false;
                Charge = 0f;
                handler.RequestDrop(carrier);
                return;
            }

            // Dragged and shared items are let go with RMB, never thrown.
            if (carrier.IsDragging || carrier.IsSharing) return;

            if (useHeld)
            {
                charging = true;
                Charge = Mathf.Clamp01(Charge + dt / carrier.Config.ThrowChargeTime);
            }
            else if (charging)
            {
                float speed = carrier.Config.ThrowSpeedFor(Charge, carrier.Held.Weight);
                handler.RequestThrow(carrier, carrier.EyeForward * speed + carrier.DropVelocity);
                charging = false;
                Charge = 0f;
            }
        }

        private Grabbable FindTarget()
        {
            // RaycastAll + nearest, because the ray starts inside our own CharacterController.
            int count = Physics.RaycastNonAlloc(carrier.EyePosition, carrier.EyeForward, hits,
                carrier.Config.Reach, ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            Grabbable found = null;
            UseTarget = null;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider == carrier.Controller || hits[i].distance >= best) continue;
                best = hits[i].distance;
                found = hits[i].rigidbody != null ? hits[i].rigidbody.GetComponent<Grabbable>() : null;
                UseTarget = found == null ? hits[i].collider.GetComponentInParent<IUsable>() : null;
            }
            return found != null && found.IsAvailable ? found : null;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !DebugView.Visible || carrier == null) return;
            Gizmos.color = Target != null ? Color.green : Color.gray;
            Gizmos.DrawLine(carrier.EyePosition, carrier.EyePosition + carrier.EyeForward * carrier.Config.Reach);
            if (carrier.Held == null) return;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(carrier.HoldPoint, 0.08f);
            Gizmos.DrawLine(carrier.HoldPoint, carrier.Held.Body.worldCenterOfMass);
        }
    }
}

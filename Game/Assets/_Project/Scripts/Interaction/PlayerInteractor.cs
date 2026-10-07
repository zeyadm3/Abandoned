using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// Picks up loot, charges throws, rotates a single held item, and distinguishes tap-drop from
    /// a physical hold-to-set-down. Team handles always release promptly.
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
        private bool dropPending;
        private float dropTime;
        private Grabbable dropItem;
        public bool IsRotating { get; private set; }
        public bool IsPlacing => carrier != null && carrier.IsPlacing;
        public bool PlacementBlocked => carrier != null && carrier.PlacementBlocked;

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
            IsRotating = false;
            dropPending = false;
            dropItem = null;
            if (carrier != null) carrier.CancelPlacement();
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
                ResetHandling();
                if (input.InventoryHeld && input.DropPressed) handler.RequestDropFromPocket(carrier);
                return;
            }

            // A shared/dragged load can trap a player beside a failing floor: never delay letting go.
            if (carrier.IsDragging || carrier.IsSharing)
            {
                ResetHandling();
                if (input.DropPressed) handler.RequestDrop(carrier);
                return;
            }

            if (dropPending && dropItem != carrier.Held) ResetHandling();
            if (input.DropPressed)
            {
                dropPending = true;
                dropItem = carrier.Held;
                dropTime = 0f;
                charging = false;
                Charge = 0f;
            }
            if (dropPending)
            {
                IsRotating = false;
                if (input.DropHeld)
                {
                    dropTime += dt;
                    if (dropTime >= carrier.Config.PlaceHoldTime)
                    {
                        if (!carrier.IsPlacing) carrier.BeginPlacement();
                        // The network guard rate-limits retries; a rejected/stale host view must not strand the hold.
                        if (carrier.PlacementReady) handler.RequestPlace(carrier);
                    }
                }
                else if (input.DropReleased || !input.DropHeld)
                {
                    bool tapped = dropTime < carrier.Config.PlaceHoldTime;
                    ResetHandling();
                    if (tapped) handler.RequestDrop(carrier);
                }
                return;
            }

            IsRotating = input.RotateHeld && carrier.CanRotate;
            if (IsRotating)
            {
                charging = false;
                Charge = 0f;
                carrier.RotateHeld(input.Look);
                return;
            }

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

        private void ResetHandling()
        {
            charging = false;
            Charge = 0f;
            IsRotating = false;
            dropPending = false;
            dropItem = null;
            carrier.CancelPlacement();
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

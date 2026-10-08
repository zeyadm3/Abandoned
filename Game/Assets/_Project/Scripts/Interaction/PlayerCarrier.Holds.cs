using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>Taking, letting go of and pocketing items: applied only after the rules (or the host) said yes.</summary>
    public partial class PlayerCarrier
    {
        public void ShowHint(string message)
        {
            Hint = message;
            HintTime = Time.time;
        }

        // Apply* methods change state and are called only by an IInteractionHandler after validation.

        internal void ApplyHold(Grabbable target)
        {
            // Mirrored holds can arrive out of order (the old item's release after the new pickup).
            if (Held != null && Held != target) ApplyRelease(Vector3.zero);
            bool drag = target.CarryClass > config.HeaviestSoloClass && CanSoloDrag(target.CarryClass);
            Held = target;
            CancelPlacement();
            holdTime = 0f;
            lastDragPosition = target.Body.worldCenterOfMass;
            // Keep the object's current facing relative to the player, so it doesn't snap-rotate.
            holdRotationOffset = Quaternion.Inverse(Quaternion.Euler(0f, transform.eulerAngles.y, 0f)) * target.Body.rotation;
            target.BeginHold(this, drag);
        }

        internal void ApplyRelease(Vector3 velocity)
        {
            CancelPlacement();
            if (IsSharing)
            {
                // Letting go of a handle: the item keeps whatever the remaining crew does with it.
                Held.Shared.RemoveCarrier(this);
                Held = null;
                return;
            }
            Grabbable item = Held;
            Held = null;
            item.EndHold(velocity);
        }

        /// <summary>
        /// Lets go of this particular item. Mirrored state can name an item this carrier no longer has
        /// in hand (it already moved on to another), so releasing "whatever is held" would drop the wrong one.
        /// </summary>
        internal void ReleaseItem(Grabbable item, Vector3 velocity)
        {
            if (item == null) return;
            if (Held == item) ApplyRelease(velocity);
            else if (item.Holder == this) item.EndHold(velocity);
        }

        internal void ApplyPocket(Grabbable target)
        {
            target.Pocket(this);
            Inventory.Add(target);
        }

        internal void ApplyUnpocketSelected()
        {
            Grabbable item = Inventory.Selected;
            if (item == null) return;
            Pose pose = UnpocketPose(EyeForward);
            ApplyUnpocket(item, pose.position, pose.rotation, DropVelocity);
        }

        internal void ApplyUnpocket(Grabbable item, Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            Inventory.Remove(item);
            item.Unpocket(position, rotation, velocity);
        }

        /// <summary>Where a pocket item reappears when taken out, looking along <paramref name="aim"/>.</summary>
        public Pose UnpocketPose(Vector3 aim)
        {
            Vector3 direction = aim.sqrMagnitude > 1e-6f ? aim.normalized : transform.forward;
            float distance = config.OneHandHoldDistance;
            // Don't spawn it inside a wall the player is facing.
            if (Physics.Raycast(cameraRoot.position, direction, out RaycastHit hit, distance,
                    ~0, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0.2f, hit.distance - 0.25f);
            return new Pose(cameraRoot.position + direction * distance, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
        }

        /// <summary>Called by <see cref="SharedCarryable"/> when this player takes or leaves one of its points.</summary>
        internal void AttachShared(Grabbable item)
        {
            CancelPlacement();
            Held = item;
            holdTime = 0f;
            lastDragPosition = item.Body.worldCenterOfMass;
        }

        internal void DetachShared(Grabbable item)
        {
            if (Held == item) { Held = null; CancelPlacement(); }
        }

        /// <summary>The held item was destroyed (shattered, despawned).</summary>
        internal void ForgetHeld(Grabbable item)
        {
            if (Held == item) { Held = null; CancelPlacement(); }
        }
    }
}

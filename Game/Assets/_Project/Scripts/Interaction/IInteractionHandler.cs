using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// Where player interaction requests go. Single-player applies them locally; in M3 a network
    /// handler sends them to the host, which validates with <see cref="PickupRules"/> and applies.
    /// </summary>
    public interface IInteractionHandler
    {
        void RequestPickup(PlayerCarrier carrier, Grabbable target);
        void RequestDrop(PlayerCarrier carrier);
        void RequestThrow(PlayerCarrier carrier, Vector3 velocity);
        void RequestDropFromPocket(PlayerCarrier carrier);
    }
}

using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Trigger capsule slightly larger than the CharacterController. CharacterControllers get no
    /// collision callbacks from rigidbodies hitting them, so this reports those hits to the ragdoll.
    /// Only the player's owner has it active, so it must also accept the kinematic copies of loot
    /// another machine simulates (host-thrown, falling in a collapse, swung by another carrier),
    /// using the motion the network gives them (<see cref="IVelocitySource"/>).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerHitDetector : MonoBehaviour
    {
        [SerializeField] private PlayerRagdoll ragdoll;

        private void OnTriggerEnter(Collider other) => Report(other);

        // An item already touching us and then swung or shoved into us counts too (QA B-27); the ragdoll
        // only reacts to real closing speed, so a resting touch does nothing.
        private void OnTriggerStay(Collider other) => Report(other);

        private void Report(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null || body.transform.IsChildOf(ragdoll.transform)) return;
            Vector3 velocity;
            if (body.TryGetComponent(out IVelocitySource source)) velocity = source.Velocity;
            else if (!body.isKinematic) velocity = body.linearVelocity;
            else return; // doors, platforms: nothing that should knock a player over
            float weight = body.TryGetComponent(out IWeighted weighted) ? weighted.GameplayWeight : body.mass;
            ragdoll.ReportHit(body, weight, velocity);
        }
    }
}

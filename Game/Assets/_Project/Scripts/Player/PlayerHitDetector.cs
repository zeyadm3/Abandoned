using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Trigger capsule slightly larger than the CharacterController. CharacterControllers get no
    /// collision callbacks from rigidbodies hitting them, so this reports those hits to the ragdoll.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerHitDetector : MonoBehaviour
    {
        [SerializeField] private PlayerRagdoll ragdoll;

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null || body.isKinematic || body.transform.IsChildOf(ragdoll.transform)) return;
            float weight = body.TryGetComponent(out IWeighted weighted) ? weighted.GameplayWeight : body.mass;
            ragdoll.ReportHit(body, weight, body.linearVelocity);
        }
    }
}

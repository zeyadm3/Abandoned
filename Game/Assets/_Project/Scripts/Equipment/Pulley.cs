using Abandoned.Core;
using Abandoned.Extraction;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// A rope and pulley rigged over a hole (GDD 6.3, M10.3): whatever goes down the hole under it - a
    /// dropped piano, a crewmate - comes down on the rope at a gentle speed instead of falling, so loot
    /// lands without damage and people without ragdolling. Host-spawned; every machine registers the
    /// rope column (players slide on their own machine), the host slows host-simulated loot. It lasts
    /// until the next run.
    /// </summary>
    public class Pulley : NetworkBehaviour
    {
        [SerializeField] private Transform rope;
        [SerializeField, Min(0.5f)] private float radius = 1.5f;
        [Tooltip("Descent speed on the rope (m/s): below every fragility's damage threshold.")]
        [SerializeField, Min(0.2f)] private float lowerSpeed = 1.2f;
        [Tooltip("Sideways drift kept per second while lowering (the rope steadies the load).")]
        [SerializeField, Range(0f, 1f)] private float sidewaysDamping = 0.6f;

        private readonly NetworkVariable<float> depth = new(3f);
        private readonly Collider[] hits = new Collider[32];

        /// <summary>Host, right after spawning: the rope reaches <paramref name="drop"/> m down to the floor below.</summary>
        public void Rig(float drop)
        {
            depth.Value = drop;
            RunDirector.RunStarted += OnRunStarted;
        }

        public override void OnNetworkSpawn()
        {
            depth.OnValueChanged += (_, d) => Apply(d);
            Apply(depth.Value);
        }

        public override void OnNetworkDespawn()
        {
            RunDirector.RunStarted -= OnRunStarted;
            SafeDescent.Unregister(this);
        }

        private void Apply(float d)
        {
            // Column from just above the hole's lip down to the floor below.
            SafeDescent.Register(this, transform.position + Vector3.up * 0.6f, radius, d + 0.6f, lowerSpeed);
            if (rope == null) return;
            rope.localScale = new Vector3(rope.localScale.x, (d + 1.8f) / 2f, rope.localScale.z);
            rope.localPosition = Vector3.up * (1.8f - (d + 1.8f) / 2f);
        }

        // Host: loot it simulates (resting, dropped, thrown and released) is lowered, not dropped.
        private void FixedUpdate()
        {
            if (!IsServer || !IsSpawned) return;
            Vector3 top = transform.position + Vector3.up * 0.6f;
            Vector3 bottom = transform.position + Vector3.down * depth.Value;
            int n = Physics.OverlapCapsuleNonAlloc(top, bottom, radius, hits, LayerMask.GetMask(GameLayers.Loot), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                Rigidbody body = hits[i].attachedRigidbody;
                if (body == null || body.isKinematic) continue;
                Vector3 v = body.linearVelocity;
                if (v.y >= -lowerSpeed) continue;
                float keep = Mathf.Pow(sidewaysDamping, Time.fixedDeltaTime);
                body.linearVelocity = new Vector3(v.x * keep, -lowerSpeed, v.z * keep);
            }
        }

        private void OnRunStarted(int seed)
        {
            if (IsServer && IsSpawned) NetworkObject.Despawn(true);
        }

        /// <summary>
        /// Host: the edge of a hole in front of <paramref name="feet"/> along <paramref name="forward"/> (within
        /// <paramref name="reach"/> m) and how far it drops; false when there's no drop of at least 2 m there.
        /// </summary>
        public static bool FindHole(Vector3 feet, Vector3 forward, float reach, int floorMask, out Vector3 at, out float drop)
        {
            at = default;
            drop = 0f;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f) return false;
            forward.Normalize();
            for (float d = 0.8f; d <= reach; d += 0.4f)
            {
                Vector3 probe = feet + forward * d + Vector3.up * 0.6f;
                // Not through a wall (chest height clears railings, which you can rig over).
                if (Physics.Linecast(feet + Vector3.up * 1.3f, probe + Vector3.up * 0.7f, floorMask, QueryTriggerInteraction.Ignore)) return false;
                // Solid floor within a step below: not the hole yet.
                if (Physics.Raycast(probe, Vector3.down, 1.6f, floorMask, QueryTriggerInteraction.Ignore)) continue;
                if (!Physics.Raycast(probe, Vector3.down, out RaycastHit below, 30f, floorMask, QueryTriggerInteraction.Ignore)) return false;
                drop = feet.y - below.point.y;
                if (drop < 2f) return false;
                // Hang it a little past the lip so the load clears the edge.
                at = new Vector3(probe.x, feet.y, probe.z) + forward * 0.6f;
                return true;
            }
            return false;
        }

#if UNITY_EDITOR
        public void EditorSetup(Transform ropeTransform) => rope = ropeTransform;
#endif
    }
}

using Abandoned.Extraction;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// A support jack (GDD 6.3, M9.4): a post braced under a floor section, raising its capacity until
    /// the next run. Host-spawned; every machine sees the post stretched from the floor below to the
    /// section's underside. It goes when its section collapses anyway, or when the run ends.
    /// </summary>
    public class SupportJack : NetworkBehaviour
    {
        [SerializeField] private Transform post;
        [SerializeField, Min(1f)] private float capacityMultiplier = 1.6f;

        private readonly NetworkVariable<float> height = new(2f);
        private StructuralSection section;

        public StructuralSection Section => section;
        public float CapacityMultiplier => capacityMultiplier;

        /// <summary>Host, right after spawning: brace this section, the post reaching up <paramref name="postHeight"/> m.</summary>
        public void Brace(StructuralSection braced, float postHeight)
        {
            section = braced;
            height.Value = postHeight;
            braced.Reinforce(capacityMultiplier);
            RunDirector.RunStarted += OnRunStarted;
        }

        public override void OnNetworkSpawn()
        {
            height.OnValueChanged += (_, h) => Stretch(h);
            Stretch(height.Value);
        }

        public override void OnNetworkDespawn() => RunDirector.RunStarted -= OnRunStarted;

        private void Stretch(float h)
        {
            if (post == null) return;
            post.localScale = new Vector3(post.localScale.x, h / 2f, post.localScale.z);
            post.localPosition = Vector3.up * h / 2f;
        }

        private void Update()
        {
            if (IsServer && IsSpawned && section != null && section.IsCollapsed) NetworkObject.Despawn(true);
        }

        private void OnRunStarted(int seed)
        {
            if (IsServer && IsSpawned) NetworkObject.Despawn(true);
        }
    }
}

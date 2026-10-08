using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Permanent map-authored evacuation rope; never consumes gear or owns network state.</summary>
    public class MallFallbackDescent : MonoBehaviour
    {
        [SerializeField] private Vector3 localTop = new(0f, 0.6f, 0f);
        [SerializeField, Min(0.1f)] private float depth = 4.6f;
        [SerializeField, Min(0.5f)] private float radius = 1.5f;
        [SerializeField, Min(0.2f)] private float speed = 1.2f;
        [SerializeField, Range(0f, 1f)] private float sidewaysDamping = 0.6f;
        private readonly Collider[] hits = new Collider[64];

        public Vector3 Top => transform.TransformPoint(localTop);
        public float Depth => depth;
        public float Radius => radius;

        private void OnEnable() => SafeDescent.Register(this, Top, radius, depth, speed);
        private void OnDisable() => SafeDescent.Unregister(this);

        private void FixedUpdate()
        {
            if (GameAuthority.IsHost) SafeDescent.LowerLoot(Top, depth, radius, speed, sidewaysDamping, hits);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && !DebugView.Visible) return;
            Gizmos.color = new Color(0.35f, 0.85f, 0.6f, 0.7f);
            Gizmos.DrawWireSphere(Top, radius);
            Gizmos.DrawWireSphere(Top + Vector3.down * depth, radius);
            Gizmos.DrawLine(Top, Top + Vector3.down * depth);
        }

#if UNITY_EDITOR
        public void EditorSetup(Vector3 top, float drop, float columnRadius = 1.5f, float descentSpeed = 1.2f,
            float damping = 0.6f)
        {
            localTop = top; depth = drop; radius = columnRadius; speed = descentSpeed; sidewaysDamping = damping;
        }
#endif
    }
}

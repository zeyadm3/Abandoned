using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Where a player appears, facing this transform's forward. Network spawning uses these in M3.
    /// </summary>
    public class PlayerSpawnPoint : MonoBehaviour
    {
        private const float GizmoHeight = 1.8f;
        private const float GizmoRadius = 0.35f;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.9f);
            Vector3 p = transform.position;
            Gizmos.DrawWireSphere(p + Vector3.up * GizmoRadius, GizmoRadius);
            Gizmos.DrawWireSphere(p + Vector3.up * (GizmoHeight - GizmoRadius), GizmoRadius);
            Vector3 eye = p + Vector3.up * 1.6f;
            Gizmos.DrawLine(eye, eye + transform.forward * 1.2f);
        }
    }
}

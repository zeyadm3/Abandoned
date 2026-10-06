using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// Where a player appears, facing this transform's forward. The host gives each connected player
    /// its own point by <see cref="Index"/> (slot 0 = the host).
    /// </summary>
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [Tooltip("Slot this point serves; the host is slot 0. Points are used in index order.")]
        [SerializeField, Min(0)] private int index;

        private const float GizmoHeight = 1.8f;
        private const float GizmoRadius = 0.35f;
        // When there are more players than points, extras stand beside the last ones instead of inside them.
        private const float OverflowSpacing = 1.2f;

        private static readonly List<PlayerSpawnPoint> Active = new();

        public int Index => index;

        public void EditorSetup(int slotIndex) => index = slotIndex;

        private void OnEnable() => Active.Add(this);

        private void OnDisable() => Active.Remove(this);

        /// <summary>Spawn pose for a slot. Falls back to the origin when the scene has no points.</summary>
        public static Pose PoseFor(int slot)
        {
            if (Active.Count == 0) return new Pose(Vector3.right * (slot * OverflowSpacing), Quaternion.identity);
            Active.Sort((a, b) => a.index != b.index ? a.index.CompareTo(b.index) : string.CompareOrdinal(a.name, b.name));
            PlayerSpawnPoint point = Active[slot % Active.Count];
            int lap = slot / Active.Count;
            Transform t = point.transform;
            return new Pose(t.position + t.right * (lap * OverflowSpacing), t.rotation);
        }

        public static int ActiveCount => Active.Count;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.9f);
            Vector3 p = transform.position;
            Gizmos.DrawWireSphere(p + Vector3.up * GizmoRadius, GizmoRadius);
            Gizmos.DrawWireSphere(p + Vector3.up * (GizmoHeight - GizmoRadius), GizmoRadius);
            Vector3 eye = p + Vector3.up * 1.6f;
            Gizmos.DrawLine(eye, eye + transform.forward * 1.2f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active.Clear();
    }
}

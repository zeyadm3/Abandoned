using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// A crash mat (0.17.1), laid by the host: a thick soft pad that takes almost all the speed out of loot
    /// (and people) landing on it, so a hole can be the quick way down. Lasts the job; it doesn't move.
    /// </summary>
    public class CrashMat : NetworkBehaviour
    {
        [SerializeField] private Vector3 size = new(2.2f, 0.35f, 2.2f);

        public override void OnNetworkSpawn()
        {
            ToolConfig config = ToolConfig.Current;
            var box = new Bounds(transform.position + Vector3.up * (config.MatDepth * 0.5f), new Vector3(size.x + 0.3f, config.MatDepth + size.y, size.z + 0.3f));
            ImpactCushion.Add(this, box, config.MatCushion);
        }

        public override void OnNetworkDespawn() => ImpactCushion.Remove(this);

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible && Application.isPlaying) return;
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.6f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * (ToolConfig.Current.MatDepth * 0.5f), new Vector3(size.x, ToolConfig.Current.MatDepth, size.z));
        }
    }
}

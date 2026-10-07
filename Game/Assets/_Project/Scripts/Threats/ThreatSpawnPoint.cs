using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>Where a threat may appear in a level (on the NavMesh, out of sight of the entrance).</summary>
    public class ThreatSpawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.9f, 0.1f, 0.3f, 0.8f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.6f);
        }
    }
}

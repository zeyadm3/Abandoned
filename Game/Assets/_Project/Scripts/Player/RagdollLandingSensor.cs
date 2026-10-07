using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>The pelvis reports the first solid floor contact after a collapse starts a body fall.</summary>
    public class RagdollLandingSensor : MonoBehaviour
    {
        [SerializeField] private PlayerRagdoll ragdoll;
        private void OnCollisionEnter(Collision collision)
        {
            if (ragdoll != null) ragdoll.ReportBodyLanding(collision);
        }
    }
}

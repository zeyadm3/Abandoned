using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Light player pushing (0.21.2): walking into a crewmate asks their machine (they own their movement) to
    /// nudge them along. Capped small; the comedy is near the edge of a hole.
    /// </summary>
    public partial class NetworkPlayer
    {
        private float nextShoveFrom;

        public void RequestShove(Vector3 push)
        {
            if (IsSpawned && !IsOwner) ShoveRpc(Vector3.ClampMagnitude(push, 2.2f));
        }

        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
        private void ShoveRpc(Vector3 push)
        {
            if (dead.Value || IsHiding || ragdoll.IsRagdolled || Time.time < nextShoveFrom) return;
            // Any machine may send this: the cap is applied here too, and a NaN would poison the motor's
            // velocity (and so the player's position) for good.
            if (!float.IsFinite(push.x) || !float.IsFinite(push.y) || !float.IsFinite(push.z)) return;
            nextShoveFrom = Time.time + 0.4f;
            motor.Shove(Vector3.ClampMagnitude(push, 2.2f));
        }
    }
}

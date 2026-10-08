using Abandoned.Interaction;
using Abandoned.Loot;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>Client requests to the host (pickup, release, unpocket, place) and the host's replies.</summary>
    public partial class NetworkLoot
    {
        // ---- Requests (client -> host) ----------------------------------------------------------

        public bool ClientRequestPickup()
        {
            if (!requestGuard.TryBegin(LootRequest.Pickup, Time.time, config.RequestRepeatGuard)) return false;
            RequestPickupRpc();
            return true;
        }

        /// <summary>The carrier's machine knows where the item really is; the host checks it's plausible.</summary>
        public bool ClientRequestRelease(Vector3 velocity, bool isThrow)
        {
            if (!requestGuard.TryBegin(LootRequest.Release, Time.time, config.RequestRepeatGuard))
            {
                // QA B-22: a throw right after another release request isn't lost; it goes as soon as it may.
                if (isThrow) (queuedThrow, queuedThrowUntil) = (velocity, Time.time + QueuedThrowLifetime);
                return false;
            }
            queuedThrowUntil = 0f;
            RequestReleaseRpc(velocity, transform.position, transform.rotation, isThrow);
            return true;
        }

        private const float QueuedThrowLifetime = 1f;
        private Vector3 queuedThrow;
        private float queuedThrowUntil;

        // Client: send a queued throw once the guard allows, if we still hold the item.
        private void SendQueuedThrow()
        {
            if (queuedThrowUntil <= 0f) return;
            bool stillOurs = grabbable.Holder != null && grabbable.Holder.IsLocal && hold.Value.Mode == LootHoldMode.Held;
            if (Time.time > queuedThrowUntil || !stillOurs)
            {
                queuedThrowUntil = 0f;
                return;
            }
            if (!requestGuard.TryBegin(LootRequest.Release, Time.time, config.RequestRepeatGuard)) return;
            queuedThrowUntil = 0f;
            RequestReleaseRpc(queuedThrow, transform.position, transform.rotation, true);
        }

        public bool ClientRequestUnpocket(Vector3 aim, Vector3 inheritedVelocity)
        {
            if (!requestGuard.TryBegin(LootRequest.Unpocket, Time.time, config.RequestRepeatGuard)) return false;
            RequestUnpocketRpc(aim, inheritedVelocity);
            return true;
        }

        public bool ClientRequestPlace()
        {
            // Careful placement must not consume the emergency drop/ragdoll release budget.
            if (!requestGuard.TryBegin(LootRequest.Place, Time.time, config.RequestRepeatGuard)) return false;
            RequestPlaceRpc(transform.position, transform.rotation, grabbable.Body.linearVelocity, grabbable.Body.angularVelocity);
            return true;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestPlaceRpc(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 spin, RpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            if (!LootServerActions.TryPlace(this, SenderCarrier(sender), velocity, spin, new Pose(position, rotation)))
                HintRpc("No clear place to set it down", RpcTarget.Single(sender, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestPickupRpc(RpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            if (!LootServerActions.TryPickup(this, SenderCarrier(sender), out string reason))
                HintRpc(reason, RpcTarget.Single(sender, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestReleaseRpc(Vector3 velocity, Vector3 position, Quaternion rotation, bool isThrow, RpcParams rpcParams = default) =>
            LootServerActions.TryRelease(this, SenderCarrier(rpcParams.Receive.SenderClientId), velocity, isThrow, new Pose(position, rotation));

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestUnpocketRpc(Vector3 aim, Vector3 inheritedVelocity, RpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            if (!LootServerActions.TryUnpocket(this, SenderCarrier(sender), aim, inheritedVelocity, out string reason))
                HintRpc(reason, RpcTarget.Single(sender, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void HintRpc(string reason, RpcParams rpcParams)
        {
            // Anyone may invoke an RPC by default; only the host's refusals are real (and short).
            if (rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId || reason == null || reason.Length > MaxHintLength) return;
            LastHint = reason;
            NetworkObject player = NetworkManager.LocalClient?.PlayerObject;
            if (player != null && player.TryGetComponent(out PlayerCarrier carrier)) carrier.ShowHint(reason);
        }

        private PlayerCarrier SenderCarrier(ulong sender) =>
            NetworkManager.ConnectedClients.TryGetValue(sender, out NetworkClient client) && client.PlayerObject != null
                ? client.PlayerObject.GetComponent<PlayerCarrier>()
                : null;
    }
}

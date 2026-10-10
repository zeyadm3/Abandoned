using Abandoned.Audio;
using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>The cab's horn button (0.21.4), on the run state's object: a short double toot everyone hears, and monsters too.</summary>
    public class RunCab : NetworkBehaviour
    {
        private float next;

        public static RunCab Current { get; private set; }

        public override void OnNetworkSpawn() => Current = this;

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        public void RequestToot() => TootRpc();

        [Rpc(SendTo.Server)]
        private void TootRpc(RpcParams rpcParams = default)
        {
            TruckCargo truck = TruckCargo.Current;
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient c) ? c.PlayerObject : null;
            if (truck == null || player == null || Time.time < next || Vector3.Distance(player.transform.position, truck.Ignition.position) > 6f) return;
            next = Time.time + 2f;
            NoiseSystem.Emit(truck.Ignition.position, 0.8f, NoiseSource.Other);
            HornRpc(truck.Ignition.position);
        }

        [Rpc(SendTo.Everyone)]
        private void HornRpc(Vector3 at) => GameAudio.Play(SoundId.CabToot, at, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}

using Abandoned.Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Shared carrying over the network. The host decides who holds which point (<see cref="SharedCarryState"/>,
    /// mirrored onto every machine's copies of the item and players, so carried weight, structural load
    /// and HUDs agree) and simulates the item itself; it never hands a shared item's physics to a client.
    /// Each client carrier sends its desired hold position every network tick (unreliable: a lost one
    /// is replaced by the next); the host clamps it and feeds it to the item's springs, falling back to
    /// its own view of the carrier when input goes stale. Clients see the result via the item's
    /// host-owned NetworkTransform, and their own motor is tethered to their handle on their copy.
    /// </summary>
    [RequireComponent(typeof(SharedCarryable), typeof(NetworkLoot))]
    public class NetworkSharedCarry : NetworkBehaviour
    {
        private const int MaxHintLength = 120;
        // Past this the guess is worse than the lag it corrects.
        private const float MaxAnchorLead = 0.5f;

        private readonly NetworkVariable<SharedCarryState> state = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private SharedCarryable shared;
        private NetworkLoot loot;
        private readonly LootRequestGuard requestGuard = new();
        private float lastTargetSent = float.NegativeInfinity;
        private bool mirrorPending;

        public SharedCarryable Shared => shared;
        public NetworkLoot Loot => loot;
        public SharedCarryState State => state.Value;
        /// <summary>Client: the host's last refusal for this item.</summary>
        public string LastHint { get; private set; }
        /// <summary>Host: hold targets accepted from clients (F1, tests).</summary>
        public int TargetsReceived { get; private set; }

        public static NetworkSharedCarry Of(Grabbable target) =>
            target != null && target.TryGetComponent(out NetworkSharedCarry net) && net.IsSpawned ? net : null;

        private void Awake()
        {
            shared = GetComponent<SharedCarryable>();
            loot = GetComponent<NetworkLoot>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                shared.CarriersChanged += OnCarriersChanged;
                WriteState();
                return;
            }
            state.OnValueChanged += OnStateChanged;
            Mirror();
        }

        public override void OnNetworkDespawn()
        {
            shared.CarriersChanged -= OnCarriersChanged;
            state.OnValueChanged -= OnStateChanged;
            // Out of the session (shattered, host left): nobody keeps holding it here.
            shared.ReleaseAll();
        }

        private void Update()
        {
            if (!IsSpawned || IsServer) return;
            if (mirrorPending) Mirror();
            // Our copy of the item is half a round trip plus the interpolation delay behind the host's.
            ulong rttMs = NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId);
            shared.AnchorLead = Mathf.Min(MaxAnchorLead, rttMs * 0.0005f + shared.Config.AnchorInterpolationLead);
        }

        private void FixedUpdate()
        {
            if (!IsSpawned || shared.CarrierCount == 0) return;
            if (IsServer) WatchGrips();
            else SendOwnTarget();
        }

        // ---- Host ---------------------------------------------------------------------------------

        private void OnCarriersChanged(SharedCarryable _) => WriteState();

        private void WriteState()
        {
            var s = default(SharedCarryState);
            for (int i = 0; i < shared.PointCount; i++)
                s.Set(i, SharedCarryServer.PlayerObjectId(shared.CarrierAt(i)), shared.GripAt(i));
            state.Value = s;
        }

        private void WatchGrips()
        {
            for (int i = 0; i < shared.PointCount; i++)
            {
                PlayerCarrier lost = SharedCarryServer.CheckGrip(shared, i);
                if (lost == null || lost.IsRagdolled) continue;
                ulong owner = LootServerActions.OwnerOf(lost);
                if (owner == NetworkManager.LocalClientId) lost.ShowHint("Lost your grip");
                else HintRpc("Lost your grip", RpcTarget.Single(owner, RpcTargetUse.Temp));
            }
        }

        // ---- Client -------------------------------------------------------------------------------

        private void OnStateChanged(SharedCarryState previous, SharedCarryState current)
        {
            requestGuard.Clear();
            Mirror();
        }

        /// <summary>Copies the host's crew onto this machine's item and player copies.</summary>
        private void Mirror()
        {
            SharedCarryState s = state.Value;
            mirrorPending = false;
            for (int i = 0; i < shared.PointCount; i++)
            {
                ulong id = s.HolderAt(i);
                PlayerCarrier carrier = id == 0 ? null : loot.ResolveCarrier(id);
                // That player may not have spawned here yet; try again next frame.
                if (id != 0 && carrier == null)
                {
                    mirrorPending = true;
                    continue;
                }
                shared.SetCarrier(i, carrier, s.GripAt(i));
            }
        }

        private void SendOwnTarget()
        {
            NetworkObject player = NetworkManager.LocalClient?.PlayerObject;
            if (player == null || !player.TryGetComponent(out PlayerCarrier own)) return;
            int point = shared.IndexOf(own);
            float interval = 1f / Mathf.Max(1, NetworkManager.NetworkConfig.TickRate);
            if (point < 0 || Time.time - lastTargetSent < interval) return;
            lastTargetSent = Time.time;
            SubmitTargetRpc(shared.ComputedTarget(point));
        }

        public bool ClientRequestGrab()
        {
            if (!requestGuard.TryBegin(LootRequest.Pickup, Time.time, loot.Config.RequestRepeatGuard)) return false;
            RequestGrabRpc();
            return true;
        }

        public bool ClientRequestLetGo()
        {
            if (!requestGuard.TryBegin(LootRequest.Release, Time.time, loot.Config.RequestRepeatGuard)) return false;
            RequestLetGoRpc();
            return true;
        }

        // ---- RPCs ---------------------------------------------------------------------------------

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestGrabRpc(RpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            if (!SharedCarryServer.TryGrab(this, SenderCarrier(sender), out string reason))
                HintRpc(reason, RpcTarget.Single(sender, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestLetGoRpc(RpcParams rpcParams = default) =>
            SharedCarryServer.TryLetGo(this, SenderCarrier(rpcParams.Receive.SenderClientId));

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone, Delivery = RpcDelivery.Unreliable)]
        private void SubmitTargetRpc(Vector3 target, RpcParams rpcParams = default)
        {
            int point = shared.IndexOf(SenderCarrier(rpcParams.Receive.SenderClientId));
            if (point < 0 || !SharedCarryServer.TryClampTarget(shared, point, target, out Vector3 accepted)) return;
            shared.SetInputTarget(point, accepted);
            TargetsReceived++;
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

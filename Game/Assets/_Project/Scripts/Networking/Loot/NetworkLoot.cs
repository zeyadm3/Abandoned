using Abandoned.Interaction;
using Abandoned.Loot;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Makes a loot item a host-controlled network object (CLAUDE.md). The host owns value, damage and
    /// who holds what (<see cref="LootHoldState"/>), and decides every request (<see cref="LootServerActions"/>).
    /// Physics belongs to the single carrier while carried (NGO ownership + owner-authoritative
    /// NetworkTransform) and to the host otherwise. The carrier reports its collisions; the host replays
    /// sounds, damage text and shatters on every machine. Offline nothing changes.
    /// </summary>
    [RequireComponent(typeof(LootItem), typeof(Grabbable))]
    public class NetworkLoot : NetworkBehaviour
    {
        [SerializeField] private LootNetConfig config;
        [SerializeField] private NetworkTransform networkTransform;

        private readonly NetworkVariable<LootValueState> value = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<LootHoldState> hold = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private LootItem item;
        private Grabbable grabbable;
        private ImpactReportFilter reportFilter;
        private readonly LootRequestGuard requestGuard = new();
        private float lastSoundSent = float.NegativeInfinity;
        private float lastReportSent = float.NegativeInfinity;
        private bool reconcilePending;

        public LootItem Item => item;
        public Grabbable Grabbable => grabbable;
        public LootNetConfig Config => config;
        public LootHoldState Hold => hold.Value;
        public LootValueState Value => value.Value;
        /// <summary>Host: speed the last release was given after clamping (F1, tests).</summary>
        public float LastReleaseSpeed { get; internal set; } = -1f;
        /// <summary>Host: impact speed it last took from a client-carried item striking it (F1, tests).</summary>
        public float LastStruckSpeed { get; internal set; } = -1f;
        /// <summary>Client: the host's last refusal ("Too far away", "Someone else has it").</summary>
        public string LastHint { get; private set; }
        /// <summary>Host: where the holder last was, to put an orphaned item back in the world.</summary>
        internal Vector3 LastHolderPosition { get; private set; }

        /// <summary>The spawned network loot behind a grabbable, or null (offline, not loot, not spawned).</summary>
        public static NetworkLoot Of(Grabbable target) =>
            target != null && target.TryGetComponent(out NetworkLoot loot) && loot.IsSpawned ? loot : null;

        private void Awake()
        {
            item = GetComponent<LootItem>();
            grabbable = GetComponent<Grabbable>();
            reportFilter = new ImpactReportFilter(config);
        }

        public override void OnNetworkSpawn()
        {
            item.SetValueAuthority(IsServer);
            // Spawned items are placed after Instantiate; make physics agree before anything simulates.
            grabbable.SnapBodyToTransform();
            SyncPhysicsAuthority();
            item.CollisionImpact += OnCollisionImpact;
            hold.OnValueChanged += OnHoldChanged;
            if (IsServer)
            {
                item.Remover = RemoveByDespawn;
                item.EnsureInitialized();
                WriteValue(item);
                item.ValueChanged += WriteValue;
                item.Damaged += OnDamaged;
                item.Shattered += OnShattered;
                return;
            }
            value.OnValueChanged += OnValueChanged;
            ApplyValue(value.Value);
            Reconcile();
        }

        public override void OnNetworkDespawn()
        {
            item.CollisionImpact -= OnCollisionImpact;
            hold.OnValueChanged -= OnHoldChanged;
            value.OnValueChanged -= OnValueChanged;
            item.ValueChanged -= WriteValue;
            item.Damaged -= OnDamaged;
            item.Shattered -= OnShattered;
            item.Remover = null;
            item.SetValueAuthority(null);
            // Out of the session (shatter, host left): nobody keeps holding or pocketing it here.
            LootServerActions.ReleaseLocally(grabbable);
            grabbable.SetPhysicsAuthority(true);
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current)
        {
            base.OnOwnershipChanged(previous, current);
            SyncPhysicsAuthority();
            if (IsServer) reportFilter.OwnerChanged(previous, Time.time);
        }

        internal void SyncPhysicsAuthority() => grabbable.SetPhysicsAuthority(!IsSpawned || IsOwner);

        private void Update()
        {
            if (!IsSpawned) return;
            if (IsServer) WatchHolder();
            else if (reconcilePending) Reconcile();
        }

        private void WatchHolder()
        {
            if (hold.Value.Mode == LootHoldMode.Free) return;
            PlayerCarrier carrier = ResolveCarrier(hold.Value.HolderObjectId);
            if (carrier != null) LastHolderPosition = carrier.transform.position;
            // The holder left the session: put the item back where they were.
            else LootServerActions.FreeOrphan(this);
        }

        // ---- Host state -------------------------------------------------------------------------

        internal void ServerSetHold(LootHoldMode mode, PlayerCarrier carrier)
        {
            ulong id = carrier != null && carrier.TryGetComponent(out NetworkObject player) && player.IsSpawned
                ? player.NetworkObjectId : 0;
            // A carrier without a network player (test rigs) can't be named; leave it free for everyone else.
            if (id == 0) mode = LootHoldMode.Free;
            if (mode != LootHoldMode.Free) LastHolderPosition = carrier.transform.position;
            hold.Value = LootHoldState.For(mode, id);
        }

        /// <summary>Host: others jump to the new pose instead of interpolating across the room.</summary>
        internal void ServerTeleport()
        {
            if (networkTransform != null && networkTransform.CanCommitToTransform)
                networkTransform.Teleport(transform.position, transform.rotation, transform.localScale);
        }

        private void WriteValue(LootItem _)
        {
            if (item.IsInitialized)
                value.Value = new LootValueState(item.FullValue, item.CurrentValue, item.Condition, item.IsShattered);
        }

        private void RemoveByDespawn(LootItem _)
        {
            if (IsSpawned) NetworkObject.Despawn(destroy: true);
            else Destroy(gameObject);
        }

        // ---- Mirroring on clients ---------------------------------------------------------------

        private void OnValueChanged(LootValueState previous, LootValueState current) => ApplyValue(current);

        private void ApplyValue(LootValueState s)
        {
            if (s.Rolled) item.ApplyNetworkValue(s.FullValue, s.CurrentValue, s.Condition, s.Shattered);
        }

        private void OnHoldChanged(LootHoldState previous, LootHoldState current)
        {
            requestGuard.Clear();
            Reconcile();
        }

        /// <summary>Client: copies the host's hold state onto this machine's item and holder copies.</summary>
        private void Reconcile()
        {
            if (IsServer) return; // the host applied it before publishing
            LootHoldState s = hold.Value;
            PlayerCarrier target = s.Mode == LootHoldMode.Free ? null : ResolveCarrier(s.HolderObjectId);
            // The holder's player may not have spawned here yet; try again next frame.
            reconcilePending = s.Mode != LootHoldMode.Free && target == null;
            if (reconcilePending) return;
            LootServerActions.Mirror(grabbable, s.Mode, target);
        }

        internal PlayerCarrier ResolveCarrier(ulong objectId)
        {
            NetworkSpawnManager spawns = NetworkManager != null ? NetworkManager.SpawnManager : null;
            return spawns != null && spawns.SpawnedObjects.TryGetValue(objectId, out NetworkObject player) && player != null
                ? player.GetComponent<PlayerCarrier>()
                : null;
        }

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
            if (!requestGuard.TryBegin(LootRequest.Release, Time.time, config.RequestRepeatGuard)) return false;
            RequestReleaseRpc(velocity, transform.position, transform.rotation, isThrow);
            return true;
        }

        public bool ClientRequestUnpocket(Vector3 aim, Vector3 inheritedVelocity)
        {
            if (!requestGuard.TryBegin(LootRequest.Unpocket, Time.time, config.RequestRepeatGuard)) return false;
            RequestUnpocketRpc(aim, inheritedVelocity);
            return true;
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
            LastHint = reason;
            NetworkObject player = NetworkManager.LocalClient?.PlayerObject;
            if (player != null && player.TryGetComponent(out PlayerCarrier carrier)) carrier.ShowHint(reason);
        }

        private PlayerCarrier SenderCarrier(ulong sender) =>
            NetworkManager.ConnectedClients.TryGetValue(sender, out NetworkClient client) && client.PlayerObject != null
                ? client.PlayerObject.GetComponent<PlayerCarrier>()
                : null;

        // ---- Impacts and feedback ---------------------------------------------------------------

        /// <summary>Carrier: tell the host about a hit only this machine's physics saw (and which loot it struck).</summary>
        public void ReportImpact(float speed, Vector3 point, ulong struckId = LootStrikes.None, Vector3 normal = default) =>
            ReportImpactRpc(speed, point, normal, struckId);

        private void OnCollisionImpact(LootItem _, LootImpact hit)
        {
            if (hit.Speed < item.DamageConfig.MinSoundSpeed) return;
            if (IsServer)
            {
                reportFilter.HostImpact(Time.time);
                // The host already applied damage and noise; everyone else needs the sound.
                if (Throttle(ref lastSoundSent, config.ImpactSoundInterval)) ImpactRpc(hit.Speed, hit.Point, RpcTarget.NotMe);
            }
            else if (IsOwner && Throttle(ref lastReportSent, config.ImpactReportInterval))
            {
                ReportImpactRpc(hit.Speed, hit.Point, hit.Normal, LootStrikes.StruckId(hit.Other));
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ReportImpactRpc(float speed, Vector3 point, Vector3 normal, ulong struckId, RpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            if (!reportFilter.TryAccept(sender, OwnerClientId, Time.time, speed, point, grabbable.GetBounds(), out float accepted)) return;
            LootStrikes.ApplyStruck(this, struckId, accepted, point, normal);
            // Sound first: a shatter despawns the item.
            ImpactRpc(accepted, point, RpcTarget.Not(sender, RpcTargetUse.Temp));
            item.ApplyImpact(accepted, point);
            item.EmitImpactNoise(accepted, point);
        }

        internal void ServerReplayImpactEverywhere(float speed, Vector3 point) => ImpactRpc(speed, point, RpcTarget.Everyone);

        [Rpc(SendTo.SpecifiedInParams)]
        private void ImpactRpc(float speed, Vector3 point, RpcParams rpcParams) => item.ReplayImpact(speed, point);

        private void OnDamaged(LootItem _, int loss, Vector3 point) => DamagedRpc(loss, point);

        private void OnShattered(LootItem _, Vector3 point) => ShatteredRpc(point);

        [Rpc(SendTo.NotServer)]
        private void DamagedRpc(int loss, Vector3 point) => item.ReplayDamaged(loss, point);

        [Rpc(SendTo.NotServer)]
        private void ShatteredRpc(Vector3 point) => item.ReplayShattered(point);

        private static bool Throttle(ref float last, float interval)
        {
            if (Time.time - last < interval) return false;
            last = Time.time;
            return true;
        }
    }
}

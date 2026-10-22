using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// The crew's van in the open world. Host-authoritative: the host simulates its wheels from the driver's input
    /// (sent by RPC), burns fuel, takes damage from hard knocks and rolls breakdowns (<see cref="VanCondition"/>), and
    /// writes the result to <see cref="State"/> for everyone. The tailgate ramp is down whenever nobody is at the wheel.
    /// Pointing at a broken-down van offers the roadside repair.
    /// </summary>
    [RequireComponent(typeof(VanDrive), typeof(VanCondition))]
    public class WorldVan : NetworkBehaviour, IUsable
    {
        [SerializeField] private VanSeats seats;
        [SerializeField] private VanTailgate tailgate;
        [Tooltip("Headlights and tail lights: on while the engine runs.")]
        [SerializeField] private Light[] lights;

        private readonly NetworkVariable<VanNetState> state = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private VanDrive drive;
        private VanCondition condition;
        private VanConfig config;
        private VanDriveInput input = VanDriveInput.Parked;
        private float inputAt = float.NegativeInfinity, nextSend;
        private VanDriveInput lastSent;
        private bool horn, lastHorn;
        // Host: a repair in progress (who is doing it and how far along).
        private ulong repairer = VanSeating.Empty;
        private float repairDone;

        /// <summary>The van this session drives (one per world).</summary>
        public static WorldVan Current { get; private set; }

        public VanNetState State => state.Value;
        public VanDrive Drive => drive;
        public VanSeats Seats => seats;
        public VanCondition Condition => condition;
        public bool HornHeld => horn;

        private void Awake()
        {
            drive = GetComponent<VanDrive>();
            condition = GetComponent<VanCondition>();
            config = VanConfig.Load();
            if (seats == null) seats = GetComponent<VanSeats>();
        }

        public override void OnNetworkSpawn()
        {
            Current = this;
            drive.SetSimulated(IsServer);
            if (IsServer) Publish();
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        /// <summary>The driver's machine: this frame's hands on the wheel (and the horn).</summary>
        public void SendDrive(VanDriveInput hands, bool hornHeld)
        {
            if (IsServer)
            {
                input = hands;
                inputAt = Time.time;
                horn = hornHeld;
                return;
            }
            // Unreliable and frequent: a lost packet is replaced by the next one, and a silent driver brakes.
            bool changed = hands.Throttle != lastSent.Throttle || hands.Steer != lastSent.Steer || hands.Handbrake != lastSent.Handbrake || hornHeld != lastHorn;
            if (!changed && Time.unscaledTime < nextSend) return;
            nextSend = Time.unscaledTime + .1f;
            lastSent = hands;
            lastHorn = hornHeld;
            DriveRpc(hands, hornHeld);
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable, InvokePermission = RpcInvokePermission.Everyone)]
        private void DriveRpc(VanDriveInput hands, bool hornHeld, RpcParams rpcParams = default)
        {
            if (seats == null || seats.Driver != rpcParams.Receive.SenderClientId) return;
            input = hands;
            inputAt = Time.time;
            horn = hornHeld;
        }

        private void FixedUpdate()
        {
            if (!IsSpawned || !IsServer) return;
            bool driver = seats != null && seats.HasDriver;
            bool fresh = Time.time - inputAt < config.InputTimeout;
            VanDriveInput hands = driver && fresh ? input : VanDriveInput.Parked;
            if (!driver) horn = false;
            bool engine = driver && condition.CanDrive;
            drive.Step(hands, engine, Time.fixedDeltaTime);
            if (engine)
            {
                Vector3 v = drive.Body.linearVelocity;
                float moved = new Vector2(v.x, v.z).magnitude * Time.fixedDeltaTime;
                if (moved > .001f) condition.Drive(moved);
                else condition.Idle(Time.fixedDeltaTime);
            }
            TickRepair();
            Publish();
        }

        private void Publish()
        {
            bool engine = seats != null && seats.HasDriver && condition.CanDrive;
            bool rampDown = !(seats != null && seats.HasDriver) && drive.Body.linearVelocity.magnitude < 1f;
            float repair = repairer != VanSeating.Empty ? Mathf.Clamp01(repairDone / config.RepairSeconds) : 0f;
            state.Value = VanNetState.Of(condition.Fuel, condition.Damage, engine, condition.BrokenDown, rampDown, engine, drive.SteerFraction, repair);
        }

        private void Update()
        {
            if (!IsSpawned) return;
            VanNetState s = state.Value;
            if (!IsServer) drive.ShowSteer(s.Steer);
            if (tailgate != null) tailgate.Show(s.RampDown);
            foreach (Light l in lights) if (l != null) l.enabled = s.Lights;
        }

        // Host: knocks hard enough hurt the van. Loot rattling in the back and people walking into it don't count.
        private void OnCollisionEnter(Collision collision)
        {
            if (!IsSpawned || !IsServer) return;
            Rigidbody other = collision.rigidbody;
            if (other != null && (other.GetComponent<Grabbable>() != null || other.GetComponent<NetworkPlayer>() != null)) return;
            if (collision.collider.GetComponentInParent<NetworkPlayer>() != null) return;
            float speed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, collision.GetContact(0).normal));
            condition.Impact(speed);
        }

        // ---- The roadside repair (pointing at the van while it's broken down) ----

        public string UsePrompt(GameObject user)
        {
            if (!IsSpawned || !state.Value.BrokenDown) return null;
            return state.Value.Repairing ? Loc.T("Repairing the van... {0:0}%", state.Value.RepairProgress * 100f)
                : Loc.T("Repair the van ({0:0} s)", config.RepairSeconds);
        }

        public void Use(GameObject user)
        {
            if (!IsSpawned || !state.Value.BrokenDown) return;
            if (IsServer) ServerStartRepair(NetworkManager.LocalClientId);
            else RepairRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RepairRpc(RpcParams rpcParams = default) => ServerStartRepair(rpcParams.Receive.SenderClientId);

        private void ServerStartRepair(ulong client)
        {
            if (!condition.BrokenDown || repairer != VanSeating.Empty) return;
            repairer = client;
            repairDone = 0f;
        }

        // Host: the repairer has to stay by the van, out of it, for the whole job.
        private void TickRepair()
        {
            if (repairer == VanSeating.Empty) return;
            NetworkPlayer p = null;
            foreach (NetworkPlayer candidate in NetworkPlayer.All)
                if (candidate != null && candidate.IsSpawned && candidate.OwnerClientId == repairer) p = candidate;
            bool near = p != null && !p.IsDead && seats.Seating.SeatOf(repairer) < 0
                && Vector3.Distance(p.transform.position, drive.Body.ClosestPointOnBounds(p.transform.position)) < config.SeatReach;
            if (!near || !condition.BrokenDown)
            {
                repairer = VanSeating.Empty;
                return;
            }
            repairDone += Time.fixedDeltaTime;
            if (repairDone < config.RepairSeconds) return;
            condition.Repair(config.RepairPatch);
            repairer = VanSeating.Empty;
        }

        /// <summary>Host: fill the tank by this many litres (a pump).</summary>
        public void ServerRefuel(float litres)
        {
            if (IsServer) condition.Refuel(litres);
        }

        /// <summary>Host: put the van somewhere at once (a new world, a respawn), stopped.</summary>
        public void ServerPlace(Vector3 position, Quaternion rotation)
        {
            if (!IsServer) return;
            drive.Body.linearVelocity = Vector3.zero;
            drive.Body.angularVelocity = Vector3.zero;
            drive.Body.position = position;
            drive.Body.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
            var nt = GetComponent<Unity.Netcode.Components.NetworkTransform>();
            if (nt != null && nt.CanCommitToTransform) nt.Teleport(position, rotation, transform.localScale);
        }

        public void EditorSetup(VanSeats vanSeats, VanTailgate gate, Light[] vanLights)
        {
            seats = vanSeats;
            tailgate = gate;
            lights = vanLights;
        }
    }
}

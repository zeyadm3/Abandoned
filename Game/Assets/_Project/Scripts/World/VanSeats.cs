using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// The van's four seats (0 drives, 1-2 in the cab, 3 on the bench in the cargo bay). The host writes who sits
    /// where; each machine moves its own player into and out of its seat (players own their movement), and every
    /// machine pins seated crewmates to its own copy of the van so nobody lags behind it.
    /// </summary>
    [DefaultExecutionOrder(-200)] // LateUpdate before the camera brain, after the van and every NetworkTransform moved.
    public partial class VanSeats : NetworkBehaviour
    {
        public const int DriverSeat = 0;

        [Tooltip("Eye point and facing of each seat.")]
        [SerializeField] private Transform[] seats = new Transform[VanSeating.Count];
        [Tooltip("Where someone climbing out of each seat stands (on the ground beside the door).")]
        [SerializeField] private Transform[] exits = new Transform[VanSeating.Count];
        [SerializeField] private WorldVan van;
        [Tooltip("How far either side of straight ahead a seated player can turn their head (degrees).")]
        [SerializeField] private float lookAngle = 120f;

        private readonly NetworkVariable<VanSeating> seating = new(VanSeating.None,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Collider[] vanColliders;
        private VanConfig config;

        public VanSeating Seating => seating.Value;
        public ulong Driver => seating.Value[DriverSeat];
        public bool HasDriver => Driver != VanSeating.Empty;
        public int Occupied
        {
            get
            {
                int n = 0;
                for (int i = 0; i < VanSeating.Count; i++) if (seating.Value[i] != VanSeating.Empty) n++;
                return n;
            }
        }

        public Transform SeatPoint(int seat) => seats[seat];

        private void Awake()
        {
            config = VanConfig.Load();
            vanColliders = GetComponentsInChildren<Collider>(true);
            if (van == null) van = GetComponent<WorldVan>();
        }

        public int FirstFree(int[] preference)
        {
            foreach (int seat in preference)
                if (seat >= 0 && seat < VanSeating.Count && seating.Value[seat] == VanSeating.Empty) return seat;
            return -1;
        }

        /// <summary>This machine's player asks for a seat.</summary>
        public void RequestSeat(int seat)
        {
            if (!IsSpawned) return;
            if (IsServer) ServerSeat(NetworkManager.LocalClientId, seat);
            else SeatRpc((sbyte)seat);
        }

        /// <summary>This machine's player asks to climb out.</summary>
        public void RequestLeave()
        {
            if (!IsSpawned) return;
            if (IsServer) ServerSeat(NetworkManager.LocalClientId, -1);
            else SeatRpc(-1);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SeatRpc(sbyte seat, RpcParams rpcParams = default) => ServerSeat(rpcParams.Receive.SenderClientId, seat);

        private void ServerSeat(ulong client, int seat)
        {
            VanSeating s = seating.Value;
            int current = s.SeatOf(client);
            if (seat < 0)
            {
                if (current < 0) return;
                s[current] = VanSeating.Empty;
                seating.Value = s;
                return;
            }
            if (seat >= VanSeating.Count || s[seat] != VanSeating.Empty) return;
            NetworkPlayer player = PlayerOf(client);
            if (player == null || player.IsDead || player.IsHiding || player.Ragdoll.IsRagdolled) return;
            if (current < 0 && Vector3.Distance(player.transform.position, exits[seat].position) > config.SeatReach) return;
            // Hands must be free: what you carry goes in the cargo bay, not on your lap.
            if (player.Carrier != null && player.Carrier.Held != null && player.Carrier.Held.CarryClass >= CarryClass.TwoHand) return;
            if (current >= 0) s[current] = VanSeating.Empty;
            s[seat] = client;
            seating.Value = s;
        }

        private static NetworkPlayer PlayerOf(ulong client)
        {
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.IsSpawned && p.OwnerClientId == client) return p;
            return null;
        }

        // Host: a seat empties when its player leaves the session, dies or is knocked down.
        private void Update()
        {
            if (!IsSpawned || !IsServer) return;
            VanSeating s = seating.Value;
            bool changed = false;
            for (int i = 0; i < VanSeating.Count; i++)
            {
                if (s[i] == VanSeating.Empty) continue;
                NetworkPlayer p = PlayerOf(s[i]);
                if (p != null && !p.IsDead && !p.Ragdoll.IsRagdolled && !p.IsHiding) continue;
                s[i] = VanSeating.Empty;
                changed = true;
            }
            if (changed) seating.Value = s;
        }

        private void LateUpdate()
        {
            if (!IsSpawned) return;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned) continue;
                int seat = seating.Value.SeatOf(p.OwnerClientId);
                if (p.IsOwner) TickOwner(p, seat);
                else if (seat >= 0) Pin(p, seat);
                SetSeatedPhysics(p, seat >= 0);
            }
        }

        // Every machine: a seated player's body never pushes the van (it sits inside the van's colliders).
        private void SetSeatedPhysics(NetworkPlayer p, bool seated)
        {
            if (p.TryGetComponent(out VanRider rider) == false) rider = p.gameObject.AddComponent<VanRider>();
            if (rider.IgnoringVan == seated) return;
            rider.IgnoringVan = seated;
            foreach (Collider mine in p.GetComponentsInChildren<Collider>(true))
            foreach (Collider theirs in vanColliders)
                if (mine != null && theirs != null) Physics.IgnoreCollision(mine, theirs, seated);
        }

        // Feet below the seat's eye point by the player's own eye height, so the camera sits where a head would.
        private void Pin(NetworkPlayer p, int seat)
        {
            PlayerLook look = p.GetComponent<PlayerLook>();
            float eye = look != null ? look.EyeRoot.localPosition.y : 1.6f;
            p.transform.position = seats[seat].position - Vector3.up * eye;
        }

        public void EditorSetup(WorldVan worldVan, Transform[] seatPoints, Transform[] exitPoints)
        {
            van = worldVan;
            seats = seatPoints;
            exits = exitPoints;
        }
    }
}

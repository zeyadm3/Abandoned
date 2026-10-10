using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Ping markers (0.19.0), on the run state's object: a player's ping key sends its eye ray; the host looks
    /// along it and marks what it finds (loot, a monster, a weak floor, or just the spot) for everyone for a
    /// few seconds. A player has at most three marks up; a fourth replaces their oldest.
    /// </summary>
    public class RunPings : NetworkBehaviour
    {
        public const float Lifetime = 12f, Reach = 60f;
        private const int PerPlayer = 3;

        private readonly NetworkList<PingMark> marks = new();
        private RunState run;
        private int lastCount;

        public static RunPings Current { get; private set; }
        public NetworkList<PingMark> Marks => marks;
        public double Now => run != null ? run.Now : 0d;

        public override void OnNetworkSpawn()
        {
            Current = this;
            run = GetComponent<RunState>();
        }

        public override void OnNetworkDespawn()
        {
            if (Current == this) Current = null;
        }

        public void RequestPing(Vector3 eye, Vector3 direction) => PingRpc(eye, direction);

        [Rpc(SendTo.Server)]
        private void PingRpc(Vector3 eye, Vector3 direction, RpcParams rpcParams = default)
        {
            ulong client = rpcParams.Receive.SenderClientId;
            NetworkObject player = NetworkManager.ConnectedClients.TryGetValue(client, out NetworkClient c) ? c.PlayerObject : null;
            if (player == null || !player.TryGetComponent(out NetworkPlayer np) || np.IsDead) return;
            if (Vector3.Distance(eye, player.transform.position + Vector3.up * 1.5f) > 1.5f || direction.sqrMagnitude < 0.5f) return;
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Debris, "Ignore Raycast");
            if (!Physics.Raycast(eye, direction.normalized, out RaycastHit hit, Reach, mask, QueryTriggerInteraction.Ignore)) return;
            var mark = new PingMark { Position = hit.point, Seat = (byte)Mathf.Max(0, np.CrewSeat), Until = run.Now + Lifetime, Kind = PingKind.Here };
            if (hit.collider.GetComponentInParent<LootItem>() is LootItem loot && loot.TryGetComponent(out NetworkObject lootObject))
            {
                mark.Kind = PingKind.Loot;
                mark.Target = lootObject.NetworkObjectId;
            }
            else if (hit.collider.GetComponentInParent<Threats.Threat>() is Threats.Threat threat)
            {
                mark.Kind = PingKind.Monster;
                mark.Target = threat.NetworkObjectId;
            }
            else if (hit.collider.GetComponentInParent<StructuralSection>() is StructuralSection s && s.CanCollapse && s.Stage > StructuralStage.Stable)
                mark.Kind = PingKind.WeakFloor;
            int mine = 0, oldest = -1;
            for (int i = 0; i < marks.Count; i++)
                if (marks[i].Seat == mark.Seat)
                {
                    mine++;
                    if (oldest < 0) oldest = i;
                }
            if (mine >= PerPlayer && oldest >= 0) marks.RemoveAt(oldest);
            marks.Add(mark);
        }

        private void Update()
        {
            if (!IsSpawned) return;
            if (marks.Count > lastCount && marks.Count > 0) GameAudio.PlayUi(SoundId.PingBlip, 0.8f);
            lastCount = marks.Count;
            if (!IsServer) return;
            for (int i = marks.Count - 1; i >= 0; i--)
                if (marks[i].Until <= run.Now) marks.RemoveAt(i);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}

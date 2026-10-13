using Abandoned.Core;
using System.Collections.Generic;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.UI;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// The replacement worker (0.19.1), on the run state's object: at the truck's radio a living player calls
    /// the agency, and the longest-dead crewmate comes back as a new hire standing in the truck bay. The fee
    /// comes off the payout at departure (a negative line through <see cref="RunState.Settling"/>). It is a
    /// respawn, not a join: "join only at the HQ" stands.
    /// </summary>
    public class RunHires : NetworkBehaviour
    {
        private readonly NetworkVariable<int> hires = new();
        private readonly List<int> fees = new();
        private readonly Dictionary<NetworkPlayer, float> diedAt = new();
        private RunState run;

        public static RunHires Current { get; private set; }

        public override void OnNetworkSpawn()
        {
            Current = this;
            run = GetComponent<RunState>();
            if (IsServer) RunState.Settling += Settle;
        }

        public override void OnNetworkDespawn()
        {
            RunState.Settling -= Settle;
            if (Current == this) Current = null;
        }

        /// <summary>The fee a hire would cost now (every machine, from the synced haul).</summary>
        public int Fee
        {
            get
            {
                BonusConfig config = BonusConfig.Current;
                int haul = run != null ? run.State.Haul : 0;
                return Mathf.Max(config.HireMinFee, Mathf.RoundToInt(haul * config.HireShare / 50f) * 50);
            }
        }

        public bool HiresLeft => hires.Value < BonusConfig.Current.HiresPerJob;

        // Deaths are noted as they happen. Before, a death was only timed when somebody first looked at
        // the radio (or the host got the request), so everyone dead by then shared one moment and "dead
        // longest" was whoever came first in the player list.
        private void Update()
        {
            if (IsSpawned) NoteDeaths();
        }

        private void NoteDeaths()
        {
            IReadOnlyList<NetworkPlayer> all = NetworkPlayer.All;
            for (int i = 0; i < all.Count; i++)
            {
                NetworkPlayer p = all[i];
                if (p == null || !p.IsSpawned) continue;
                if (!p.IsDead) diedAt.Remove(p);
                else if (!diedAt.ContainsKey(p)) diedAt[p] = Time.time;
            }
        }

        /// <summary>The crewmate a hire would replace: the one dead longest (every machine's view).</summary>
        public NetworkPlayer Replaces()
        {
            NoteDeaths();
            NetworkPlayer best = null;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || !p.IsDead) continue;
                if (best == null || diedAt[p] < diedAt[best]) best = p;
            }
            return best;
        }

        public void RequestHire() => HireRpc();

        [Rpc(SendTo.Server)]
        private void HireRpc(RpcParams rpcParams = default)
        {
            NetworkObject caller = NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient c) ? c.PlayerObject : null;
            TruckCargo truck = TruckCargo.Current;
            if (caller == null || truck == null || run == null || run.State.Phase != RunPhase.Running || !HiresLeft) return;
            if (!caller.TryGetComponent(out NetworkPlayer np) || np.IsDead) return;
            if (Vector3.Distance(caller.transform.position, truck.Ignition.position) > run.Config.IgnitionRange + 1.5f) return;
            NetworkPlayer dead = Replaces();
            if (dead == null) return;
            int fee = Fee;
            fees.Add(fee);
            hires.Value++;
            Vector3 at = truck.transform.position;
            if (Physics.Raycast(at + Vector3.up * 2f, Vector3.down, out RaycastHit floor, 4f, ~LayerMask.GetMask(Core.GameLayers.Player, Core.GameLayers.Loot), QueryTriggerInteraction.Ignore))
                at = floor.point + Vector3.up * 0.05f;
            dead.ServerRespawn(new Pose(at, truck.transform.rotation));
            HiredRpc(dead.DisplayName, fee);
            Debug.Log($"[Run] {dead.DisplayName} replaced by a new hire for ${fee:N0}.");
        }

        [Rpc(SendTo.Everyone)]
        private void HiredRpc(string name, int fee) =>
            ToastFeed.Show(Loc.T("NEW HIRE"), Loc.T("The agency sent a replacement for {0}. ${1:N0} comes off the payout.", name, fee), null, ToastFeed.Kind.Warn);

        private void Settle(IReadOnlyCollection<LootItem> aboard, List<(int pay, string line)> lines)
        {
            foreach (int fee in fees) lines.Add((-fee, Loc.Wire("AGENCY FEE: a replacement worker. -${0}", fee.ToString("N0"))));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}

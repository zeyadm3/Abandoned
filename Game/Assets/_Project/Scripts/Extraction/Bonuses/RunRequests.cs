using Abandoned.Core;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.UI;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Boss requests (0.18.0), on the run state's object. A few seconds into each job the boss names one of
    /// the big pieces actually in the building ("bring me the piano"): any item of that kind aboard when the
    /// truck leaves pays a bonus on top of the haul, met quota or not. Pushes crews toward heavy loot.
    /// </summary>
    public class RunRequests : NetworkBehaviour
    {
        private readonly NetworkVariable<FixedString64Bytes> wantId = new();
        private readonly NetworkVariable<FixedString64Bytes> wantName = new();
        private readonly NetworkVariable<int> bonus = new();
        private float callAt = -1f;
        private string toasted;
        // Host: the wanted definition's whole id (the synced copy is cut to fit the wire).
        private string wantFullId;

        public static RunRequests Current { get; private set; }
        public string WantName => wantName.Value.ToString();
        public int Bonus => bonus.Value;

        public override void OnNetworkSpawn()
        {
            Current = this;
            RunGoals.Add(Goal);
            if (!IsServer) return;
            callAt = Time.time + BonusConfig.Current.RequestDelay;
            RunState.Settling += Settle;
        }

        public override void OnNetworkDespawn()
        {
            RunGoals.Remove(Goal);
            RunState.Settling -= Settle;
            if (Current == this) Current = null;
        }

        private string Goal() => bonus.Value > 0 ? Loc.T("BOSS WANTS: the {0} (+${1:N0})", Loc.T(WantName), bonus.Value) : null;

        private void Update()
        {
            if (!IsSpawned) return;
            if (bonus.Value > 0 && toasted != WantName)
            {
                toasted = WantName;
                ToastFeed.Show(Loc.T("BOSS REQUEST"), Loc.T("\"Bring me the {0}.\" +${1:N0} on top of quota.", Loc.T(WantName), bonus.Value), null, ToastFeed.Kind.Info);
            }
            if (!IsServer || callAt < 0f || Time.time < callAt) return;
            callAt = -1f;
            Pick();
        }

        private void Pick()
        {
            NetworkLootSpawner spawner = NetworkLootSpawner.Current;
            RunState run = GetComponent<RunState>();
            if (spawner == null || run == null) return;
            List<LootItem> found = spawner.Spawned.Where(l => l != null && l.Item != null && l.Item.Definition != null).Select(l => l.Item).ToList();
            // The biggest things in the building first: Huge, then Heavy, then anything two-handed.
            List<LootItem> pool = found.Where(i => i.CarryClass >= CarryClass.Heavy).ToList();
            if (pool.Count == 0) pool = found.Where(i => i.CarryClass >= CarryClass.TwoHand).ToList();
            if (pool.Count == 0) return;
            pool = pool.OrderBy(i => i.Definition.Id).ThenBy(i => i.FullValue).ToList();
            LootItem pick = pool[new System.Random(run.State.Seed ^ 0x0b0180).Next(pool.Count)];
            BonusConfig config = BonusConfig.Current;
            int pay = Mathf.Max(config.RequestMinBonus, Mathf.RoundToInt(pick.FullValue * config.RequestValueShare / 50f) * 50);
            wantFullId = pick.Definition.Id;
            // Loot is data: a long id or name must be cut to fit, not throw (FixedString64 holds 61 bytes).
            wantId.Value = Fit(pick.Definition.Id);
            wantName.Value = Fit(pick.Definition.DisplayName);
            bonus.Value = pay;
            Debug.Log($"[Run] Boss request: {pick.Definition.DisplayName} for +${pay:N0}.");
        }

        private static FixedString64Bytes Fit(string text)
        {
            var fs = new FixedString64Bytes();
            fs.CopyFromTruncated(text ?? "");
            return fs;
        }

        private void Settle(IReadOnlyCollection<LootItem> aboard, List<(int pay, string line)> lines)
        {
            if (bonus.Value <= 0) return;
            string id = wantFullId ?? wantId.Value.ToString();
            bool brought = aboard.Any(i => i != null && i.Definition != null && i.Definition.Id == id && !i.IsShattered);
            lines.Add(brought ? (bonus.Value, Loc.Wire("BOSS REQUEST: the {0} made it. +${1}", Loc.Word(WantName), bonus.Value.ToString("N0")))
                : (0, Loc.Wire("BOSS REQUEST missed: no {0}. The boss noticed.", Loc.Word(WantName))));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}

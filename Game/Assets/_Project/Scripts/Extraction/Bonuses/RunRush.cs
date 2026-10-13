using Abandoned.Core;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.UI;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Rush orders (0.18.1), on the run state's object. Mid-job, dispatch radios a rush: one kind of item
    /// still in the building pays double if it reaches the truck in the next few minutes. The host notes each
    /// item of that kind that lands in the bay during the window; those still aboard at departure pay extra.
    /// </summary>
    public class RunRush : NetworkBehaviour
    {
        private readonly NetworkVariable<FixedString64Bytes> rushName = new();
        private readonly NetworkVariable<double> rushUntil = new();
        private readonly HashSet<LootItem> rushed = new();
        // Already in the bay when the rush was called: they were not rushed there, so they don't pay.
        private readonly HashSet<LootItem> aboardAtCall = new();
        private string rushId;
        private RunState run;
        private System.Random random;
        private float nextRush = -1f, nextCheck;
        private double toastedUntil;

        public override void OnNetworkSpawn()
        {
            run = GetComponent<RunState>();
            RunGoals.Add(Goal);
            if (!IsServer) return;
            nextRush = Time.time + BonusConfig.Current.RushFirst;
            RunState.Settling += Settle;
        }

        public override void OnNetworkDespawn()
        {
            RunGoals.Remove(Goal);
            RunState.Settling -= Settle;
        }

        private float Left => run != null ? Mathf.Max(0f, (float)(rushUntil.Value - run.Now)) : 0f;

        private string Goal()
        {
            float left = Left;
            return left > 0f ? Loc.T("RUSH: {0} pays x{1:0.#} for {2}:{3:00}", Loc.T(rushName.Value.ToString()), BonusConfig.Current.RushMultiplier, (int)left / 60, (int)left % 60) : null;
        }

        private void Update()
        {
            if (!IsSpawned || run == null) return;
            if (rushUntil.Value > toastedUntil && Left > 0f)
            {
                toastedUntil = rushUntil.Value;
                ToastFeed.Show(Loc.T("RUSH ORDER"), Loc.T("Any {0} in the truck in the next {1:0.#} minutes pays x{2:0.#}.", Loc.T(rushName.Value.ToString()), BonusConfig.Current.RushSeconds / 60f, BonusConfig.Current.RushMultiplier), null, ToastFeed.Kind.Info);
                RunHorrorDirector.Say(Loc.T("RUSH ORDER / {0}", Loc.T(rushName.Value.ToString())), 6f);
            }
            if (!IsServer || run.State.Phase != RunPhase.Running) return;
            random ??= new System.Random(run.State.Seed ^ 0x0b0181);
            if (nextRush >= 0f && Time.time >= nextRush) Call();
            if (Left > 0f && Time.time >= nextCheck && TruckCargo.Current != null)
            {
                nextCheck = Time.time + 0.5f;
                foreach (LootItem item in TruckCargo.Current.ItemsInside())
                    if (item != null && item.Definition != null && item.Definition.Id == rushId && !aboardAtCall.Contains(item)) rushed.Add(item);
            }
        }

        private void Call()
        {
            BonusConfig config = BonusConfig.Current;
            nextRush = Time.time + config.RushSeconds + Mathf.Lerp(config.RushGap.x, config.RushGap.y, (float)random.NextDouble());
            NetworkLootSpawner spawner = NetworkLootSpawner.Current;
            TruckCargo truck = TruckCargo.Current;
            if (spawner == null) return;
            // Something still out there: not already in the bay.
            var inBay = truck != null ? new HashSet<LootItem>(truck.ItemsInside()) : new HashSet<LootItem>();
            var kinds = spawner.Spawned.Where(l => l != null && l.Item != null && l.Item.Definition != null && !l.Item.IsShattered && !inBay.Contains(l.Item))
                .Select(l => l.Item.Definition).Distinct().OrderBy(d => d.Id).ToList();
            if (kinds.Count == 0) return;
            LootDefinition pick = kinds[random.Next(kinds.Count)];
            rushId = pick.Id;
            // The kind is picked because one of it is still out there, but others of it may already be
            // loaded: without this they'd be noted on the next check and paid double for nothing.
            aboardAtCall.Clear();
            foreach (LootItem item in inBay)
                if (item != null && item.Definition != null && item.Definition.Id == rushId) aboardAtCall.Add(item);
            var wireName = new FixedString64Bytes();
            wireName.CopyFromTruncated(pick.DisplayName ?? ""); // a long name must not throw on the wire
            rushName.Value = wireName;
            rushUntil.Value = run.Now + config.RushSeconds;
            Debug.Log($"[Run] Rush order: {pick.DisplayName} for {config.RushSeconds:0} s.");
        }

        private void Settle(IReadOnlyCollection<LootItem> aboard, List<(int pay, string line)> lines)
        {
            if (rushed.Count == 0) return;
            float extra = BonusConfig.Current.RushMultiplier - 1f;
            int pay = 0, count = 0;
            string kind = null;
            bool mixed = false;
            foreach (LootItem item in rushed)
                if (item != null && aboard.Contains(item) && !item.IsShattered)
                {
                    pay += Mathf.RoundToInt(item.CurrentValue * extra);
                    count++;
                    // Several rushes in one job pay together: only name the kind when it was the only one.
                    string itemKind = item.Definition != null ? item.Definition.DisplayName : null;
                    if (kind == null) kind = itemKind;
                    else if (kind != itemKind) mixed = true;
                }
            if (count > 0) lines.Add((pay, Loc.Wire("RUSH ORDER: {0} x {1} in time. +${2}", count, Loc.Word(mixed || kind == null ? Loc.Mark("rushed items") : kind), pay.ToString("N0"))));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Host: the run's funny numbers for the appraisal (PLAYBOOK 5.4): the most expensive thing broken and
    /// who broke it, who broke the most, the longest fall, floors that gave way, what was left behind.
    /// Damage is blamed on whoever last held the item within a few seconds (a throw counts).
    /// </summary>
    public sealed class RunStats
    {
        private const float BlameWindow = 5f;

        private readonly Dictionary<LootItem, (ulong client, float time)> lastHeld = new();
        private readonly Dictionary<ulong, int> brokenValue = new();
        private readonly Dictionary<LootItem, int> damageSoFar = new();
        private readonly Dictionary<ulong, float> airborneFrom = new(), longestFall = new();
        private readonly List<NetworkLoot> items = new();
        private (string item, int value, ulong? by) worstLoss;
        private int collapses;

        public void Begin(IReadOnlyList<NetworkLoot> runItems)
        {
            End();
            lastHeld.Clear();
            brokenValue.Clear();
            damageSoFar.Clear();
            airborneFrom.Clear();
            longestFall.Clear();
            worstLoss = default;
            collapses = 0;
            items.AddRange(runItems.Where(l => l != null));
            foreach (NetworkLoot l in items)
            {
                l.Item.Damaged += OnDamaged;
                l.Item.Shattered += OnShattered;
            }
            StructureSignals.SectionCollapsed += OnCollapsed;
        }

        public void End()
        {
            foreach (NetworkLoot l in items)
            {
                if (l == null) continue;
                l.Item.Damaged -= OnDamaged;
                l.Item.Shattered -= OnShattered;
            }
            items.Clear();
            StructureSignals.SectionCollapsed -= OnCollapsed;
        }

        /// <summary>Host, every frame: who's holding what, and who's falling.</summary>
        public void Tick(NetworkManager manager)
        {
            foreach (NetworkLoot l in items)
            {
                if (l == null || !l.IsSpawned || l.Hold.Mode == LootHoldMode.Free) continue;
                if (manager.SpawnManager.SpawnedObjects.TryGetValue(l.Hold.HolderObjectId, out NetworkObject holder))
                    lastHeld[l.Item] = (holder.OwnerClientId, Time.time);
            }
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.NetworkManager != manager) continue;
                ulong id = p.OwnerClientId;
                float y = p.transform.position.y;
                if (!p.State.Grounded) { if (!airborneFrom.ContainsKey(id)) airborneFrom[id] = y; }
                else if (airborneFrom.TryGetValue(id, out float from))
                {
                    airborneFrom.Remove(id);
                    float drop = from - y;
                    if (drop > longestFall.GetValueOrDefault(id)) longestFall[id] = drop;
                }
            }
        }

        private ulong? Blame(LootItem item) =>
            lastHeld.TryGetValue(item, out var h) && Time.time - h.time <= BlameWindow ? h.client : null;

        private void OnDamaged(LootItem item, int loss, Vector3 point)
        {
            damageSoFar[item] = damageSoFar.GetValueOrDefault(item) + loss;
            if (Blame(item) is ulong who) brokenValue[who] = brokenValue.GetValueOrDefault(who) + loss;
        }

        // A shatter takes whatever value was left (it doesn't raise Damaged).
        private void OnShattered(LootItem item, Vector3 point)
        {
            int remaining = Mathf.Max(0, item.FullValue - damageSoFar.GetValueOrDefault(item));
            ulong? who = Blame(item);
            if (who.HasValue) brokenValue[who.Value] = brokenValue.GetValueOrDefault(who.Value) + remaining;
            if (item.FullValue > worstLoss.value) worstLoss = (item.Definition.DisplayName, item.FullValue, who);
        }

        private void OnCollapsed(Bounds surface) => collapses++;

        public string[] Lines(RunResults results)
        {
            var lines = new List<string>();
            if (worstLoss.item != null)
                lines.Add(worstLoss.by is ulong w
                    ? Loc.Wire("Most expensive mistake: {0} (${1}) - {2}", Loc.Word(worstLoss.item), worstLoss.value.ToString("N0"), Name(w))
                    : Loc.Wire("Most expensive mistake: {0} (${1})", Loc.Word(worstLoss.item), worstLoss.value.ToString("N0")));
            if (brokenValue.Count > 0)
            {
                var top = brokenValue.OrderByDescending(kv => kv.Value).First();
                lines.Add(Loc.Wire("Butterfingers: {0} broke ${1} worth of loot", Name(top.Key), top.Value.ToString("N0")));
            }
            if (longestFall.Count > 0)
            {
                var top = longestFall.OrderByDescending(kv => kv.Value).First();
                if (top.Value > 1.5f) lines.Add(Loc.Wire("Longest fall: {0}, {1} m", Name(top.Key), top.Value.ToString("0.0")));
            }
            if (collapses > 0) lines.Add(Loc.Wire("Floors that gave way: {0}", collapses));
            int carried = results.Items.Length;
            var left = items.Where(l => l != null && l.IsSpawned && !l.Item.IsShattered).ToList();
            int leftCount = Mathf.Max(0, left.Count - carried);
            if (leftCount > 0) lines.Add(Loc.Wire("Left in the building: about {0} items", leftCount));
            if (lines.Count == 0) lines.Add(Loc.Mark("A clean job. Suspiciously clean."));
            return lines.ToArray();
        }

        /// <summary>
        /// 0.19.3: one job title per player, the first that fits them best: who brought the most home, who
        /// broke the most, who fell furthest, who died; the rest were present.
        /// </summary>
        public string[] Blame(RunResults results)
        {
            var delivered = new Dictionary<ulong, int>();
            TruckCargo truck = TruckCargo.Current;
            foreach (NetworkLoot l in items)
            {
                if (l == null || !l.IsSpawned || l.Item.IsShattered || truck == null || !truck.Carries(l.transform.position)) continue;
                if (lastHeld.TryGetValue(l.Item, out var h)) delivered[h.client] = delivered.GetValueOrDefault(h.client) + l.Item.CurrentValue;
            }
            // Lines travel as English with their values ({0} the player, {1} the number): every machine words them.
            var titles = new Dictionary<ulong, (string line, int value)>();
            void Award(Dictionary<ulong, int> by, string line)
            {
                foreach (var kv in by.OrderByDescending(kv => kv.Value))
                    if (kv.Value > 0 && !titles.ContainsKey(kv.Key)) { titles[kv.Key] = (line, kv.Value); return; }
            }
            Award(delivered, Loc.Mark("{0}: Employee of the Month (brought home ${1})"));
            Award(brokenValue, Loc.Mark("{0}: Demolition Specialist (broke ${1} of stock)"));
            Award(longestFall.ToDictionary(kv => kv.Key, kv => Mathf.RoundToInt(kv.Value)), Loc.Mark("{0}: Gravity Tester (fell {1} m)"));
            var lines = new List<string>();
            foreach (RunResults.Player p in results.Players)
                lines.Add(titles.TryGetValue(p.ClientId, out var title) ? Loc.Wire(title.line, p.Name, title.value.ToString("N0"))
                    : Loc.Wire(p.Died ? Loc.Mark("{0}: Deceased (unpaid)") : !p.Extracted ? Loc.Mark("{0}: Missing, presumed lazy") : Loc.Mark("{0}: Present"), p.Name));
            return lines.ToArray();
        }

        // A stand-in name travels as a word (Loc.Wire value): each machine words "Player 2" itself.
        private static string Name(ulong client) => NetworkPlayer.WireNameOf(client);
    }
}

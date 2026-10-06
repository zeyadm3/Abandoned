using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// The 'loot' scenario's verdict, free of networking so EditMode tests can feed it good and bad
    /// data. The host's view is the truth (it owns value and resting physics): every machine must see
    /// the same items with the same value, condition and hold state, within tolerance of the host's
    /// position; every client must have been handed the physics on pickup and lost it on the throw;
    /// the thrown items must have moved and carry the damage the host applied.
    /// </summary>
    public static class LootNetTestCheck
    {
        public const ulong HostId = 0;

        public static List<string> Verify(IReadOnlyList<NetTestLootView> views, IReadOnlyList<NetTestLootAction> actions,
            int expectedPlayers, int expectedClients, float tolerance, float minMove)
        {
            var errors = new List<string>();
            if (views.Count != expectedPlayers)
                errors.Add($"expected a loot view from each of {expectedPlayers} machines, got {views.Count}");
            NetTestLootView host = views.FirstOrDefault(v => v.observer == HostId);
            if (host == null)
            {
                errors.Add("no loot view from the host");
                return errors;
            }
            if (host.items.Count == 0) errors.Add("the host sees no networked loot at all");

            CheckActions(host, actions, expectedClients, minMove, errors);
            foreach (NetTestLootSnapshot truth in host.items)
            {
                if (truth.owner != HostId || truth.hold != LootHoldState.Free.ToString())
                    errors.Add($"item {Name(truth)} should be free and host-simulated at the end, is owner c{truth.owner}, {truth.hold}");
            }
            foreach (NetTestLootView view in views.Where(v => v != host))
                CompareWithHost(host, view, tolerance, errors);
            return errors;
        }

        private static void CheckActions(NetTestLootView host, IReadOnlyList<NetTestLootAction> actions, int expectedClients,
            float minMove, List<string> errors)
        {
            if (actions.Count != expectedClients)
                errors.Add($"expected a pickup+throw from each of {expectedClients} clients, got {actions.Count}");
            foreach (NetTestLootAction action in actions)
            {
                if (!action.gotOwnership) errors.Add($"client {action.client} never got item #{action.item}'s physics after picking it up");
                if (!action.returnedOwnership) errors.Add($"item #{action.item} didn't go back to the host after client {action.client} threw it");
                NetTestLootSnapshot truth = host.items.FirstOrDefault(i => i.id == action.item);
                if (truth == null)
                {
                    errors.Add($"the host no longer has client {action.client}'s item #{action.item}");
                    continue;
                }
                Vector3 moved = truth.position - action.start;
                moved.y = 0f;
                if (moved.magnitude < minMove)
                    errors.Add($"client {action.client}'s item {Name(truth)} moved only {moved.magnitude:0.00} m on the host (need {minMove:0.00})");
                if (truth.currentValue >= truth.fullValue)
                    errors.Add($"client {action.client}'s item {Name(truth)} shows no damage on the host (${truth.currentValue} of ${truth.fullValue})");
            }
        }

        private static void CompareWithHost(NetTestLootView host, NetTestLootView view, float tolerance, List<string> errors)
        {
            string who = $"machine {view.observer}";
            foreach (NetTestLootSnapshot truth in host.items)
            {
                NetTestLootSnapshot seen = view.items.FirstOrDefault(i => i.id == truth.id);
                if (seen == null)
                {
                    errors.Add($"{who} is missing item {Name(truth)}");
                    continue;
                }
                if (seen.fullValue != truth.fullValue || seen.currentValue != truth.currentValue || seen.shattered != truth.shattered)
                    errors.Add($"{who} sees {Name(truth)} at ${seen.currentValue}/{seen.fullValue}{(seen.shattered ? " shattered" : "")}, host ${truth.currentValue}/{truth.fullValue}{(truth.shattered ? " shattered" : "")}");
                if (seen.owner != truth.owner || seen.hold != truth.hold || seen.hidden != truth.hidden)
                    errors.Add($"{who} sees {Name(truth)} as owner c{seen.owner}, {seen.hold}{(seen.hidden ? ", hidden" : "")}; host: c{truth.owner}, {truth.hold}{(truth.hidden ? ", hidden" : "")}");
                float off = Vector3.Distance(seen.position, truth.position);
                if (!truth.hidden && off > tolerance)
                    errors.Add($"{who} sees {Name(truth)} at {seen.position}, {off:0.00} m from the host's {truth.position} (tolerance {tolerance:0.00})");
            }
            foreach (NetTestLootSnapshot extra in view.items.Where(i => host.items.All(t => t.id != i.id)))
                errors.Add($"{who} has item {Name(extra)} the host doesn't (a stale or duplicated copy)");
        }

        private static string Name(NetTestLootSnapshot s) => $"#{s.id} {s.item}";
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// A whole run in the mall across processes: everyone sees the same run (seed, loot count), the host
    /// loads loot into the truck and the haul reaches every screen, a client starts the truck from the
    /// bay, the last client stays outside; after the honk every machine gets the same results (who made it
    /// out), then the host starts the next run and everyone is back on their spawn with fresh loot.
    /// </summary>
    public sealed class RunNetTestScenario : INetTestScenario, INetTestScene
    {
        public const string ScenarioName = "run";
        private const float ConnectTimeout = 40f, StepTimeout = 25f;
        private const string Board = "board", Boarded = "boarded", Start = "start", Results = "results",
            NextRun = "nextrun", Respawned = "respawned", Done = "done";

        public string Name => ScenarioName;
        public string Scene => "Mall";

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            RunState run = null;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players
                                           && (run = RunState.Current) != null && run.IsSpawned && TruckCargo.Current != null,
                $"{players} players and the run", ConnectTimeout);
            if (ctx.Aborted) yield break;
            int seed = run.State.Seed, loot = LootCount(ctx.Manager);
            ctx.Note($"run seed {seed}, {loot} loot items");

            // Everyone but the last client boards; the first client will start the truck.
            List<ulong> clients = ctx.Manager.ConnectedClientsIds.Where(id => id != ctx.Manager.LocalClientId).OrderBy(id => id).ToList();
            ulong stayer = clients.Last(), driver = clients.First();
            foreach (ulong c in clients) ctx.Channel.Send(c, Board, $"{(c == stayer ? 0 : 1)};{seed};{loot}");
            ctx.OwnPlayer.OwnerTeleport(BayPoint(0));
            var boarded = new List<NetTestMessage>();
            yield return ctx.Collect(Boarded, clients.Count, boarded, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in boarded.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender}: {m.Payload}");

            // Load something into the truck: a resting item the host simulates.
            NetworkLoot cargo = Object.FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None)
                .First(l => l.IsSpawned && l.Hold.Mode == LootHoldMode.Free && l.Item.Definition.CarryClass <= Abandoned.Interaction.CarryClass.OneHand
                            && l.Item.Definition.Fragility <= Abandoned.Loot.Fragility.Low);
            cargo.Grabbable.Body.position = TruckCargo.Current.transform.TransformPoint(new Vector3(0f, 1.2f, -1.5f)); // rear of the bay
            cargo.transform.position = cargo.Grabbable.Body.position;
            cargo.ServerTeleport();
            yield return ctx.WaitFor(() => run.State.Haul > 0, "the haul to count the cargo", StepTimeout);
            if (ctx.Aborted) yield break;
            ctx.Note($"haul ${run.State.Haul:N0} with {cargo.Item.Definition.DisplayName}");

            ctx.Channel.Send(driver, Start, "");
            yield return ctx.WaitFor(() => run.Results != null, "the truck to leave", StepTimeout);
            if (ctx.Aborted) yield break;
            RunResults r = run.Results;
            ctx.Note($"left with ${r.Haul:N0}; aboard: {string.Join(", ", r.Players.Select(p => $"{p.Name}={(p.Extracted ? "in" : "out")}"))}");
            if (r.Players.Count(p => p.Extracted) != players - 1) ctx.Fail($"expected {players - 1} aboard, got {r.Players.Count(p => p.Extracted)}");
            if (r.Players.Single(p => p.ClientId == stayer).Extracted) ctx.Fail("the client who stayed outside rode along");
            ctx.Channel.SendToClients(Results, $"{r.Haul};{stayer}");
            var reports = new List<NetTestMessage>();
            yield return ctx.Collect(Results, clients.Count, reports, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in reports.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender}: {m.Payload}");

            Object.FindAnyObjectByType<RunDirector>().StartNextRun();
            yield return ctx.WaitFor(() => RunState.Current != null && RunState.Current != run && RunState.Current.IsSpawned, "the next run", StepTimeout);
            if (ctx.Aborted) yield break;
            ctx.Note($"next run seed {RunState.Current.State.Seed}");
            ctx.Channel.SendToClients(NextRun, RunState.Current.State.Seed.ToString());
            var respawned = new List<NetTestMessage>();
            yield return ctx.Collect(Respawned, clients.Count, respawned, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in respawned.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender}: {m.Payload}");
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null && RunState.Current != null
                                           && RunState.Current.IsSpawned && TruckCargo.Current != null, "own player and the run", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ctx.Result.clientId = ctx.Manager.LocalClientId;
            string[] order = null;
            yield return ctx.Receive(Board, StepTimeout, m => order = m.Payload.Split(';'));
            if (ctx.Aborted) yield break;
            bool board = order[0] == "1";
            RunState run = RunState.Current;
            string problem = run.State.Seed.ToString() != order[1] ? $"seed {run.State.Seed} vs host {order[1]}" : null;
            yield return ctx.WaitFor(() => LootCount(ctx.Manager).ToString() == order[2], "the host's loot to arrive", StepTimeout);
            if (ctx.Aborted) yield break;
            ctx.OwnPlayer.OwnerTeleport(board ? BayPoint((int)ctx.Manager.LocalClientId) : new Vector3(8f, 0.05f, -16f));
            yield return NetTestContext.Seconds(0.5f);
            ctx.Channel.SendToHost(Boarded, problem ?? "ok");

            NetTestMessage next = default;
            yield return ctx.WaitFor(() => ctx.Channel.TryTake(Start, out next) || ctx.Channel.TryTake(Results, out next), "the host's next step", StepTimeout);
            if (ctx.Aborted) yield break;
            if (next.Kind == Start)
            {
                yield return ctx.WaitFor(() => run.State.Haul > 0, "the haul to reach this screen", StepTimeout);
                if (ctx.Aborted) yield break;
                ctx.Note($"haul here ${run.State.Haul:N0}; starting the truck");
                run.RequestDepart();
                yield return ctx.WaitFor(() => run.State.Phase == RunPhase.Honking, "the honk", StepTimeout);
                if (ctx.Aborted) yield break;
                yield return ctx.Receive(Results, StepTimeout, m => next = m);
                if (ctx.Aborted) yield break;
            }
            string[] expected = next.Payload.Split(';');
            yield return ctx.WaitFor(() => run.Results != null, "the results here", StepTimeout);
            if (ctx.Aborted) yield break;
            RunResults r = run.Results;
            RunResults.Player me = r.Players.Single(p => p.ClientId == ctx.Manager.LocalClientId);
            string verdict = r.Haul.ToString() != expected[0] ? $"haul {r.Haul} vs host {expected[0]}"
                : me.Extracted != board ? $"extracted={me.Extracted}, boarded={board}" : "ok";
            ctx.Note($"results: ${r.Haul:N0}, me {(me.Extracted ? "aboard" : "left behind")}");
            ctx.Channel.SendToHost(Results, verdict);

            NetTestMessage seedMessage = default;
            yield return ctx.Receive(NextRun, StepTimeout, m => seedMessage = m);
            if (ctx.Aborted) yield break;
            yield return ctx.WaitFor(() => RunState.Current != null && RunState.Current != run && RunState.Current.IsSpawned
                                           && RunState.Current.State.Seed.ToString() == seedMessage.Payload, "the next run here", StepTimeout);
            if (ctx.Aborted) yield break;
            yield return NetTestContext.Seconds(1f);
            bool slotKnown = TryOwnSpawn(ctx, out Vector3 spawn);
            float off = Vector3.Distance(ctx.OwnPlayer.transform.position, spawn);
            string again = !slotKnown ? "no spawn point" : off > 2f ? $"{off:0.0} m from my spawn after the next run" :
                RunState.Current.State.Haul != 0 ? "haul not reset" : "ok";
            ctx.Note($"next run: {off:0.0} m from spawn, {LootCount(ctx.Manager)} loot");
            ctx.Channel.SendToHost(Respawned, again);
            yield return ctx.Receive(Done, StepTimeout, m =>
            {
                if (m.Payload != "pass") ctx.Fail("the host's check failed (see the host's result)");
            });
        }

        private static Vector3 BayPoint(int i) =>
            TruckCargo.Current.transform.position + new Vector3(0f, 0.45f, 0f) + Vector3.forward * (i % 2 == 0 ? 0.6f : -0.6f) + Vector3.right * (i * 0.5f - 0.5f);

        // Same rule as the host's slots: the player's spawn point is the one nearest to where NGO first put them.
        private static bool TryOwnSpawn(NetTestContext ctx, out Vector3 spawn)
        {
            PlayerSpawnPoint[] points = Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
            spawn = points.Length == 0 ? Vector3.zero
                : points.Select(p => p.transform.position).OrderBy(p => Vector3.Distance(p, ctx.OwnPlayer.transform.position)).First();
            return points.Length > 0;
        }

        private static int LootCount(NetworkManager manager) =>
            manager.SpawnManager == null ? 0 : manager.SpawnManager.SpawnedObjectsList.Count(o => o != null && o.GetComponent<NetworkLoot>() != null);
    }
}

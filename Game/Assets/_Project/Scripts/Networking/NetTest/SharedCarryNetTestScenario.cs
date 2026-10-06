using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Shared carrying end to end: the host spawns the server rack in front of the two clients standing
    /// furthest east; each grabs a handle through the real interaction handler, and once both hold it
    /// they walk it forward together (the host simulates it from their streamed hold targets), wait,
    /// and let go. After it settles every machine reports where it sees the rack and the host checks
    /// they agree (<see cref="SharedCarryNetTestCheck"/>). Other clients only watch.
    /// </summary>
    public sealed class SharedCarryNetTestScenario : INetTestScenario
    {
        public const string ScenarioName = "sharedcarry";
        public const string TestItemId = "server_rack";
        public const int Carriers = 2;
        public const float Tolerance = 0.25f, MinMove = 0.8f, MinLift = 0.15f;
        private const float ConnectTimeout = 40f, StepTimeout = 25f, ActionTimeout = 6f;
        private const float SpawnAhead = 1.2f, LandTime = 1.5f, WalkDistance = 1.2f, MaxWalkTime = 5f, HoldTime = 1f, SettleTime = 2.5f;
        private const string Role = "role", Grabbed = "grabbed", Walk = "walk", Carried = "carried", Report = "sharedreport", Done = "done";
        private const string Watch = "watch";

        public string Name => ScenarioName;

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players,
                $"{players} connected players", ConnectTimeout);
            if (ctx.Aborted) yield break;
            NetworkObject prefab = LootNetTestScenario.FindPrefab(ctx.Manager, TestItemId);
            List<NetworkPlayer> crew = ctx.Players.Where(p => p.OwnerClientId != ctx.Manager.LocalClientId)
                .OrderByDescending(p => p.transform.position.x).Take(Carriers).ToList();
            if (prefab == null || crew.Count < Carriers)
            {
                ctx.Abort(prefab == null ? $"no registered network prefab for loot '{TestItemId}'" : $"need {Carriers} clients to carry");
                yield break;
            }

            // Between the two carriers, a little ahead, its long axis along the line between them.
            Vector3 a = crew[0].transform.position, b = crew[1].transform.position;
            Vector3 forward = Vector3.ProjectOnPlane(crew[0].transform.forward, Vector3.up).normalized;
            float halfHeight = prefab.GetComponent<Loot.LootItem>().Definition.Size.y / 2f;
            Vector3 at = (a + b) / 2f + forward * SpawnAhead + Vector3.up * (halfHeight + 0.05f);
            NetworkObject spawned = ctx.Manager.SpawnManager.InstantiateAndSpawn(prefab, position: at,
                rotation: Quaternion.LookRotation(Vector3.ProjectOnPlane(b - a, Vector3.up)));
            var net = spawned.GetComponent<NetworkSharedCarry>();
            var host = new NetTestSharedCarryHostView { item = spawned.NetworkObjectId };
            ctx.Result.sharedHost = host;
            foreach (NetworkPlayer p in ctx.Players.Where(p => p.OwnerClientId != ctx.Manager.LocalClientId))
                ctx.Channel.Send(p.OwnerClientId, Role, crew.Contains(p) ? host.item.ToString() : Watch);

            yield return NetTestContext.Seconds(LandTime);
            host.start = spawned.transform.position;
            float rest = net.Shared.Grabbable.GetBounds().min.y;
            var grabbed = new List<NetTestMessage>();
            yield return ctx.Collect(Grabbed, Carriers, grabbed, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetworkPlayer p in crew) ctx.Channel.Send(p.OwnerClientId, Walk, "");

            var carried = new List<NetTestMessage>();
            yield return ctx.WaitFor(() =>
            {
                host.maxCarriers = Mathf.Max(host.maxCarriers, net.Shared.CarrierCount);
                host.maxLift = Mathf.Max(host.maxLift, net.Shared.Grabbable.GetBounds().min.y - rest);
                while (ctx.Channel.TryTake(Carried, out NetTestMessage m))
                    if (carried.All(x => x.Sender != m.Sender)) carried.Add(m);
                return carried.Count >= Carriers;
            }, $"{Carriers} carriers to finish", StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in carried) ctx.Result.sharedActions.Add(JsonUtility.FromJson<NetTestSharedCarryAction>(m.Payload));

            yield return NetTestContext.Seconds(SettleTime);
            host.end = spawned.transform.position;
            host.endCarriers = net.Shared.CarrierCount;
            host.targetsReceived = net.TargetsReceived;
            ctx.Note($"rack #{host.item}: crew {host.maxCarriers}, lifted {host.maxLift:0.00} m, {host.targetsReceived} targets, moved {host.start} -> {host.end}");

            ctx.Channel.SendToClients(Report);
            ctx.Result.lootViews.Add(NetTestLootView.Take(ctx.Manager));
            var reports = new List<NetTestMessage>();
            yield return ctx.Collect(Report, ctx.ExpectedClients, reports, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in reports) ctx.Result.lootViews.Add(JsonUtility.FromJson<NetTestLootView>(m.Payload));

            foreach (string error in SharedCarryNetTestCheck.Verify(ctx.Result.lootViews, ctx.Result.sharedActions, host,
                         players, Carriers, Tolerance, MinMove, MinLift))
                ctx.Fail(error);
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null, "own player to spawn", ConnectTimeout);
            if (ctx.Aborted) yield break;
            string role = null;
            yield return ctx.Receive(Role, ConnectTimeout, m => role = m.Payload);
            if (ctx.Aborted) yield break;

            if (role != Watch && ulong.TryParse(role, out ulong id))
            {
                var action = new NetTestSharedCarryAction { client = ctx.Manager.LocalClientId, item = id };
                yield return Carry(ctx, id, action);
                ctx.Result.sharedActions.Add(action);
                ctx.Channel.SendToHost(Carried, JsonUtility.ToJson(action));
                if (ctx.Aborted) yield break;
            }

            yield return ctx.Receive(Report, StepTimeout + MaxWalkTime, _ => { });
            if (ctx.Aborted) yield break;
            NetTestLootView view = NetTestLootView.Take(ctx.Manager);
            ctx.Result.lootViews.Add(view);
            ctx.Channel.SendToHost(Report, JsonUtility.ToJson(view));
            yield return ctx.Receive(Done, StepTimeout, m =>
            {
                if (m.Payload != "pass") ctx.Fail("the host's cross-machine check failed (see the host's result)");
            });
        }

        private static IEnumerator Carry(NetTestContext ctx, ulong id, NetTestSharedCarryAction action)
        {
            NetworkSharedCarry net = null;
            yield return ctx.WaitFor(() => (net = Find(ctx.Manager, id)) != null, $"rack #{id} to spawn here", ActionTimeout);
            if (ctx.Aborted) yield break;
            yield return NetTestContext.Seconds(LandTime);

            NetworkPlayer player = ctx.OwnPlayer;
            PlayerCarrier carrier = player.Carrier;
            SharedCarryable shared = net.Shared;
            action.carrierStart = player.transform.position;
            InteractionService.Handler.RequestPickup(carrier, shared.Grabbable);
            yield return ctx.WaitFor(() => shared.IsCarriedBy(carrier), "the host to give us a handle", ActionTimeout);
            if (ctx.Aborted)
            {
                ctx.Note($"grab refused: hint '{carrier.Hint}', host '{net.LastHint}', {net.State}, player at {player.transform.position}");
                yield break;
            }
            action.grabbed = true;
            ctx.Channel.SendToHost(Grabbed, "");

            yield return ctx.Receive(Walk, StepTimeout, _ => { });
            if (ctx.Aborted) yield break;
            yield return ctx.WaitFor(() => shared.IsLifted, "the full crew to lift it here", ActionTimeout);
            if (ctx.Aborted) yield break;
            action.sawLifted = true;

            yield return WalkWithIt(player);
            InteractionService.Handler.RequestDrop(carrier);
            yield return ctx.WaitFor(() => !shared.IsCarriedBy(carrier), "the host to confirm letting go", ActionTimeout);
            if (ctx.Aborted) yield break;
            action.letGo = true;
            action.carrierEnd = player.transform.position;
            ctx.Note($"carried rack #{id} from {action.carrierStart} to {action.carrierEnd}");
        }

        /// <summary>The owner's real motor (tethered and speed-capped by the carry), forward then standing still.</summary>
        private static IEnumerator WalkWithIt(NetworkPlayer player)
        {
            PlayerMotor motor = player.Motor;
            motor.enabled = false;
            var forward = new PlayerInputFrame(Vector2.up, Vector2.zero, false, false, false, false, false, false, false, false, false, false);
            Vector3 start = player.transform.position;
            float end = Time.realtimeSinceStartup + MaxWalkTime;
            while (Time.realtimeSinceStartup < end)
            {
                Vector3 d = player.transform.position - start;
                if (new Vector2(d.x, d.z).magnitude >= WalkDistance) break;
                motor.Simulate(forward, Mathf.Min(Time.deltaTime, 1f / 30f));
                yield return null;
            }
            for (float t = 0f; t < HoldTime; t += Mathf.Min(Time.deltaTime, 1f / 30f))
            {
                motor.Simulate(default, Mathf.Min(Time.deltaTime, 1f / 30f));
                yield return null;
            }
            motor.enabled = true;
        }

        private static NetworkSharedCarry Find(NetworkManager manager, ulong id) =>
            manager.SpawnManager != null && manager.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject no) && no != null
                ? no.GetComponent<NetworkSharedCarry>()
                : null;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Loot;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Networked loot end to end: the host spawns a laptop in front of each client; each client picks
    /// it up through the real interaction handler (host validates, hands over the physics), carries it
    /// briefly and throws it (host takes the physics back and clamps the throw). After it settles the
    /// host damages each thrown laptop, then every machine reports every loot item it sees, including
    /// the scene-placed ones, and the host checks they all agree (<see cref="LootNetTestCheck"/>).
    /// </summary>
    public sealed class LootNetTestScenario : INetTestScenario
    {
        public const string ScenarioName = "loot";
        public const string TestItemId = "laptop";
        public const float Tolerance = 0.25f, MinMove = 0.75f;
        /// <summary>Known damage the host applies after the throw (laptop: 25% loss), so value replication is exercised.</summary>
        public const float HostImpactSpeed = 9f;
        private const float ConnectTimeout = 40f, StepTimeout = 20f, ActionTimeout = 5f;
        private const float SpawnDistance = 1f, SpawnHeight = 0.3f, LandTime = 1.5f, CarryTime = 0.75f;
        // Thrown items roll and slide; remote copies trail by the interpolation delay.
        private const float SettleTime = 2.5f, ReplicateTime = 1f;
        private static readonly Vector2 ThrowSpeed = new(3f, 1.5f); // forward, up (m/s)
        private const string Item = "loot", Thrown = "thrown", Report = "lootreport", Done = "done";

        public string Name => ScenarioName;

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players,
                $"{players} connected players", ConnectTimeout);
            if (ctx.Aborted) yield break;
            NetworkObject prefab = FindPrefab(ctx.Manager, TestItemId);
            if (prefab == null)
            {
                ctx.Abort($"no registered network prefab for loot '{TestItemId}'");
                yield break;
            }
            ctx.Note($"{players} players connected; {NetTestLootView.SpawnedLoot(ctx.Manager).Count()} loot items spawned");

            var spawned = new List<NetworkLoot>();
            foreach (NetworkPlayer player in ctx.Players.Where(p => p.OwnerClientId != ctx.Manager.LocalClientId))
            {
                Transform t = player.transform;
                Vector3 at = t.position + t.forward * SpawnDistance + Vector3.up * SpawnHeight;
                NetworkObject item = ctx.Manager.SpawnManager.InstantiateAndSpawn(prefab, position: at, rotation: t.rotation);
                spawned.Add(item.GetComponent<NetworkLoot>());
                ctx.Channel.Send(player.OwnerClientId, Item, item.NetworkObjectId.ToString());
            }

            var thrown = new List<NetTestMessage>();
            yield return ctx.Collect(Thrown, ctx.ExpectedClients, thrown, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in thrown) ctx.Result.lootActions.Add(JsonUtility.FromJson<NetTestLootAction>(m.Payload));

            yield return NetTestContext.Seconds(SettleTime);
            foreach (NetworkLoot loot in spawned.Where(l => l != null && l.IsSpawned))
            {
                int before = loot.Item.CurrentValue;
                loot.Item.ApplyImpact(HostImpactSpeed, loot.transform.position);
                ctx.Note($"host damaged #{loot.NetworkObjectId}: ${before} -> ${loot.Item.CurrentValue}");
            }
            yield return NetTestContext.Seconds(ReplicateTime);

            ctx.Channel.SendToClients(Report);
            ctx.Result.lootViews.Add(NetTestLootView.Take(ctx.Manager));
            var reports = new List<NetTestMessage>();
            yield return ctx.Collect(Report, ctx.ExpectedClients, reports, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in reports) ctx.Result.lootViews.Add(JsonUtility.FromJson<NetTestLootView>(m.Payload));

            foreach (string error in LootNetTestCheck.Verify(ctx.Result.lootViews, ctx.Result.lootActions, players,
                         ctx.ExpectedClients, Tolerance, MinMove))
                ctx.Fail(error);
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null, "own player to spawn", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ulong id = 0;
            yield return ctx.Receive(Item, ConnectTimeout, m => ulong.TryParse(m.Payload, out id));
            if (ctx.Aborted) yield break;

            NetworkLoot loot = null;
            yield return ctx.WaitFor(() => (loot = Find(ctx.Manager, id)) != null, $"item #{id} to spawn here", ActionTimeout);
            if (ctx.Aborted) yield break;
            yield return NetTestContext.Seconds(LandTime);

            var action = new NetTestLootAction { client = ctx.Manager.LocalClientId, item = id, start = loot.transform.position };
            yield return PickUpAndThrow(ctx, loot, action);
            ctx.Result.lootActions.Add(action);
            ctx.Note($"item #{id}: owned after pickup {action.gotOwnership}, back to host after throw {action.returnedOwnership}");
            ctx.Channel.SendToHost(Thrown, JsonUtility.ToJson(action));
            if (ctx.Aborted) yield break;

            yield return ctx.Receive(Report, StepTimeout, _ => { });
            if (ctx.Aborted) yield break;
            NetTestLootView view = NetTestLootView.Take(ctx.Manager);
            ctx.Result.lootViews.Add(view);
            ctx.Channel.SendToHost(Report, JsonUtility.ToJson(view));

            yield return ctx.Receive(Done, StepTimeout, m =>
            {
                if (m.Payload != "pass") ctx.Fail("the host's cross-machine check failed (see the host's result)");
            });
        }

        private static IEnumerator PickUpAndThrow(NetTestContext ctx, NetworkLoot loot, NetTestLootAction action)
        {
            PlayerCarrier carrier = ctx.OwnPlayer.Carrier;
            // The same path a key press takes: the handler checks locally, then asks the host.
            InteractionService.Handler.RequestPickup(carrier, loot.Grabbable);
            yield return ctx.WaitFor(() => loot.IsOwner && carrier.Held == loot.Grabbable, "the host to grant the pickup", ActionTimeout);
            if (ctx.Aborted)
            {
                ctx.Note(Describe(carrier, loot));
                yield break;
            }
            action.gotOwnership = true;
            yield return NetTestContext.Seconds(CarryTime);

            Transform t = carrier.transform;
            action.released = loot.transform.position;
            InteractionService.Handler.RequestThrow(carrier, t.forward * ThrowSpeed.x + Vector3.up * ThrowSpeed.y);
            yield return ctx.WaitFor(() => !loot.IsOwner && carrier.Held == null, "the host to take the thrown item back", ActionTimeout);
            if (ctx.Aborted)
            {
                ctx.Note(Describe(carrier, loot));
                yield break;
            }
            action.returnedOwnership = true;
        }

        // What this machine believed when a step timed out: refusal hints, hold state, positions.
        private static string Describe(PlayerCarrier carrier, NetworkLoot loot) =>
            $"item #{loot.NetworkObjectId} at {loot.transform.position}: {loot.Hold}, owner c{loot.OwnerClientId}, " +
            $"host said '{loot.LastHint}'; player at {carrier.transform.position} eye {carrier.EyePosition}, " +
            $"holding {(carrier.Held != null ? carrier.Held.name : "nothing")}, hint '{carrier.Hint}'";

        private static NetworkLoot Find(NetworkManager manager, ulong id) =>
            manager.SpawnManager != null && manager.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject no) && no != null
                ? no.GetComponent<NetworkLoot>()
                : null;

        /// <summary>From the session's own prefab list, so it works in a player build (no AssetDatabase).</summary>
        public static NetworkObject FindPrefab(NetworkManager manager, string lootId)
        {
            foreach (NetworkPrefab p in manager.NetworkConfig.Prefabs.Prefabs)
            {
                if (p.Prefab == null || !p.Prefab.TryGetComponent(out LootItem item)) continue;
                if (item.Definition != null && item.Definition.Id == lootId) return p.Prefab.GetComponent<NetworkObject>();
            }
            return null;
        }
    }
}

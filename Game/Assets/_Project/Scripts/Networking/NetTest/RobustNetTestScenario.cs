using System.Collections;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Leaving, both ways. The client with the highest id picks up a laptop and quits while carrying
    /// it: the host must take the item back (free, host-owned, host-simulated, still spawned) and the
    /// other clients must see it free. Then the host leaves: every remaining client must end up back
    /// at the menu (scene reloaded, offline, not auto-hosting) showing "The host left the game.".
    /// Needs at least 2 clients. Version mismatch and the full-game refusal are PlayMode-tested in process.
    /// </summary>
    public sealed class RobustNetTestScenario : INetTestScenario
    {
        public const string ScenarioName = "robust";
        public const string ItemId = "laptop";
        /// <summary>The freed item drops where its carrier was last seen; anything further means it went astray.</summary>
        public const float MaxDropDistance = 3f;
        private const float ConnectTimeout = 40f, StepTimeout = 20f, MenuTimeout = 15f;
        private const float SpawnDistance = 1.2f, SpawnHeight = 0.6f, LandTime = 1f, FlushTime = 0.4f;
        private const string Grab = "grab", Holding = "holding", CheckFree = "checkfree", Free = "free", Leaving = "leaving";

        public string Name => ScenarioName;

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            if (ctx.ExpectedClients < 2)
            {
                ctx.Abort("'robust' needs at least 2 clients (one leaves, the rest watch the host leave)");
                yield break;
            }
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players,
                $"{players} connected players", ConnectTimeout);
            if (ctx.Aborted) yield break;
            NetworkObject prefab = LootNetTestScenario.FindPrefab(ctx.Manager, ItemId);
            if (prefab == null)
            {
                ctx.Abort($"no registered network prefab for loot '{ItemId}'");
                yield break;
            }

            ulong leaverId = ctx.Manager.ConnectedClientsIds.Where(id => id != ctx.Manager.LocalClientId).Max();
            NetworkPlayer leaver = ctx.PlayerOf(leaverId);
            Transform t = leaver.transform;
            NetworkObject spawned = ctx.Manager.SpawnManager.InstantiateAndSpawn(prefab,
                position: t.position + t.forward * SpawnDistance + Vector3.up * SpawnHeight, rotation: t.rotation);
            ulong itemId = spawned.NetworkObjectId;
            var loot = spawned.GetComponent<NetworkLoot>();
            ctx.Channel.Send(leaverId, Grab, itemId.ToString());

            yield return ctx.Receive(Holding, StepTimeout, _ => { });
            if (ctx.Aborted) yield break;
            yield return ctx.WaitFor(() => loot.Hold.Mode == LootHoldMode.Held, "the host to see the leaver holding the item", StepTimeout);
            if (ctx.Aborted) yield break;
            ctx.Note($"client {leaverId} holds #{itemId} (owner c{loot.OwnerClientId}) and quits");

            Vector3 lastSeen = leaver.transform.position;
            yield return ctx.WaitFor(() =>
            {
                if (leaver != null) lastSeen = leaver.transform.position;
                return !ctx.Manager.ConnectedClientsIds.Contains(leaverId);
            }, $"client {leaverId} to leave", StepTimeout);
            if (ctx.Aborted) yield break;

            yield return ctx.WaitFor(() => loot != null && loot.IsSpawned && loot.Hold.Mode == LootHoldMode.Free &&
                                           loot.OwnerClientId == NetworkManager.ServerClientId && loot.Grabbable.HasPhysicsAuthority,
                "the host to take the leaver's item back", StepTimeout);
            if (ctx.Aborted) yield break;
            yield return NetTestContext.Seconds(LandTime);
            float drift = Vector3.Distance(Flat(loot.transform.position), Flat(lastSeen));
            ctx.Note($"#{itemId} free and host-owned again, {drift:0.00} m from where its carrier was");
            if (drift > MaxDropDistance) ctx.Fail($"the freed item ended up {drift:0.0} m from its carrier's last position");

            ctx.Channel.SendToClients(CheckFree, itemId.ToString());
            var free = new System.Collections.Generic.List<NetTestMessage>();
            yield return ctx.Collect(Free, ctx.ExpectedClients - 1, free, StepTimeout);
            if (ctx.Aborted) yield break;

            ctx.Channel.SendToClients(Leaving);
            yield return NetTestContext.Seconds(FlushTime);
            ctx.Note("host leaving the game");
            ctx.Bootstrap.Disconnect();
            yield return NetTestContext.Seconds(FlushTime);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null, "own player to spawn", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ctx.Result.clientId = ctx.Manager.LocalClientId;

            NetTestMessage order = default;
            yield return ctx.WaitFor(() => ctx.Channel.TryTake(Grab, out order) || ctx.Channel.TryTake(CheckFree, out order),
                "the host's instruction", ConnectTimeout);
            if (ctx.Aborted) yield break;
            if (!ulong.TryParse(order.Payload, out ulong itemId))
            {
                ctx.Abort($"bad item id '{order.Payload}'");
                yield break;
            }

            if (order.Kind == Grab) yield return GrabAndQuit(ctx, itemId);
            else yield return WatchTheHostLeave(ctx, itemId);
        }

        private static IEnumerator GrabAndQuit(NetTestContext ctx, ulong itemId)
        {
            NetworkLoot loot = null;
            yield return ctx.WaitFor(() => (loot = Find(ctx.Manager, itemId)) != null, $"item #{itemId} to spawn here", StepTimeout);
            if (ctx.Aborted) yield break;
            yield return NetTestContext.Seconds(LandTime);

            PlayerCarrier carrier = ctx.OwnPlayer.Carrier;
            InteractionService.Handler.RequestPickup(carrier, loot.Grabbable);
            yield return ctx.WaitFor(() => loot.IsOwner && carrier.Held == loot.Grabbable, "the host to grant the pickup", StepTimeout);
            if (ctx.Aborted) yield break;
            ctx.Channel.SendToHost(Holding);
            ctx.Note($"holding #{itemId}; quitting with it");
            // Let the message leave before the runner disconnects and quits.
            yield return NetTestContext.Seconds(FlushTime);
        }

        private static IEnumerator WatchTheHostLeave(NetTestContext ctx, ulong itemId)
        {
            NetworkLoot loot = Find(ctx.Manager, itemId);
            yield return ctx.WaitFor(() => (loot = Find(ctx.Manager, itemId)) != null && loot.Hold.Mode == LootHoldMode.Free,
                $"the leaver's item #{itemId} to be free here", StepTimeout);
            if (ctx.Aborted) yield break;
            ctx.Note($"#{itemId} is free here too");
            ctx.Channel.SendToHost(Free);

            yield return ctx.Receive(Leaving, StepTimeout, _ => { });
            if (ctx.Aborted) yield break;

            // The session is about to end, so ctx.WaitFor (which aborts on a lost session) can't be used.
            int before = UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;
            float end = Time.realtimeSinceStartup + MenuTimeout;
            while (Time.realtimeSinceStartup < end && !BackAtMenu(before)) yield return null;
            if (!BackAtMenu(before))
            {
                ctx.Fail($"not back at the menu {MenuTimeout:0} s after the host left (notice '{SessionEndNotice.Message}')");
                yield break;
            }
            if (SessionEndNotice.Message != SessionMessages.HostLeft)
                ctx.Fail($"the menu says '{SessionEndNotice.Message}', expected '{SessionMessages.HostLeft}'");
            ctx.Note($"back at the menu: '{SessionEndNotice.Message}'");
        }

        // A fresh copy of the scene (the session itself persists), offline, and it didn't auto-host.
        private static bool BackAtMenu(int sceneBefore)
        {
            NetworkBootstrap now = NetworkBootstrap.Instance;
            return now != null && UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle != sceneBefore
                   && !now.IsRunning && SessionEndNotice.ReturnedFromSession;
        }

        private static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);

        private static NetworkLoot Find(NetworkManager manager, ulong id) =>
            manager != null && manager.SpawnManager != null && manager.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject no) && no != null
                ? no.GetComponent<NetworkLoot>()
                : null;
    }
}

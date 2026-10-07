using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Networking
{
    /// <summary>
    /// Session travel across processes (M6.0): host + clients start in TestBuilding, the host takes
    /// everyone to the mall (every machine loads it, the run and its loot appear for all, everyone stands
    /// on their own mall spawn) and back again, all on one connection: same client ids, no rejoin.
    /// </summary>
    public sealed class TravelNetTestScenario : INetTestScenario
    {
        public const string ScenarioName = "travel";
        private const float ConnectTimeout = 40f, StepTimeout = 30f;
        private const string Check = "check", Report = "report", Done = "done";

        public string Name => ScenarioName;

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players
                                           && SessionTravel.Current != null, $"{players} players and the travel object", ConnectTimeout);
            if (ctx.Aborted) yield break;
            List<ulong> clients = ctx.Manager.ConnectedClientsIds.Where(id => id != ctx.Manager.LocalClientId).ToList();

            foreach (string level in new[] { "Mall", "TestBuilding" })
            {
                SessionTravel.Current.Travel(level);
                bool mall = level == "Mall";
                yield return ctx.WaitFor(() => SceneManager.GetActiveScene().name == level && SessionTravel.LevelReady
                                               && (!mall || (RunState.Current != null && RunState.Current.IsSpawned && LootCount(ctx.Manager) > 0)),
                    $"everyone in {level}", StepTimeout);
                if (ctx.Aborted) yield break;
                yield return NetTestContext.Seconds(1.5f);
                ctx.Note($"in {level}: {ctx.Manager.ConnectedClientsIds.Count} connected, {LootCount(ctx.Manager)} loot");
                foreach (ulong c in clients)
                {
                    Vector3 spawn = ctx.Bootstrap.Slots.TryGetSlot(c, out int slot) ? PlayerSpawnPoint.PoseFor(slot).position : Vector3.zero;
                    ctx.Channel.Send(c, Check, $"{level};{LootCount(ctx.Manager)};{F(spawn.x)};{F(spawn.y)};{F(spawn.z)}");
                }
                var reports = new List<NetTestMessage>();
                yield return ctx.Collect(Report, clients.Count, reports, StepTimeout);
                if (ctx.Aborted) yield break;
                foreach (NetTestMessage m in reports.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender} in {level}: {m.Payload}");
            }
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null, "own player", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ulong id = ctx.Manager.LocalClientId;
            ctx.Result.clientId = id;
            NetworkPlayer me = ctx.OwnPlayer;
            for (int step = 0; step < 2; step++)
            {
                NetTestMessage check = default;
                yield return ctx.Receive(Check, StepTimeout, m => check = m);
                if (ctx.Aborted) yield break;
                string[] w = check.Payload.Split(';');
                var spawn = new Vector3(P(w[2]), P(w[3]), P(w[4]));
                yield return ctx.WaitFor(() => LootCount(ctx.Manager).ToString() == w[1], $"{w[1]} loot here", StepTimeout);
                if (ctx.Aborted) yield break;
                float off = Vector3.Distance(me.transform.position, spawn);
                string verdict = SceneManager.GetActiveScene().name != w[0] ? $"on {SceneManager.GetActiveScene().name}, host on {w[0]}"
                    : ctx.Manager.LocalClientId != id ? "my client id changed (rejoined?)"
                    : ctx.OwnPlayer != me ? "my player object was replaced"
                    : off > 1.5f ? $"{off:0.0} m from my spawn"
                    : w[0] == "Mall" && (RunState.Current == null || !RunState.Current.IsSpawned) ? "no run here" : "ok";
                ctx.Note($"in {w[0]}: {verdict}, {LootCount(ctx.Manager)} loot");
                ctx.Channel.SendToHost(Report, verdict);
            }
            yield return ctx.Receive(Done, StepTimeout, m =>
            {
                if (m.Payload != "pass") ctx.Fail("the host's check failed (see the host's result)");
            });
        }

        private static int LootCount(NetworkManager manager) =>
            manager.SpawnManager == null ? 0 : manager.SpawnManager.SpawnedObjectsList.Count(o => o != null && o.GetComponent<NetworkLoot>() != null);

        private static string F(float v) => v.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        private static float P(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }
}

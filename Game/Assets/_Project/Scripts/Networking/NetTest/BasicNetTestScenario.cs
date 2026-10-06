using System.Collections;
using System.Collections.Generic;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Host + N clients connect; on the host's "move" each client walks its player forward a scripted
    /// distance with the real motor; after a settle time every machine reports where it sees all
    /// players, and the host checks they agree (<see cref="BasicNetTestCheck"/>).
    /// </summary>
    public sealed class BasicNetTestScenario : INetTestScenario
    {
        public const string ScenarioName = "basic";
        public const float MoveDistance = 2f, MinDistance = 1.5f, Tolerance = 0.25f;
        private const float ConnectTimeout = 40f, StepTimeout = 20f, MaxWalkTime = 5f, StopTime = 0.6f;
        // Remote copies are interpolated a few ticks behind; give them time to arrive.
        private const float SettleTime = 1.5f;
        private const string Move = "move", Moved = "moved", Report = "report", Done = "done";

        public string Name => ScenarioName;

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && Count(ctx) == players,
                $"{players} connected players", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ctx.Note($"{players} players connected");

            ctx.Channel.SendToClients(Move);
            var moved = new List<NetTestMessage>();
            yield return ctx.Collect(Moved, ctx.ExpectedClients, moved, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in moved) ctx.Result.moves.Add(JsonUtility.FromJson<NetTestMove>(m.Payload));

            yield return NetTestContext.Seconds(SettleTime);
            ctx.Channel.SendToClients(Report);
            ctx.Result.views.Add(ctx.Snapshot());
            var reports = new List<NetTestMessage>();
            yield return ctx.Collect(Report, ctx.ExpectedClients, reports, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in reports) ctx.Result.views.Add(JsonUtility.FromJson<NetTestView>(m.Payload));

            foreach (string error in BasicNetTestCheck.Verify(ctx.Result.views, ctx.Result.moves, players,
                         ctx.ExpectedClients, Tolerance, MinDistance))
                ctx.Fail(error);
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            // Let the verdict leave before the runner shuts the host down.
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null, "own player to spawn", ConnectTimeout);
            if (ctx.Aborted) yield break;
            yield return ctx.Receive(Move, ConnectTimeout, _ => { });
            if (ctx.Aborted) yield break;

            NetTestMove move = null;
            yield return Walk(ctx.OwnPlayer, m => move = m);
            ctx.Result.moves.Add(move);
            ctx.Note($"walked {move.HorizontalDistance:0.00} m to {move.end}");
            ctx.Channel.SendToHost(Moved, JsonUtility.ToJson(move));

            yield return ctx.Receive(Report, StepTimeout, _ => { });
            if (ctx.Aborted) yield break;
            NetTestView view = ctx.Snapshot();
            ctx.Result.views.Add(view);
            if (view.players.Count != ctx.ExpectedPlayers)
                ctx.Fail($"sees {view.players.Count} players, expected {ctx.ExpectedPlayers}");
            ctx.Channel.SendToHost(Report, JsonUtility.ToJson(view));

            yield return ctx.Receive(Done, StepTimeout, m =>
            {
                if (m.Payload != "pass") ctx.Fail("the host's cross-machine check failed (see the host's result)");
            });
        }

        /// <summary>Drives the owner's real motor forward, then lets it stop, so the replicated end pose is at rest.</summary>
        private static IEnumerator Walk(NetworkPlayer player, System.Action<NetTestMove> done)
        {
            PlayerMotor motor = player.Motor;
            // The motor's own Update would add a zero-input step every frame on top of ours.
            motor.enabled = false;
            var move = new NetTestMove { owner = player.OwnerClientId, start = player.transform.position };
            var forward = new PlayerInputFrame(Vector2.up, Vector2.zero, false, false, false, false, false, false, false, false, false, false);
            var idle = default(PlayerInputFrame);

            float walked = 0f, end = Time.realtimeSinceStartup + MaxWalkTime;
            while (walked < MoveDistance && Time.realtimeSinceStartup < end)
            {
                motor.Simulate(forward, Mathf.Min(Time.deltaTime, 1f / 30f));
                Vector3 d = player.transform.position - move.start;
                walked = new Vector2(d.x, d.z).magnitude;
                yield return null;
            }
            for (float t = 0f; t < StopTime; t += Mathf.Min(Time.deltaTime, 1f / 30f))
            {
                motor.Simulate(idle, Mathf.Min(Time.deltaTime, 1f / 30f));
                yield return null;
            }
            move.end = player.transform.position;
            motor.enabled = true;
            done(move);
        }

        private static int Count(NetTestContext ctx)
        {
            int n = 0;
            foreach (NetworkPlayer _ in ctx.Players) n++;
            return n;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// M7.6 performance probe: a mall run with the full crew for a fixed stretch while everyone moves,
    /// the host throws loot around and drops a block of upper floors (debris everywhere). Every machine
    /// records its frame times; the host also records peak awake bodies and live debris. Headless, so
    /// this measures simulation and networking, not rendering; four processes share one machine, so the
    /// budget is generous and the numbers are for trends (they're written to the results).
    /// </summary>
    public sealed class PerfNetTestScenario : INetTestScenario, INetTestScene
    {
        public const string ScenarioName = "perf";
        private const float ConnectTimeout = 40f, StepTimeout = 30f, Duration = 30f, BudgetP95Ms = 50f;
        private const string Go = "go", Report = "report", Done = "done";

        public string Name => ScenarioName;
        public string Scene => "Mall";

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players
                                           && RunState.Current != null && RunState.Current.IsSpawned, $"{players} players and the run", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ctx.Channel.SendToClients(Go, "");

            var stats = new NetTestFrameStats();
            int peakBodies = 0, peakDebris = 0, collapsed = 0, thrown = 0;
            var random = new System.Random(7);
            float start = Time.realtimeSinceStartup, nextMove = 0f, nextCount = 0f;
            bool threwEarly = false, threwLate = false, dropped = false;
            while (Time.realtimeSinceStartup - start < Duration)
            {
                float t = Time.realtimeSinceStartup - start;
                stats.Sample();
                if (t >= nextMove) { nextMove = t + 0.5f; Wander(ctx, 0, t); }
                if (!threwEarly && t > 5f) { threwEarly = true; thrown += Throw(ctx, random, 12); }
                if (!dropped && t > 10f) { dropped = true; collapsed = Drop(8); }
                if (!threwLate && t > 20f) { threwLate = true; thrown += Throw(ctx, random, 12); }
                if (t >= nextCount)
                {
                    nextCount = t + 1f;
                    peakBodies = Mathf.Max(peakBodies, Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Count(b => !b.isKinematic && !b.IsSleeping()));
                    peakDebris = Mathf.Max(peakDebris, DebrisSpawner.LiveChunks);
                }
                yield return null;
            }
            ctx.Note($"host: {stats.Summary}; threw {thrown} items, dropped {collapsed} sections, peak awake bodies {peakBodies}, peak debris {peakDebris}");
            if (stats.Percentile(0.95f) > BudgetP95Ms) ctx.Fail($"host p95 frame {stats.Percentile(0.95f):0.0} ms over the {BudgetP95Ms} ms budget");
            if (collapsed == 0) ctx.Fail("no sections dropped: the load wasn't applied");

            var reports = new List<NetTestMessage>();
            yield return ctx.Collect(Report, players - 1, reports, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in reports)
            {
                string[] p = m.Payload.Split(';');
                ctx.Note($"client {m.Sender}: {p[3]}");
                if (float.Parse(p[1], CultureInfo.InvariantCulture) > BudgetP95Ms) ctx.Fail($"client {m.Sender} p95 frame {p[1]} ms over budget");
            }
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null && RunState.Current != null && RunState.Current.IsSpawned,
                "own player and the run", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ctx.Result.clientId = ctx.Manager.LocalClientId;
            yield return ctx.Receive(Go, StepTimeout, _ => { });
            if (ctx.Aborted) yield break;
            var stats = new NetTestFrameStats();
            float start = Time.realtimeSinceStartup, nextMove = 0f;
            int lane = (int)ctx.Manager.LocalClientId;
            while (Time.realtimeSinceStartup - start < Duration)
            {
                float t = Time.realtimeSinceStartup - start;
                stats.Sample();
                if (t >= nextMove) { nextMove = t + 0.5f; Wander(ctx, lane, t); }
                yield return null;
            }
            ctx.Note($"client: {stats.Summary}, debris here {DebrisSpawner.LiveChunks}");
            ctx.Channel.SendToHost(Report, string.Join(";", F(stats.Average), F(stats.Percentile(0.95f)), F(stats.Worst), stats.Summary));
            yield return ctx.Receive(Done, StepTimeout, m =>
            {
                if (m.Payload != "pass") ctx.Fail("the host's check failed (see the host's result)");
            });
        }

        // A lap of the ground-floor concourse, one lane per machine, so everyone keeps moving.
        private static void Wander(NetTestContext ctx, int lane, float t)
        {
            float a = t * 0.4f + lane * 1.6f;
            ctx.OwnPlayer.OwnerTeleport(new Vector3(24f + Mathf.Cos(a) * (6f + lane), 0.05f, 16f + Mathf.Sin(a) * 9f));
        }

        // Host: fling resting loot into the air (host-simulated, so it's real physics and replication).
        private static int Throw(NetTestContext ctx, System.Random random, int count)
        {
            var free = Object.FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None)
                .Where(l => l.IsSpawned && l.NetworkManager == ctx.Manager && l.Hold.Mode == LootHoldMode.Free && !l.Grabbable.Body.isKinematic)
                .Take(count).ToList();
            foreach (NetworkLoot l in free)
                l.Grabbable.Body.linearVelocity = new Vector3((float)random.NextDouble() * 6f - 3f, 4f + (float)random.NextDouble() * 3f, (float)random.NextDouble() * 6f - 3f);
            return free.Count;
        }

        // Host: the upper floors around the atrium give way (debris on every machine, cascades below).
        private static int Drop(int count)
        {
            StructureSimulation sim = Object.FindAnyObjectByType<StructureSimulation>();
            if (sim == null) return 0;
            var centre = new Vector3(24f, 4f, 20f);
            List<StructuralSection> picks = sim.Sections
                .Where(s => s.CanCollapse && !s.IsCollapsed && s.Type != SectionType.Stair && s.transform.position.y > 3f)
                .OrderBy(s => Vector3.Distance(s.transform.position, centre)).Take(count).ToList();
            foreach (StructuralSection s in picks) s.Collapse();
            return picks.Count;
        }

        private static string F(float v) => v.ToString("0.00", CultureInfo.InvariantCulture);
    }
}

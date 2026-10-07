using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.Profiling;

namespace Abandoned.Networking
{
    /// <summary>
    /// M8.6 soak: several mall runs back to back with the full crew (collapses, thrown loot, a death, the
    /// truck leaving, the next run). At the start of every run each machine reports its object count and
    /// memory, so leaks (HUD elements, debris, loot, ghosts) show up as growth between the first run and
    /// the last. Errors in any log already fail the nettest.
    /// </summary>
    public sealed class SoakNetTestScenario : INetTestScenario, INetTestScene
    {
        public const string ScenarioName = "soak";
        private const int Runs = 3;
        private const float ConnectTimeout = 40f, StepTimeout = 40f, PlaySeconds = 6f;
        private const float MaxObjectGrowth = 1.25f, MaxMemoryGrowthMb = 150f;
        private const string Mark = "mark", Marked = "marked", Done = "done";

        public string Name => ScenarioName;
        public string Scene => "Mall";

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            var first = new Dictionary<ulong, Snapshot>();
            var last = new Dictionary<ulong, Snapshot>();
            RunState previous = null;
            var random = new System.Random(11);
            for (int i = 0; i < Runs; i++)
            {
                RunState run = null;
                yield return ctx.WaitFor(() => ctx.Players.Count() == players && (run = RunState.Current) != null && run.IsSpawned
                                               && run != previous && TruckCargo.Current != null && LootCount(ctx) > 0,
                    $"run {i + 1} with {players} players", i == 0 ? ConnectTimeout : StepTimeout);
                if (ctx.Aborted) yield break;
                previous = run;
                yield return NetTestContext.Seconds(1f);

                // Everyone's numbers at the start of this run.
                Record(i, ctx.Manager.LocalClientId, Snapshot.Take(), first, last);
                ctx.Channel.SendToClients(Mark, i.ToString(CultureInfo.InvariantCulture));
                var marks = new List<NetTestMessage>();
                yield return ctx.Collect(Marked, players - 1, marks, StepTimeout);
                if (ctx.Aborted) yield break;
                foreach (NetTestMessage m in marks) Record(i, m.Sender, Snapshot.Parse(m.Payload), first, last);

                // Load the run: loot flying, floors falling, somebody dying (in the middle run).
                int thrown = PerfNetTestScenario.Throw(ctx, random, 10), dropped = PerfNetTestScenario.Drop(6);
                if (i == 1) ctx.PlayerOf(ctx.Manager.ConnectedClientsIds.First(id => id != ctx.Manager.LocalClientId)).ServerKill();
                float until = Time.realtimeSinceStartup + PlaySeconds;
                while (Time.realtimeSinceStartup < until)
                {
                    PerfNetTestScenario.Wander(ctx, 0, Time.realtimeSinceStartup);
                    yield return NetTestContext.Seconds(0.5f);
                }

                // Leave: the host starts the truck from the bay and waits out the honk.
                ctx.OwnPlayer.OwnerTeleport(TruckCargo.Current.transform.position + Vector3.up * 0.45f);
                yield return NetTestContext.Seconds(0.3f);
                run.RequestDepart();
                yield return ctx.WaitFor(() => run.Results != null, $"run {i + 1}'s truck to leave", StepTimeout);
                if (ctx.Aborted) yield break;
                ctx.Note($"run {i + 1}: seed {run.State.Seed}, threw {thrown}, dropped {dropped}, results for {run.Results.Players.Length} players");
                if (i < Runs - 1) Object.FindAnyObjectByType<RunDirector>().StartNextRun();
            }

            foreach (ulong id in first.Keys.OrderBy(k => k))
            {
                Snapshot a = first[id], b = last[id];
                ctx.Note($"machine {id}: objects {a.Objects} -> {b.Objects}, managed {a.ManagedMb:0} -> {b.ManagedMb:0} MB, native {a.NativeMb:0} -> {b.NativeMb:0} MB");
                if (b.Objects > a.Objects * MaxObjectGrowth) ctx.Fail($"machine {id}: objects grew {a.Objects} -> {b.Objects} over {Runs} runs (leak?)");
                if (b.ManagedMb + b.NativeMb - a.ManagedMb - a.NativeMb > MaxMemoryGrowthMb)
                    ctx.Fail($"machine {id}: memory grew {b.ManagedMb + b.NativeMb - a.ManagedMb - a.NativeMb:0} MB over {Runs} runs");
            }
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null, "own player", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ctx.Result.clientId = ctx.Manager.LocalClientId;
            int lane = (int)ctx.Manager.LocalClientId;
            float deadline = Time.realtimeSinceStartup + Runs * 60f + ConnectTimeout;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (ctx.Channel.TryTake(Mark, out NetTestMessage _)) ctx.Channel.SendToHost(Marked, Snapshot.Take().ToString());
                if (ctx.Channel.TryTake(Done, out NetTestMessage done))
                {
                    if (done.Payload != "pass") ctx.Fail("the host's check failed (see the host's result)");
                    yield break;
                }
                if (ctx.OwnPlayer != null && !ctx.OwnPlayer.IsDead) PerfNetTestScenario.Wander(ctx, lane, Time.realtimeSinceStartup);
                yield return NetTestContext.Seconds(0.5f);
            }
            ctx.Fail("the host never finished the soak");
        }

        private static void Record(int run, ulong id, Snapshot s, Dictionary<ulong, Snapshot> first, Dictionary<ulong, Snapshot> last)
        {
            if (run == 0) first[id] = s;
            last[id] = s;
        }

        private static int LootCount(NetTestContext ctx) =>
            ctx.Manager.SpawnManager == null ? 0 : ctx.Manager.SpawnManager.SpawnedObjectsList.Count(o => o != null && o.GetComponent<NetworkLoot>() != null);

        private readonly struct Snapshot
        {
            public readonly int Objects;
            public readonly float ManagedMb, NativeMb;

            private Snapshot(int objects, float managed, float native)
            {
                Objects = objects;
                ManagedMb = managed;
                NativeMb = native;
            }

            public static Snapshot Take() => new(
                Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length,
                System.GC.GetTotalMemory(true) / (1024f * 1024f),
                Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f));

            public override string ToString() =>
                string.Join(";", Objects.ToString(CultureInfo.InvariantCulture), ManagedMb.ToString("0.0", CultureInfo.InvariantCulture), NativeMb.ToString("0.0", CultureInfo.InvariantCulture));

            public static Snapshot Parse(string s)
            {
                string[] p = s.Split(';');
                return new Snapshot(int.Parse(p[0], CultureInfo.InvariantCulture), float.Parse(p[1], CultureInfo.InvariantCulture), float.Parse(p[2], CultureInfo.InvariantCulture));
            }
        }
    }
}

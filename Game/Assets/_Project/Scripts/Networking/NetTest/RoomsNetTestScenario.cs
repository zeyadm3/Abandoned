using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Doors and lockers across processes (0.16.0), in the mall: every machine sees the same doors shut at
    /// the start; a client slides one and everyone sees it move; the host breaks it down and everyone sees
    /// it broken and open; a client hides in a locker (the host sees them inside, a second client is turned
    /// away from the same locker) and climbs out again.
    /// </summary>
    public sealed class RoomsNetTestScenario : INetTestScenario, INetTestScene
    {
        public const string ScenarioName = "rooms";
        private const float ConnectTimeout = 40f, StepTimeout = 20f;
        private const string Slide = "slide", Masks = "masks", Hide = "hide", Leave = "leave", Done = "done", Report = "report";

        public string Name => ScenarioName;
        public string Scene => "Mall";

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            RunDoors doors = null;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players
                                           && (doors = RunDoors.Current) != null && doors.IsSpawned && doors.Ready,
                $"{players} players and the run's doors", ConnectTimeout);
            if (ctx.Aborted) yield break;
            SlidingDoor door = SlidingDoor.All.OrderBy(d => d.Index).FirstOrDefault();
            HidingLocker locker = HidingLocker.All.OrderBy(l => l.Index).FirstOrDefault();
            if (door == null || locker == null)
            {
                ctx.Abort($"the mall has {SlidingDoor.All.Count} doors and {HidingLocker.All.Count} lockers: run Tools/Abandoned/Mall/Add Doors and Lockers (0.16.0)");
                yield break;
            }
            ctx.Note($"{SlidingDoor.All.Count} doors, {HidingLocker.All.Count} lockers; shut at the start: {doors.ShutMask}");
            List<ulong> clients = ctx.Manager.ConnectedClientsIds.Where(id => id != ctx.Manager.LocalClientId).OrderBy(id => id).ToList();
            ulong actor = clients.First();

            // A client slides a door; every machine ends up with the same masks.
            bool wasShut = doors.IsShut(door.Index);
            ctx.Channel.Send(actor, Slide, door.Index.ToString());
            yield return ctx.WaitFor(() => doors.IsShut(door.Index) != wasShut, "the client's push on the door", StepTimeout);
            if (ctx.Aborted) yield break;
            yield return Agree(ctx, clients, doors);
            if (ctx.Aborted) yield break;

            // The host breaks it down: broken and open everywhere, and it won't shut again.
            doors.ServerSet(door.Index, true);
            doors.ServerBreak(door.Index);
            doors.ServerSet(door.Index, true);
            if (doors.IsShut(door.Index) || !doors.IsBroken(door.Index)) ctx.Fail("a broken door must stay open");
            yield return Agree(ctx, clients, doors);
            if (ctx.Aborted) yield break;

            // A client hides; the host sees them in the locker, and nobody else gets in.
            NetworkPlayer hider = ctx.PlayerOf(actor);
            ctx.Channel.Send(actor, Hide, locker.Index.ToString());
            yield return ctx.WaitFor(() => hider.HidingIn == locker.Index && Vector3.Distance(hider.transform.position, locker.InsidePoint) < 0.6f,
                "the client to be seen inside the locker", StepTimeout);
            if (ctx.Aborted) yield break;
            if (locker.Occupant != hider) ctx.Fail("the locker doesn't know its occupant");
            if (clients.Count > 1)
            {
                ulong second = clients[1];
                ctx.Channel.Send(second, Hide, locker.Index.ToString());
                yield return NetTestContext.Seconds(1.5f);
                if (ctx.PlayerOf(second).IsHiding) ctx.Fail("two players got into one locker");
            }
            ctx.Channel.Send(actor, Leave, "");
            yield return ctx.WaitFor(() => !hider.IsHiding && Vector3.Distance(hider.transform.position, locker.ExitPoint) < 1f,
                "the client to climb out", StepTimeout);
            if (ctx.Aborted) yield break;

            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        // Every client reports once its own masks match the host's.
        private static IEnumerator Agree(NetTestContext ctx, List<ulong> clients, RunDoors doors)
        {
            ctx.Channel.SendToClients(Masks, $"{doors.ShutMask};{doors.BrokenMask}");
            var reports = new List<NetTestMessage>();
            yield return ctx.Collect(Report, clients.Count, reports, StepTimeout);
            foreach (NetTestMessage m in reports.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender}: {m.Payload}");
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null && RunDoors.Current != null
                                           && RunDoors.Current.IsSpawned && RunDoors.Current.Ready, "own player and the run's doors", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ctx.Result.clientId = ctx.Manager.LocalClientId;
            RunDoors doors = RunDoors.Current;
            while (true)
            {
                NetTestMessage next = default;
                yield return ctx.WaitFor(() => ctx.Channel.TryTake(Slide, out next) || ctx.Channel.TryTake(Masks, out next) || ctx.Channel.TryTake(Hide, out next)
                                               || ctx.Channel.TryTake(Leave, out next) || ctx.Channel.TryTake(Done, out next), "the host's next step", ConnectTimeout);
                if (ctx.Aborted || next.Kind == Done) yield break;
                if (next.Kind == Slide)
                {
                    SlidingDoor door = SlidingDoor.ByIndex(int.Parse(next.Payload));
                    if (door == null) continue;
                    // Stand inside the store, a step from the middle of its doorway, and push.
                    ctx.OwnPlayer.OwnerTeleport(door.StandPoint(door.transform.position + door.transform.forward, 1.2f) + Vector3.up * 0.05f);
                    yield return NetTestContext.Seconds(0.75f);
                    doors.RequestToggle(door.Index);
                }
                else if (next.Kind == Masks)
                {
                    string[] wanted = next.Payload.Split(';');
                    float until = Time.realtimeSinceStartup + StepTimeout;
                    while (Time.realtimeSinceStartup < until && (doors.ShutMask.ToString() != wanted[0] || doors.BrokenMask.ToString() != wanted[1])) yield return null;
                    bool same = doors.ShutMask.ToString() == wanted[0] && doors.BrokenMask.ToString() == wanted[1];
                    ctx.Channel.SendToHost(Report, same ? "ok" : $"doors here shut {doors.ShutMask} broken {doors.BrokenMask}, host {next.Payload}");
                }
                else if (next.Kind == Hide)
                {
                    HidingLocker locker = HidingLocker.ByIndex(int.Parse(next.Payload));
                    if (locker == null) continue;
                    ctx.OwnPlayer.OwnerTeleport(locker.ExitPoint);
                    yield return NetTestContext.Seconds(0.75f);
                    ctx.OwnPlayer.RequestHide(locker.Index);
                }
                else if (next.Kind == Leave) ctx.OwnPlayer.RequestLeaveHiding();
            }
        }
    }
}

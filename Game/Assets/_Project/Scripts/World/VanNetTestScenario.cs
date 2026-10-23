using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// The van across machines (nettest "van", in Scenes/WorldVanTest): each client climbs into its own seat, the host
    /// drives, and every machine checks it sees the van where the host does, sits pinned in its seat and reads the same
    /// fuel; then the first client takes the wheel and drives it by RPC; then everyone climbs out beside their door.
    /// </summary>
    public sealed class VanNetTestScenario : INetTestScenario, INetTestScene
    {
        public const string ScenarioName = "van";
        private const float ConnectTimeout = 40f, StepTimeout = 25f;
        private const string Board = "board", Check = "check", Report = "report", TakeWheel = "wheel", Leave = "leave", Done = "done";

        public string Name => ScenarioName;
        public string Scene => "WorldVanTest";

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players
                                           && WorldVan.Current != null && WorldVan.Current.IsSpawned, $"{players} players and the van", ConnectTimeout);
            if (ctx.Aborted) yield break;
            WorldVan van = WorldVan.Current;
            List<ulong> clients = ctx.Manager.ConnectedClientsIds.Where(id => id != ctx.Manager.LocalClientId).ToList();
            var reports = new List<NetTestMessage>();

            // Everyone in: clients take seats 1..3, the host the wheel.
            for (int i = 0; i < clients.Count; i++) ctx.Channel.Send(clients[i], Board, (1 + i).ToString());
            yield return ctx.Collect(Report, clients.Count, reports, StepTimeout);
            if (ctx.Aborted) yield break;
            FailBad(ctx, reports, "boarding");
            ctx.OwnPlayer.OwnerTeleport(van.Seats.ExitPoint(VanSeats.DriverSeat));
            van.Seats.RequestSeat(VanSeats.DriverSeat);
            yield return ctx.WaitFor(() => van.Seats.Driver == ctx.Manager.LocalClientId, "the host at the wheel", StepTimeout);
            if (ctx.Aborted) yield break;

            yield return DriveAndCheck(ctx, van, clients, () => VanTestDriver.Hold(1f, .2f, 4f), "host driving");
            if (ctx.Aborted) yield break;

            // The host gets out; the first client slides over and drives (its input goes by RPC).
            van.Seats.RequestLeave();
            yield return ctx.WaitFor(() => !van.Seats.HasDriver, "the wheel free", StepTimeout);
            if (ctx.Aborted || clients.Count == 0) yield break;
            ctx.Channel.Send(clients[0], TakeWheel, "");
            yield return ctx.WaitFor(() => van.Seats.Driver == clients[0], "client at the wheel", StepTimeout);
            if (ctx.Aborted) yield break;
            yield return DriveAndCheck(ctx, van, clients, null, "client driving");
            if (ctx.Aborted) yield break;

            ctx.Channel.SendToClients(Leave, "");
            reports.Clear();
            yield return ctx.Collect(Report, clients.Count, reports, StepTimeout);
            FailBad(ctx, reports, "climbing out");
            if (van.Seats.Occupied != 0) ctx.Fail($"{van.Seats.Occupied} seat(s) still taken after everyone got out");
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(.5f);
        }

        // Drive (host's hands, or wait for the client's), stop, then compare everyone's view with the host's.
        private IEnumerator DriveAndCheck(NetTestContext ctx, WorldVan van, List<ulong> clients, System.Action hostHands, string what)
        {
            Vector3 start = van.transform.position;
            float fuel = van.State.Fuel;
            hostHands?.Invoke();
            yield return NetTestContext.Seconds(5f);
            yield return ctx.WaitFor(() => van.Drive.SpeedKmh < 1f, $"the van stopped ({what})", StepTimeout);
            if (ctx.Aborted) yield break;
            float moved = Vector3.Distance(start, van.transform.position);
            ctx.Note($"{what}: moved {moved:0.0} m, fuel {fuel:0.00} -> {van.State.Fuel:0.00} L, damage {van.State.Damage:0}%");
            if (moved < 8f) ctx.Fail($"{what}: the van only moved {moved:0.0} m");
            Vector3 p = van.transform.position;
            ctx.Channel.SendToClients(Check, $"{F(p.x)};{F(p.y)};{F(p.z)};{F(van.State.Fuel)}");
            var reports = new List<NetTestMessage>();
            yield return ctx.Collect(Report, clients.Count, reports, StepTimeout);
            FailBad(ctx, reports, what);
        }

        private static void FailBad(NetTestContext ctx, List<NetTestMessage> reports, string step)
        {
            foreach (NetTestMessage m in reports.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender} {step}: {m.Payload}");
            reports.Clear();
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null && WorldVan.Current != null, "own player and the van", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ctx.Result.clientId = ctx.Manager.LocalClientId;
            WorldVan van = WorldVan.Current;
            NetworkPlayer me = ctx.OwnPlayer;
            ulong id = ctx.Manager.LocalClientId;

            NetTestMessage board = default;
            yield return ctx.Receive(Board, StepTimeout, m => board = m);
            if (ctx.Aborted) yield break;
            int seat = int.Parse(board.Payload);
            me.OwnerTeleport(van.Seats.ExitPoint(seat));
            yield return NetTestContext.Seconds(.3f);
            van.Seats.RequestSeat(seat);
            yield return ctx.WaitFor(() => van.Seats.Seating.SeatOf(id) == seat, $"seat {seat}", StepTimeout);
            ctx.Channel.SendToHost(Report, van.Seats.Seating.SeatOf(id) == seat ? "ok" : $"not in seat {seat}");

            for (int step = 0; step < 2; step++)
            {
                if (step == 1 && seat == 1)
                {
                    // The first client: wait to be told, slide over to the wheel and drive by RPC.
                    yield return ctx.Receive(TakeWheel, StepTimeout * 2, _ => { });
                    if (ctx.Aborted) yield break;
                    van.Seats.RequestSeat(VanSeats.DriverSeat);
                    yield return ctx.WaitFor(() => van.Seats.Driver == id, "the wheel", StepTimeout);
                    seat = VanSeats.DriverSeat;
                    VanTestDriver.Hold(1f, -.2f, 4f);
                }
                NetTestMessage check = default;
                yield return ctx.Receive(Check, StepTimeout * 2, m => check = m);
                if (ctx.Aborted) yield break;
                string[] w = check.Payload.Split(';');
                var hostVan = new Vector3(P(w[0]), P(w[1]), P(w[2]));
                yield return NetTestContext.Seconds(.5f);
                float off = Vector3.Distance(hostVan, van.transform.position);
                float eyeOff = Vector3.Distance(me.GetComponent<Player.PlayerLook>().EyeRoot.position, van.Seats.SeatPoint(seat).position);
                float fuelOff = Mathf.Abs(P(w[3]) - van.State.Fuel);
                string verdict = off > 1.5f ? $"my van is {off:0.0} m from the host's"
                    : eyeOff > .6f ? $"my eyes are {eyeOff:0.00} m from seat {seat}"
                    : fuelOff > .05f ? $"fuel {van.State.Fuel:0.00} L, host says {P(w[3]):0.00}" : "ok";
                ctx.Note($"check {step}: van off {off:0.00} m, eyes off {eyeOff:0.00} m, fuel {van.State.Fuel:0.00} L: {verdict}");
                ctx.Channel.SendToHost(Report, verdict);
            }

            yield return ctx.Receive(Leave, StepTimeout * 2, _ => { });
            if (ctx.Aborted) yield break;
            van.Seats.RequestLeave();
            yield return ctx.WaitFor(() => van.Seats.Seating.SeatOf(id) < 0, "out of the van", StepTimeout);
            yield return null;
            yield return null;
            Vector3 door = van.Seats.ExitPoint(seat), landed = me.transform.position;
            yield return NetTestContext.Seconds(.5f);
            float fromDoor = Vector3.Distance(me.transform.position, door);
            ctx.Note($"out of seat {seat}: door {door:F2}, just out {landed:F2}, 0.5 s later {me.transform.position:F2}");
            ctx.Channel.SendToHost(Report, van.Seats.Seating.SeatOf(id) >= 0 ? "still seated" : fromDoor > 1.5f ? $"{fromDoor:0.0} m from my door" : "ok");
            yield return ctx.Receive(Done, StepTimeout, m => { if (m.Payload != "pass") ctx.Fail("the host's check failed (see the host's result)"); });
        }

        private static string F(float v) => v.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        private static float P(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }
}

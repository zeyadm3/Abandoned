using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Company;
using Abandoned.Contracts;
using Abandoned.Extraction;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Networking
{
    /// <summary>
    /// The company loop across processes (M6.1-6.3): everyone starts at the HQ and sees the same board;
    /// the host takes a job and a client drives the van; every machine runs the mall on the contract's
    /// terms; after the truck leaves every machine shows the same payday; the host drives everyone home
    /// to a fresh board, all on one connection.
    /// </summary>
    public sealed class CompanyNetTestScenario : INetTestScenario, INetTestScene
    {
        public const string ScenarioName = "company";
        private const float ConnectTimeout = 40f, StepTimeout = 35f;
        private const string Board = "board", Drive = "drive", InMall = "inmall", Board2 = "boardtruck", Boarded = "boarded",
            Payday = "payday", Home = "home", Done = "done";

        public string Name => ScenarioName;
        public string Scene => CompanyService.HomeLevel;

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            CompanyService company = null;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players
                                           && (company = CompanyService.Current) != null, $"{players} players at the HQ", ConnectTimeout);
            if (ctx.Aborted) yield break;
            List<ulong> clients = ctx.Manager.ConnectedClientsIds.Where(id => id != ctx.Manager.LocalClientId).OrderBy(id => id).ToList();
            ulong driver = clients.First(), stayer = clients.Last();
            company.Select(1);
            Contract job = company.Board[1];
            ctx.Note($"job: {job.ModifierName}, quota ${job.Quota:N0}, seed {job.Seed}, power {(job.PowerOff ? "off" : "on")}");
            ctx.Channel.SendToClients(Board, $"{job.Seed};{job.Quota}");
            var seen = new List<NetTestMessage>();
            yield return ctx.Collect(Board, clients.Count, seen, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in seen.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender} board: {m.Payload}");

            ctx.Channel.Send(driver, Drive, "");
            yield return ctx.WaitFor(() => SceneManager.GetActiveScene().name == job.Scene && RunState.Current != null && RunState.Current.IsSpawned,
                "the van to take everyone to the job", StepTimeout);
            if (ctx.Aborted) yield break;
            RunNetState s = RunState.Current.State;
            if (s.Quota != job.Quota || s.Seed != job.Seed || s.PowerOff != job.PowerOff) ctx.Fail($"the run isn't on the contract's terms (quota {s.Quota}, seed {s.Seed})");
            ctx.Channel.SendToClients(InMall, $"{job.Quota};{job.Seed};{(job.PowerOff ? 1 : 0)}");
            var inMall = new List<NetTestMessage>();
            yield return ctx.Collect(InMall, clients.Count, inMall, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in inMall.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender} in the mall: {m.Payload}");

            // Some cargo, everyone but the stayer aboard, the driver starts the truck.
            NetworkLoot cargo = Object.FindObjectsByType<NetworkLoot>(FindObjectsSortMode.None)
                .First(l => l.IsSpawned && l.Item.Definition.CarryClass == Abandoned.Interaction.CarryClass.OneHand && l.Item.Definition.Fragility <= Abandoned.Loot.Fragility.Medium);
            Vector3 bay = TruckCargo.Current.transform.TransformPoint(new Vector3(0f, 1.2f, -1.5f));
            cargo.Grabbable.Body.position = bay;
            cargo.transform.position = bay;
            cargo.ServerTeleport();
            ctx.OwnPlayer.OwnerTeleport(TruckCargo.Current.transform.TransformPoint(new Vector3(0f, 0.45f, 0.5f)));
            foreach (ulong c in clients) ctx.Channel.Send(c, Board2, c == stayer ? "out" : c == driver ? "drive" : "in");
            yield return ctx.WaitFor(() => company.LastOutcome.Run > 0, "payday", StepTimeout);
            if (ctx.Aborted) yield break;
            OutcomeNet o = company.LastOutcome;
            ctx.Note($"payday: +${o.Payout:N0} -${o.Penalty:N0}, company ${company.State.Money:N0}");
            ctx.Channel.SendToClients(Payday, $"{o.Payout};{o.Penalty};{company.State.Money}");
            var paid = new List<NetTestMessage>();
            yield return ctx.Collect(Payday, clients.Count, paid, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in paid.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender} payday: {m.Payload}");

            company.ReturnToHq();
            yield return ctx.WaitFor(() => SceneManager.GetActiveScene().name == CompanyService.HomeLevel && SessionTravel.LevelReady, "home", StepTimeout);
            if (ctx.Aborted) yield break;
            ctx.Channel.SendToClients(Home, $"{company.State.Money};{company.Board[0].Seed}");
            var home = new List<NetTestMessage>();
            yield return ctx.Collect(Home, clients.Count, home, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in home.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender} at home: {m.Payload}");
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null && CompanyService.Current != null, "the HQ", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ctx.Result.clientId = ctx.Manager.LocalClientId;
            CompanyService company = CompanyService.Current;

            string[] board = null;
            yield return ctx.Receive(Board, StepTimeout, m => board = m.Payload.Split(';'));
            if (ctx.Aborted) yield break;
            yield return ctx.WaitFor(() => company.Selected >= 0, "the host's choice to show here", StepTimeout);
            if (ctx.Aborted) yield break;
            Contract mine = company.Board[company.Selected];
            ctx.Channel.SendToHost(Board, mine.Seed.ToString() == board[0] && mine.Quota.ToString() == board[1] ? "ok" : $"my board says seed {mine.Seed}, quota {mine.Quota}");

            NetTestMessage next = default;
            yield return ctx.WaitFor(() => ctx.Channel.TryTake(Drive, out next) || ctx.Channel.TryTake(InMall, out next), "drive or arrive", StepTimeout);
            if (ctx.Aborted) yield break;
            if (next.Kind == Drive)
            {
                Object.FindAnyObjectByType<HqVan>().Use(ctx.OwnPlayer.gameObject);
                yield return ctx.Receive(InMall, StepTimeout, m => next = m);
                if (ctx.Aborted) yield break;
            }
            string[] t = next.Payload.Split(';');
            yield return ctx.WaitFor(() => RunState.Current != null && RunState.Current.IsSpawned, "the run here", StepTimeout);
            if (ctx.Aborted) yield break;
            RunNetState s = RunState.Current.State;
            bool lightsOff = Object.FindAnyObjectByType<PowerController>() is PowerController power && !power.Powered;
            string terms = s.Quota.ToString() != t[0] || s.Seed.ToString() != t[1] ? $"run here: quota {s.Quota}, seed {s.Seed}"
                : (t[2] == "1") != lightsOff ? $"power here {(lightsOff ? "off" : "on")}, contract says {(t[2] == "1" ? "off" : "on")}" : "ok";
            ctx.Channel.SendToHost(InMall, terms);

            string role = null;
            yield return ctx.Receive(Board2, StepTimeout, m => role = m.Payload);
            if (ctx.Aborted) yield break;
            if (role != "out") ctx.OwnPlayer.OwnerTeleport(TruckCargo.Current.transform.TransformPoint(new Vector3(0f, 0.45f, role == "drive" ? 1.8f : -0.6f)));
            if (role == "drive")
            {
                yield return NetTestContext.Seconds(1f);
                RunState.Current.RequestDepart();
            }
            string[] pay = null;
            yield return ctx.Receive(Payday, StepTimeout, m => pay = m.Payload.Split(';'));
            if (ctx.Aborted) yield break;
            // The message outruns the replicated company state by a tick; wait for it.
            float until = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < until && company.State.Money.ToString() != pay[2]) yield return null;
            OutcomeNet o = company.LastOutcome;
            ctx.Channel.SendToHost(Payday, o.Payout.ToString() == pay[0] && o.Penalty.ToString() == pay[1] && company.State.Money.ToString() == pay[2]
                ? "ok" : $"payday here: +{o.Payout} -{o.Penalty}, money {company.State.Money}");

            string[] home = null;
            yield return ctx.Receive(Home, StepTimeout, m => home = m.Payload.Split(';'));
            if (ctx.Aborted) yield break;
            yield return ctx.WaitFor(() => SceneManager.GetActiveScene().name == CompanyService.HomeLevel, "home here", StepTimeout);
            if (ctx.Aborted) yield break;
            until = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < until && (company.Selected >= 0 || company.Board[0].Seed.ToString() != home[1])) yield return null;
            ctx.Channel.SendToHost(Home, company.State.Money.ToString() == home[0] && company.Board[0].Seed.ToString() == home[1] && company.Selected < 0
                ? "ok" : $"home here: money {company.State.Money}, board {company.Board[0].Seed}, selected {company.Selected}");
            ctx.Note($"back at the HQ with ${company.State.Money:N0}");
            yield return ctx.Receive(Done, StepTimeout, m =>
            {
                if (m.Payload != "pass") ctx.Fail("the host's check failed (see the host's result)");
            });
        }
    }
}

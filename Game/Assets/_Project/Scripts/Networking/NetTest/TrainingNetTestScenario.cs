#if UNITY_EDITOR || DEVELOPMENT_BUILD
// The nettest harness ships only in the editor and Development builds (Tools/unity.sh nettest uses a dev build).
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Tutorial;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Networking
{
    /// <summary>
    /// The training job across processes: host + clients start in TestMap, the host takes everyone to the training
    /// house, and every machine must agree on the lesson the crew is on. A client completes the first (personal)
    /// lesson and the whole crew moves on; the host skips two more and everyone follows. Each player arrives with the
    /// training kit (a flashlight, nothing else).
    /// </summary>
    public sealed class TrainingNetTestScenario : INetTestScenario
    {
        public const string ScenarioName = "training";
        private const float ConnectTimeout = 40f, StepTimeout = 30f;
        private const string Check = "check", Report = "report", Act = "act", Done = "done";

        public string Name => ScenarioName;

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players
                                           && SessionTravel.Current != null, $"{players} players and the travel object", ConnectTimeout);
            if (ctx.Aborted) yield break;
            List<ulong> clients = ctx.Manager.ConnectedClientsIds.Where(id => id != ctx.Manager.LocalClientId).ToList();

            SessionTravel.Current.Travel(TutorialMode.Scene);
            yield return ctx.WaitFor(() => SceneManager.GetActiveScene().name == TutorialMode.Scene && SessionTravel.LevelReady
                                           && RunTraining.Current != null && RunTraining.Current.IsSpawned, "everyone in the training house", StepTimeout);
            if (ctx.Aborted) yield break;
            yield return NetTestContext.Seconds(1.5f);
            RunTraining training = RunTraining.Current;
            ctx.Note($"training: lesson {training.Step + 1}/{training.Catalog.Count}, {TutorialLootSpot.All.Count} salvage spots");
            if (training.Step != 0) ctx.Fail($"the training started on lesson {training.Step}, not the first");

            // A client does the first lesson (looking around is personal): the crew moves on together.
            if (clients.Count > 0)
            {
                ctx.Channel.Send(clients[0], Act, "0");
                yield return ctx.WaitFor(() => training.Step >= 1, "a client's report moving the crew on", StepTimeout);
                if (ctx.Aborted) yield break;
            }
            else training.ReportDone(0);
            for (int i = 0; i < 2; i++)
            {
                int before = training.Step;
                training.RequestSkip();
                yield return ctx.WaitFor(() => training.Step > before, "the host's skip", StepTimeout);
                if (ctx.Aborted) yield break;
            }
            yield return NetTestContext.Seconds(1f);
            foreach (ulong c in clients) ctx.Channel.Send(c, Check, training.Step.ToString());
            var reports = new List<NetTestMessage>();
            yield return ctx.Collect(Report, clients.Count, reports, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in reports.Where(m => m.Payload != "ok")) ctx.Fail($"client {m.Sender}: {m.Payload}");
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null, "own player", ConnectTimeout);
            if (ctx.Aborted) yield break;
            ctx.Result.clientId = ctx.Manager.LocalClientId;
            yield return ctx.WaitFor(() => RunTraining.Current != null && RunTraining.Current.IsSpawned, "the training here", StepTimeout + ConnectTimeout);
            if (ctx.Aborted) yield break;

            // Only the first client is asked to act; everyone waits for the host's check either way.
            float until = Time.time + 8f;
            while (Time.time < until)
            {
                if (ctx.Channel.TryTake(Act, out NetTestMessage act))
                {
                    RunTraining.Current.ReportDone(int.Parse(act.Payload));
                    break;
                }
                yield return null;
            }
            NetTestMessage check = default;
            yield return ctx.Receive(Check, StepTimeout * 2f, m => check = m);
            if (ctx.Aborted) yield break;
            int hostStep = int.Parse(check.Payload);
            yield return ctx.WaitFor(() => RunTraining.Current.Step == hostStep, $"lesson {hostStep} here too", StepTimeout);
            Equipment.PlayerEquipment kit = ctx.OwnPlayer.GetComponent<Equipment.PlayerEquipment>();
            int carried = 0;
            for (int slot = 0; slot < Equipment.EquipState.SlotCount; slot++) if (kit != null && kit.InSlot(slot) != null) carried++;
            string verdict = SceneManager.GetActiveScene().name != TutorialMode.Scene ? "not in the training house"
                : RunTraining.Current.Step != hostStep ? $"on lesson {RunTraining.Current.Step}, host on {hostStep}"
                : kit == null || !kit.Has(Equipment.EquipmentKind.Flashlight) || carried != 1 ? $"training kit wrong ({carried} pieces of gear)"
                : "ok";
            ctx.Note($"training: {verdict}");
            ctx.Channel.SendToHost(Report, verdict);
            yield return ctx.Receive(Done, StepTimeout, m =>
            {
                if (m.Payload != "pass") ctx.Fail("the host's check failed (see the host's result)");
            });
        }
    }
}

#endif

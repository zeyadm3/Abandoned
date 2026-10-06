using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Voice;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Proximity voice across processes: every instance "talks" (a synthetic tone, open mic), and every
    /// instance must receive and decode a steady stream from every other player through the host's
    /// relay, with nothing refused. Real microphones and Steam voice are human-tested.
    /// </summary>
    public sealed class VoiceNetTestScenario : INetTestScenario, INetTestSetup
    {
        public const string ScenarioName = "voice";
        /// <summary>Half a second of 16 kHz audio from each speaker.</summary>
        public const int MinSamples = 8000;
        private const float ConnectTimeout = 40f, HearTimeout = 15f, StepTimeout = 20f;
        private const string Report = "voicereport", Done = "done";

        public string Name => ScenarioName;

        public void Prepare()
        {
            VoiceBackends.CaptureOverride = () => new ToneVoiceCapture();
            VoiceSettings.Mode = VoiceMode.OpenMic;
        }

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players,
                $"{players} connected players", ConnectTimeout);
            if (ctx.Aborted) yield break;
            yield return WaitToHearEveryone(ctx, players);
            if (ctx.Aborted) yield break;

            ctx.Channel.SendToClients(Report);
            var reports = new List<NetTestMessage>();
            yield return ctx.Collect(Report, ctx.ExpectedClients, reports, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in reports)
                if (m.Payload != "ok") ctx.Fail($"client {m.Sender}: {m.Payload}");
            int rejected = NetworkVoice.All.Where(v => v.NetworkManager == ctx.Manager).Sum(v => v.PacketsRejected);
            if (rejected > 0) ctx.Fail($"the host refused {rejected} voice packet(s)");
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null, "own player to spawn", ConnectTimeout);
            if (ctx.Aborted) yield break;
            yield return WaitToHearEveryone(ctx, ctx.ExpectedPlayers);
            string verdict = ctx.Aborted ? string.Join("; ", ctx.Result.errors) : "ok";
            yield return ctx.Receive(Report, StepTimeout, _ => { });
            ctx.Channel.SendToHost(Report, verdict);
            yield return ctx.Receive(Done, StepTimeout, m =>
            {
                if (m.Payload != "pass") ctx.Fail("the host's check failed (see the host's result)");
            });
        }

        // Every other player's voice, decoded into this machine's playback buffer.
        private static IEnumerator WaitToHearEveryone(NetTestContext ctx, int players)
        {
            IEnumerable<NetworkVoice> Others() => NetworkVoice.All.Where(v => v != null && v.NetworkManager == ctx.Manager && !v.IsOwner);
            yield return ctx.WaitFor(() => Others().Count() == players - 1 && Others().All(v => v.Playback.SamplesReceived >= MinSamples),
                $"half a second of voice from each of the {players - 1} other players", HearTimeout);
            foreach (NetworkVoice v in Others())
            {
                VoiceJitterBuffer b = v.Playback.Buffer;
                ctx.Note($"p{v.OwnerClientId}: {v.PacketsReceived} packets, {v.Playback.SamplesReceived} samples, " +
                         $"level {v.LastLevel:0.00}, underruns {b?.Underruns ?? 0}, dropped {b?.Dropped ?? 0}");
            }
        }
    }
}

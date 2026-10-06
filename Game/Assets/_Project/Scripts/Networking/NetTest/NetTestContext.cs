using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Everything a scenario needs on one machine: its session, the coordination channel, the result
    /// it fills in and waiting helpers that turn timeouts into recorded failures instead of hangs.
    /// Works per NetworkManager (not through NetworkPlayer.Local) so several machines can share a process.
    /// </summary>
    public sealed class NetTestContext
    {
        public NetworkBootstrap Bootstrap { get; }
        public NetworkManager Manager => Bootstrap.Manager;
        public NetTestChannel Channel { get; }
        public NetTestResult Result { get; }

        /// <summary>Clients the host expects (the whole session is this + 1).</summary>
        public int ExpectedClients { get; }
        public int ExpectedPlayers => ExpectedClients + 1;

        /// <summary>A wait timed out or a check failed hard; scenarios should stop.</summary>
        public bool Aborted { get; private set; }

        public NetTestContext(NetworkBootstrap bootstrap, NetTestChannel channel, NetTestResult result, int expectedClients)
        {
            Bootstrap = bootstrap;
            Channel = channel;
            Result = result;
            ExpectedClients = expectedClients;
        }

        public IEnumerable<NetworkPlayer> Players => NetworkPlayer.All.Where(p => p != null && p.NetworkManager == Manager);

        public NetworkPlayer OwnPlayer => Players.FirstOrDefault(p => p.IsOwner);

        public NetworkPlayer PlayerOf(ulong owner) => Players.FirstOrDefault(p => p.OwnerClientId == owner);

        /// <summary>Every player this machine can see right now.</summary>
        public NetTestView Snapshot()
        {
            var view = new NetTestView { observer = Manager.LocalClientId };
            foreach (NetworkPlayer p in Players.OrderBy(p => p.OwnerClientId))
                view.players.Add(new NetTestPlayerSnapshot(p.OwnerClientId, p.transform.position));
            return view;
        }

        public void Fail(string error)
        {
            Result.Fail(error);
            Debug.LogWarning($"[NetTest] FAIL: {error}");
        }

        public void Abort(string error)
        {
            Fail(error);
            Aborted = true;
        }

        public void Note(string note)
        {
            Result.notes.Add(note);
            Debug.Log($"[NetTest] {note}");
        }

        /// <summary>Waits until <paramref name="condition"/> holds; aborts the scenario on timeout or lost session.</summary>
        public IEnumerator WaitFor(Func<bool> condition, string what, float timeout)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!condition())
            {
                if (!Manager.IsListening)
                {
                    Abort($"session ended while waiting for {what}");
                    yield break;
                }
                if (Time.realtimeSinceStartup > end)
                {
                    Abort($"timed out after {timeout:0.#} s waiting for {what}");
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary>Collects one message of <paramref name="kind"/> from each of <paramref name="count"/> different senders.</summary>
        public IEnumerator Collect(string kind, int count, List<NetTestMessage> into, float timeout)
        {
            yield return WaitFor(() =>
            {
                while (Channel.TryTake(kind, out NetTestMessage m))
                    if (into.All(x => x.Sender != m.Sender)) into.Add(m);
                return into.Count >= count;
            }, $"{count} '{kind}' message(s)", timeout);
        }

        /// <summary>Waits for one message of <paramref name="kind"/>; null when it never came.</summary>
        public IEnumerator Receive(string kind, float timeout, Action<NetTestMessage> onReceived)
        {
            NetTestMessage received = default;
            bool got = false;
            yield return WaitFor(() => got = Channel.TryTake(kind, out received), $"'{kind}' from the host", timeout);
            if (got) onReceived(received);
        }

        public static IEnumerator Seconds(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }
    }
}

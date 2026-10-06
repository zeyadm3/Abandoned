using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>The 'basic' nettest verdict must catch each way the session can be wrong.</summary>
    public class BasicNetTestCheckTests
    {
        private const float Tolerance = 0.25f, MinDistance = 1.5f;
        private static readonly Vector3 HostPos = new(0f, 0f, 0f);

        // Host 0 stands still; clients 1-3 each walked 2 m forward from x = owner * 2.
        private static List<NetTestMove> Moves() => Enumerable.Range(1, 3).Select(i => new NetTestMove
        {
            owner = (ulong)i,
            start = new Vector3(i * 2f, 0f, 0f),
            end = new Vector3(i * 2f, 0f, 2f),
        }).ToList();

        private static List<NetTestView> Views(List<NetTestMove> moves) => Enumerable.Range(0, 4).Select(observer =>
        {
            var view = new NetTestView { observer = (ulong)observer };
            view.players.Add(new NetTestPlayerSnapshot(0, HostPos));
            foreach (NetTestMove m in moves) view.players.Add(new NetTestPlayerSnapshot(m.owner, m.end));
            return view;
        }).ToList();

        private static List<string> Check(List<NetTestView> views, List<NetTestMove> moves) =>
            BasicNetTestCheck.Verify(views, moves, 4, 3, Tolerance, MinDistance);

        [Test]
        public void AgreeingViewsPass()
        {
            List<NetTestMove> moves = Moves();
            List<NetTestView> views = Views(moves);
            views[2].players[1].position += new Vector3(0.1f, 0f, -0.1f); // interpolation slop within tolerance
            CollectionAssert.IsEmpty(Check(views, moves));
        }

        [Test]
        public void AMachineMissingAPlayerFails()
        {
            List<NetTestMove> moves = Moves();
            List<NetTestView> views = Views(moves);
            views[3].players.RemoveAt(1);
            StringAssert.Contains("machine 3 sees 3 players", Check(views, moves).Single());
        }

        [Test]
        public void ARemoteCopyLeftBehindFails()
        {
            List<NetTestMove> moves = Moves();
            List<NetTestView> views = Views(moves);
            views[0].players[2].position = moves[1].start; // host still shows player 2 at its spawn
            StringAssert.Contains("machine 0 sees player 2", Check(views, moves).Single());
        }

        [Test]
        public void ADisagreementAboutTheStillPlayerFails()
        {
            List<NetTestMove> moves = Moves();
            List<NetTestView> views = Views(moves);
            views[1].players[0].position = HostPos + Vector3.right;
            StringAssert.Contains("machine 1 sees player 0", Check(views, moves).Single());
        }

        [Test]
        public void ShortMovesMissingMovesAndMissingViewsFail()
        {
            List<NetTestMove> moves = Moves();
            List<NetTestView> views = Views(moves);
            moves[0].end = moves[0].start + Vector3.forward * 0.5f;
            foreach (NetTestView v in views) v.players[1].position = moves[0].end;
            StringAssert.Contains("moved only 0.50 m", Check(views, moves).Single());

            List<NetTestMove> two = Moves().Take(2).ToList();
            Assert.That(Check(Views(Moves()), two), Has.Some.Contains("expected 3 scripted moves"));
            Assert.That(Check(Views(Moves()).Take(3).ToList(), Moves()), Has.Some.Contains("view from each of 4 machines"));
        }
    }
}

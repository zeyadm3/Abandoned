using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>The 'collapse' nettest verdict catches every kind of disagreement.</summary>
    public class CollapseNetTestCheckTests
    {
        private const int Target = 7;

        private static NetTestCollapseView View(ulong observer, bool stood = true, float fall = 3.5f, int seed = 1234, bool off = true,
            bool mirror = true, bool collapsed = true)
        {
            var v = new NetTestCollapseView { observer = observer, mirror = observer != 0 && mirror, stoodOnTarget = observer != 0 && stood,
                startHeight = 4f, lowestHeight = 4f - (observer != 0 ? fall : 0f) };
            if (collapsed) v.collapsed.Add(new NetTestCollapsedSection { id = Target, name = "Tile_U_3_3", seed = seed, collidersOff = off });
            return v;
        }

        private static List<NetTestCollapseView> Good() => new() { View(0), View(1), View(2), View(3) };

        private static List<string> Verify(List<NetTestCollapseView> views) =>
            CollapseNetTestCheck.Verify(views, 4, Target, CollapseNetTestScenario.MinFall);

        [Test]
        public void AgreeingMachinesPass() => Assert.IsEmpty(Verify(Good()));

        [Test]
        public void DifferentSeedFails()
        {
            var views = Good();
            views[2] = View(2, seed: 99);
            Assert.That(Verify(views).Single(), Does.Contain("seed 99"));
        }

        [Test]
        public void ClientStillStandingOnIt()
        {
            var views = Good();
            views[1] = View(1, off: false);
            Assert.That(Verify(views).Single(), Does.Contain("colliders"));
        }

        [Test]
        public void ClientThatNeverSawTheCollapse()
        {
            var views = Good();
            views[3] = View(3, collapsed: false);
            Assert.That(string.Join(";", Verify(views)), Does.Contain("never saw section 7"));
        }

        [Test]
        public void ClientShowingAnExtraCollapse()
        {
            var views = Good();
            views[1].collapsed.Add(new NetTestCollapsedSection { id = 9, name = "X", seed = 1, collidersOff = true });
            Assert.That(Verify(views).Single(), Does.Contain("section 9"));
        }

        [Test]
        public void PlayerWhoDidntFall()
        {
            var views = Good();
            views[2] = View(2, fall: 0.3f);
            Assert.That(Verify(views).Single(), Does.Contain("fell only 0.30"));
        }

        [Test]
        public void ClientSimulatingItsOwnBuilding()
        {
            var views = Good();
            views[1] = View(1, mirror: false);
            Assert.That(Verify(views).Single(), Does.Contain("isn't mirroring"));
        }

        [Test]
        public void HostWithoutTheCollapseOrAViewFails()
        {
            var views = Good();
            views[0] = View(0, collapsed: false);
            Assert.IsTrue(Verify(views).Any(e => e.Contains("never collapsed on the host")));
            Assert.IsTrue(Verify(Good().Skip(1).ToList()).Any(e => e.Contains("no view from the host")));
            Assert.IsTrue(Verify(Good().Take(3).ToList()).Any(e => e.Contains("expected a view from each of 4")));
        }

        [Test]
        public void ScenarioIsRegisteredAndPositionsRoundTrip()
        {
            Assert.IsTrue(NetTestScenarios.TryCreate("collapse", out INetTestScenario s));
            Assert.AreEqual(CollapseNetTestScenario.ScenarioName, s.Name);
            Assert.IsTrue(CollapseNetTestScenario.TryParse("12.8;4.05;-1.2", out Vector3 v));
            Assert.AreEqual(new Vector3(12.8f, 4.05f, -1.2f), v);
            Assert.IsFalse(CollapseNetTestScenario.TryParse("1;2", out _));
            Assert.IsFalse(CollapseNetTestScenario.TryParse(null, out _));
        }
    }
}

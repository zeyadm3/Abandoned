using System.Collections.Generic;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>The host's trust rules for networked loot, without a session.</summary>
    public class NetworkLootRulesTests
    {
        private const ulong Host = 0, Carrier = 1, Other = 2;
        private LootNetConfig config;
        private ImpactReportFilter filter;
        private readonly Bounds item = new(Vector3.zero, Vector3.one * 0.4f);

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<LootNetConfig>();
            filter = new ImpactReportFilter(config);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        private bool Report(ulong sender, ulong owner, float now, float speed, Vector3 point, out float accepted) =>
            filter.TryAccept(sender, owner, now, speed, point, item, out accepted);

        [Test]
        public void OnlyTheCarrierMayReportImpacts()
        {
            Assert.IsTrue(Report(Carrier, Carrier, 1f, 6f, Vector3.zero, out float accepted));
            Assert.AreEqual(6f, accepted);
            Assert.IsFalse(Report(Other, Carrier, 2f, 6f, Vector3.zero, out _), "a bystander can't damage someone else's carry");
            Assert.IsFalse(Report(Carrier, Host, 3f, 6f, Vector3.zero, out _), "not the owner any more, never was released by them");
        }

        [Test]
        public void ReportsAreRateLimitedClampedAndMustBePlausible()
        {
            Assert.IsTrue(Report(Carrier, Carrier, 1f, 6f, Vector3.zero, out _));
            Assert.IsFalse(Report(Carrier, Carrier, 1f + config.ImpactReportInterval * 0.5f, 6f, Vector3.zero, out _), "same physics step twice");
            Assert.IsTrue(Report(Carrier, Carrier, 2f, 500f, Vector3.zero, out float clamped));
            Assert.AreEqual(config.MaxReportedImpactSpeed, clamped, "absurd speeds are clamped");
            Assert.IsFalse(Report(Carrier, Carrier, 3f, float.NaN, Vector3.zero, out _));
            Assert.IsFalse(Report(Carrier, Carrier, 4f, -3f, Vector3.zero, out _));
            Assert.IsFalse(Report(Carrier, Carrier, 5f, 6f, new Vector3(float.PositiveInfinity, 0f, 0f), out _));
            Assert.IsFalse(Report(Carrier, Carrier, 6f, 6f, Vector3.up * (config.MaxImpactPointDistance + 1f), out _), "too far from the item");
            Assert.IsTrue(Report(Carrier, Carrier, 7f, 6f, Vector3.up * 0.5f, out _), "a contact on the item's surface is fine");
        }

        [Test]
        public void JustReleasedCarrierReportsCountUntilTheHostSimulatesAHitItself()
        {
            filter.OwnerChanged(Carrier, 10f);
            Assert.IsTrue(Report(Carrier, Host, 10.1f, 6f, Vector3.zero, out _), "in flight when the host took it back");
            Assert.IsFalse(Report(Carrier, Host, 10f + config.ReportGraceTime + 0.1f, 6f, Vector3.zero, out _), "grace over");

            filter.OwnerChanged(Carrier, 20f);
            filter.HostImpact(20.2f);
            Assert.IsFalse(Report(Carrier, Host, 20.3f, 6f, Vector3.zero, out _), "the host's own physics covered that hit");

            filter.OwnerChanged(Carrier, 30f);
            Assert.IsTrue(Report(Carrier, Host, 30.1f, 6f, Vector3.zero, out _), "an older host impact doesn't block a new release");
        }

        [Test]
        public void RequestGuardSendsEachKindOncePerWindowUntilCleared()
        {
            var guard = new LootRequestGuard();
            Assert.IsTrue(guard.TryBegin(LootRequest.Release, 1f, 0.4f));
            Assert.IsFalse(guard.TryBegin(LootRequest.Release, 1.1f, 0.4f), "a snagged item asks every step; send once");
            Assert.IsTrue(guard.TryBegin(LootRequest.Pickup, 1.1f, 0.4f), "kinds are independent");
            Assert.IsTrue(guard.TryBegin(LootRequest.Release, 1.5f, 0.4f), "unanswered requests are retried after the window");
            guard.Clear();
            Assert.IsTrue(guard.TryBegin(LootRequest.Release, 1.6f, 0.4f), "the host's answer clears it");
        }

        [Test]
        public void HoldStateNamesItsHolderOnlyWhenHeld()
        {
            Assert.AreEqual(LootHoldMode.Free, LootHoldState.Free.Mode);
            LootHoldState held = LootHoldState.For(LootHoldMode.Held, 7);
            Assert.IsTrue(held.IsHeldBy(7));
            Assert.IsFalse(held.IsHeldBy(8));
            Assert.IsTrue(LootHoldState.For(LootHoldMode.Pocketed, 7).IsHeldBy(7));
            LootHoldState free = LootHoldState.For(LootHoldMode.Free, 7);
            Assert.AreEqual(0ul, free.HolderObjectId, "a free item has no holder");
            Assert.IsFalse(free.IsHeldBy(7));
            Assert.AreEqual("held by #7", held.ToString());
            Assert.AreEqual("free", free.ToString());
        }

        [Test]
        public void ConfigDefaultsValidateAndBadValuesAreCaught()
        {
            var errors = new List<string>();
            config.Validate(errors);
            Assert.IsEmpty(errors);
            Assert.Greater(config.MaxReportedImpactSpeed, 10f, "real hits must still be able to break things");
            Assert.LessOrEqual(config.ImpactReportInterval, 0.1f);

            var value = new LootValueState(1000, 750, 0.9f, false);
            Assert.IsTrue(value.Rolled, "a host-written value is marked rolled");
            Assert.IsFalse(default(LootValueState).Rolled, "a zeroed state never overwrites a client's item");
        }
    }
}

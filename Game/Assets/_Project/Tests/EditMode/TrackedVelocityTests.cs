using Abandoned.Core;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>
    /// The velocity estimate a kinematic, network-moved loot copy reports to the player hit detector
    /// (M3.4 review: a host-thrown item must still knock a client's player down on the client).
    /// </summary>
    public class TrackedVelocityTests
    {
        private const float Dt = 1f / 60f;

        [Test]
        public void SteadyMotionConvergesToTheRealVelocity()
        {
            var v = new TrackedVelocity();
            Vector3 velocity = new(6f, -1f, 0f);
            Vector3 p = new(3f, 1f, 2f);
            for (int i = 0; i < 20; i++, p += velocity * Dt) v.Sample(p, Dt);
            Assert.Less(Vector3.Distance(velocity, v.Value), 0.05f, $"estimated {v.Value}");
        }

        [Test]
        public void FirstSampleAndAResetReportNoMotion()
        {
            var v = new TrackedVelocity();
            v.Sample(new Vector3(100f, 0f, 0f), Dt);
            Assert.AreEqual(Vector3.zero, v.Value, "nothing to compare against yet");
            v.Sample(new Vector3(100.1f, 0f, 0f), Dt);
            Assert.Greater(v.Value.x, 0f);
            v.Reset();
            Assert.AreEqual(Vector3.zero, v.Value);
            v.Sample(new Vector3(-50f, 0f, 0f), Dt);
            Assert.AreEqual(Vector3.zero, v.Value, "after a reset the next position only primes it");
        }

        [Test]
        public void ATeleportIsNotAHit()
        {
            var v = new TrackedVelocity();
            Vector3 p = Vector3.zero;
            for (int i = 0; i < 10; i++, p += Vector3.forward * 4f * Dt) v.Sample(p, Dt);
            v.Sample(p + Vector3.up * 5f, Dt); // 300 m/s: unpocketed / snapped to the host's pose
            Assert.AreEqual(Vector3.zero, v.Value);
            v.Sample(p + Vector3.up * 5f, Dt);
            Assert.AreEqual(Vector3.zero, v.Value, "standing still at the new spot");
        }

        [Test]
        public void ZeroTimeStepAndNonFinitePositionsAreIgnored()
        {
            var v = new TrackedVelocity();
            v.Sample(Vector3.zero, Dt);
            v.Sample(Vector3.right * 0.1f, Dt);
            Vector3 before = v.Value;
            v.Sample(Vector3.right * 0.2f, 0f);
            Assert.AreEqual(before, v.Value, "a zero step keeps the estimate");
            v.Sample(new Vector3(float.NaN, 0f, 0f), Dt);
            Assert.IsTrue(float.IsFinite(v.Value.sqrMagnitude));
        }
    }
}

using Abandoned.Core;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    public class BlindOneTests
    {
        private BlindOneConfig config;
        private BlindOneBrain brain;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<BlindOneConfig>();
            brain = new BlindOneBrain(config);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        private static NoiseEvent Noise(Vector3 at, float loudness) => new(at, loudness, NoiseSource.Footstep, 0f);

        [Test]
        public void HearingFadesWithDistanceAndWalls()
        {
            NoiseEvent step = Noise(Vector3.zero, 0.25f); // a 10 m radius
            Assert.Greater(Hearing.Perceive(step, new Vector3(2f, 0f, 0f), 0, 1f, 0.6f), Hearing.Perceive(step, new Vector3(8f, 0f, 0f), 0, 1f, 0.6f));
            Assert.AreEqual(0f, Hearing.Perceive(step, new Vector3(11f, 0f, 0f), 0, 1f, 0.6f), "out of earshot");
            Assert.AreEqual(0f, Hearing.Perceive(step, new Vector3(7f, 0f, 0f), 1, 1f, 0.6f), "a wall shrinks 10 m to 6 m");
            Assert.Greater(Hearing.Perceive(step, new Vector3(7f, 0f, 0f), 0, 1f, 0.6f), 0f);
            Assert.Greater(Hearing.Perceive(step, new Vector3(11f, 0f, 0f), 0, 1.5f, 0.6f), 0f, "danger sharpens its hearing");
        }

        [Test]
        public void ItWandersUntilItHearsSomethingThenInvestigates()
        {
            Assert.AreEqual(BlindOneState.Wander, brain.State);
            brain.Hear(new Vector3(5f, 0f, 0f), 0.2f, 1f);
            Assert.AreEqual(BlindOneState.Investigate, brain.State);
            Assert.AreEqual(new Vector3(5f, 0f, 0f), brain.Target);
            Assert.AreEqual(config.WalkSpeed, brain.Speed);
        }

        [Test]
        public void ALoudNoiseOrAStreakOfThemStartsAHunt()
        {
            brain.Hear(Vector3.one, config.HuntThreshold + 0.01f, 1f);
            Assert.AreEqual(BlindOneState.Hunt, brain.State);
            Assert.AreEqual(config.HuntSpeed, brain.Speed);

            var other = new BlindOneBrain(config);
            for (int i = 0; i < config.HuntNoiseCount; i++) other.Hear(Vector3.right * i, 0.1f, 1f + i * 0.5f);
            Assert.AreEqual(BlindOneState.Hunt, other.State, "someone keeps making noise");
        }

        [Test]
        public void ALouderNoiseTakesOverAFadingLead()
        {
            brain.Hear(Vector3.zero, 0.3f, 0f);
            brain.Hear(Vector3.right * 10f, 0.2f, 1f);
            Assert.AreEqual(Vector3.zero, brain.Target, "the first is still louder");
            brain.Hear(Vector3.right * 20f, 0.2f, config.Memory * 0.6f);
            Assert.AreEqual(Vector3.right * 20f, brain.Target, "the old lead has faded below it");
        }

        [Test]
        public void ItListensWhereTheNoiseWasThenWandersOff()
        {
            brain.Hear(Vector3.zero, 0.2f, 0f);
            brain.Tick(Vector3.zero, true, 1f);
            Assert.AreEqual(BlindOneState.Investigate, brain.State);
            brain.Tick(Vector3.zero, true, 1f + config.Linger + 0.1f);
            Assert.AreEqual(BlindOneState.Wander, brain.State);
            Assert.IsFalse(brain.HasTarget);
        }

        [Test]
        public void SilenceEndsAHuntAndAKillPausesIt()
        {
            brain.Hear(Vector3.zero, 1f, 0f);
            brain.Tick(Vector3.one, false, config.HuntForget + 0.5f);
            Assert.AreEqual(BlindOneState.Investigate, brain.State, "lost the trail: goes to listen");

            brain.Hear(Vector3.zero, 1f, 10f);
            brain.Attacked(10f);
            Assert.AreEqual(BlindOneState.Attack, brain.State);
            Assert.AreEqual(0f, brain.Speed);
            brain.Tick(Vector3.zero, false, 10f + config.AttackPause + 0.1f);
            Assert.AreEqual(BlindOneState.Hunt, brain.State);
        }
    }
}

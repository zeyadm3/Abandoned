using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    public class StalkerBrainTests
    {
        [Test]
        public void WatchedItFreezesAloneAndLookingAwayItRushes()
        {
            var c = ScriptableObject.CreateInstance<StalkerConfig>();
            try
            {
                var brain = new StalkerBrain(c);
                brain.Tick(watched: true, isolated: true, 1f);
                Assert.AreEqual(StalkerState.Frozen, brain.State, "being watched freezes it");
                Assert.AreEqual(0f, brain.Dread);

                for (float t = 0f; t < c.RushAfter - 0.5f; t += 0.5f) brain.Tick(false, true, 0.5f);
                Assert.AreEqual(StalkerState.Follow, brain.State, "not yet");
                brain.Tick(false, true, 1f);
                Assert.AreEqual(StalkerState.Rush, brain.State, "alone and looking away too long");

                brain.Tick(true, true, 0.1f);
                Assert.AreEqual(StalkerState.Frozen, brain.State, "turning round in time still saves you");
                Assert.AreEqual(0f, brain.Dread);

                brain.Tick(false, true, c.RushAfter - 1f);
                brain.Tick(false, false, 4f);
                Assert.Less(brain.Dread, c.RushAfter - 1f, "company makes it lose interest");
                Assert.AreEqual(StalkerState.Follow, brain.State);
            }
            finally
            {
                Object.DestroyImmediate(c);
            }
        }
    }
}

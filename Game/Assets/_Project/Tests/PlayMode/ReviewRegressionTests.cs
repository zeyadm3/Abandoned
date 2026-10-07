using System.Collections;
using Abandoned.Interaction;
using Abandoned.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>Regression tests for bugs confirmed in the Milestone 1 review.</summary>
    public class ReviewRegressionTests
    {
        private PlayerTestRig rig;
        private readonly System.Collections.Generic.List<GameObject> extras = new();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // A level left loaded by an earlier test would be in the way.
            yield return NetTestHarness.CleanWorld();
            rig = OnFlatGround(new Vector3(0f, 0.05f, 0f));
            yield return null;
            rig.Settle();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject go in extras) if (go != null) Object.DestroyImmediate(go);
            extras.Clear();
            rig.Destroy();
            yield return null;
        }

        private static IEnumerator WaitFixed(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator WalkingIntoARestingHeavyItemDoesNotRagdoll()
        {
            Grabbable safe = TestCarryable.Create(new Vector3(0f, 0.45f, 2f), CarryClass.Heavy, 400f, 0.9f);
            extras.Add(safe.gameObject);
            yield return WaitFixed(0.5f);
            var ragdoll = rig.Player.GetComponent<PlayerRagdoll>();
            for (int i = 0; i < 90; i++)
            {
                rig.Motor.Simulate(Frame(Vector2.up, sprint: true), Dt);
                yield return new WaitForFixedUpdate();
            }
            Assert.IsFalse(ragdoll.IsRagdolled, "walking into a resting safe knocked the player over");
        }

        [Test]
        public void SteppingOffAKerbIsNotAHardLanding()
        {
            rig.Destroy();
            rig = new PlayerTestRig();
            rig.AddBox("Kerb", new Vector3(0f, 0.15f, -3f), new Vector3(4f, 0.3f, 6f)); // top at 0.3, ends at z = 0
            rig.AddBox("Road", new Vector3(0f, -0.5f, 5f), new Vector3(10f, 1f, 10f));
            rig.SpawnPlayer(new Vector3(0f, 0.35f, -1f), Quaternion.identity);
            rig.Settle();

            float landing = -1f;
            rig.Motor.Landed += (_, speed) => landing = Mathf.Max(landing, speed);
            rig.Run(Frame(Vector2.up), 1.5f);
            Assert.GreaterOrEqual(landing, 0f, "should have stepped off");
            Assert.Less(landing, 4f, "a 30 cm drop must not land at ground-stick speed");
        }

        [Test]
        public void HoldPointStaysOutsideTheBodyWhenLookingDown()
        {
            var carrier = rig.Player.GetComponent<PlayerCarrier>();
            rig.Look.ApplyLook(new Vector2(0f, -100000f)); // max pitch down
            Vector3 hold = carrier.HoldPoint;
            Vector3 flat = new(hold.x - rig.Player.transform.position.x, 0f, hold.z - rig.Player.transform.position.z);
            Assert.GreaterOrEqual(flat.magnitude, carrier.Config.MinHoldRadius - 1e-3f);
        }

        [UnityTest]
        public IEnumerator FarPickupIsNotDroppedOnTheFirstPhysicsStep()
        {
            var carrier = rig.Player.GetComponent<PlayerCarrier>();
            float reach = carrier.Config.Reach;
            // Resting on the floor near the edge of reach: well beyond BreakDistance from the hold point.
            Grabbable item = TestCarryable.Create(new Vector3(0f, 0.2f, reach * 0.95f), CarryClass.OneHand, 1f);
            extras.Add(item.gameObject);
            yield return WaitFixed(0.2f);
            InteractionService.Handler.RequestPickup(carrier, item);
            Assert.IsNotNull(carrier.Held);
            yield return WaitFixed(0.3f);
            Assert.IsNotNull(carrier.Held, "dropped before it could arrive");
        }

        [UnityTest]
        public IEnumerator GettingUpNeverStandsOnLoot()
        {
            var ragdoll = rig.Player.GetComponent<PlayerRagdoll>();
            ragdoll.Enter(Vector3.zero);
            yield return WaitFixed(0.8f);
            // Put a crate right over where the body lies, like a dropped TV.
            Grabbable crate = TestCarryable.Create(ragdoll.Pelvis.position + Vector3.up * 0.6f, CarryClass.TwoHand, 15f, 0.8f);
            crate.Body.isKinematic = true;
            extras.Add(crate.gameObject);
            Physics.SyncTransforms();
            ragdoll.Recover();
            Assert.AreEqual(0f, rig.Player.transform.position.y, 0.15f, "stood on the crate instead of the floor");
        }

        [UnityTest]
        public IEnumerator RecoveringDoesNotResumeTheOldRun()
        {
            rig.Run(Frame(Vector2.up, sprint: true), 0.5f);
            var ragdoll = rig.Player.GetComponent<PlayerRagdoll>();
            ragdoll.Enter(Vector3.zero);
            yield return WaitFixed(0.5f);
            ragdoll.Recover();
            rig.Motor.enabled = false; // back under manual control
            rig.Motor.Simulate(Frame(), Dt);
            Assert.Less(rig.Motor.HorizontalSpeed, 0.5f);
        }
    }
}

using System.Collections;
using Abandoned.Interaction;
using Abandoned.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    public class RagdollTests
    {
        private PlayerTestRig rig;
        private PlayerRagdoll ragdoll;
        private readonly System.Collections.Generic.List<GameObject> extras = new();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            rig = OnFlatGround(new Vector3(0f, 0.05f, 0f));
            ragdoll = rig.Player.GetComponent<PlayerRagdoll>();
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

        [Test]
        public void StartsControlledWithRagdollHidden()
        {
            Assert.IsFalse(ragdoll.IsRagdolled);
            Assert.IsFalse(ragdoll.Pelvis.gameObject.activeInHierarchy);
            Assert.IsTrue(rig.Player.GetComponent<CharacterController>().enabled);
        }

        [UnityTest]
        public IEnumerator EnterSwitchesToPhysicsAndCameraFollowsHead()
        {
            ragdoll.Enter(Vector3.forward * 3f);
            Assert.IsTrue(ragdoll.IsRagdolled);
            Assert.IsFalse(rig.Player.GetComponent<CharacterController>().enabled);
            Assert.IsTrue(ragdoll.Pelvis.gameObject.activeInHierarchy);
            Assert.IsFalse(rig.Player.transform.Find("Body").gameObject.activeSelf);
            yield return WaitFixed(0.5f);
            yield return null; // LateUpdate moves the camera root
            Transform cameraRoot = rig.Player.transform.Find("CameraRoot");
            Assert.Less(Vector3.Distance(cameraRoot.position, ragdoll.Head.position), 0.05f);
            Assert.Less(ragdoll.Pelvis.position.y, 0.95f, "body should collapse to the floor");
        }

        [UnityTest]
        public IEnumerator RecoversWhereTheBodySettled()
        {
            ragdoll.Enter(Vector3.forward * 4f);
            yield return WaitFixed(1f);
            Vector3 pelvis = ragdoll.Pelvis.position;
            Abandoned.Core.ScreenshotCapture.CaptureFrom(pelvis + new Vector3(2.5f, 1.8f, -2.5f), pelvis, "M1_4_ragdoll", 60f);
            ragdoll.Recover();
            Assert.IsFalse(ragdoll.IsRagdolled);
            Assert.IsTrue(rig.Player.GetComponent<CharacterController>().enabled);
            Assert.IsTrue(rig.Player.transform.Find("Body").gameObject.activeSelf);
            Assert.AreEqual(pelvis.x, rig.Player.transform.position.x, 0.05f);
            Assert.AreEqual(pelvis.z, rig.Player.transform.position.z, 0.05f);
            Assert.AreEqual(0f, rig.Player.transform.position.y, 0.1f, "stands on the floor");
            Transform cameraRoot = rig.Player.transform.Find("CameraRoot");
            Assert.AreEqual(0f, cameraRoot.localPosition.x, 1e-4f);
            Assert.AreEqual(0f, cameraRoot.localPosition.z, 1e-4f);
        }

        [UnityTest]
        public IEnumerator GetsUpAutomaticallyOnceSettled()
        {
            ragdoll.Enter(Vector3.zero);
            float limit = ragdoll.Config.MaxRagdollTime + 0.5f;
            for (float t = 0f; t < limit && ragdoll.IsRagdolled; t += Time.deltaTime) yield return null;
            Assert.IsFalse(ragdoll.IsRagdolled);
        }

        [Test]
        public void FallFromAboveTheThresholdRagdolls()
        {
            float landedHeight = -1f;
            rig.Motor.Landed += (h, _) => landedHeight = h;
            rig.Teleport(new Vector3(0f, ragdoll.Config.FallHeight + 1.5f, 0f));
            // IsGrounded is stale until the first Simulate after the teleport, so step first.
            rig.Motor.Simulate(Frame(), Dt);
            rig.RunUntil(Frame(), () => ragdoll.IsRagdolled || rig.Motor.IsGrounded, 3f);
            Assert.IsTrue(ragdoll.IsRagdolled, $"landed from {landedHeight:F2} m, y now {rig.Player.transform.position.y:F2}");
        }

        [Test]
        public void ShortFallDoesNotRagdoll()
        {
            rig.Teleport(new Vector3(0f, 2f, 0f));
            rig.Run(Frame(), 1.5f);
            Assert.IsTrue(rig.Motor.IsGrounded);
            Assert.IsFalse(ragdoll.IsRagdolled);
        }

        [Test]
        public void JumpingDoesNotRagdoll()
        {
            rig.Motor.Simulate(Frame(jump: true), Dt);
            rig.Run(Frame(), 1.5f);
            Assert.IsFalse(ragdoll.IsRagdolled);
        }

        private Rigidbody Projectile(Vector3 position, float weight, Vector3 velocity)
        {
            Interaction.Grabbable g = TestCarryable.Create(position, CarryClass.Heavy, weight, 0.6f);
            extras.Add(g.gameObject);
            g.Body.useGravity = false;
            g.Body.linearVelocity = velocity;
            return g.Body;
        }

        [UnityTest]
        public IEnumerator HeavyObjectHitKnocksThePlayerDown()
        {
            Projectile(new Vector3(0f, 1f, 3f), 300f, Vector3.back * 6f);
            yield return WaitFixed(1f);
            Assert.IsTrue(ragdoll.IsRagdolled);
        }

        [UnityTest]
        public IEnumerator LightObjectHitDoesNot()
        {
            Projectile(new Vector3(0f, 1f, 3f), 2f, Vector3.back * 10f);
            yield return WaitFixed(1f);
            Assert.IsFalse(ragdoll.IsRagdolled);
        }

        [UnityTest]
        public IEnumerator RagdollingDropsTheHeldItem()
        {
            var carrier = rig.Player.GetComponent<PlayerCarrier>();
            Interaction.Grabbable item = TestCarryable.Create(carrier.HoldPoint, CarryClass.OneHand, 2f);
            extras.Add(item.gameObject);
            yield return null;
            InteractionService.Handler.RequestPickup(carrier, item);
            Assert.IsNotNull(carrier.Held);
            ragdoll.Enter(Vector3.zero);
            Assert.IsNull(carrier.Held);
            Assert.IsTrue(item.Body.useGravity);
        }
    }
}

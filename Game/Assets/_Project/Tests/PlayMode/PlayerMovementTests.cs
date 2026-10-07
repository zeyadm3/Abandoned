using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>Movement on flat ground with exact inputs: speeds, snappiness, stamina, jump, crouch.</summary>
    public class PlayerMovementTests
    {
        private static readonly Vector2 Forward = Vector2.up;
        private PlayerTestRig rig;

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
            rig.Destroy();
            yield return null;
        }

        [Test]
        public void SettlesGroundedAtFeetPivot()
        {
            Assert.IsTrue(rig.Motor.IsGrounded);
            Assert.AreEqual(0f, rig.Player.transform.position.y, 0.1f);
        }

        [Test]
        public void SomethingAppearingInsideThePlayerDoesNotFlingThem()
        {
            // A crate spawned (or landing) overlapping the capsule: the controller pushes us out, and that
            // push over a very short frame used to become a speed of hundreds of m/s.
            rig.AddBox("Intruder", rig.Player.transform.position + new Vector3(0.55f, 1f, 0.3f), Vector3.one);
            rig.Motor.Simulate(Frame(), 0.0005f);
            Vector3 after = rig.Player.transform.position;
            rig.Run(Frame(), 0.25f);
            Assert.Less(rig.Motor.HorizontalSpeed, 0.5f, "standing still stays still");
            Assert.Less(Vector3.ProjectOnPlane(rig.Player.transform.position - after, Vector3.up).magnitude, 0.2f, "and isn't carried off");
        }

        [Test]
        public void WalkReachesWalkSpeedWithinATenthOfASecond()
        {
            float t = rig.RunUntil(Frame(Forward), () => rig.Motor.HorizontalSpeed >= rig.Config.WalkSpeed * 0.95f, 1f);
            Assert.That(t, Is.InRange(0f, 0.1f), "acceleration too slow (floaty)");
            rig.Run(Frame(Forward), 0.5f);
            Assert.AreEqual(rig.Config.WalkSpeed, rig.Motor.HorizontalSpeed, 0.05f);
        }

        [Test]
        public void StopsWithinATenthOfASecond()
        {
            rig.Run(Frame(Forward), 0.5f);
            float t = rig.RunUntil(Frame(), () => rig.Motor.HorizontalSpeed < 0.05f, 1f);
            Assert.That(t, Is.InRange(0f, 0.1f), "braking too slow (slidey)");
        }

        [Test]
        public void SprintIsFasterAndDrainsStamina()
        {
            rig.Run(Frame(Forward, sprint: true), 1f);
            Assert.IsTrue(rig.Motor.IsSprinting);
            Assert.AreEqual(rig.Config.SprintSpeed, rig.Motor.HorizontalSpeed, 0.05f);
            float expected = rig.Config.MaxStamina - rig.Config.SprintDrainPerSecond * 1f;
            Assert.AreEqual(expected, rig.Stamina.Current, 1f);
        }

        [Test]
        public void SprintWithoutMovingDoesNotDrain()
        {
            rig.Run(Frame(sprint: true), 1f);
            Assert.IsFalse(rig.Motor.IsSprinting);
            Assert.AreEqual(rig.Config.MaxStamina, rig.Stamina.Current, 0.01f);
        }

        [Test]
        public void ExhaustionLocksSprintUntilThreshold()
        {
            float drainTime = rig.Config.MaxStamina / rig.Config.SprintDrainPerSecond;
            rig.Run(Frame(Forward, sprint: true), drainTime + 0.2f);
            Assert.IsTrue(rig.Stamina.IsExhausted);
            Assert.IsFalse(rig.Motor.IsSprinting);
            Assert.AreEqual(rig.Config.WalkSpeed, rig.Motor.HorizontalSpeed, 0.05f);

            // Stop sprinting: regen starts after the delay; sprint unlocks at the threshold, not before.
            rig.Run(Frame(), rig.Config.RegenDelay + 0.1f);
            Assert.IsTrue(rig.Stamina.IsExhausted);
            float refill = rig.Config.SprintResumeThreshold / rig.Config.RegenPerSecond;
            rig.Run(Frame(), refill + 0.1f);
            Assert.IsFalse(rig.Stamina.IsExhausted);
            rig.Run(Frame(Forward, sprint: true), 0.2f);
            Assert.IsTrue(rig.Motor.IsSprinting);
        }

        [Test]
        public void SpeedMultiplierScalesSpeed()
        {
            rig.Motor.SpeedMultiplier = 0.5f;
            rig.Run(Frame(Forward), 0.5f);
            Assert.AreEqual(rig.Config.WalkSpeed * 0.5f, rig.Motor.HorizontalSpeed, 0.05f);
        }

        [Test]
        public void StaminaDrainMultiplierScalesSprintDrain()
        {
            rig.Motor.StaminaDrainMultiplier = 2f;
            rig.Run(Frame(Forward, sprint: true), 1f);
            float expected = rig.Config.MaxStamina - rig.Config.SprintDrainPerSecond * 2f;
            Assert.AreEqual(expected, rig.Stamina.Current, 1.5f);
        }

        [Test]
        public void JumpReachesConfiguredHeightAndLands()
        {
            float startY = rig.Player.transform.position.y;
            float peak = startY;
            rig.Motor.Simulate(Frame(jump: true), Dt);
            Assert.AreEqual(rig.Config.MaxStamina - rig.Config.JumpStaminaCost, rig.Stamina.Current, 0.01f);
            for (int i = 0; i < 120; i++)
            {
                rig.Motor.Simulate(Frame(), Dt);
                peak = Mathf.Max(peak, rig.Player.transform.position.y);
            }
            Assert.AreEqual(rig.Config.JumpHeight, peak - startY, 0.1f);
            Assert.IsTrue(rig.Motor.IsGrounded, "should have landed within 2s");
        }

        [Test]
        public void JumpIsSnappyNotFloaty()
        {
            // Total airtime for a 1.1m jump should be well under a second.
            rig.Motor.Simulate(Frame(jump: true), Dt);
            rig.Motor.Simulate(Frame(), Dt);
            float airtime = rig.RunUntil(Frame(), () => rig.Motor.IsGrounded, 2f);
            Assert.That(airtime, Is.InRange(0.4f, 0.8f));
        }

        [Test]
        public void CannotDoubleJump()
        {
            rig.Motor.Simulate(Frame(jump: true), Dt);
            rig.Run(Frame(), 0.25f);
            float vyBefore = rig.Motor.Velocity.y;
            rig.Motor.Simulate(Frame(jump: true), Dt);
            Assert.Less(rig.Motor.Velocity.y, vyBefore, "second jump in the air should not add upward speed");
        }

        [Test]
        public void JumpBufferedJustBeforeLandingStillJumps()
        {
            rig.Motor.Simulate(Frame(jump: true), Dt);
            rig.Motor.Simulate(Frame(), Dt);
            // Fall until just above the ground, press jump, and expect a second take-off on landing.
            rig.RunUntil(Frame(), () => rig.Motor.Velocity.y < 0f && rig.Player.transform.position.y < 0.15f, 2f);
            rig.Motor.Simulate(Frame(jump: true), Dt);
            float t = rig.RunUntil(Frame(), () => rig.Motor.Velocity.y > 1f, rig.Config.JumpBufferTime + 0.05f);
            Assert.That(t, Is.GreaterThanOrEqualTo(0f), "buffered jump was dropped");
        }

        [Test]
        public void CoyoteJumpAfterWalkingOffALedge()
        {
            rig.Destroy();
            rig = new PlayerTestRig();
            rig.AddBox("Ledge", new Vector3(0f, -0.5f, -5f), new Vector3(4f, 1f, 10f)); // ends at z = 0
            rig.AddBox("Pit", new Vector3(0f, -20.5f, 0f), new Vector3(50f, 1f, 50f));
            rig.SpawnPlayer(new Vector3(0f, 0.05f, -2f), Quaternion.identity);
            rig.Settle();

            rig.RunUntil(Frame(Forward), () => !rig.Motor.IsGrounded, 2f);
            Assert.IsFalse(rig.Motor.IsGrounded);
            rig.Motor.Simulate(Frame(Forward, jump: true), Dt);
            Assert.Greater(rig.Motor.Velocity.y, 1f, "jump just after leaving the ledge should work");
        }

        [Test]
        public void CrouchLowersHeightAndSpeed()
        {
            rig.Run(Frame(Forward, crouchHeld: true), 0.5f);
            Assert.IsTrue(rig.Motor.IsCrouching);
            Assert.AreEqual(rig.Config.CrouchHeight, rig.Motor.CurrentHeight, 0.01f);
            Assert.AreEqual(rig.Config.CrouchSpeed, rig.Motor.HorizontalSpeed, 0.05f);

            Transform cameraRoot = rig.Player.transform.Find("CameraRoot");
            Assert.AreEqual(rig.Config.CrouchEyeHeight, cameraRoot.localPosition.y, 0.01f);
        }

        [Test]
        public void CannotSprintOrJumpWhileCrouched()
        {
            rig.Run(Frame(Forward, sprint: true, crouchHeld: true), 0.5f);
            Assert.IsFalse(rig.Motor.IsSprinting);
            rig.Motor.Simulate(Frame(Forward, crouchHeld: true, jump: true), Dt);
            Assert.LessOrEqual(rig.Motor.Velocity.y, 0f);
        }

        [Test]
        public void CannotStandUpUnderALowCeiling()
        {
            rig.Run(Frame(crouchHeld: true), 0.3f);
            // Ceiling between crouched (1.1m) and standing (1.8m) head height.
            rig.AddBox("Ceiling", new Vector3(0f, 1.5f, 0f), new Vector3(4f, 0.2f, 4f));
            rig.Run(Frame(), 0.5f);
            Assert.IsTrue(rig.Motor.IsCrouching, "stood up into the ceiling");
            Assert.AreEqual(rig.Config.CrouchHeight, rig.Motor.CurrentHeight, 0.01f);

            rig.Teleport(new Vector3(6f, 0.05f, 0f));
            rig.Run(Frame(), 0.5f);
            Assert.IsFalse(rig.Motor.IsCrouching, "should stand once clear of the ceiling");
            Assert.AreEqual(rig.Config.StandingHeight, rig.Motor.CurrentHeight, 0.01f);
        }

        [Test]
        public void WallStopsMovementWithoutSpeedBuildUp()
        {
            rig.AddBox("Wall", new Vector3(0f, 1.5f, 2f), new Vector3(6f, 3f, 0.2f));
            rig.Run(Frame(Forward), 2f);
            Assert.Less(rig.Player.transform.position.z, 2f - 0.1f - rig.Config.Radius + 0.1f);
            Assert.Less(rig.Motor.HorizontalSpeed, 0.2f);
        }

        [Test]
        public void LookClampsPitch()
        {
            rig.Look.ApplyLook(new Vector2(0f, -100000f));
            Assert.AreEqual(rig.Config.MaxPitch, rig.Look.Pitch, 0.01f);
            rig.Look.ApplyLook(new Vector2(0f, 100000f));
            Assert.AreEqual(-rig.Config.MaxPitch, rig.Look.Pitch, 0.01f);
        }

        [Test]
        public void LookYawTurnsMovementDirection()
        {
            rig.Look.ApplyLook(new Vector2(90f / rig.Config.MouseSensitivity, 0f));
            rig.Run(Frame(Forward), 0.5f);
            Assert.Greater(rig.Player.transform.position.x, 1f, "after a 90° turn, forward should be +X");
            Assert.AreEqual(0f, rig.Player.transform.position.z, 0.1f);
        }
    }
}

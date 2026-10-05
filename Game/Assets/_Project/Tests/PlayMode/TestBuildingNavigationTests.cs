using System.Collections;
using Abandoned.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>
    /// Walks the real TestBuilding with the scene's player: doors, stairs, balconies, railings.
    /// Coordinates follow TestBuildingBuilder: 4m tiles, x 0..20, z 0..16, upper floor at y = 4.
    /// </summary>
    public class TestBuildingNavigationTests
    {
        private const string SceneName = "TestBuilding";
        private static readonly Vector2 Forward = Vector2.up;
        private PlayerTestRig rig;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            var motor = Object.FindFirstObjectByType<PlayerMotor>();
            Assert.IsNotNull(motor, "TestBuilding has no player");
            rig = ForExisting(motor.gameObject);
        }

        [Test]
        public void PlayerSpawnsGroundedInTheParkingLot()
        {
            rig.Settle();
            Assert.IsTrue(rig.Motor.IsGrounded);
            Assert.Less(rig.Player.transform.position.z, 0f, "spawn should be outside, south of the front wall");
        }

        [Test]
        public void WalksThroughTheNarrowDoor()
        {
            rig.Teleport(new Vector3(6f, 0.05f, -3f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Forward), () => rig.Player.transform.position.z > 2f, 3f);
            Assert.GreaterOrEqual(t, 0f, "blocked at the narrow door");
            Assert.AreEqual(0f, rig.Player.transform.position.y, 0.15f);
        }

        [Test]
        public void WalksThroughTheLoadingDoor()
        {
            rig.Teleport(new Vector3(14f, 0.05f, -3f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Forward), () => rig.Player.transform.position.z > 2f, 3f);
            Assert.GreaterOrEqual(t, 0f, "blocked at the loading door");
        }

        [Test]
        public void ClimbsTheStairsToTheUpperFloor()
        {
            rig.Teleport(new Vector3(18f, 0.05f, 2f));
            rig.Settle();
            float t = rig.RunUntil(Frame(Forward), () => rig.Player.transform.position.z > 13f, 6f);
            Assert.GreaterOrEqual(t, 0f, "didn't reach the top of the stairs");
            rig.Settle();
            Assert.IsTrue(rig.Motor.IsGrounded);
            Assert.AreEqual(4f, rig.Player.transform.position.y, 0.15f);
        }

        [Test]
        public void SprintsDownTheStairsWithoutLosingGround()
        {
            rig.Teleport(new Vector3(18f, 4.05f, 13.5f), 180f);
            rig.Settle();
            int airborneFrames = 0;
            float t = rig.RunUntil(Frame(Forward, sprint: true), () =>
            {
                if (!rig.Motor.IsGrounded) airborneFrames++;
                return rig.Player.transform.position.z < 3f;
            }, 6f);
            Assert.GreaterOrEqual(t, 0f, "didn't reach the bottom");
            Assert.Less(airborneFrames, 6, "skipped off the stairs instead of hugging them");
            Assert.AreEqual(0f, rig.Player.transform.position.y, 0.2f);
        }

        [Test]
        public void BalconyHoldsThePlayerAndRailingStopsFallsIntoTheAtrium()
        {
            // Balcony_U_1_3 spans x 4..8, z 12..16; the atrium edge railing is at z = 12.
            // x = 4.8 keeps clear of the glass sculpture placed at (6, 14).
            rig.Teleport(new Vector3(4.8f, 4.05f, 14f), 180f);
            rig.Settle();
            Assert.IsTrue(rig.Motor.IsGrounded);
            Assert.AreEqual(4f, rig.Player.transform.position.y, 0.15f);

            rig.Run(Frame(Forward, sprint: true), 2f);
            Assert.Greater(rig.Player.transform.position.z, 12f, "went through the railing");
            Assert.AreEqual(4f, rig.Player.transform.position.y, 0.15f);
        }

        [Test]
        public void CrouchesUnderTheUpperFloorEdgeIsNotNeeded()
        {
            // Ground-floor headroom is 3.7m; standing must be possible everywhere on the ground floor.
            rig.Teleport(new Vector3(2f, 0.05f, 2f));
            rig.Run(Frame(crouchHeld: true), 0.3f);
            rig.Run(Frame(), 0.5f);
            Assert.IsFalse(rig.Motor.IsCrouching);
        }
    }
}

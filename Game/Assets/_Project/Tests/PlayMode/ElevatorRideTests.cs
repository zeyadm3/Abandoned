using System.Collections;
using Abandoned.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    public class ElevatorRideTests
    {
        private PlayerTestRig rig;
        private MovingPlatform platform;
        private Vector3 origin;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return NetTestHarness.CleanWorld();
            rig=new PlayerTestRig();
            GameObject deck=rig.AddBox("LiftDeck",new Vector3(0,-.15f,0),new Vector3(3.4f,.3f,3.4f));
            platform=deck.AddComponent<MovingPlatform>();origin=deck.transform.position;
            platform.MoveTo(origin);
            GameObject roof=rig.AddBox("LiftRoof",new Vector3(0,2.85f,0),new Vector3(3.4f,.1f,3.4f));
            roof.transform.SetParent(deck.transform,true);
            rig.SpawnPlayer(new Vector3(0,.05f,0),Quaternion.identity);
            yield return null;
            rig.Settle();
        }

        [UnityTearDown]
        public IEnumerator TearDown(){rig.Destroy();yield return null;}

        [TestCase(1f/60)] [TestCase(1f/30)] [TestCase(.1f)] [TestCase(.25f)]
        public void PlayerRidesToRestaurantAndStaysOnTheStoppedCab(float dt)
        {
            for(float h=0;h<404;)
            {
                h=Mathf.Min(404,h+8*dt);
                platform.MoveTo(origin+Vector3.up*h);
                Physics.SyncTransforms();
                rig.Motor.Simulate(Frame(),dt);
                Assert.That(rig.Player.transform.position.y,Is.EqualTo(h).Within(.20f),"Rider lost cab support at "+h+"m with step "+dt);
            }
            for(int i=0;i<90;i++)
            {
                platform.MoveTo(origin+Vector3.up*404);
                rig.Motor.Simulate(Frame(),Dt);
            }
            Assert.That(rig.Player.transform.position.y,Is.EqualTo(404).Within(.10f));
            Assert.IsTrue(rig.Motor.IsGrounded);
        }

        [Test]
        public void RiderStaysSupportedOnDescentAndCanJumpOffAHorizontallyMovingDeck()
        {
            for(int i=1;i<=60;i++)
            {
                platform.MoveTo(origin-Vector3.up*(i*.25f));
                rig.Motor.Simulate(Frame(),1f/30);
                Assert.That(rig.Player.transform.position.y,Is.EqualTo(-i*.25f).Within(.15f));
            }
            rig.Motor.Simulate(Frame(jump:true),Dt);
            for(int i=1;i<=15;i++)
            {
                platform.MoveTo(origin+new Vector3(i*.25f,-15,0));
                rig.Motor.Simulate(Frame(),Dt);
            }
            Assert.That(rig.Player.transform.position.x,Is.LessThan(.2f),"Jumping must detach from cached support.");
            Assert.IsFalse(rig.Motor.IsGrounded);
        }

        [Test]
        public void DescendingHeightCorrectionDoesNotCatchTheRiderOnTheCabRoof()
        {
            platform.MoveTo(origin+Vector3.down*4);
            rig.Motor.Simulate(Frame(),Dt);
            Assert.That(rig.Player.transform.position.y,Is.EqualTo(-4).Within(.15f));
            Assert.IsTrue(rig.Motor.IsGrounded);
        }
    }
}

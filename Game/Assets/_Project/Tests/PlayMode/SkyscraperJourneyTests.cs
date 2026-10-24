using System.Collections;
using System.Reflection;
using Abandoned.Networking;
using Abandoned.Skyscraper;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    public class SkyscraperJourneyTests
    {
        [UnityTearDown]
        public IEnumerator Clean()=>NetTestHarness.CleanWorld();

        [UnityTest]
        public IEnumerator SavedCabClosesBeforeDepartureReachesApexAndHasASafeExit()
        {
            yield return TestMapScene.Load("Skyscraper");
            var lift=Object.FindFirstObjectByType<SkyscraperElevator>();
            Assert.IsNotNull(lift);Assert.IsTrue(lift.IsSpawned);Assert.IsTrue(lift.IsPowered);
            var rig=PlayerTestRig.ForExisting(TestMapScene.Player);
            rig.Teleport(new Vector3(0,.04f,0));rig.Motor.ResetFallTracking();rig.Settle();
            var cabDoor=lift.GetComponentInChildren<SkyscraperElevatorDoor>();
            Assert.IsNotNull(cabDoor);
            // Exercise the actual host/RPC/gate logic; accelerate the distance, not the door interlock.
            typeof(SkyscraperElevator).GetField("travelSpeed",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(lift,80f);
            float until=Time.time+2;
            while(!cabDoor.IsOpen && Time.time<until){rig.Motor.Simulate(Frame(),Time.deltaTime);yield return null;}
            Assert.IsTrue(cabDoor.IsOpen);
            lift.RequestFloor(2);
            until=Time.time+12;
            while((lift.IsMoving || lift.CurrentFloor!=2) && Time.time<until)
            {
                rig.Motor.Simulate(Frame(),Time.deltaTime);
                Assert.That(rig.Player.transform.position.y,Is.GreaterThanOrEqualTo(lift.DeckPosition.y-.2f),"Player fell through the moving cab.");
                if(!cabDoor.IsClosed)Assert.That(lift.DeckPosition.y,Is.EqualTo(0).Within(.02f),"Car departed before its doors closed.");
                yield return null;
            }
            Assert.That(lift.CurrentFloor,Is.EqualTo(2));Assert.IsFalse(lift.IsMoving);
            Assert.That(lift.DeckPosition.y,Is.EqualTo(404).Within(.01f));
            until=Time.time+2;
            while(!cabDoor.IsOpen && Time.time<until){rig.Motor.Simulate(Frame(),Time.deltaTime);yield return null;}
            Assert.IsTrue(cabDoor.IsOpen);
            for(float t=0;t<1.2f;t+=Dt)
            {
                rig.Motor.Simulate(Frame(Vector2.down),Dt);
                Assert.That(rig.Player.transform.position.y,Is.EqualTo(404).Within(.15f),"Gap between car and restaurant floor.");
                yield return null;
            }
            Assert.That(rig.Player.transform.position.z,Is.LessThan(-3.3f),"Could not leave the lift.");
            Assert.IsTrue(rig.Motor.IsGrounded);Assert.IsFalse(NetworkPlayer.Local.IsDead);
            for(int i=0;i<72;i++){rig.Motor.Simulate(Frame(Vector2.up),Dt);yield return null;}
            Assert.That(Mathf.Abs(rig.Player.transform.position.z),Is.LessThan(1.1f));
            lift.RequestFloor(0);until=Time.time+12;
            while((lift.IsMoving || lift.CurrentFloor!=0) && Time.time<until)
            {
                rig.Motor.Simulate(Frame(),Time.deltaTime);
                Assert.That(rig.Player.transform.position.y,Is.EqualTo(lift.DeckPosition.y).Within(.2f));
                yield return null;
            }
            Assert.That(lift.CurrentFloor,Is.EqualTo(0));Assert.IsFalse(lift.IsMoving);
            Assert.IsFalse(NetworkPlayer.Local.IsDead);
        }
    }
}

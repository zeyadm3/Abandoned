using System.Collections;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>The run in the mall, solo: haul counts what's in the truck, the truck leaves after its honk with whoever's aboard.</summary>
    public class MallRunTests
    {
        private PlayerTestRig rig;
        private RunState run;
        private TruckCargo truck;
        private NetworkLootSpawner spawner;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestMapScene.Load("Mall");
            rig = ForExisting(TestMapScene.Player);
            for (int i = 0; i < 60 && RunState.Current == null; i++) yield return null;
            run = RunState.Current;
            truck = TruckCargo.Current;
            spawner = Object.FindAnyObjectByType<NetworkLootSpawner>();
            Assert.IsNotNull(run, "the host spawned the run");
            Assert.IsNotNull(truck);
        }

        private Vector3 BayCenter => truck.transform.position + Vector3.up * 1.2f;

        private NetworkLoot Spawned(System.Func<NetworkLoot, bool> pick) =>
            spawner.Spawned.First(l => l != null && l.IsSpawned && pick(l));

        private static void Move(NetworkLoot loot, Vector3 to)
        {
            Rigidbody body = loot.Grabbable.Body;
            body.position = to;
            loot.transform.position = to;
            body.linearVelocity = Vector3.zero;
        }

        [Test]
        public void TheRunStartsWithAQuotaAWindowAndAnEmptyTruck()
        {
            RunNetState s = run.State;
            Assert.AreEqual(RunPhase.Running, s.Phase);
            Assert.AreEqual(run.Config.Quota, s.Quota);
            Assert.AreEqual(run.Config.WindowSeconds, run.WindowRemaining, 2f);
            Assert.AreEqual(0, s.Haul);
            Assert.AreEqual(spawner.Seed, s.Seed);
        }

        [UnityTest]
        public IEnumerator LootInTheBayCountsTowardTheHaul()
        {
            NetworkLoot laptop = Spawned(l => l.Item.Definition.CarryClass == CarryClass.OneHand && l.Item.Definition.Fragility <= Abandoned.Loot.Fragility.Medium);
            Move(laptop, BayCenter);
            yield return new WaitForSeconds(0.8f);
            Assert.AreEqual(laptop.Item.CurrentValue, run.State.Haul);
            Assert.Greater(run.State.CargoVolume, 0f);
            Move(laptop, BayCenter + Vector3.left * 12f);
            yield return new WaitForSeconds(0.6f);
            Assert.AreEqual(0, run.State.Haul, "taken back out");
        }

        [UnityTest]
        public IEnumerator TheTruckHonksThenLeavesWithItsCargoAndCrew()
        {
            var ignition = truck.Ignition.GetComponent<TruckIgnition>();
            run.RequestDepart();
            yield return null;
            Assert.AreEqual(RunPhase.Running, run.State.Phase, "nobody at the ignition: ignored");

            rig.Teleport(truck.transform.position + Vector3.up * 0.45f + Vector3.forward * 0.8f);
            rig.Settle();
            // Something sturdy: it's dropped onto the bay floor.
            NetworkLoot item = Spawned(l => l.Item.Definition.CarryClass == CarryClass.TwoHand && l.Item.Definition.Fragility <= Abandoned.Loot.Fragility.Medium);
            Move(item, BayCenter + Vector3.back * 0.8f);
            StringAssert.StartsWith("Start the truck", ignition.UsePrompt(rig.Player));
            ignition.Use(rig.Player);
            yield return null;
            Assert.AreEqual(RunPhase.Honking, run.State.Phase);
            Assert.IsNull(ignition.UsePrompt(rig.Player), "can't start it twice");

            RunResults results = null;
            run.Departed += r => results = r;
            float end = Time.time + run.Config.HonkSeconds + 3f;
            while (results == null && Time.time < end) yield return null;
            Assert.IsNotNull(results, "the truck never left");
            Assert.AreEqual(RunPhase.Departed, run.State.Phase);
            Assert.IsTrue(results.Items.Any(i => i.Name == item.Item.Definition.DisplayName), "the cargo is in the results");
            Assert.AreEqual(results.Haul, run.State.Haul);
            Assert.IsTrue(results.Players.Single().Extracted, "the player rode along");
            Assert.AreEqual(run.Config.Quota, results.Quota);
        }

        [UnityTest]
        public IEnumerator SomeoneLeftBehindLosesTheirPockets()
        {
            NetworkLoot pocket = Spawned(l => l.Item.Definition.CarryClass == CarryClass.Pocket);
            PlayerCarrier carrier = rig.Player.GetComponent<PlayerCarrier>();
            rig.Teleport(truck.transform.position + Vector3.up * 0.45f);
            rig.Settle();
            Move(pocket, carrier.EyePosition + carrier.EyeForward * 0.8f);
            yield return null;
            Assert.IsTrue(LootServerActions.TryPickup(pocket, carrier, out string reason), reason);
            Assert.IsTrue(pocket.Grabbable.IsPocketed);

            run.RequestDepart();
            yield return null;
            rig.Teleport(new Vector3(24f, 0.05f, -8f)); // runs back to the parking lot
            rig.Settle();
            RunResults results = null;
            run.Departed += r => results = r;
            float end = Time.time + run.Config.HonkSeconds + 3f;
            while (results == null && Time.time < end) yield return null;
            Assert.IsNotNull(results);
            RunResults.Player me = results.Players.Single();
            Assert.IsFalse(me.Extracted);
            Assert.AreEqual(pocket.Item.CurrentValue, me.PocketValueLost);
            Assert.IsFalse(results.Items.Any(i => i.Pocketed), "left-behind pockets don't count");
            Assert.AreEqual(0, results.Haul, "an item pocketed in the bay rides in the pocket, not the cargo");
        }

        private IEnumerator Depart(System.Action<RunResults> got)
        {
            rig.Teleport(truck.transform.position + Vector3.up * 0.45f);
            rig.Settle();
            RunResults results = null;
            run.Departed += r => results = r;
            run.RequestDepart();
            float end = Time.time + run.Config.HonkSeconds + 3f;
            while (results == null && Time.time < end) yield return null;
            Assert.IsNotNull(results, "the truck never left");
            got(results);
        }

        [UnityTest]
        public IEnumerator TheAppraisalNamesWhoBrokeWhat()
        {
            NetworkLoot fragile = Spawned(l => l.Item.Definition.Fragility == Abandoned.Loot.Fragility.Extreme && l.Item.Definition.CarryClass <= CarryClass.TwoHand);
            PlayerCarrier carrier = rig.Player.GetComponent<PlayerCarrier>();
            rig.Teleport(fragile.transform.position + Vector3.back * 1.2f);
            rig.Settle();
            Move(fragile, carrier.EyePosition + carrier.EyeForward * 0.9f);
            yield return null;
            Assert.IsTrue(LootServerActions.TryPickup(fragile, carrier, out string reason), reason);
            yield return null;
            fragile.Item.ApplyImpact(25f, fragile.transform.position);
            Assert.IsTrue(fragile.Item.IsShattered);
            yield return new WaitForSeconds(0.2f);

            RunResults results = null;
            yield return Depart(r => results = r);
            Assert.IsTrue(results.Stats.Any(l => l.StartsWith("Most expensive mistake") && l.Contains("Player 1")), string.Join(" | ", results.Stats));
            Assert.IsTrue(results.Stats.Any(l => l.StartsWith("Butterfingers: Player 1")), string.Join(" | ", results.Stats));
            Assert.IsNotNull(Object.FindAnyObjectByType<AppraisalScreen>().Showing ? (object)true : null, "the appraisal is up");
        }

        [UnityTest]
        public IEnumerator TheNextRunStartsWellUnderThirtySeconds()
        {
            RunResults results = null;
            yield return Depart(r => results = r);
            int oldSeed = run.State.Seed;
            int oldGeneration = Object.FindAnyObjectByType<Abandoned.Structure.StructureSimulation>().Generation;
            RunState oldRun = run;

            float started = Time.realtimeSinceStartup;
            Object.FindAnyObjectByType<RunDirector>().StartNextRun();
            while ((RunState.Current == null || RunState.Current == oldRun || !RunState.Current.IsSpawned) && Time.realtimeSinceStartup - started < 30f)
                yield return null;
            float took = Time.realtimeSinceStartup - started;
            yield return new WaitForSeconds(0.3f);
            Assert.Less(took, 30f, "next run");
            Assert.Less(took, 3f, "in place, nobody reconnects: it should be near-instant");

            RunState next = RunState.Current;
            Assert.AreEqual(RunPhase.Running, next.State.Phase);
            Assert.AreNotEqual(oldSeed, next.State.Seed, "a new run, a new seed");
            Assert.AreEqual(0, next.State.Haul);
            Assert.AreEqual(next.State.Seed, spawner.Seed);
            Assert.Greater(spawner.Spawned.Count(l => l != null && l.IsSpawned), 40, "fresh loot");
            Assert.Greater(Object.FindAnyObjectByType<Abandoned.Structure.StructureSimulation>().Generation, oldGeneration, "building restored and re-rolled");
            Assert.Less(Vector3.Distance(rig.Player.transform.position, Abandoned.Player.PlayerSpawnPoint.PoseFor(0).position), 1f, "back at the spawn");
            Assert.IsFalse(Object.FindAnyObjectByType<AppraisalScreen>().Showing);
        }
    }
}

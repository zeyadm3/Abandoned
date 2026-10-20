using System.Collections;
using System.Linq;
using System.Reflection;
using Abandoned.Networking;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>Escaping an encounter must create time to move, including for a solo Stalker target.</summary>
    public class ThreatEscapeTests
    {
        private PlayerTestRig rig;
        private Stalker stalker;
        private ThreatPacingConfig original, tuning;
        private static FieldInfo CurrentField => typeof(ThreatPacingConfig).GetField("current", BindingFlags.Static | BindingFlags.NonPublic);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestMapScene.Load("Mall");
            rig = PlayerTestRig.ForExisting(TestMapScene.Player);
            for (int i = 0; i < 60 && ThreatDirector.Current == null; i++) yield return null;
            Assert.IsNotNull(ThreatDirector.Current);
            yield return MallTestSpots.StepInside();
            // Keep this an encounter test: unrelated arrivals must not interfere with the prey.
            ThreatDirector director = ThreatDirector.Current;
            director.enabled = false;
            foreach (Threat t in Threat.All.ToArray())
                if (t != null && t.IsSpawned) t.NetworkObject.Despawn(true);

            original = ThreatPacingConfig.Current;
            tuning = Object.Instantiate(original);
            Tune("MaxEncounterSeconds", 20f);
            Tune("LostSightSearchSeconds", 1.5f);
            Tune("RecoverySeconds", 10f);
            CurrentField.SetValue(null, tuning);
            stalker = director.SpawnOf<Stalker>() as Stalker;
            Assert.IsNotNull(stalker);
            (Vector3 me, Vector3 spot) = MallTestSpots.OpenLine(7f);
            Assert.IsTrue(stalker.GetComponent<NavMeshAgent>().Warp(spot));
            rig.Teleport(new Vector3(me.x, spot.y + 0.05f, me.z), 0f);
            rig.Settle();
            yield return new WaitForSeconds(0.2f);
            float until = Time.time + 3f;
            while (stalker.State != StalkerState.Frozen && Time.time < until) yield return null;
            Assert.AreEqual(StalkerState.Frozen, stalker.State, "the open atrium gives it a visible, watching prey");
        }

        [TearDown]
        public void TearDown()
        {
            if (tuning != null)
            {
                CurrentField.SetValue(null, original);
                Object.DestroyImmediate(tuning);
            }
            rig?.Destroy();
        }

        private void Tune(string field, float value) => typeof(ThreatPacingConfig)
            .GetField($"<{field}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(tuning, value);

        [UnityTest]
        public IEnumerator LosingSightEndsTheSearchAndReappearingDoesNotImmediatelyRestartIt()
        {
            Vector3 mid = (stalker.transform.position + rig.Player.transform.position) * 0.5f;
            GameObject blocker = rig.AddBox("Sight blocker", new Vector3(mid.x, 2f, mid.z), new Vector3(40f, 6f, 0.5f));
            NavMeshObstacle obstacle = blocker.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Vector3.one;
            obstacle.carveOnlyStationary = false;
            obstacle.carving = true;
            rig.Teleport(rig.Player.transform.position - Vector3.right * 4f);
            rig.Settle();
            float until = Time.time + tuning.LostSightSearchSeconds + 0.75f;
            while (!stalker.IsWithdrawing && Time.time < until) yield return null;
            Assert.IsTrue(stalker.IsWithdrawing, "a lost trail ends in withdrawal rather than locker camping");
            Assert.IsNull(stalker.Target);
            Assert.IsFalse(stalker.Hunts(NetworkPlayer.Local), "the prey warning clears too");

            blocker.SetActive(false);
            Vector3 near = stalker.transform.position + Vector3.back * 3f;
            rig.Teleport(near);
            rig.Settle();
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(stalker.IsWithdrawing);
            Assert.IsNull(stalker.Target, "visible movement during recovery cannot instantly restart the chase");
        }

        [UnityTest]
        public IEnumerator WatchingASoloStalkerEventuallyMakesItLeave()
        {
            Tune("MaxEncounterSeconds", 2f);
            float until = Time.time + 2.75f;
            while (!stalker.IsWithdrawing && Time.time < until) yield return null;
            Assert.IsTrue(stalker.IsWithdrawing, "a player need not stare at it for the rest of the run");
            Assert.IsNull(stalker.Target);
            Assert.AreEqual(0f, stalker.Dread01);
            Assert.IsFalse(NetworkPlayer.Local.IsDead);
            yield return null;
            Assert.IsTrue(stalker.WithdrawalDestination.HasValue, "withdrawal chooses a connected place to go");
        }
    }
}

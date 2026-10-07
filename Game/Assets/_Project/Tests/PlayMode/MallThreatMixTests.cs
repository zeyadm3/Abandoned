using System.Collections;
using System.Linq;
using System.Reflection;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>The Stalker and the Collector in the mall (M6.6), with their clocks squeezed.</summary>
    public class MallThreatMixTests
    {
        private PlayerTestRig rig;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestMapScene.Load("Mall");
            rig = ForExisting(TestMapScene.Player);
            for (int i = 0; i < 60 && (RunState.Current == null || ThreatDirector.Current == null); i++) yield return null;
        }

        private static T Fast<T>(Threat threat, string field, params (string name, object value)[] values) where T : ScriptableObject
        {
            FieldInfo configField = threat.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            var copy = Object.Instantiate((T)configField.GetValue(threat));
            foreach ((string name, object value) in values)
                typeof(T).GetField($"<{name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(copy, value);
            configField.SetValue(threat, copy);
            return copy;
        }

        [Test]
        public void RunsOpenWithAMixOfThreats()
        {
            ThreatDirector director = ThreatDirector.Current;
            var counts = Enumerable.Range(1, 400).GroupBy(director.Opening).ToDictionary(g => g.Key, g => g.Count());
            Assert.AreEqual(4, counts.Count, "all four kinds can open a run");
            Assert.Greater(counts[0], counts[1], "the Blind One most often");
        }

        [UnityTest]
        public IEnumerator TheStalkerFreezesWhenWatchedAndKillsWhenIgnored()
        {
            var stalker = (Stalker)ThreatDirector.Current.SpawnOf<Stalker>();
            Assert.IsNotNull(stalker);
            Fast<StalkerConfig>(stalker, "config", ("RushAfter", 1.5f));
            Vector3 spot = stalker.transform.position;
            yield return new WaitForSeconds(0.2f);

            // Face it from a few metres: it stops.
            Vector3 me = spot + Vector3.back * 6f;
            rig.Teleport(new Vector3(me.x, spot.y + 0.05f, me.z), 0f);
            rig.Settle();
            yield return new WaitForSeconds(0.5f);
            Assert.IsTrue(stalker.IsWatchedBy(NetworkPlayer.Local));
            Assert.AreEqual(StalkerState.Frozen, stalker.State);

            // Turn away (alone): it rushes and kills.
            rig.Teleport(rig.Player.transform.position, 180f);
            float end = Time.time + 8f;
            while (!NetworkPlayer.Local.IsDead && Time.time < end) yield return null;
            var agent = stalker.GetComponent<UnityEngine.AI.NavMeshAgent>();
            Assert.IsTrue(NetworkPlayer.Local.IsDead, $"it got you (state {stalker.State}, dread {stalker.Brain.Dread:0.0}, " +
                $"distance {Vector3.Distance(stalker.transform.position, rig.Player.transform.position):0.0}, me {rig.Player.transform.position}, it {stalker.transform.position}, " +
                $"onMesh {agent.isOnNavMesh}, path {agent.pathStatus}, stopped {agent.isStopped}, remaining {agent.remainingDistance:0.0}, " +
                $"speed {agent.speed} accel {agent.acceleration} vel {agent.velocity} desired {agent.desiredVelocity} hasPath {agent.hasPath} updPos {agent.updatePosition} enabled {agent.enabled} scale {stalker.SpeedScale})");
        }

        [UnityTest]
        public IEnumerator TheCollectorStealsUnattendedLootAndDropsItWhenCaught()
        {
            var collector = (Collector)ThreatDirector.Current.SpawnOf<Collector>();
            Fast<CollectorConfig>(collector, "config", ("Cooldown", 0f), ("HideDistance", 8f));
            rig.Teleport(new Vector3(24f, 0.05f, -8f)); // out in the parking lot: nothing is guarded
            rig.Settle();
            float end = Time.time + 25f;
            while (collector.Carried == null && Time.time < end) yield return null;
            Assert.IsNotNull(collector.Carried, "it took something nobody was near");
            NetworkLoot stolen = collector.Carried;
            Vector3 takenFrom = stolen.transform.position;
            yield return new WaitForSeconds(1.5f);
            Assert.Greater(Vector3.Distance(stolen.transform.position, takenFrom), 1.5f, "and carried it off");

            // Catch it in the act.
            rig.Teleport(collector.transform.position + collector.transform.forward * 1.5f + Vector3.up * 0.05f);
            rig.Settle();
            yield return new WaitForSeconds(0.5f);
            Assert.IsNull(collector.Carried, "dropped it");
            Assert.AreEqual(CollectorState.Fleeing, collector.State);
            Assert.IsFalse(stolen.Grabbable.Body.isKinematic, "the loot is loose again");
            Assert.AreEqual(0, collector.Kills, "it never hurts anyone");
        }
    }
}

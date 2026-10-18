using System.Collections;
using System.Linq;
using System.Reflection;
using Abandoned.Extraction;
using Abandoned.Networking;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>
    /// 0.24.10 in the mall with a fast clock: a hurt player peaks the Crew Director, its quiet holds the next
    /// arrival and sends an idle monster away from the crew, and arrivals resume once the quiet is over.
    /// </summary>
    public class MallCrewDirectorTests
    {
        private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        private CrewDirectorConfig fast;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestMapScene.Load("Mall");
            for (int i = 0; i < 60 && (RunState.Current == null || ThreatDirector.Current == null || CrewDirector.Current == null); i++) yield return null;
            // Same rules, the loop squeezed into seconds.
            fast = ScriptableObject.Instantiate(CrewDirectorConfig.Current);
            Set("MinBuildUp", 0.5f);
            Set("SustainSeconds", new Vector2(0.5f, 0.5f));
            Set("MaxFade", 1.5f);
            Set("RelaxSeconds", new Vector2(3f, 3f));
            typeof(CrewDirectorConfig).GetField("current", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, fast);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            typeof(CrewDirectorConfig).GetField("current", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
            Object.Destroy(fast);
            yield return null;
        }

        private void Set(string field, object value) =>
            typeof(CrewDirectorConfig).GetField($"<{field}>k__BackingField", Hidden).SetValue(fast, value);

        private static void ArriveNow()
        {
            // The opening wave is over and the next arrival is due right now.
            typeof(ThreatDirector).GetField("spawnAt", Hidden).SetValue(ThreatDirector.Current, 0f);
            typeof(ThreatDirector).GetField("nextArrival", Hidden).SetValue(ThreatDirector.Current, Time.time);
        }

        [UnityTest]
        public IEnumerator AHurtCrewGetsAQuietThenTheMonstersComeAgain()
        {
            ThreatDirector threats = ThreatDirector.Current;
            Assert.AreEqual(RunPhase.Running, RunState.Current.State.Phase);
            NetworkPlayer me = NetworkPlayer.Local;
            // Monsters only come once the crew is inside, on the mall's NavMesh: stand on the best connected marker.
            Vector3 floor = MallTestSpots.BestConnected(threats);
            me.OwnerTeleport(floor + Vector3.up * 0.1f);
            yield return new WaitForSeconds(0.3f);
            yield return new WaitForSeconds(0.6f);
            Assert.IsFalse(CrewDirector.Quiet, "a calm crew: building up");

            me.DealDamage(60f, "test");
            Assert.GreaterOrEqual(CrewDirector.Current.Tracks[me].Value, fast.Peak, "a big hit is a peak");
            float end = Time.time + 3f;
            while (!CrewDirector.Quiet && Time.time < end) yield return null;
            Assert.IsTrue(CrewDirector.Quiet, "held at the peak, then the quiet");
            // After the hit's noise, so it has nothing to hunt: a fight already on is left to end by itself.
            BlindOne idle = threats.Spawn();
            Assert.IsNotNull(idle, "a monster to send away");

            int alive = threats.Alive;
            ArriveNow();
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(alive, threats.Alive, "no arrival during the quiet");
            Assert.IsTrue(idle.RoamHint.HasValue, $"the idle monster has somewhere else to be (motion {idle.DesiredMotion}, state {idle.State}, {Vector3.Distance(idle.transform.position, me.transform.position):0} m away, quiet {CrewDirector.Quiet}, pacing {threats.GetComponent<ThreatPacing>() != null})");
            Assert.Greater(Vector3.Distance(idle.RoamHint.Value, me.transform.position), 14f, "away from the crew");

            end = Time.time + 8f;
            while (CrewDirector.Quiet && Time.time < end) yield return null;
            Assert.IsFalse(CrewDirector.Quiet, "the quiet ends");
            Assert.AreEqual(CrewDirectorPhase.BuildUp, CrewDirector.Current.Cycle.Phase);
            ArriveNow();
            end = Time.time + 5f;
            while (threats.Alive <= alive && Time.time < end) yield return null;
            Assert.Greater(threats.Alive, alive, "arrivals resume");
        }

        [UnityTest]
        public IEnumerator SwitchedOffItNeverHoldsAnything()
        {
            Set("Enabled", false);
            NetworkPlayer.Local.DealDamage(60f, "test");
            yield return new WaitForSeconds(2.5f);
            Assert.IsFalse(CrewDirector.Quiet);
        }
    }
}

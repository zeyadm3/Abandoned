using System.Collections;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Structure;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>Danger escalation in the mall with a fast clock: the building weakens, monsters sharpen, the lights flicker.</summary>
    public class MallDangerTests
    {
        private DangerDirector director;
        private DangerConfig fast;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestBuildingScene.Load("Mall");
            for (int i = 0; i < 60 && RunState.Current == null; i++) yield return null;
            director = Object.FindAnyObjectByType<DangerDirector>();
            // Same rules, minutes squeezed into seconds.
            fast = ScriptableObject.Instantiate(director.Config);
            typeof(DangerConfig).GetField("<LevelInterval>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(fast, 1f);
            typeof(DangerConfig).GetField("<AgingInterval>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(fast, 1f);
            typeof(DangerDirector).GetField("config", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(director, fast);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(fast);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DangerRisesTheBuildingWeakensAndTheMonstersSharpen()
        {
            StructureSimulation sim = Object.FindAnyObjectByType<StructureSimulation>();
            float healthBefore = sim.Sections.Where(s => s.CanCollapse).Sum(s => s.HealthFraction);
            var startedAbove = sim.Sections.Where(s => s.CanCollapse && s.HealthFraction >= fast.AgingFloor).ToList();
            BlindOne monster = ThreatDirector.Current.Spawn();
            var light = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(l => l.type == LightType.Point);
            float lit = light.intensity;
            bool flickered = false;

            float end = Time.time + 3.5f;
            while (Time.time < end)
            {
                flickered |= !Mathf.Approximately(light.intensity, lit);
                yield return null;
            }
            int level = RunState.Current.State.Danger;
            Assert.GreaterOrEqual(level, 2, "a level per (fast) interval");
            Assert.AreEqual(1f + level * fast.DecayPerLevel, sim.DangerDecay, 1e-4f, "overloaded floors fail faster");
            Assert.Greater(monster.HearingScale, 1f);
            Assert.Greater(monster.SpeedScale, 1f);
            Assert.Less(sim.Sections.Where(s => s.CanCollapse).Sum(s => s.HealthFraction), healthBefore, "the building ages");
            Assert.IsTrue(startedAbove.Where(s => !s.IsCollapsed).All(s => s.HealthFraction >= fast.AgingFloor - 0.01f || s.Load > 0f),
                "ageing alone never pushes a floor below its floor (pre-damage may start some lower)");
            Assert.IsTrue(flickered, "the lights flickered when the level rose");
            if (level >= fast.ExtraThreatLevel) Assert.GreaterOrEqual(Threat.All.Count, 2, "danger brought another threat");
        }

        [UnityTest]
        public IEnumerator TheNextRunIsCalmAgain()
        {
            yield return new WaitForSeconds(2.2f);
            Assert.Greater(RunState.Current.State.Danger, 0);
            Object.FindAnyObjectByType<RunDirector>().StartNextRun();
            yield return null;
            yield return null;
            Assert.AreEqual(0, RunState.Current.State.Danger);
        }
    }
}

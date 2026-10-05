using System.Collections.Generic;
using System.Linq;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>Pure structural rules: drain, impacts, stages, stability scaling, seeded pre-damage.</summary>
    public class StructureRulesTests
    {
        private static StructureConfig Config() =>
            AssetDatabase.LoadAssetAtPath<StructureConfig>("Assets/_Project/Data/Structure/StructureConfig.asset");

        [Test]
        public void ConfigsAreValid()
        {
            var errors = new List<string>();
            Config().Validate(errors);
            AssetDatabase.LoadAssetAtPath<StructureVisualConfig>("Assets/_Project/Data/Structure/StructureVisualConfig.asset").Validate(errors);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [Test]
        public void NoDrainAtOrUnderCapacity()
        {
            Assert.AreEqual(0f, StructureMath.OverloadDrain(500f, 1000f, 20f, 1f, 1f));
            Assert.AreEqual(0f, StructureMath.OverloadDrain(1000f, 1000f, 20f, 1f, 1f));
        }

        [Test]
        public void DrainScalesWithOverloadDecayAndTime()
        {
            Assert.AreEqual(20f, StructureMath.OverloadDrain(2000f, 1000f, 20f, 1f, 1f), 1e-4f, "2x capacity = full rate");
            Assert.AreEqual(5f, StructureMath.OverloadDrain(1250f, 1000f, 20f, 1f, 1f), 1e-4f);
            Assert.AreEqual(10f, StructureMath.OverloadDrain(1250f, 1000f, 20f, 2f, 1f), 1e-4f, "low stability decays faster");
            Assert.AreEqual(2.5f, StructureMath.OverloadDrain(1250f, 1000f, 20f, 1f, 0.5f), 1e-4f);
        }

        [Test]
        public void ImpactsBelowThresholdDoNothing()
        {
            Assert.AreEqual(0f, StructureMath.ImpactDamage(299f, 300f, 0.02f));
            Assert.AreEqual(2f, StructureMath.ImpactDamage(400f, 300f, 0.02f), 1e-4f);
        }

        [Test]
        public void StagesFollowHealthAndLoad()
        {
            StructureConfig c = Config();
            Assert.AreEqual(StructuralStage.Stable, StructureMath.StageFor(1f, 0.5f, c));
            Assert.AreEqual(StructuralStage.Stressed, StructureMath.StageFor(1f, c.StressedLoadRatio, c), "near capacity creaks even when healthy");
            Assert.AreEqual(StructuralStage.Stressed, StructureMath.StageFor(c.StressedBelow - 0.01f, 0f, c));
            Assert.AreEqual(StructuralStage.Cracking, StructureMath.StageFor(c.CrackingBelow - 0.01f, 0f, c));
            Assert.AreEqual(StructuralStage.Failing, StructureMath.StageFor(0f, 0f, c));
        }

        [Test]
        public void StabilityScalesCapacityAndDecay()
        {
            StructureConfig c = Config();
            Assert.AreEqual(1f, c.CapacityScale(1f), 1e-5f);
            Assert.AreEqual(c.CapacityAtZeroStability, c.CapacityScale(0f), 1e-5f);
            Assert.Less(c.CapacityScale(0.63f), 1f);
            Assert.AreEqual(1f, c.DecayScale(1f), 1e-5f);
            Assert.Greater(c.DecayScale(0.3f), 1f);
            Assert.AreEqual(0f, c.PreDamagedFraction(1f), 1e-5f);
        }

        [Test]
        public void PreDamageIsSeededAndRespectsEligibility()
        {
            StructureConfig c = Config();
            bool[] eligible = Enumerable.Range(0, 40).Select(i => i % 4 != 0).ToArray();
            float[] a = StructureMath.PreDamagePlan(eligible, 0.2f, 99, c);
            float[] b = StructureMath.PreDamagePlan(eligible, 0.2f, 99, c);
            float[] other = StructureMath.PreDamagePlan(eligible, 0.2f, 100, c);
            CollectionAssert.AreEqual(a, b, "same seed, same damage");
            CollectionAssert.AreNotEqual(a, other, "different seed, different damage");
            for (int i = 0; i < eligible.Length; i++)
            {
                if (!eligible[i]) Assert.AreEqual(0f, a[i], $"section {i} can't collapse and mustn't be pre-damaged");
                if (a[i] > 0f) Assert.That(a[i], Is.InRange(c.PreDamageMin, c.PreDamageMax));
            }
            Assert.IsTrue(a.Any(x => x > 0f), "low stability damages something");
            Assert.IsTrue(StructureMath.PreDamagePlan(eligible, 1f, 99, c).All(x => x == 0f), "100% stability damages nothing");
        }

        [Test]
        public void PreDamageOfOneSectionDoesNotDependOnOthers()
        {
            StructureConfig c = Config();
            bool[] all = Enumerable.Repeat(true, 20).ToArray();
            bool[] some = all.ToArray();
            some[3] = false;
            float[] a = StructureMath.PreDamagePlan(all, 0.1f, 5, c);
            float[] b = StructureMath.PreDamagePlan(some, 0.1f, 5, c);
            for (int i = 0; i < 20; i++)
                if (i != 3) Assert.AreEqual(a[i], b[i]);
        }
    }
}

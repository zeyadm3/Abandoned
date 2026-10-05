using System.Collections.Generic;
using Abandoned.Interaction;
using Abandoned.Loot;
using NUnit.Framework;
using UnityEditor;

namespace Abandoned.Tests
{
    public class LootDataTests
    {
        private const string Folder = "Assets/_Project/Data/Loot";

        private static IEnumerable<LootDefinition> AllDefinitions()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:LootDefinition", new[] { Folder }))
                yield return AssetDatabase.LoadAssetAtPath<LootDefinition>(AssetDatabase.GUIDToAssetPath(guid));
        }

        private static LootDefinition Load(string id) =>
            AssetDatabase.LoadAssetAtPath<LootDefinition>($"{Folder}/Loot_{id}.asset");

        private static LootDamageConfig DamageConfig() =>
            AssetDatabase.LoadAssetAtPath<LootDamageConfig>($"{Folder}/LootDamageConfig.asset");

        [Test]
        public void EveryDefinitionValidatesAndHasAUniqueId()
        {
            var errors = new List<string>();
            var ids = new HashSet<string>();
            int count = 0;
            foreach (LootDefinition d in AllDefinitions())
            {
                count++;
                d.Validate(errors);
                Assert.IsTrue(ids.Add(d.Id), $"duplicate id {d.Id}");
            }
            Assert.GreaterOrEqual(count, 10);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [TestCase("gold_watch", CarryClass.Pocket, Fragility.Low)]
        [TestCase("cash_bundle", CarryClass.Pocket, Fragility.None)]
        [TestCase("laptop", CarryClass.OneHand, Fragility.Medium)]
        [TestCase("flatscreen_tv", CarryClass.TwoHand, Fragility.High)]
        [TestCase("antique_vase", CarryClass.TwoHand, Fragility.Extreme)]
        [TestCase("glass_sculpture", CarryClass.TwoHand, Fragility.Extreme)]
        [TestCase("server_rack", CarryClass.Heavy, Fragility.Low)]
        [TestCase("safe", CarryClass.Heavy, Fragility.None)]
        [TestCase("marble_statue", CarryClass.Huge, Fragility.Medium)]
        [TestCase("grand_piano", CarryClass.Huge, Fragility.Medium)]
        public void GddExampleHasExpectedClassAndFragility(string id, CarryClass carryClass, Fragility fragility)
        {
            LootDefinition d = Load(id);
            Assert.IsNotNull(d, id);
            Assert.AreEqual(carryClass, d.CarryClass);
            Assert.AreEqual(fragility, d.Fragility);
        }

        [Test]
        public void GameplayWeightIsSeparateFromClampedPhysicsMass()
        {
            LootDefinition statue = Load("marble_statue");
            Assert.AreEqual(2000f, statue.GameplayWeight);
            Assert.AreEqual(LootDefinition.MaxPhysicsMass, statue.PhysicsMass);
        }

        [Test]
        public void ValueRollIsDeterministicAndInRange()
        {
            LootDefinition painting = Load("small_painting");
            var a = LootMath.RollValue(painting.ValueMin, painting.ValueMax, painting.ConditionMin, painting.ConditionMax, 42);
            var b = LootMath.RollValue(painting.ValueMin, painting.ValueMax, painting.ConditionMin, painting.ConditionMax, 42);
            Assert.AreEqual(a.Value, b.Value);
            Assert.AreEqual(a.Condition, b.Condition);
            for (int seed = 1; seed < 200; seed++)
            {
                var r = LootMath.RollValue(painting.ValueMin, painting.ValueMax, painting.ConditionMin, painting.ConditionMax, seed);
                Assert.That(r.BaseValue, Is.InRange(painting.ValueMin, painting.ValueMax));
                Assert.That(r.Condition, Is.InRange(painting.ConditionMin, painting.ConditionMax));
            }
        }

        [Test]
        public void ImpactLossFollowsFragilityProfiles()
        {
            LootDamageConfig config = DamageConfig();
            FragilityProfile medium = config.Profile(Fragility.Medium);
            Assert.AreEqual(0, LootMath.ImpactLoss(medium, 1000, medium.ImpactThreshold - 0.1f, out _));
            int loss = LootMath.ImpactLoss(medium, 1000, medium.ImpactThreshold + 2f, out bool shattered);
            Assert.IsFalse(shattered);
            Assert.AreEqual(UnityEngine.Mathf.RoundToInt(1000 * 2f * medium.LossPerSpeed), loss);

            FragilityProfile extreme = config.Profile(Fragility.Extreme);
            Assert.AreEqual(1000, LootMath.ImpactLoss(extreme, 1000, 5.2f, out bool vaseShatters));
            Assert.IsTrue(vaseShatters, "a hand-height drop must shatter Extreme items");

            FragilityProfile none = config.Profile(Fragility.None);
            Assert.AreEqual(0, LootMath.ImpactLoss(none, 1000, 30f, out bool noneShatters));
            Assert.IsFalse(noneShatters);
        }

        [Test]
        public void DamageConfigIsValid()
        {
            var errors = new List<string>();
            DamageConfig().Validate(errors);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }
    }
}

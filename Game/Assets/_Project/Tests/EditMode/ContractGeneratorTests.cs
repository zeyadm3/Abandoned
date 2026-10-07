using System.Linq;
using Abandoned.Contracts;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    public class ContractGeneratorTests
    {
        private ContractConfig config;
        private ContractModifier[] modifiers;

        [SetUp]
        public void SetUp()
        {
            modifiers = new[]
            {
                Mod("power_off", powerOff: true), Mod("unstable", stability: -0.2f), Mod("rush_job", window: 0.6f, bonus: 0.25f),
                Mod("fragile_collection", fragile: 3f), Mod("late_game", minLevel: 8),
            };
            config = ScriptableObject.CreateInstance<ContractConfig>();
            config.EditorSetModifiers(modifiers);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
            foreach (ContractModifier m in modifiers) Object.DestroyImmediate(m);
        }

        private static ContractModifier Mod(string id, bool powerOff = false, float stability = 0f, float window = 1f, float bonus = 0f, float fragile = 1f, int minLevel = 1)
        {
            var m = ScriptableObject.CreateInstance<ContractModifier>();
            m.EditorSetup(id, id, "", stability, window, bonus, powerOff, fragile, 1f, minLevel);
            return m;
        }

        [Test]
        public void TheBoardShowsThreeDifferentContractsTheSameOnEveryMachine()
        {
            var a = ContractGenerator.Roll(config, 1, 77, 300000);
            var b = ContractGenerator.Roll(config, 1, 77, 300000);
            Assert.AreEqual(3, a.Count);
            CollectionAssert.AreEqual(a, b, "seeded");
            Assert.AreEqual(3, a.Select(c => c.ModifierId).Distinct().Count(), "no modifier twice on one board");
            Assert.IsTrue(a.All(c => c.IsValid && c.Scene == ContractGenerator.MallScene));
            Assert.IsFalse(a.Any(c => c.ModifierId == "late_game"), "level-gated modifiers stay off early boards");
        }

        [Test]
        public void ModifiersShapeTheContract()
        {
            for (int seed = 1; seed < 200; seed++)
                foreach (Contract c in ContractGenerator.Roll(config, 1, seed, 300000))
                {
                    Assert.That(c.Stability, Is.InRange(0.2f, 1f));
                    Assert.Greater(c.Quota, 0);
                    Assert.Less(c.LootMin, c.LootMax);
                    if (c.ModifierId == "power_off") Assert.IsTrue(c.PowerOff);
                    if (c.ModifierId == "unstable") Assert.LessOrEqual(c.Stability, config.StabilityMax - 0.2f + 0.011f);
                    if (c.ModifierId == "rush_job")
                    {
                        Assert.LessOrEqual(c.WindowSeconds, config.WindowMax * 0.6f + 15f);
                        Assert.GreaterOrEqual(c.PayoutBonus, 0.25f - 0.001f);
                    }
                }
        }

        [Test]
        public void QuotasGrowWithTheCompany()
        {
            double early = Enumerable.Range(1, 50).SelectMany(s => ContractGenerator.Roll(config, 1, s, 300000)).Average(c => c.Quota);
            double later = Enumerable.Range(1, 50).SelectMany(s => ContractGenerator.Roll(config, 6, s, 300000)).Average(c => c.Quota);
            Assert.Greater(later, early * 1.5f);
        }
    }
}

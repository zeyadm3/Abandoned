using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Contracts;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.Tests
{
    public class ContractGeneratorTests
    {
        private ContractConfig config;
        private ContractModifier[] modifiers;
        private LocationCatalog locations;
        private LocationDefinition[] definitions;

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
            definitions = new[]
            {
                Location("mall", "Abandoned Mall", "Mall"),
                Location("school", "Pine Hollow Middle School", "School", stability: -0.03f, quota: 0.82f),
                Location("factory", "Abandoned Factory", "Factory", stability: -0.08f, window: 1.05f, quota: 1.1f),
                Location("prison", "Abandoned Prison", "Prison", stability: -0.06f, window: 0.9f),
            };
            locations = ScriptableObject.CreateInstance<LocationCatalog>();
            locations.EditorSet(definitions);
            config.EditorSetLocations(locations);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
            foreach (ContractModifier m in modifiers) Object.DestroyImmediate(m);
            foreach (LocationDefinition location in definitions) Object.DestroyImmediate(location);
            Object.DestroyImmediate(locations);
        }

        private static ContractModifier Mod(string id, bool powerOff = false, float stability = 0f, float window = 1f, float bonus = 0f, float fragile = 1f, int minLevel = 1)
        {
            var m = ScriptableObject.CreateInstance<ContractModifier>();
            m.EditorSetup(id, id, "", stability, window, bonus, powerOff, fragile, 1f, minLevel);
            return m;
        }

        private static LocationDefinition Location(string id, string displayName, string scene, float stability = 0f,
            float window = 1f, float quota = 1f)
        {
            var location = ScriptableObject.CreateInstance<LocationDefinition>();
            location.EditorSetup(id, displayName, "", scene, stabilityDelta: stability, windowMultiplier: window,
                quotaMultiplier: quota);
            return location;
        }

        [Test]
        public void TheBoardUsesDistinctEligibleLocationsAndIsSeeded()
        {
            var a = ContractGenerator.Roll(config, 1, 77, 300000);
            var b = ContractGenerator.Roll(config, 1, 77, 300000);
            Assert.AreEqual(config.Offers + 1, a.Count, "normal offers plus the daily");
            CollectionAssert.AreEqual(a, b, "seeded");
            Assert.AreEqual(config.Offers + 1, a.Select(c => c.ModifierId).Distinct().Count(), "no modifier twice on one board");
            Assert.IsTrue(a.All(c => c.IsValid && locations.Find(c.LocationId) != null));
            Assert.AreEqual(config.Offers, a.Take(config.Offers).Select(c => c.LocationId).Distinct().Count(),
                "ordinary offers cover different destinations while four are eligible");
            Assert.IsFalse(a.Any(c => c.ModifierId == "late_game"), "level-gated modifiers stay off early boards");
        }

        [Test]
        public void BoardsEventuallyOfferEveryCatalogLocation()
        {
            var seen = new HashSet<string>();
            for (int seed = 1; seed <= 32; seed++)
                foreach (Contract contract in ContractGenerator.Roll(config, 1, seed, 300000)) seen.Add(contract.LocationId);

            CollectionAssert.AreEquivalent(definitions.Select(x => x.Id), seen);
        }

        [Test]
        public void DailyLocationAndSeedAreStableForItsDay()
        {
            DateTime day = new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
            Contract one = ContractGenerator.Daily(config, 300000, day);
            Contract two = ContractGenerator.Daily(config, 300000, day);

            Assert.AreEqual(one, two);
            Assert.AreEqual(ContractGenerator.DailyId, one.ModifierId);
            Assert.IsNotNull(locations.Find(one.LocationId));
            Assert.AreEqual(ContractGenerator.DayId(day), ContractGenerator.DailyDay(config, one, day));
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
                    LocationDefinition location = locations.Find(c.LocationId);
                    if (c.ModifierId == "unstable") Assert.LessOrEqual(c.Stability, config.StabilityMax + location.StabilityDelta - 0.2f + 0.011f);
                    if (c.ModifierId == "rush_job")
                    {
                        Assert.LessOrEqual(c.WindowSeconds, config.WindowMax * location.WindowMultiplier * 0.6f + 15f);
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

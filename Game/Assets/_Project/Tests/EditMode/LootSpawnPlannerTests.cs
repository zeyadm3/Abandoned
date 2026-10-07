using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    public class LootSpawnPlannerTests
    {
        private readonly List<Object> made = new();
        private List<LootSpawnPoint> points;
        private List<LootDefinition> defs;
        private LootSpawnConfig config;

        [SetUp]
        public void SetUp()
        {
            config = Make<LootSpawnConfig>();
            defs = new List<LootDefinition>
            {
                Def("ring", CarryClass.Pocket, false, 1f, "jewelry"),
                Def("tv", CarryClass.TwoHand, false, 1f, "electronics"),
                Def("rack", CarryClass.Heavy, false, 1f, "electronics"),
                Def("statue", CarryClass.Huge, true, 1f, "gallery"),
                Def("piano", CarryClass.Huge, true, 1f, "gallery", "furniture"),
            };
            points = new List<LootSpawnPoint>();
            for (int i = 0; i < 60; i++) points.Add(Point(i % 2 == 0 ? "jewelry" : "electronics", CarryClass.Pocket, CarryClass.TwoHand, false));
            points.Add(Point("electronics", CarryClass.OneHand, CarryClass.Heavy, false));
            points.Add(Point("gallery", CarryClass.Huge, CarryClass.Huge, true));
            points.Add(Point("furniture", CarryClass.Huge, CarryClass.Huge, true));
            points.Add(Point("gallery", CarryClass.Huge, CarryClass.Huge, true));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object o in made) Object.DestroyImmediate(o);
            made.Clear();
        }

        private T Make<T>() where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            made.Add(o);
            return o;
        }

        private LootDefinition Def(string id, CarryClass c, bool jackpot, float rarity, params string[] tags)
        {
            LootDefinition d = Make<LootDefinition>();
            d.EditorSetup(id, id, 100, 100, c >= CarryClass.Heavy ? 100f : 1f, c, Vector3.one * 0.3f, Fragility.Low, SurfaceMaterial.Metal, 0.1f, rarity, PlaceholderShape.Cube, Color.white);
            d.EditorSetSpawning(tags, jackpot);
            return d;
        }

        private LootSpawnPoint Point(string tag, CarryClass min, CarryClass max, bool jackpot)
        {
            var go = new GameObject("Point");
            made.Add(go);
            LootSpawnPoint p = go.AddComponent<LootSpawnPoint>();
            p.EditorSetup(tag, min, max, jackpot);
            return p;
        }

        [Test]
        public void TheSameSeedPlansTheSameRun()
        {
            var a = LootSpawnPlanner.Plan(points, defs, config, 42);
            var b = LootSpawnPlanner.Plan(points, defs, config, 42);
            CollectionAssert.AreEqual(a.Select(p => (p.Point, p.Definition, p.ValueSeed)), b.Select(p => (p.Point, p.Definition, p.ValueSeed)));
            var c = LootSpawnPlanner.Plan(points, defs, config, 43);
            CollectionAssert.AreNotEqual(a.Select(p => (p.Point, p.Definition)), c.Select(p => (p.Point, p.Definition)), "another seed, another run");
        }

        [Test]
        public void EveryItemFitsItsPointsKindAndSize()
        {
            for (int seed = 1; seed < 50; seed++)
                foreach (var p in LootSpawnPlanner.Plan(points, defs, config, seed))
                {
                    LootSpawnPoint point = points[p.Point];
                    Assert.IsTrue(p.Definition.HasSpawnTag(point.Tag), $"{p.Definition.Id} on a {point.Tag} point");
                    Assert.That(p.Definition.CarryClass, Is.InRange(point.MinClass, point.MaxClass));
                    Assert.AreEqual(point.IsJackpot, p.Definition.Jackpot, "jackpots only on jackpot points, and nothing else there");
                }
        }

        [Test]
        public void OneOrTwoJackpotsPerRun()
        {
            var counts = new HashSet<int>();
            for (int seed = 1; seed < 100; seed++)
            {
                int jackpots = LootSpawnPlanner.Plan(points, defs, config, seed).Count(p => p.Definition.Jackpot);
                Assert.That(jackpots, Is.InRange(config.JackpotsMin, config.JackpotsMax));
                counts.Add(jackpots);
            }
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, counts, "both happen");
        }

        [Test]
        public void AHeavyJackpotJobAddsAJackpot()
        {
            int jackpotPoints = points.Count(p => p.IsJackpot);
            for (int seed = 1; seed < 60; seed++)
            {
                int jackpots = LootSpawnPlanner.Plan(points, defs, config, seed, extraJackpots: 1).Count(p => p.Definition.Jackpot);
                Assert.That(jackpots, Is.InRange(System.Math.Min(jackpotPoints, config.JackpotsMin + 1), System.Math.Min(jackpotPoints, config.JackpotsMax + 1)));
            }
        }

        [Test]
        public void AboutTheFillChanceOfOrdinaryPointsGetLoot()
        {
            int filled = 0, runs = 50;
            for (int seed = 1; seed <= runs; seed++)
                filled += LootSpawnPlanner.Plan(points, defs, config, seed).Count(p => !p.Definition.Jackpot);
            float ratio = filled / (float)(runs * 61);
            Assert.AreEqual(config.FillChance, ratio, 0.05f);
        }
    }
}

using System.Collections;
using System.Linq;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>
    /// M7.2: the mall's ceiling fixtures. They light only with power and while their ceiling slab is
    /// up, dead tubes stay dark, and every fixture stutters when the building is disturbed.
    /// </summary>
    public class MallLightingTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestMapScene.Load("Mall");
        }

        [Test]
        public void FixturesHangThroughoutTheMallAndMostAreLit()
        {
            LightFixture[] fixtures = Object.FindObjectsByType<LightFixture>(FindObjectsSortMode.None);
            Assert.Greater(fixtures.Length, 60, "fixtures over every floor");
            // The 0.15.5 salvage mall is mostly dark by design: 75 % dead tubes, 10 % faulty (MeridianPresentationConfig;
            // Docs/SALVAGE_MALL_UNITY_SETUP.md), and the horror director cuts some sectors' power each job.
            Assert.That(fixtures.Count(f => f.Dead) / (float)fixtures.Length, Is.InRange(0.5f, 0.9f), "most tubes are dead");
            Assert.IsTrue(fixtures.Any(f => f.Lit), "but some still light the way");
            Assert.IsTrue(fixtures.Any(f => f.Faulty), "some tubes flicker");
            foreach (LightFixture f in fixtures.Where(f => f.Dead))
                Assert.IsFalse(f.Lamp.enabled, $"{f.name} is a dead tube");
            Assert.IsTrue(RenderSettings.fog, "the mall has fog");
        }

        [UnityTest]
        public IEnumerator PowerAndCeilingSwitchAFixture()
        {
            LightFixture fixture = Object.FindObjectsByType<LightFixture>(FindObjectsSortMode.None)
                .First(f => f.Lit && !f.Faulty && f.GetComponent<SectionProp>() != null && f.GetComponent<SectionProp>().Section.CanCollapse);
            Light lamp = fixture.Lamp;
            float full = fixture.FullIntensity;
            Assert.IsTrue(lamp.enabled);

            fixture.SetPowered(false);
            Assert.IsFalse(lamp.enabled, "power off: dark");
            fixture.SetPowered(true);
            Assert.IsTrue(lamp.enabled);
            Assert.AreEqual(full, lamp.intensity, 1e-4f);

            StructuralSection ceiling = fixture.GetComponent<SectionProp>().Section;
            ceiling.Collapse();
            yield return null;
            Assert.IsFalse(lamp.enabled, "its ceiling fell: the fixture went with it");
            Assert.IsFalse(fixture.GetComponentInChildren<Renderer>().enabled, "and its panel is gone");
            fixture.SetPowered(true);
            Assert.IsFalse(lamp.enabled, "power doesn't relight a fallen fixture");
            ceiling.ResetState();
            yield return null;
            Assert.IsTrue(lamp.enabled, "the next run restores it");
        }

        [UnityTest]
        public IEnumerator DisturbanceMakesSteadyFixturesStutter()
        {
            LightFixture fixture = Object.FindObjectsByType<LightFixture>(FindObjectsSortMode.None).First(f => f.Lit && !f.Faulty);
            yield return new WaitUntil(() => !LightFixture.IsDisturbed); // an earlier test's groan may still be fading
            float full = fixture.FullIntensity;
            LightFixture.Disturb(1.5f);
            float lowest = full;
            for (float t = 0f; t < 1.5f; t += Time.deltaTime)
            {
                lowest = Mathf.Min(lowest, fixture.Lamp.enabled ? fixture.Lamp.intensity : 0f);
                yield return null;
            }
            Assert.Less(lowest, full * 0.6f, "the light dipped while the building groaned");
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(full, fixture.Lamp.intensity, 1e-4f, "steady again afterwards");
        }
    }
}

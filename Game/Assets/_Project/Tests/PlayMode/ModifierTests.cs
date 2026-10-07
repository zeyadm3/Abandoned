using System.Collections;
using Abandoned.Audio;
using Abandoned.Extraction;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>M9.2: night darkens the mall, a storm dulls the threats' hearing, speeds decay and howls.</summary>
    public class ModifierTests
    {
        [UnityTest]
        public IEnumerator NightAndStormChangeTheRun()
        {
            yield return TestBuildingScene.Load("Mall");
            RunState run = RunState.Current;
            var power = Object.FindAnyObjectByType<PowerController>();
            yield return null;
            Light sun = RenderSettings.sun;
            float daySun = sun.intensity;
            float dayAmbient = RenderSettings.ambientSkyColor.grayscale;

            RunState.RunTerms t = run.Terms;
            run.ServerSetTerms(new RunState.RunTerms(t.Quota, t.Window, t.Bonus, false, night: true, storm: true, hearing: 0.6f, decay: 1.4f));
            yield return null;
            yield return null;
            Assert.IsTrue(power.IsNight);
            Assert.Less(sun.intensity, daySun * 0.2f, "the sun is down");
            Assert.Less(RenderSettings.ambientSkyColor.grayscale, dayAmbient * 0.5f, "the building is darker");

            BlindOne blind = ThreatDirector.Current.Spawn();
            yield return new WaitForSeconds(1.2f); // the danger director re-applies scales every frame
            Assert.Less(blind.HearingScale, 1f, "the storm covers footsteps");
            Assert.Greater(Object.FindAnyObjectByType<Abandoned.Structure.StructureSimulation>().DangerDecay, 1.3f, "and works on the structure");
            var wind = Object.FindAnyObjectByType<LevelAmbience>().transform.Find("Wind").GetComponent<AudioSource>();
            Assert.Greater(wind.pitch, 1.1f, "the wind howls");

            run.ServerSetTerms(new RunState.RunTerms(t.Quota, t.Window, t.Bonus, false));
            yield return null;
            yield return null;
            Assert.AreEqual(daySun, sun.intensity, 1e-3f, "daylight again");
        }
    }
}
